<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Drawing.dll</Reference>
  <Namespace>System.Drawing</Namespace>
</Query>

public class Program
{
	static void Main()
	{
		new Program().Run();
	}

	static RectangleF WorldRect = new RectangleF(-300, -300, 600, 600);
	static Rectangle ScreenRect = new Rectangle(0, 0, 400, 400);
	static RectangleF ScreenRectF = ToRectangleF(ScreenRect);

	static PointF CircleCentre = new PointF(0, 0);
	static float Radius = 150;

	static float DeltaY = 120;
	static float CameraDistance = 300;
	static PointF CameraPoint = new PointF(CameraDistance, 0);
	static PointF[] ScreenLine = new[] { new PointF(Radius, WorldRect.Top), new PointF(Radius, WorldRect.Bottom) };
	static PointF[] SightLine = new[] { new PointF(Radius - CameraDistance, -2 * DeltaY),  CameraPoint};

	public void Run()
	{
		this.Dump();
	}

	object ToDump()
	{
		var bitmap = new Bitmap(ScreenRect.Width, ScreenRect.Height);
		var font = new Font(FontFamily.GenericSansSerif, 7.0f, FontStyle.Regular);

		using (var g = Graphics.FromImage(bitmap))
		{
			g.Clear(Color.DarkGray);
			
			var circleRect = new RectangleF(CircleCentre.X - Radius, CircleCentre.Y - Radius, 2*Radius, 2*Radius);
			var circleRectTL = Project(circleRect.Location, WorldRect, ScreenRectF);
			var circleRectSize = Project(new PointF(circleRect.Width, circleRect.Height), WorldRect.Size, ScreenRectF.Size);
			g.DrawEllipse(Pens.White, circleRectTL.X, circleRectTL.Y, circleRectSize.X, circleRectSize.Y);

			g.DrawLine(Pens.Yellow,
				Project(ScreenLine[0], WorldRect, ScreenRectF),
				Project(ScreenLine[1], WorldRect, ScreenRectF));

			g.DrawLine(Pens.Yellow,
				Project(SightLine[0], WorldRect, ScreenRectF),
				Project(SightLine[1], WorldRect, ScreenRectF));
			
			var theta0 = (DeltaY / CameraDistance) * 180 / Math.PI;
			var theta1 = 180 - 90 - theta0;
			var y1 = (float)Math.Cos(theta0) * Radius;
			var x1 = (float)Math.Sqrt(Radius * Radius - y1 * y1);
			g.DrawLine(Pens.Magenta,
				Project(new PointF(x1, y1), WorldRect, ScreenRectF),
				Project(new PointF(x1, 0), WorldRect, ScreenRectF));

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
