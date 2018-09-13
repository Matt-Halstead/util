<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.dll</Reference>
  <Namespace>System.ComponentModel</Namespace>
</Query>

[ConfigProfile("Test", isExclusive: false)]
public class TestConfig
{
	[ConfigSetting]
	public string Name { get; set; } = "defaultName";

	[ConfigSetting]
	public int Age { get; set; } = -1;
}

[ConfigProfile]
public class TestConfig2
{
	[ConfigSetting]
	public string Name { get; set; } = "blah";

	[ConfigSetting("Age", typeof(int))]
	public string AgeAsString { get; set; } = "777";
}

//[ConfigProfile("Test1", isExclusive: false)]
public class TestConstantsConfig
{
	[ConfigSetting]
	public readonly string Name = "ConstantName";

	[ConfigSetting]
	public readonly int Age = 999;
}

public interface IConfigProfile
{
	object Target { get; }
	string Id { get; }
	IEnumerable<string> Keys { get; }

	bool ContainsKey(string key);
	T Get<T>(string key);
}

public interface IConfigProvider
{
	void RegisterConfig(object configSource, Type configType = null);
	void RegisterConfig(object configSource, string contextId);
	void UnregisterConfig(string contextId);
	void UnregisterConfig(Type contextType);

	IConfigProfile Get(string contextId);
	T Get<T>(string contextId) where T : class;
	T GetSetting<T>(string contextId, string keyName);
}

void Main()
{
	var provider = new ConfigProvider();
	
	var name = provider.Get("Test").Get<string>(nameof(TestConfig.Name));
	var age = provider.Get("Test").Get<int>(nameof(TestConfig.Age));
	(name, age).Dump("defaults after setup via reflection");

	var newTestConfig = new TestConfig { Name = "newTest", Age = 100 };
	provider.RegisterConfig(newTestConfig, "Test");
	name = provider.Get("Test").Get<string>(nameof(TestConfig.Name));
	age = provider.Get("Test").Get<int>(nameof(TestConfig.Age));
	(name, age).Dump("after manually added settings");

	newTestConfig.Name = "Matt";
	newTestConfig.Age = 37;
	name = provider.Get("Test").Get<string>(nameof(TestConfig.Name));
	age = provider.Get("Test").Get<int>(nameof(TestConfig.Age));
	(name, age).Dump("after direct alteration of settings source object");

	// verify the settings source is that most recently added
	var recentSettings = provider.Get<TestConfig>("Test");
	Debug.Assert(recentSettings.Name == name);
	Debug.Assert(recentSettings.Age == age);

	provider.RegisterConfig(new TestConstantsConfig(), "Test");
	name = provider.Get("Test").Get<string>(nameof(TestConfig.Name));
	age = provider.Get("Test").Get<int>(nameof(TestConfig.Age));
	(name, age).Dump("after manually added settings from fields");

	// verify the settings source is TestConstantsConfig most recently added
	var mostRecentSettings = provider.Get<TestConstantsConfig>("Test");
	Debug.Assert(mostRecentSettings.Name == name);
	Debug.Assert(mostRecentSettings.Age == age);

	provider.UnregisterConfig("Test");
	name = provider.Get("Test").Get<string>(nameof(TestConfig.Name));
	age = provider.Get("Test").Get<int>(nameof(TestConfig.Age));
	(name, age).Dump("after removing manual settings");

	var tc = new TestConfig2();
	provider.RegisterConfig(tc);
	var p1 = provider.Get(nameof(TestConfig2)).Target;
	var p2 = provider.Get<TestConfig2>(nameof(TestConfig2));
	ReferenceEquals(p1, p2).Dump("target profile is correct reference");
	
	age = provider.GetSetting<int>(nameof(TestConfig2), "Age");
	(p2.Name, age).Dump("after more manually added settings with custom parsed Age");
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ConfigProfileAttribute : Attribute
{
	/// <summary>
	/// Creates a new instance of the ConfigProfileAttribute class.
	/// </summary>
	/// <param name="contextId">the id of the context within the configuration system. Must not be null/empty.</param>
	/// <param name="isExclusive">if set true, indicates to configuration system that only this class may use the given configId.</param>
	public ConfigProfileAttribute(string contextId = null, bool isExclusive = false)
	{
		ContextId = contextId;
		IsExclusive = isExclusive;
	}

	/// <summary>
	/// Gets the id for the context this profile will be added to.
	/// </summary>
	public string ContextId { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the config id must be unique in the system, and used only by this instance.
	/// </summary>
	public bool IsExclusive { get; private set; }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ConfigSettingAttribute : Attribute
{
	/// <summary>
	/// Creates a new instance of the ConfigSettingAttribute class.
	/// </summary>
	/// <param name="key">the key name of the setting. If set, this will be used in place of the name of the property/field.</param>
	/// <param name="valueType">the type of the setting's value. If set, this will be used in place of that of the property/field.</param>
	public ConfigSettingAttribute(string key = null, Type valueType = null)
	{
		Key = key;
		ValueType = valueType;
	}

	/// <summary>
	/// Gets the configuration setting's key.
	/// </summary>
	public string Key { get; private set; }

	/// <summary>
	/// Gets the configuration setting's value type.
	/// </summary>
	public Type ValueType { get; private set; }
}

internal class ConfigSetting
{
    public ConfigSetting(string key, Type valueType, Func<object, object> valueGetter)
    {
        Key = key;
        ValueType = valueType;
        ValueGetter = valueGetter;
    }

    public string Key { get; private set; }
    public Type ValueType { get; private set; }
    public Func<object, object> ValueGetter { get; private set; }

    public T Get<T>(object target)
    {
        var v = ValueGetter(target);
        var result = default(T);
        if (v != null)
        {
            var converter = TypeDescriptor.GetConverter(ValueType);
            if (!converter.CanConvertTo(typeof(T)))
			{
				throw new ApplicationException($"Cannot convert setting '{Key}' [{ValueType.Name}] to requested type {typeof(T).Name}");
			}

			result = (T)converter.ConvertTo(v, typeof(T));
		}

		return result;
	}
}

internal class ConfigProfile : IConfigProfile
{
	/// <summary>
	/// Default constructor is used to create profiles via reflection.
	/// </summary>
	internal ConfigProfile(object target)
	{
		if (target == null)
		{
			throw new ArgumentException("Target object may not be null.");
		}

		Target = target;
		ConfigureInstanceViaReflection();
	}

	public object Target { get; private set; }

	public string Id { get; internal set; }

	public IEnumerable<string> Keys => _settings.Select(s => s.Key).ToArray();

	public bool ContainsKey(string key) => _settings.ContainsKey(key);

	private Dictionary<string, ConfigSetting> _settings = new Dictionary<string, ConfigSetting>();

	public T Get<T>(string key)
	{
		if (TryGet(key, out T value))
		{
			return value;
		}

		throw new ApplicationException($"Config profile {Id} does not define key '{key}'.");
	}

	public bool TryGet<T>(string key, out T value)
	{
		var success = false;
		value = default(T);

		if (_settings.TryGetValue(key, out ConfigSetting setting))
		{
			value = setting.Get<T>(Target);
			success = true;
		}

		return success;
	}

	internal void Add(ConfigSetting setting)
	{
		if (_settings.ContainsKey(setting.Key))
		{
			throw new ApplicationException($"A setting with key '{setting.Key}' already exists in profile '{Id}'");
		}

		_settings[setting.Key] = setting;
	}

	private void ConfigureInstanceViaReflection()
	{
		// Assign Id from the ConfigSetAttribute if set, else the type name.
		var configSetAttr = Target?.GetType().GetCustomAttributes(typeof(ConfigProfileAttribute), true).FirstOrDefault() as ConfigProfileAttribute;
		Id = configSetAttr?.ContextId ?? Target?.GetType().Name;

		// Extract the settings from properties with ConfigSettingAttributes.
		foreach (PropertyInfo propInfo in Target?.GetType().GetProperties())
		{
			var attr = Attribute.GetCustomAttributes(propInfo, true).OfType<ConfigSettingAttribute>().FirstOrDefault();
			if (attr != null)
			{
				var key = attr.Key ?? propInfo.Name;
				var valueType = attr.ValueType ?? propInfo.PropertyType;
				var setting = new ConfigSetting(key, valueType, target => propInfo.GetValue(target));
				Add(setting);
			}
		}

		// Extract the settings from fields with ConfigSettingAttributes.
		foreach (FieldInfo fieldInfo in Target?.GetType().GetFields())
		{
			var attr = Attribute.GetCustomAttributes(fieldInfo, true).OfType<ConfigSettingAttribute>().FirstOrDefault();
			if (attr != null)
			{
				var key = attr.Key ?? fieldInfo.Name;
				var valueType = attr.ValueType ?? fieldInfo.FieldType;
				var setting = new ConfigSetting(key, valueType, target => fieldInfo.GetValue(target));
				Add(setting);
			}
		}
	}
}

public class ConfigProvider : IConfigProvider
{
    private static readonly Dictionary<string, List<ConfigProfile>> _configLookup = new Dictionary<string, List<ConfigProfile>>();

    static ConfigProvider()
    {
        //RegisterAppConfigSettings();
        FindAllTypesWithConfigProfileAttribute();
    }

    //private static void RegisterAppConfigSettings()
    //{
    //    var appConfigProfile = new ConfigProfile(ConfigurationManager.AppSettings);
        
    //    foreach (var key in ConfigurationManager.AppSettings.AllKeys)
    //    {
    //        var setting = new ConfigSetting(key, typeof(string), target => ((NameValueCollection)target)[key]);
    //        appConfigProfile.Add(setting);
    //    }

    //    Register(appConfigProfile, null, new ConfigProfileAttribute("AppConfig", true));
    //}

    private static ConfigProfileAttribute GetConfigProfileAttribute(Type t)
    {
        return t?.GetCustomAttributes(typeof(ConfigProfileAttribute), true).FirstOrDefault() as ConfigProfileAttribute;
    }

    // Find classes with ConfigSetAttributes and instantiate these.
    private static void FindAllTypesWithConfigProfileAttribute()
    {
        var configSettingsTypes =
            from a in AppDomain.CurrentDomain.GetAssemblies()
            from t in a.GetTypes()
            let attribute = GetConfigProfileAttribute(t)
            where attribute != null
            select new { Type = t, Attribute = attribute };

        foreach (var typeInfo in configSettingsTypes)
        {
            object target;
            try
            {
                target = Activator.CreateInstance(typeInfo.Type);
            }
            catch (Exception)
            {
                throw new ApplicationException($"The type {typeInfo.Type.Name} has a {nameof(ConfigProfileAttribute)} but does not provide a default constructor.");
            }

            Register(new ConfigProfile(target), typeInfo.Attribute.ContextId, typeInfo.Attribute.IsExclusive);
        }
    }

	public void RegisterConfig(object configSource, Type contextType = null)
	{
		if (configSource == null)
		{
			throw new ArgumentException("Cannot register a null object as a configuration profile.");
		}

		var attr = GetConfigProfileAttribute(configSource.GetType());
		var id = contextType?.Name ?? configSource.GetType().Name;
		Register(new ConfigProfile(configSource), id, attr?.IsExclusive ?? false);
	}

	public void RegisterConfig(object configSource, string contextId)
	{
		if (configSource == null)
		{
			throw new ArgumentException("Cannot register a null object as a configuration profile.");
		}

		var attr = GetConfigProfileAttribute(configSource.GetType());
		var id = contextId ?? attr?.ContextId ?? configSource.GetType().Name;
		Register(new ConfigProfile(configSource), id, attr?.IsExclusive ?? false);
	}

	public void UnregisterConfig(Type contextType)
	{
		if (contextType == null)
		{
			throw new ArgumentException("Cannot unregister a configuration profile with null type.");
		}
		
		UnregisterConfig(contextType.Name);
	}
	
	public void UnregisterConfig(string contextId)
	{
		if (string.IsNullOrEmpty(contextId))
		{
			throw new ArgumentException("Cannot unregister config for empty id.");
		}

		if (_configLookup.TryGetValue(contextId, out List<ConfigProfile> profiles))
		{
			if (profiles.Count == 1)
			{
				throw new ApplicationException("Cannot remove last, default profile.");
			}

			var profile = FindProfileById(contextId);
			if (profile == null)
			{
				throw new ArgumentException($"Cannot remove profile '{contextId}': not found.");
			}

			profiles.Remove(profile);
		}
	}
	
	public IConfigProfile Get(string contextId)
	{
		if (string.IsNullOrEmpty(contextId))
		{
			throw new ArgumentException("Configuration id is null/empty.");
		}

		var profile = FindProfileById(contextId);
		if (profile == null)
		{
			throw new ApplicationException($"No settings found for id {contextId}.");
		}
		
		return profile;
	}

	public T Get<T>(string contextId) where T : class
	{
		T target = default(T);

		var profile = Get(contextId);		
		target = profile.Target as T ?? null;
		if (target == null)
		{
			throw new ApplicationException($"Configuration for id {contextId} is not of type {typeof(T).Name}.");
		}
		
		return target;
	}
	
	public T GetSetting<T>(string contextId, string keyName)
	{
		var context = FindProfileById(contextId);
		if (context == null)
		{
			throw new ApplicationException($"ConfigProvider: unknown config id '{contextId}'.");
		}
		
		return context.Get<T>(keyName);
	}

	internal static ConfigProfile FindProfileById(string contextId)
	{
		ConfigProfile result = null;
		if (string.IsNullOrEmpty(contextId))
		{
			throw new ArgumentException($"contextId must not be null.");
		}

		if (_configLookup.TryGetValue(contextId, out List<ConfigProfile> profiles))
		{
			result = profiles.LastOrDefault();
		}
		
		return result;
	}

	private static void Register(ConfigProfile profile, string contextId, bool isExclusive)
	{
		if (profile == null)
		{
			throw new ArgumentException("Cannot add null profile.");
		}

		string id = contextId ?? profile.Target?.GetType().Name ?? string.Empty;;
		if (string.IsNullOrEmpty(id))
		{
			throw new ApplicationException($"Cannot add profile, unable to resolve a useful context id.");
		}

		profile.Id = id;

		if (_configLookup.TryGetValue(id, out List<ConfigProfile> profiles))
		{
			// Ensure we are not trying to register an instance that violates a singleton clause of a ConfigSetAttribute.
			if (isExclusive)
			{
				var existingProfile = FindProfileById(id);
				if (existingProfile != null)
				{
					throw new ApplicationException($"New instance of {profile.GetType().Name} duplicates the singleton ConfigSetAttribute Id '{id}', already defined by type {existingProfile.GetType().Name}.");
				}
			}

			if (profile.Id != profiles[0].Id)
			{
				throw new ArgumentException($"Cannot add profile, id '{profile.Id}' differs to default profile id '{profiles[0].Id}");
			}

			if (profiles.Contains(profile))
			{
				throw new ApplicationException($"Cannot add profile '{profile.Id}' twice.");
			}

			profiles.Add(profile);
		}
		else
		{
			_configLookup[profile.Id] = new List<ConfigProfile>(new[] { profile });
		}
	}
}