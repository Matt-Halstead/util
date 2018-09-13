<Query Kind="Program" />

public class Program
{
	public static readonly double[,] matrixData = new double[10, 10]
	{
		{ -3.0, -3.0, -3.0, -3.0, -2.0, -2.0, -2.0, -3.0, -3.0, -3.0 },
		{ -3.0, -2.0, -2.0, -1.0, -0.7, -1.0, -0.7, -1.1, -2.0, -3.0 },
		{ -3.0, -2.0, -1.1, -0.7,  1.0,  0.7,  0.5, -0.3, -1.5, -3.0 },
		{ -3.0, -2.0, -0.7,  0.5,  1.5,  1.5,  1.2,  0.9, -1.5, -3.0 },
		{ -3.0, -1.5,  0.1,  0.7,  1.5,  2.0,  0.8, -0.9, -1.1, -3.0 },
		{ -3.0, -0.7, -0.5,  0.3,  1.5,  1.5, -0.7, -1.0, -2.0, -3.0 },
		{ -3.0, -1.0, -0.7, -0.1,  1.0, -1.0, -1.0, -1.5, -2.0, -3.0 },
		{ -3.0, -2.0, -1.0, -0.8, -0.9, -1.0, -2.0, -2.0, -2.0, -3.0 },
		{ -3.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -3.0 },
		{ -3.0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0, -3.0 },
	};

	static void Main()
	{
		new Program().Run();
	}

	public void Run()
	{
		var matrix = new Matrix(matrixData);
		matrix.Dump();
	}
}

public class Matrix
{
	private enum Neighbor
	{
		N, NE, E, SE, S, SW, W, NW
	}

	private Dictionary<Neighbor, Tuple<int, int>> offsetsMap = new Dictionary<UserQuery.Matrix.Neighbor, System.Tuple<int, int>>
	{
		{ Neighbor.N, Tuple.Create(0, -1) },
		{ Neighbor.NE, Tuple.Create(1, -1) },
		{ Neighbor.E, Tuple.Create(1, 0) },
		{ Neighbor.SE, Tuple.Create(1, 1) },
		{ Neighbor.S, Tuple.Create(0, 1) },
		{ Neighbor.SW, Tuple.Create(-1, 1) },
		{ Neighbor.W, Tuple.Create(-1, 0) },
		{ Neighbor.NW, Tuple.Create(-1, -1) },
	};

	public Matrix(double[,] data)
	{
		this.Data = data;
	}

	public double[,] Data { get; private set; }

	public int RowCount
	{
		get
		{
			return this.Data.GetLength(0);
		}
	}

	public int ColumnCount
	{
		get
		{
			return this.Data.GetLength(1);
		}
	}

	public double this[int row, int col]
	{
		get
		{
			return this.Data[row, col];
		}

		set
		{
			this.Data[row, col] = value;
		}
	}

	public void getStats(out double minimum, out double maximum)
	{
		// Find min and max values in matrix
		double min = Double.MaxValue;
		double max = Double.MinValue;
		this.traverse((r, c, v) =>
		{
			min = Math.Min(min, v);
			max = Math.Max(max, v);
		});

		minimum = min;
		maximum = max;
		Console.WriteLine($"Min: {min}, Max: {max}.");
	}

	//	public Matrix getNormalised()
	//	{
	//		// Normalise values between min and max
	//		var result = new Matrix(this.Data);
	//		result.traverse((r, c, v) =>
	//		{
	//			result[r, c] = ((v - min) / (max - min)) - 1;
	//			Console.WriteLine($"Normalised {v} to {result[r, c]}.");
	//		});
	//
	//		return result;
	//	}

	public void traverse(Action<int, int, double> emit)
	{
		int nRows = this.Data.GetLength(0);
		for (int r = 0; r < nRows; ++r)
		{
			int nCols = this.Data.GetLength(1);
			for (int c = 0; c < nCols; ++c)
			{
				emit(r, c, this.Data[r, c]);
			}
		}
	}

	private bool isEdgeCell(int col, int row, double v)
	{
		return false;
	}

	object ToDump()
	{
		double min, max;
		this.getStats(out min, out max);

		int width = 400;
		int height = 400;
		int cellWidth = width / this.ColumnCount;
		int cellHeight = height / this.RowCount;

		var bitmap = new System.Drawing.Bitmap(width, height);

		var font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 7.0f, System.Drawing.FontStyle.Regular);

		using (var g = System.Drawing.Graphics.FromImage(bitmap))
		{
			g.Clear(System.Drawing.Color.Black);

			this.traverse((r, c, v) =>
			{
				var proportion = (v - min) / (max - min);
				//Console.WriteLine($"Proportion of {v} in [{min}, {max}]: {proportion}");

				var rect = new System.Drawing.Rectangle(c * cellWidth, r * cellHeight, cellWidth, cellHeight);
				var midPoint = new System.Drawing.Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

				var color = v > 0
					? System.Drawing.Color.FromArgb(255, 0, 0, (int)(proportion * 255))
					: System.Drawing.Color.FromArgb(255, (int)(proportion * 255), (int)(proportion * 255), (int)(proportion * 255));

				var brush = new System.Drawing.SolidBrush(color);
				g.FillRectangle(brush, rect);
				//g.DrawRectangle(System.Drawing.Pens.Black, rect);

				g.DrawString(v.ToString(), font, System.Drawing.Brushes.White, midPoint.X - 8, midPoint.Y - 8);

				brush.Dispose();
			});

			// points at the centre of each cell, just to test line drawing for regions
			// todo: calculate these according to the gradients between each cell on the region boundary instead of hard coding
			var points = new[]
			{
				new System.Drawing.Point(3, 3),
				new System.Drawing.Point(4, 2),
				new System.Drawing.Point(5, 2),
				new System.Drawing.Point(6, 2),
				new System.Drawing.Point(7, 3),
				new System.Drawing.Point(6, 4),
				new System.Drawing.Point(5, 5),
				new System.Drawing.Point(4, 6),
				new System.Drawing.Point(3, 5),
				new System.Drawing.Point(2, 4)
			};

			var translated = points.Select(p =>
			{
				var rect = new System.Drawing.Rectangle(p.X * cellWidth, p.Y * cellHeight, cellWidth, cellHeight);
				var midPoint = new System.Drawing.Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
				return new System.Drawing.Point(midPoint.X, midPoint.Y);
			}).ToList();
			translated.Add(translated.First());

			var pen = new System.Drawing.Pen(System.Drawing.Color.Yellow);
			pen.Width = 3;
			g.DrawCurve(pen, translated.ToArray());
			pen.Dispose();
		}

		font.Dispose();

		return bitmap;
	}
}