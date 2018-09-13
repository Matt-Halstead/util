<Query Kind="Program">
  <Reference>C:\Source\Seymour\Main Branch\UIClient\bin\x64\Debug\NavigateSurgical.Common.General.dll</Reference>
  <Reference>C:\Source\Seymour\Main Branch\UIClient\bin\x64\Debug\NavigateSurgical.Common.Interfaces.dll</Reference>
  <Namespace>NavigateSurgical.Common.General.Configuration</Namespace>
  <Namespace>NavigateSurgical.Common.Interfaces</Namespace>
  <Namespace>NavigateSurgical.Common.Interfaces.Factories</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Reflection</Namespace>
</Query>

// !!!!! BEFORE YOU RUN THIS !!!!!!!
//
// Update this path to point to an existing build of Inliant, the path to the exe and dlls.

const string MasterFactoryBinPath = @"C:\Source\Seymour\Main Branch\UIClient\bin\x64\Debug";

void Main()
{
	if (!Directory.Exists(MasterFactoryBinPath))
	{
		throw new ApplicationException("Bin path not set or does not exist.");
	}
	
	// the old way: find a suitable factory which must first be instantiated, then call a method in that factory
	IDataFolderProvider provider = MasterFactory.GetFactory<IDataFolderProviderFactory>().GetDataFolderProvider();
	Console.WriteLine($"The data folder name is '{provider.DataFolderName}'.");

	// the new way: MasterFactory is the factory, works out how based on attributes via reflection
	IDataFolderProvider newProvider = MasterFactory.GetInstanceOf<IDataFolderProvider>();
	Console.WriteLine($"The data folder name is '{newProvider.DataFolderName}'.");
}

// Anything decorated with this attribute can be 'seen' by the new factory system.
// This applies only to classes that can be created without additional state, i.e. via a default constructor.  If this is not the case, existing method is required.
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FactoryTargetAttribute : Attribute
{
}

[FactoryTarget]
internal class NewDataFolderProvider : IDataFolderProvider
{
	public string DataFolderKey { get; } = "new awesome";
	public string DataFolderName { get; } = "new awesome";
	public string ImportDataFolderName { get; } = "new awesome";
	public string DbServerUser { get; } = "new awesome";
	public string DbServerPass { get; } = "new awesome";

	public bool IsPostActive { get; set; }
}


// ============= Factory impl ============/

/// <summary>
/// This class provides a singleton instance that is a generic container for factory objects 
/// that derive from the IFactory (an empty) interface. At startup, the application registers 
/// new instances of the factory object with the master factory by calling its InitializeFactories() method.  
/// Instances can then be retrieved using its GetFactory() call. This approach allows each project 
/// in the solution to only reference the ‘Interfaces’ and ‘Factories’ projects, simplifying the 
/// project interdependencies.
/// </summary>
public class MasterFactory
{
	private class FactoryItem
	{
		public readonly IFactory Factory;
		public readonly string AssemblyFileName;
		public readonly string TypeName;

		internal FactoryItem(IFactory factory, string assemblyFileName, string typeName)
		{
			Factory = factory;
			AssemblyFileName = assemblyFileName;
			TypeName = typeName;
		}
	}

	private static readonly MasterFactory _instance = new MasterFactory();
	private readonly List<FactoryItem> _factories;

	static MasterFactory()
	{
		InitializeFactories(ignoredAssemblies: null, ignoreTypeInitializationExceptions: true);
	}

	/// <summary>
	/// Gets singleton instance.
	/// </summary>
	public static MasterFactory Instance { get { return _instance; } }

	/// <summary>
	/// Initialize factories.
	/// </summary>
	/// <param name="ignoredAssemblies">Assembilis to ignore.</param>
	/// <param name="ignoreTypeInitializationExceptions">True if ignore type initialization exceptions.</param>
	public static void InitializeFactories(List<string> ignoredAssemblies = null, bool ignoreTypeInitializationExceptions = false)
	{
		// find all assemblies containing IFactory implementation

//// Pointing to the constant path set above!
		//var executingDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;		
		var executingDirectory = MasterFactoryBinPath;
/////////////
		
		var candidates = Directory.EnumerateFiles(executingDirectory, "NavigateSurgical*.dll");

		foreach (var c in candidates)
		{
			var assyFileName = Path.Combine(executingDirectory, c);
			LoadFactories(assyFileName, ignoredAssemblies, ignoreTypeInitializationExceptions);
		}
	}

	private static void LoadFactories(string assyFileName, List<string> ignoredAssemblies, bool ignoreTypeInitializationExceptions)
	{
		try
		{
			var factoryType = typeof(IFactory);
			var assy = Assembly.LoadFrom(assyFileName);
			var assyTypes = assy.GetTypes();

			foreach (var at in assyTypes)
			{
				if ((ignoredAssemblies == null) || !ignoredAssemblies.Contains(at.Name))
				{
					if (at.IsClass && factoryType.IsAssignableFrom(at))
					{
						//Console.WriteLine("Found IFactory implementation {0} ({1})", at.Name, at.FullName);
						//System.Diagnostics.Debug.WriteLine("Found IFactory implementation {0} ({1})", at.Name, at.FullName);
						Instance.AddFactory(assyFileName, at.FullName);
					}
					
					if (at.IsClass)
					{
						ExtractFactoryTargetsFrom(at);
					}
				}
			}
		}
		catch (ReflectionTypeLoadException ex)
		{
			Console.WriteLine($"ReflectionTypeLoadException");

			foreach (var le in ex.LoaderExceptions)
			{
				Console.WriteLine($"\t{le.ToString()}");
			}

			if (!ignoreTypeInitializationExceptions)
			{
				throw ex;
			}
		}
		catch (BadImageFormatException ex)
		{
			// System.BadImageFormatException when loading NavigateSurgical.Renderer.FontWrapper.dll.
			Console.WriteLine($"BadImageFormatException");
			Console.WriteLine($"\t{ex.Message}");
			Console.WriteLine($"\t{ex.FusionLog}");

			if (!ignoreTypeInitializationExceptions)
			{
				throw ex;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	/// <summary>
	/// Initializes a new instance of the MasterFactory class.
	/// </summary>
	public MasterFactory()
	{
		_factories = new List<FactoryItem>();
	}

	/// <summary>
	/// Registers factory instance using the assembly file name and type name.
	/// </summary>
	/// <param name="assyFileName"></param>
	/// <param name="typeName"></param>
	private void AddFactory(string assyFileName, string typeName)
	{
		if (!_factories.Any(f => (f.AssemblyFileName == assyFileName) && (f.TypeName == typeName)))
		{
			_factories.Add(new FactoryItem((IFactory)Activator.CreateInstanceFrom(assyFileName, typeName).Unwrap(), assyFileName, typeName));
		}
	}

	//[Obsolete]
	/// <summary>
	/// Registers factory instance. Special case for diagnosing problems with renderer tests
	/// </summary>
	/// <typeparam name="TFactory">Type of factory</typeparam>
	/// <param name="factory">Factory instance</param>
	/// <returns></returns>
	public TFactory AddFactory<TFactory>(TFactory factory) where TFactory : IFactory
	{
		if (!_factories.Any(f => f.Factory.GetType() == typeof(TFactory)))
		{
			_factories.Add(new FactoryItem(factory, string.Empty, typeof(TFactory).ToString()));
		}

		return factory;
	}

	/// <summary>
	/// Gets factory instance of the specified factory type.
	/// </summary>
	/// <typeparam name="T">Type of factory.</typeparam>
	/// <returns>Factory instance</returns>
	public static T GetFactory<T>(bool allowMissing = false) where T : class, IFactory
	{
		T res = null;
		var factoryItem = _instance._factories.Where(x => x.Factory is T).FirstOrDefault();

		if (factoryItem != null)
		{
			res = factoryItem.Factory as T;

			if ((res == null) && !allowMissing)
			{
				var error = string.Format("Factory interface {0} not found", typeof(T));
				Console.WriteLine("Exception: " + error);
				throw new ApplicationException(error);
			}

		}

		return res;
	}

	//============ New stuff ===============/

	private static Dictionary<Type, Lazy<object>> _factoryTargetsLookup = new Dictionary<System.Type, System.Lazy<object>>();

	private static void ExtractFactoryTargetsFrom(Type type)
	{
		var factoryTargetAttr = type.GetCustomAttributes(typeof(FactoryTargetAttribute), true).FirstOrDefault() as FactoryTargetAttribute;
		if (factoryTargetAttr != null)
		{
			foreach (var interfaceType in type.GetInterfaces())
			{
				// get the interfaces implemented by this type and for each, add an entry to the lookup pointing to the same instance.
				if (_factoryTargetsLookup.ContainsKey(interfaceType))
				{
					throw new ApplicationException($"The type {interfaceType.Name} has already been registered as a factory target.");
				}
				
				_factoryTargetsLookup[interfaceType] = new Lazy<object>(() => Activator.CreateInstance(interfaceType));
			}
		}
	}
	

	public static T GetInstanceOf<T>(bool allowMissing = false) where T : class
	{
		T result = null;
		if (_factoryTargetsLookup.TryGetValue(typeof(T), out Lazy<object> initializer))
		{
			result = initializer.Value as T;
		}

		if (result == null && !allowMissing)
		{
			throw new ApplicationException($"The type {typeof(T).Name} cannot be instantiated by the factory.");
		}
		
		return result;
	}
}