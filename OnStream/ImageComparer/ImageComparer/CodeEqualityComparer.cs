namespace ImageComparer;

public class ColorEqualityComparer : IEqualityComparer<ColorData>
{
  private readonly double _tolerance;
  private static readonly double MaxDifference = Math.Sqrt(255 * 255 * 4);

  public ColorEqualityComparer(double tolerance)
  {
    if (tolerance < 0.0 || tolerance > 1.0)
    {
      throw new ArgumentException("Tolerance must be between 0.0 and 1.0.");
    }

    this._tolerance = tolerance;
  }

  public bool Equals(ColorData color1, ColorData color2)
  {
    int diffA = Math.Abs(color1.A - color2.A);
    int diffR = Math.Abs(color1.R - color2.R);
    int diffG = Math.Abs(color1.G - color2.G);
    int diffB = Math.Abs(color1.B - color2.B);

    double totalDifference = Math.Sqrt(diffA * diffA + diffR * diffR + diffG * diffG + diffB * diffB);

    return totalDifference <= MaxDifference * _tolerance;
  }

  public int GetHashCode(ColorData color)
  {
    return color.GetHashCode();
  }
}