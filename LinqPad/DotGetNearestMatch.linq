<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
</Query>

class Program
{
	static void Main()
	{
		new Program();
	}
	
	public Program()
	{
		float x = 1461.59899f;
		float y = 945.491394f;
		var result = findNearest(x, y, out float d);
		result.Dump($"closest to ({x}, {y}) with distance {d}");
	}

	private Tuple<int, int, PointF, int, double> findNearest(float x, float y, out float distance)
	{
		Tuple<int, int, PointF, int, double> result = null;

		distance = float.MaxValue;
		
		for (int i = 0; i < known.Length; ++i)
		{
			var p = known[i];
			float dx = p.Item3.X - x;
			float dy = p.Item3.Y - y;
			var d = (float)Math.Sqrt((dx * dx) + (dy * dy));

			if (d < distance)
			{
				distance = d;
				result = p;
			}
		}
		return result;
	}

	Tuple<int, int, PointF, int, double>[] known = new Tuple<int, int, PointF, int, double>[]
	{
		Tuple.Create(477, 0, new PointF(-11.508f,  33.559f), 185, 256.375),
		Tuple.Create(477, 1, new PointF(-13.428f,  35.479f), 298, 271.043),
		Tuple.Create(477, 2, new PointF(-13.428f,  31.639f), 298, 241.708),
		Tuple.Create(477, 3, new PointF(-11.508f,  31.639f), 185, 241.708),
		Tuple.Create(477, 4, new PointF( -9.588f,  31.639f), 72, 241.708),
		Tuple.Create(477, 5, new PointF( -9.588f,  35.479f), 72, 271.043),
		Tuple.Create(477, 6, new PointF(-11.508f,  35.479f), 185, 271.043),
		Tuple.Create(733, 0, new PointF(-11.508f,  26.303f), 185, 200.942),
		Tuple.Create(733, 1, new PointF(-13.428f,  28.223f), 298, 215.609),
		Tuple.Create(733, 2, new PointF(-13.428f,  24.383f), 298, 186.274),
		Tuple.Create(733, 3, new PointF(-11.508f,  24.383f), 185, 186.274),
		Tuple.Create(733, 4, new PointF( -9.588f,  24.383f), 72, 186.274),
		Tuple.Create(733, 5, new PointF( -9.588f,  28.223f), 72, 215.609),
		Tuple.Create(733, 6, new PointF(-11.508f,  28.223f), 185, 215.609),
		Tuple.Create(989, 0, new PointF(-11.508f,  19.056f), 185, 145.577),
		Tuple.Create(989, 1, new PointF(-13.428f,  20.976f), 298, 160.244),
		Tuple.Create(989, 2, new PointF(-13.428f,  17.136f), 298, 130.909),
		Tuple.Create(989, 3, new PointF(-11.508f,  17.136f), 185, 130.909),
		Tuple.Create(989, 4, new PointF( -9.588f,  17.136f), 72, 130.909),
		Tuple.Create(989, 5, new PointF( -9.588f,  20.976f), 72, 160.244),
		Tuple.Create(989, 6, new PointF(-11.508f,  20.976f), 185, 160.244),
		Tuple.Create(1245, 0, new PointF(-11.508f,  11.810f), 185, 90.223),
		Tuple.Create(1245, 1, new PointF(-13.428f,  13.730f), 298, 104.891),
		Tuple.Create(1245, 2, new PointF(-13.428f,   9.890f), 298, 75.556),
		Tuple.Create(1245, 3, new PointF(-11.508f,   9.890f), 185, 75.556),
		Tuple.Create(1245, 4, new PointF( -9.588f,   9.890f), 72, 75.556),
		Tuple.Create(1245, 5, new PointF( -9.588f,  13.730f), 72, 104.891),
		Tuple.Create(1245, 6, new PointF(-11.508f,  13.730f), 185, 104.891),
		Tuple.Create(1501, 0, new PointF(-11.508f,   4.567f), 185, 34.893),
		Tuple.Create(1501, 1, new PointF(-13.428f,   6.487f), 298, 49.560),
		Tuple.Create(1501, 2, new PointF(-13.428f,   2.647f), 298, 20.225),
		Tuple.Create(1501, 3, new PointF(-11.508f,   2.647f), 185, 20.225),
		Tuple.Create(1501, 4, new PointF( -9.588f,   2.647f), 72, 20.225),
		Tuple.Create(1501, 5, new PointF( -9.588f,   6.487f), 72, 49.560),
		Tuple.Create(1501, 6, new PointF(-11.508f,   6.487f), 185, 49.560),
		Tuple.Create(1757, 0, new PointF(-11.508f,  -2.687f), 185, 339.471),
		Tuple.Create(1757, 1, new PointF(-13.428f,  -0.767f), 298, 354.138),
		Tuple.Create(1757, 2, new PointF(-13.428f,  -4.607f), 298, 324.803),
		Tuple.Create(1757, 3, new PointF(-11.508f,  -4.607f), 185, 324.803),
		Tuple.Create(1757, 4, new PointF( -9.588f,  -4.607f), 72, 324.803),
		Tuple.Create(1757, 5, new PointF( -9.588f,  -0.767f), 72, 354.138),
		Tuple.Create(1757, 6, new PointF(-11.508f,  -0.767f), 185, 354.138),
		Tuple.Create(3, 0, new PointF(-18.755f,  29.919f), 611, 228.564),
		Tuple.Create(3, 1, new PointF(-20.675f,  31.839f), 724, 243.232),
		Tuple.Create(3, 2, new PointF(-20.675f,  29.919f), 724, 228.564),
		Tuple.Create(163, 0, new PointF(-18.755f,  22.660f), 611, 173.113),
		Tuple.Create(163, 1, new PointF(-20.675f,  24.580f), 724, 187.781),
		Tuple.Create(163, 2, new PointF(-20.675f,  22.660f), 724, 173.113),
		Tuple.Create(163, 3, new PointF(-16.835f,  22.660f), 498, 173.113),
		Tuple.Create(163, 4, new PointF(-18.755f,  24.580f), 611, 187.781),
		Tuple.Create(103, 0, new PointF(-18.755f,  15.416f), 611, 117.771),
		Tuple.Create(103, 1, new PointF(-20.675f,  17.336f), 724, 132.439),
		Tuple.Create(103, 2, new PointF(-20.675f,  15.416f), 724, 117.771),
		Tuple.Create(103, 3, new PointF(-20.675f,  13.496f), 724, 103.103),
		Tuple.Create(103, 4, new PointF(-16.835f,  15.416f), 498, 117.771),
		Tuple.Create(103, 5, new PointF(-16.835f,  17.336f), 498, 132.439),
		Tuple.Create(61, 0, new PointF(-18.755f,   8.125f), 611, 62.068),
		Tuple.Create(61, 1, new PointF(-20.675f,  10.045f), 724, 76.736),
		Tuple.Create(61, 2, new PointF(-20.675f,   6.205f), 724, 47.400),
		Tuple.Create(61, 3, new PointF(-18.755f,   6.205f), 611, 47.400),
		Tuple.Create(61, 4, new PointF(-16.835f,   6.205f), 498, 47.400),
		Tuple.Create(61, 5, new PointF(-16.835f,   8.125f), 498, 62.068),
		Tuple.Create(37, 0, new PointF(-18.755f,   0.819f), 611, 6.256),
		Tuple.Create(37, 1, new PointF(-20.675f,   2.739f), 724, 20.924),
		Tuple.Create(37, 2, new PointF(-20.675f,  -1.101f), 724, 351.589),
		Tuple.Create(37, 3, new PointF(-16.835f,   0.819f), 498, 6.256),
		Tuple.Create(21, 0, new PointF(-26.006f,  33.531f), 1037, 256.158),
		Tuple.Create(21, 1, new PointF(-27.926f,  35.451f), 1150, 270.825),
		Tuple.Create(21, 2, new PointF(-27.926f,  31.611f), 1150, 241.490),
		Tuple.Create(21, 3, new PointF(-24.086f,  31.611f), 924, 241.490),
		Tuple.Create(131, 0, new PointF(-26.006f,  26.270f), 1037, 200.690),
		Tuple.Create(131, 1, new PointF(-27.926f,  28.190f), 1150, 215.357),
		Tuple.Create(131, 2, new PointF(-27.926f,  26.270f), 1150, 200.690),
		Tuple.Create(131, 3, new PointF(-26.006f,  28.190f), 1037, 215.357),
		Tuple.Create(57, 0, new PointF(-26.006f,  19.026f), 1037, 145.348),
		Tuple.Create(57, 1, new PointF(-27.926f,  20.946f), 1150, 160.015),
		Tuple.Create(57, 2, new PointF(-26.006f,  17.106f), 1037, 130.680),
		Tuple.Create(57, 3, new PointF(-24.086f,  17.106f), 924, 130.680),
		Tuple.Create(57, 4, new PointF(-24.086f,  19.026f), 924, 145.348),
		Tuple.Create(167, 0, new PointF(-26.006f,  11.779f), 1037, 89.988),
		Tuple.Create(167, 1, new PointF(-27.926f,  13.699f), 1150, 104.656),
		Tuple.Create(167, 2, new PointF(-27.926f,  11.779f), 1150, 89.988),
		Tuple.Create(167, 3, new PointF(-27.926f,   9.859f), 1150, 75.321),
		Tuple.Create(167, 4, new PointF(-24.086f,  11.779f), 924, 89.988),
		Tuple.Create(167, 5, new PointF(-26.006f,  13.699f), 1037, 104.656),
		Tuple.Create(23, 0, new PointF(-26.006f,   4.537f), 1037, 34.664),
		Tuple.Create(23, 1, new PointF(-27.926f,   6.457f), 1150, 49.331),
		Tuple.Create(23, 2, new PointF(-27.926f,   4.537f), 1150, 34.664),
		Tuple.Create(23, 3, new PointF(-27.926f,   2.617f), 1150, 19.996),
		Tuple.Create(23, 4, new PointF(-24.086f,   2.617f), 924, 19.996),
		Tuple.Create(161, 0, new PointF(-26.006f,  -2.717f), 1037, 339.241),
		Tuple.Create(161, 1, new PointF(-27.926f,  -0.797f), 1150, 353.909),
		Tuple.Create(161, 2, new PointF(-24.086f,  -2.717f), 924, 339.241),
		Tuple.Create(161, 3, new PointF(-26.006f,  -0.797f), 1037, 353.909),
		Tuple.Create(7, 0, new PointF(-33.257f,  29.886f), 1464, 228.312),
		Tuple.Create(7, 1, new PointF(-35.177f,  31.806f), 1577, 242.980),
		Tuple.Create(7, 2, new PointF(-35.177f,  29.886f), 1577, 228.312),
		Tuple.Create(7, 3, new PointF(-35.177f,  27.966f), 1577, 213.644),
		Tuple.Create(105, 0, new PointF(-33.257f,  22.635f), 1464, 172.918),
		Tuple.Create(105, 1, new PointF(-35.177f,  24.555f), 1577, 187.586),
		Tuple.Create(105, 2, new PointF(-33.257f,  20.715f), 1464, 158.251),
		Tuple.Create(105, 3, new PointF(-31.337f,  22.635f), 1351, 172.918),
		Tuple.Create(105, 4, new PointF(-31.337f,  24.555f), 1351, 187.586),
		Tuple.Create(25, 0, new PointF(-33.257f,  15.386f), 1464, 117.542),
		Tuple.Create(25, 1, new PointF(-35.177f,  17.306f), 1577, 132.210),
		Tuple.Create(25, 2, new PointF(-33.257f,  13.466f), 1464, 102.874),
		Tuple.Create(25, 3, new PointF(-31.337f,  13.466f), 1351, 102.874),
		Tuple.Create(99, 0, new PointF(-33.257f,   8.146f), 1464, 62.229),
		Tuple.Create(99, 1, new PointF(-35.177f,  10.066f), 1577, 76.896),
		Tuple.Create(99, 2, new PointF(-35.177f,   8.146f), 1577, 62.229),
		Tuple.Create(99, 3, new PointF(-31.337f,   8.146f), 1351, 62.229),
		Tuple.Create(99, 4, new PointF(-31.337f,  10.066f), 1351, 76.896),
		Tuple.Create(41, 0, new PointF(-33.257f,   0.902f), 1464, 6.892),
		Tuple.Create(41, 1, new PointF(-35.177f,   2.822f), 1577, 21.560),
		Tuple.Create(41, 2, new PointF(-33.257f,  -1.018f), 1464, 352.225),
		Tuple.Create(41, 3, new PointF(-31.337f,   0.902f), 1351, 6.892),
		Tuple.Create(2013, 0, new PointF(-40.513f,  33.501f), 1891, 255.928),
		Tuple.Create(2013, 1, new PointF(-42.433f,  35.421f), 2004, 270.596),
		Tuple.Create(2013, 2, new PointF(-42.433f,  31.581f), 2004, 241.261),
		Tuple.Create(2013, 3, new PointF(-40.513f,  31.581f), 1891, 241.261),
		Tuple.Create(2013, 4, new PointF(-38.593f,  31.581f), 1778, 241.261),
		Tuple.Create(2013, 5, new PointF(-38.593f,  35.421f), 1778, 270.596),
		Tuple.Create(2013, 6, new PointF(-40.513f,  35.421f), 1891, 270.596),
		Tuple.Create(2269, 0, new PointF(-40.513f,  26.233f), 1891, 200.409),
		Tuple.Create(2269, 1, new PointF(-42.433f,  28.153f), 2004, 215.076),
		Tuple.Create(2269, 2, new PointF(-42.433f,  24.313f), 2004, 185.741),
		Tuple.Create(2269, 3, new PointF(-40.513f,  24.313f), 1891, 185.741),
		Tuple.Create(2269, 4, new PointF(-38.593f,  24.313f), 1778, 185.741),
		Tuple.Create(2269, 5, new PointF(-38.593f,  28.153f), 1778, 215.076),
		Tuple.Create(2269, 6, new PointF(-40.513f,  28.153f), 1891, 215.076),
		Tuple.Create(2525, 0, new PointF(-40.513f,  18.998f), 1891, 145.136),
		Tuple.Create(2525, 1, new PointF(-42.433f,  20.918f), 2004, 159.803),
		Tuple.Create(2525, 2, new PointF(-42.433f,  17.078f), 2004, 130.468),
		Tuple.Create(2525, 3, new PointF(-40.513f,  17.078f), 1891, 130.468),
		Tuple.Create(2525, 4, new PointF(-38.593f,  17.078f), 1778, 130.468),
		Tuple.Create(2525, 5, new PointF(-38.593f,  20.918f), 1778, 159.803),
		Tuple.Create(2525, 6, new PointF(-40.513f,  20.918f), 1891, 159.803),
		Tuple.Create(2781, 0, new PointF(-40.513f,  11.757f), 1891, 89.816),
		Tuple.Create(2781, 1, new PointF(-42.433f,  13.677f), 2004, 104.484),
		Tuple.Create(2781, 2, new PointF(-42.433f,   9.837f), 2004, 75.149),
		Tuple.Create(2781, 3, new PointF(-40.513f,   9.837f), 1891, 75.149),
		Tuple.Create(2781, 4, new PointF(-38.593f,   9.837f), 1778, 75.149),
		Tuple.Create(2781, 5, new PointF(-38.593f,  13.677f), 1778, 104.484),
		Tuple.Create(2781, 6, new PointF(-40.513f,  13.677f), 1891, 104.484),
		Tuple.Create(3037, 0, new PointF(-40.513f,   4.514f), 1891, 34.486),
		Tuple.Create(3037, 1, new PointF(-42.433f,   6.434f), 2004, 49.154),
		Tuple.Create(3037, 2, new PointF(-42.433f,   2.594f), 2004, 19.818),
		Tuple.Create(3037, 3, new PointF(-40.513f,   2.594f), 1891, 19.818),
		Tuple.Create(3037, 4, new PointF(-38.593f,   2.594f), 1778, 19.818),
		Tuple.Create(3037, 5, new PointF(-38.593f,   6.434f), 1778, 49.154),
		Tuple.Create(3037, 6, new PointF(-40.513f,   6.434f), 1891, 49.154),
		Tuple.Create(3293, 0, new PointF(-40.513f,  -2.748f), 1891, 339.006),
		Tuple.Create(3293, 1, new PointF(-42.433f,  -0.828f), 2004, 353.674),
		Tuple.Create(3293, 2, new PointF(-42.433f,  -4.668f), 2004, 324.339),
		Tuple.Create(3293, 3, new PointF(-40.513f,  -4.668f), 1891, 324.339),
		Tuple.Create(3293, 4, new PointF(-38.593f,  -4.668f), 1778, 324.339),
		Tuple.Create(3293, 5, new PointF(-38.593f,  -0.828f), 1778, 353.674),
		Tuple.Create(3293, 6, new PointF(-40.513f,  -0.828f), 1891, 353.674)
	};	
}