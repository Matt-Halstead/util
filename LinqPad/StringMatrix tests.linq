<Query Kind="Program">
  <Namespace>System.Globalization</Namespace>
</Query>

void Main()
{
	var identityToString = StringMatrix.MatrixToString(StringMatrix.Identity, ';');
	var identityLongToString = StringMatrix.MatrixToString(StringMatrix.IdentityLong, ';');
	identityToString.Dump("identityToString");
	identityLongToString.Dump("identityLongToString");

	var identityFromString = StringMatrix.StringToMatrix(identityToString, ';');
	var identityFromStringBackToString = StringMatrix.MatrixToString(identityFromString, ';');
	identityFromStringBackToString.Dump("identityFromStringBackToString");

	var identityFromStringToLong = StringMatrix.IdentityLong;
	StringMatrix.Copy(identityFromString, ref identityFromStringToLong);

	var backToStringAgain = StringMatrix.MatrixToString(identityFromStringToLong, ';');
	backToStringAgain.Dump("backToStringAgain");

}

public static class StringMatrix
{
	public static double[,] Identity => new double[,] { { 1, 0, 0, 0 }, { 22, 1, 0, 0 }, { 33, 0, 1, 0 }, { 44, 0, 0, 99 } };
	public static double[] IdentityLong => new double[] { 1, 0, 0, 0, 22, 1, 0, 0, 33, 0, 1, 0, 44, 0, 0, 99 };

	public static readonly string IdentityAsString = MatrixToString(IdentityLong);

	public static string MatrixToString(double[] transform, char delimiter = ' ')
	{
		var sb = new StringBuilder();

		for (var i = 0; i < 16; i++)
		{
			sb.Append(string.Format(CultureInfo.InvariantCulture, i > 0 ? "{0}{1}" : "{1}", delimiter, transform[i]));
		}

		return sb.ToString();
	}

	public static string MatrixToString(double[,] matrix, char delimiter = ' ')
	{
		var sb = new StringBuilder();

		for (var i = 0; i < 4; i++)
		{
			for (var j = 0; j < 4; j++)
			{
				sb.Append(string.Format(CultureInfo.InvariantCulture, i + j > 0 ? "{0}{1}" : "{1}", delimiter, matrix[i, j]));
			}
		}

		return sb.ToString();
	}

	public static double[,] StringToMatrix(string stringMatrix, char delimiter = ' ')
	{
		var matrix = new double[4, 4];

		var lineParts = (stringMatrix?.Trim() ?? string.Empty).Split(delimiter);

		if (lineParts.Length == 16)
		{
			int count = 0;
			for (var i = 0; i < 4; i++)
			{
				for (var j = 0; j < 4; j++)
				{
					matrix[i, j] = Convert.ToDouble(lineParts[count++], CultureInfo.InvariantCulture);
				}
			}
		}

		return matrix;
	}

	public static void SetIdentity(ref double[,] matrix)
	{
		Copy(IdentityLong, ref matrix);
	}

	public static void Copy(string sourceMatrix, ref double[] destMatrixLong, char delimiter = ' ')
	{
		var lineParts = (sourceMatrix?.Trim() ?? string.Empty).Split(delimiter);

		if (lineParts.Length == 16)
		{
			for (var i = 0; i < 16; i++)
			{
				destMatrixLong[i] = Convert.ToDouble(lineParts[i], CultureInfo.InvariantCulture);
			}
		}
	}

	public static void Copy(double[] sourceMatrixLong, ref double[,] destMatrix)
	{
		for (var i = 0; i < 4; i++)
		{
			for (var j = 0; j < 4; j++)
			{
				destMatrix[i, j] = sourceMatrixLong[i * 4 + j];
			}
		}
	}

	public static void Copy(double[,] sourceMatrix, ref double[] destMatrixLong)
	{
		for (var i = 0; i < 4; i++)
		{
			for (var j = 0; j < 4; j++)
			{
				destMatrixLong[i * 4 + j] = sourceMatrix[i, j];
			}
		}
	}
}
