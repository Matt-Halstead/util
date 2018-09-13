<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Xml.Linq</Namespace>
</Query>

void Main()
{
	new Program().Dump();
}

class PTConfigFileReader
{
	private string _filename;
	
	public PTConfigFileReader(string filename)
	{
		_filename = filename;
	}
	
	public bool Parse(out PTConfig config)
	{	
		bool result = false;
		config = null;
		
		var content = File.ReadAllText(_filename);
		XDocument doc = XDocument.Parse(content);
		var root = doc.Root;
		if (root != null && root.Name == "opencv_storage")
		{
			config = new PTConfig();

			config.Target = SafeReadInt(root.Element("target"));
			config.Square = SafeReadDouble(root.Element("square"));
			config.Step = SafeReadDouble(root.Element("step"));
			config.TubeRadius = SafeReadDouble(root.Element("radius"));
			config.DotDiameter = SafeReadDouble(root.Element("diameter"));
			config.RepeatingMarkerId = SafeReadInt(root.Element("identifier"));
			config.OffsetX = SafeReadDouble(root.Element("x"));
			config.OffsetY = SafeReadDouble(root.Element("y"));
			config.OffsetZ = SafeReadDouble(root.Element("z"));
			config.Rotation = SafeReadDouble(root.Element("rotate"));
			
			var markers = root.Element("markers")?.Elements("_") ?? new XElement[0];
			double currentDistance = 0;
			foreach (var marker in markers)
			{
				int id = SafeReadInt(marker.Element("id"));
				double rotation = SafeReadDouble(marker.Element("rotation"));
				currentDistance = SafeReadDouble(marker.Element("distance"), currentDistance);

				config.Add(new PTMarker(id, currentDistance, rotation));
				result = true;
			}
		}
		
		return result;
	}
	
	private string SafeReadString(XElement element)
	{
		return element?.Value ?? string.Empty;
	}
	
	private int SafeReadInt(XElement element, int defaultValue = 0)
	{
		return int.TryParse(SafeReadString(element), out int result) ? result : defaultValue;		
	}

	private double SafeReadDouble(XElement element, double defaultValue = 0.0)
	{
		return double.TryParse(SafeReadString(element), out double result) ? result : defaultValue;
	}
}

class PTMarker
{
	private static PointF[] DotRelativeOffsets = new PointF[]
	{
		new PointF(-1.0f,  1.0f),
		new PointF(-1.0f,    0f),
		new PointF(-1.0f, -1.0f),
		new PointF(   0f, -1.0f),
		new PointF( 1.0f, -1.0f),
		new PointF( 1.0f,    0f),
		new PointF( 1.0f,  1.0f),
		new PointF(   0f,  1.0f)
	};

	private Marker _marker;

	public int Id { get; internal set; }
	public double Distance { get; internal set; }
	public double Rotation { get; internal set; }

	public int TrueId => Id % 256;

	public PTMarker(int id, double distance, double rotation)
	{
		Id = id;
		Distance = distance;
		Rotation = rotation;

		_marker = KnownMarkers.GetMarker(TrueId);
		if (_marker == null || _marker.Id == 0)
		{
			throw new ArgumentException($"The id {Id} ({TrueId}) is not for a recognized marker.");
		}
	}
	
	public PointF GetDotRelativePosition(int dotIdx, int rotations, int signX = 1, int signY = 1)
	{
		int idx = (dotIdx - 1 + (rotations * 2)) % 8;
		return new PointF(DotRelativeOffsets[idx].X * signX, DotRelativeOffsets[idx].Y * signY);
	}
}

class PTConfig
{
	public enum PTOrienation { A, B }

	public int Target { get; internal set; } = 0;
	public double Square { get; internal set; } = 6.25;
	public double Step { get; internal set; } = 0.256;
	public double TubeRadius { get; internal set; } = 7.5;
	public double DotDiameter { get; internal set; } = 1.5;
	public int RepeatingMarkerId { get; internal set; } = 221;
	public double OffsetX { get; internal set; } = 6.0;
	public double OffsetY { get; internal set; } = 17.5;
	public double OffsetZ { get; internal set; } = -32.0;
	public double Rotation { get; internal set; } = 0.0;

	public PTOrienation Orientation => RepeatingMarkerId == 187 ? PTOrienation.A : PTOrienation.B;

	public List<PTMarker> MarkerDefinitions => _markerDefinitions;

	public void Add(PTMarker marker)
	{
		_markerDefinitions.Add(marker);
	}

	private readonly List<PTMarker> _markerDefinitions = new List<PTMarker>();
}

class Program
{
	private readonly PTConfig config = new PTConfig();
	private readonly List<PointF> centreDots = new List<PointF>();
	private readonly List<Tuple<int, int, PointF, int, double>> allDots = new List<Tuple<int, int, PointF, int, double>>();
	
	public Program()
	{
		//string filename = @"C:\Source\Seymour\Main Branch\PatientTrackerMeasurement\_config\pt-5100090-sn170013-input.xml";
		string filename = @"C:\Source\Seymour\Main Branch\PatientTrackerMeasurement\_config\pt-5100100-sn170013-input.xml";
		
		if (new PTConfigFileReader(filename).Parse(out config))
		{
			foreach (var markerDef in config.MarkerDefinitions)
			{
				double markerY;
				int rotations;
				
				if (config.Orientation == UserQuery.PTConfig.PTOrienation.A)
				{
					markerY = -1.0 * config.TubeRadius * markerDef.Rotation;
					rotations = 0;
				}
				else
				{
					markerY = config.TubeRadius * (markerDef.Rotation + Math.PI);
					rotations = 2;
				}
				
				double dotSpacing = config.Step * config.TubeRadius;

				double xMinPixel = 71.741;
				double xMaxPixel = 2003.584;
				double xMinWorld = Math.Abs(-42.433);
				double xMaxWorld = Math.Abs(-9.588);
				
				var dotIndices = GetDotIndices(markerDef.Id).ToList();
				var relPositions = dotIndices
					.Select(dotI => Tuple.Create(dotI, markerDef.GetDotRelativePosition(dotI, rotations, Math.Sign(markerDef.Distance))))
					.ToList();
				relPositions.Insert(0, Tuple.Create(0, new PointF(0, 0)));
								
				foreach (var r in relPositions)
				{
					// Ignore sign of x when offsetting centre point for dot locations, else will mirror the dots.
					var dotPos = new PointF((float)(dotSpacing * r.Item2.X + markerDef.Distance), (float)(dotSpacing * r.Item2.Y + markerY));

					double proportion = (dotPos.X - xMinWorld) / (xMaxWorld - xMinWorld);
					int imagePixelX = (int)Math.Round(xMinPixel + (proportion * (xMaxPixel - xMinPixel)));

					// NOTE: All measured nominal dot angles will need to be at least similar to this known angle.
					// If they dont match then the stage offset angle is not set correctly.
					double knownAngle = ClampAngleDegrees((dotPos.Y / (Math.PI * 2 * config.TubeRadius)) * 360.0);

					allDots.Add(Tuple.Create(markerDef.Id, r.Item1, dotPos, imagePixelX, knownAngle));
				}
			}

//			allDots
//				.Select(dot => $"{dot.Item1}_{dot.Item2} ({dot.Item3.X,7:n3}, {dot.Item3.Y,7:n3}):  ImageX: {dot.Item4,4}, Angle: {dot.Item5,7:n3}")
//				.Dump($"all dots");
		}
	}

	public static double ClampAngleDegrees(double angle)
	{
		while (angle < 0.0)
		{
			angle += 360.0;
		}

		while (angle >= 360.0)
		{
			angle -= 360.0;
		}

		return angle;
	}

	private static int[] GetDotIndices(int markerId)
	{
		var indices = new List<int>();
		for (int i = 0; i < 8; i++)
		{
			if ((markerId & (1 << i)) > 0)
			{
				indices.Add(i + 1);
			}
		}
		return indices.ToArray();
	}

	////////////////////////////////////////////////////////////////////////
	///////////////////// Drawing code  ////////////////////////////////////
	////////////////////////////////////////////////////////////////////////

	object ToDump()
	{
		Rectangle ScreenRect = new Rectangle(0, 0, 1200, 1200);
		RectangleF ScreenRectF = ToRectangleF(ScreenRect);
		RectangleF WorldRect = new RectangleF(-60, -60, 120, 120);

		var bitmap = new Bitmap(ScreenRect.Width, ScreenRect.Height);
		var font = new Font(FontFamily.GenericSansSerif, 9.0f, FontStyle.Regular);

		using (var g = Graphics.FromImage(bitmap))
		{
			g.Clear(Color.DarkGray);

			float Radius = (float)(config.DotDiameter / 8.0);

			foreach (var dot in allDots)
			{
				float x = Math.Abs(dot.Item3.X);
				float y = dot.Item3.Y;

				var circleRect = new RectangleF(x - Radius, y - Radius, 2 * Radius, 2 * Radius);
				var circleRectTL = Project(circleRect.Location, WorldRect, ScreenRectF);
				var circleRectSize = Project(new PointF(circleRect.Width, circleRect.Height), WorldRect.Size, ScreenRectF.Size);

				var brush = Brushes.Black;
				g.FillEllipse(brush, circleRectTL.X, ScreenRectF.Height - circleRectTL.Y, circleRectSize.X, circleRectSize.Y);

				if (dot.Item2 == 0)
				{
					var pt = Project(new PointF(x, y), WorldRect, ScreenRectF);
					g.DrawString($"{dot.Item1}", font, Brushes.BlueViolet, pt.X, ScreenRectF.Height - pt.Y);

					float dotSpacing = (float)(config.Step * config.TubeRadius);
					float markerBoxWidth = (dotSpacing / 2.0f) * 6.5f;
					var markerRect = new RectangleF(x - (markerBoxWidth / 2.0f), y + (markerBoxWidth / 2.0f), markerBoxWidth, markerBoxWidth);
					var markerRectTL = Project(markerRect.Location, WorldRect, ScreenRectF);
					var markerRectSize = Project(new PointF(markerRect.Width, markerRect.Height), WorldRect.Size, ScreenRectF.Size);
					g.DrawRectangle(Pens.Black, markerRectTL.X, ScreenRectF.Height - markerRectTL.Y, markerRectSize.X, markerRectSize.Y);
				}
				else if (dot.Item2 <= 3)
				{
					var pt = Project(new PointF(x, y), WorldRect, ScreenRectF);
					g.DrawString($"{dot.Item2}", font, Brushes.Aqua, pt.X, ScreenRectF.Height - pt.Y);
				}
			}

			// Draw axes and border
			g.DrawLine(Pens.Aqua,
				Project(new PointF(WorldRect.Left, 0), WorldRect, ScreenRectF),
				Project(new PointF(WorldRect.Right, 0), WorldRect, ScreenRectF));
			g.DrawLine(Pens.Aqua,
				Project(new PointF(0, WorldRect.Top), WorldRect, ScreenRectF),
				Project(new PointF(0, WorldRect.Bottom), WorldRect, ScreenRectF));

			var p = Project(new PointF(0, 0), WorldRect, ScreenRectF);
			g.DrawString("(0, 0)", font, Brushes.Aqua, ScreenRectF.Height - p.X, p.Y);
			p = Project(new PointF(47, 0), WorldRect, ScreenRectF);
			g.DrawString("x", font, Brushes.Aqua, p.X, ScreenRectF.Height - p.Y);
			p = Project(new PointF(0, 47), WorldRect, ScreenRectF);
			g.DrawString("y", font, Brushes.Aqua, p.X, ScreenRectF.Height - p.Y);

			g.ResetTransform();
		}

		font.Dispose();

		return bitmap;
	}

	/// <summary>
	/// Projects the given point from its current coordinate space to coordinate space of given rectangle destRegion.
	/// Result is the proportionally equivalent coordinate in destRegion, after removing offset from pointRegion.Location
	/// and then adding offset by destRegion.Location.
	/// </summary>
	protected static PointF Project(PointF point, RectangleF pointRegion, RectangleF destRegion)
	{
		return Project(point, pointRegion.Location, pointRegion.Size, destRegion.Location, destRegion.Size);
	}

	/// <summary>
	/// Projects the given point from its current coordinate space to coordinate space of given rectangle destRegion.
	/// Result is the proportionally equivalent coordinate in destRegion, after adding offset by destRegion.Location.
	/// Assumes given pointRegion's Location is 0,0.
	/// </summary>
	protected static PointF Project(PointF point, SizeF pointRegionSize, RectangleF destRegion)
	{
		return Project(point, PointF.Empty, pointRegionSize, destRegion.Location, destRegion.Size);
	}

	/// <summary>
	/// Projects the given point from its current coordinate space to coordinate space of a region destRegionSize in size.
	/// Result is the proportionally equivalent coordinate in destRegion.
	/// Assumes given Location of both pointRegion and destRegion is 0,0.
	/// </summary>
	protected static PointF Project(PointF point, SizeF pointRegionSize, SizeF destRegionSize)
	{
		return Project(point, PointF.Empty, pointRegionSize, PointF.Empty, destRegionSize);
	}

	/// <summary>
	/// Projects the given point from its current coordinate space to that of the given rectangle (destRegionSize, destRegionLocation).
	/// Result is the proportionally equivalent coordinate in destRegion, after removing offset from Location of pointRegion
	/// and then adding offset by Location of destRegion.
	/// </summary>
	protected static PointF Project(PointF point, PointF pointRegionLocation, SizeF pointRegionSize, PointF destLocation, SizeF destRegionSize)
	{
		float x = point.X - pointRegionLocation.X;
		float y = point.Y - pointRegionLocation.Y;
		x = (x / pointRegionSize.Width) * destRegionSize.Width;
		y = (y / pointRegionSize.Height) * destRegionSize.Height;
		x = x + destLocation.X;
		y = y + destLocation.Y;
		return new PointF(x, y);
	}

	protected static RectangleF ToRectangleF(Rectangle r)
	{
		return new RectangleF(r.Left, r.Top, r.Width, r.Height);
	}
}

////////////////////////////////////////////////////////////////////////
///////////////////// Tracker code  ////////////////////////////////////
////////////////////////////////////////////////////////////////////////

/// <summary>
/// Contains the definitions for all the known markers
/// </summary>
internal static class KnownMarkers
{
	private static Dictionary<int, Marker> _knownMarkers;

	/// <summary>
	/// Get the marker and the rotation count given the marker id.
	/// </summary>
	/// <param name="id">Marker id.</param>
	/// <returns>Marker class.</returns>
	public static Marker GetMarker(int id)
	{
		Marker markerId;
		return _knownMarkers.TryGetValue(id, out markerId) ?
			markerId : Marker.Unknown;
	}

	/// <summary>
	/// Check if marker is mirrored, ie symetric on single axis.
	/// </summary>
	/// <param name="id">Marker id</param>
	/// <returns>true if the marker is mirrored, false otherwise.</returns>
	public static bool IsMirrored(int id)
	{
		int rotated = id << 4;
		rotated = (rotated & 0xff) | ((rotated & 0xf00) >> 8);
		return id == rotated;
	}

	/// <summary>
	/// Check if marker is symetric on all axes.
	/// </summary>
	/// <param name="id">Marker id</param>
	/// <returns>true if the marker is symetric, false otherwise.</returns>
	public static bool IsSymmetric(int id)
	{
		int rotated = id << 2;
		rotated = (rotated & 0xff) | ((rotated & 0x300) >> 8);
		return id == rotated;
	}

	/// <summary>
	/// Initialize all the possible markers including the rotations into a Dictionary.
	/// Marker Id 3 (0000 0011) can be read as following:
	/// single rotation 12  (0000 1100), 
	/// two rotations   48  (0011 0000), 
	/// three rotations 192 (1100 0000).
	/// 
	/// All four values are inserted into a dictionary with the number of 
	/// rotations indicated as Marker class instances.
	/// </summary>
	private static void InitMarkers()
	{
		_knownMarkers = new Dictionary<int, Marker>();
		// all the possible marker ids
		int[] markerIds = new int[]
		{
				3,5,7,9,11,13,15,19,
				21,23,25,27,29,31,33,35,
				37,39,41,43,45,47,53,55,
				57,59,61,63,87,89,91,95,
				97,99,103,105,107,111,121,123,
				127,129,131,135,137,139,143,155,
				159,161,163,167,169,171,175,191,
                // special cases
                51, 85, 153, 187, 221, 255
		};

		// add all the markers considering all the possible rotations
		foreach (var id in markerIds)
		{
			int inputId = id;
			for (int rotation = 0; rotation < 4; rotation++)
			{
				// if check added for the 'special' cases since they can rotate only twice
				if (!_knownMarkers.ContainsKey(inputId))
				{
					_knownMarkers.Add(inputId, new Marker(id, rotation));
				}
				inputId = ((inputId & 0x3) << 6) | (inputId >> 2);
			}
		}
	}

	/// <summary>
	/// Static class constructor.
	/// </summary>
	static KnownMarkers()
	{
		InitMarkers();
	}
}

internal class Marker
{
	/// <summary>
	/// Unknown marker.
	/// </summary>
	public static readonly Marker Unknown = new Marker(0, 0);

	/// <summary>
	/// Unique marker ID.
	/// </summary>
	public int Id { private set; get; }

	/// <summary>
	/// Numer of rotations of the marker.
	/// </summary>
	public int Rotations { private set; get; }

	/// <summary>
	/// Create a KnownMarker class with an id and number of rotations.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="rotations"></param>
	public Marker(int id, int rotations)
	{
		Id = id;
		Rotations = rotations;
	}
}