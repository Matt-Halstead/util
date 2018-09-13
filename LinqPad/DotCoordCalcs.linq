<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
</Query>


void Main()
{
	new Program().Dump();
}

class Program
{
	static RectangleF WorldRect = new RectangleF(-50, -50, 100, 100);

	private static int[] Dot7 = new[] { 0, 1, 2, 4, 6, 7, 8 };
	private static int[] Dot3 = new[] { 4, 5, 8 };
	private static int[] Dot5A = new[] { 3, 4, 5, 7, 8 };
	private static int[] Dot6A = new[] { 2, 3, 4, 5, 6, 8 };
	private static int[] Dot6B = new[] { 0, 1, 2, 3, 4, 8 };
	private static int[] Dot4A = new[] { 2, 3, 4, 8 };
	private static int[] Dot4B = new[] { 0, 2, 4, 8 };
	private static int[] Dot4C = new[] { 4, 5, 7, 8 };
	private static int[] Dot5B = new[] { 0, 1, 3, 4, 8 };
	private static int[] Dot6C = new[] { 2, 3, 4, 5, 7, 8 };
	private static int[] Dot5C = new[] { 0, 2, 4, 5, 8 };
	private static int[] Dot4D = new[] { 3, 4, 7, 8 };
	private static int[] Dot4E = new[] { 2, 4, 5, 8 };
	private static int[] Dot5D = new[] { 1, 3, 4, 6, 8 };
	private static int[] Dot4F = new[] { 0, 1, 4, 8 };
	private static int[] Dot5E = new[] { 3, 4, 5, 6, 8 };
	private static int[] Dot4G = new[] { 1, 3, 4, 8 };

	private static Tuple<int[], float, float>[] markerDefinitions = new[]
	{
		Tuple.Create(Dot7, 11.5f, 33.625f),
		Tuple.Create(Dot7, 11.5f, 26.375f),
		Tuple.Create(Dot7, 11.5f, 19.125f),
		Tuple.Create(Dot7, 11.5f, 11.875f),
		Tuple.Create(Dot7, 11.5f, 4.625f),
		Tuple.Create(Dot7, 11.5f, -2.625f),

		Tuple.Create(Dot3, 18.75f, 30.000f),
		Tuple.Create(Dot5A, 18.75f, 22.750f),
		Tuple.Create(Dot6A, 18.75f, 15.500f),
		Tuple.Create(Dot6B, 18.75f, 8.250f),
		Tuple.Create(Dot4A, 18.75f, 1.000f),

		Tuple.Create(Dot4B, 26.00f, 33.625f),
		Tuple.Create(Dot4C, 26.00f, 26.375f),
		Tuple.Create(Dot5B, 26.00f, 19.125f),
		Tuple.Create(Dot6C, 26.00f, 11.875f),
		Tuple.Create(Dot5C, 26.00f, 4.625f),
		Tuple.Create(Dot4D, 26.00f, -2.625f),

		Tuple.Create(Dot4E, 33.25f, 30.000f),
		Tuple.Create(Dot5D, 33.25f, 22.750f),
		Tuple.Create(Dot4F, 33.25f, 15.500f),
		Tuple.Create(Dot5E, 33.25f, 8.250f),
		Tuple.Create(Dot4G, 33.25f, 1.000f),

		Tuple.Create(Dot7, 40.5f, 33.625f),
		Tuple.Create(Dot7, 40.5f, 26.375f),
		Tuple.Create(Dot7, 40.5f, 19.125f),
		Tuple.Create(Dot7, 40.5f, 11.875f),
		Tuple.Create(Dot7, 40.5f, 4.625f),
		Tuple.Create(Dot7, 40.5f, -2.625f)
	};

	private static float dotXSpacingMm = 1.923f;
	//private static float markerCentreXSpacingMm = 7.25f;

	private static PointF[] DotRelCoords = new PointF[]
	{
		new PointF(-dotXSpacingMm, dotXSpacingMm), new PointF(0, dotXSpacingMm), new PointF(dotXSpacingMm, dotXSpacingMm),
		new PointF(-dotXSpacingMm, 0), new PointF(0, 0), new PointF(dotXSpacingMm, 0),
		new PointF(-dotXSpacingMm, -dotXSpacingMm), new PointF(0, -dotXSpacingMm), new PointF(dotXSpacingMm, -dotXSpacingMm),
	};

	private List<PointF> allDots = new List<PointF>();

	public Program()
	{
		//		double[] relXPositionsMm = new double[15];
		//		for (int marker = 0; marker < 5; ++marker)
		//		{
		//			double markerCentre = (marker - 2) * markerCentreXSpacingMm;
		//			for (int dot = 0; dot < 3; ++dot)
		//			{
		//				double dotCentre = (dot - 1) * dotXSpacingMm;
		//				relXPositionsMm[(marker * 3) + dot] = markerCentre + (dotCentre) + 26;
		//			}
		//		}

		foreach (var marker in markerDefinitions.OrderBy(m => m.Item3).ThenBy(m => m.Item2))
		{
			allDots.AddRange(GetDotCoords(marker.Item1, new PointF(marker.Item2, marker.Item3)));
		}

		allDots
//			.Select(d => $"new PointF({d.X:n3}f, {d.Y:n3}f), rot = {GetTubeAngle(d.Y):n3}")
			.Select(d => $"new PointF({d.X:n3}f, {d.Y:n3}f)")
			.Dump("all coords");

		GetDotCoords(Dot3, new PointF(18.75f, 30.000f))
			.Select(d => $"new PointF({d.X:n3}f, {d.Y:n3}f), rot = {GetTubeAngle(d.Y):n3}")
			.Dump("marker 5a");
	}
	
	private double GetTubeAngle(double dy)
	{
		return ((dy / (Math.PI * 2 * 7.5)) * 360.0);
	}

	private PointF[] GetDotCoords(int[] dots, PointF dotCentre)
	{
		return dots.Select(dotIdx =>
		{
			var relPoint = DotRelCoords[dotIdx];
			return new PointF(relPoint.X + dotCentre.X, relPoint.Y + dotCentre.Y);
		}).ToArray();
	}

	object ToDump()
	{
		Rectangle ScreenRect = new Rectangle(0, 0, 400, 400);
		RectangleF ScreenRectF = ToRectangleF(ScreenRect);

		var bitmap = new Bitmap(ScreenRect.Width, ScreenRect.Height);
		var font = new Font(FontFamily.GenericSansSerif, 7.0f, FontStyle.Regular);

		using (var g = Graphics.FromImage(bitmap))
		{
			g.Clear(Color.DarkGray);

			float Radius = 0.75f;

			foreach (var dot in allDots)
			{
				var circleRect = new RectangleF(dot.X - Radius, dot.Y - Radius, 2 * Radius, 2 * Radius);
				var circleRectTL = Project(circleRect.Location, WorldRect, ScreenRectF);
				var circleRectSize = Project(new PointF(circleRect.Width, circleRect.Height), WorldRect.Size, ScreenRectF.Size);
				g.DrawEllipse(Pens.White, circleRectTL.X, circleRectTL.Y, circleRectSize.X, circleRectSize.Y);
			}
			
			// Draw axes and border
			g.DrawLine(Pens.Aqua,
				Project(new PointF(WorldRect.Left, 0), WorldRect, ScreenRectF),
				Project(new PointF(WorldRect.Right, 0), WorldRect, ScreenRectF));
			g.DrawLine(Pens.Aqua,
				Project(new PointF(0, WorldRect.Top), WorldRect, ScreenRectF),
				Project(new PointF(0, WorldRect.Bottom), WorldRect, ScreenRectF));
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
