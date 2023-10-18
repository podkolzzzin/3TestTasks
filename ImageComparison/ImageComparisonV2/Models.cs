using System.Runtime.CompilerServices;

namespace ImageComparisonV2;

public readonly record struct Rectangle(int Left, int Top, int Width, int Height);

public readonly record struct ColorData(byte R, byte G, byte B, byte A)
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ColorData FromInt(int color)
  {
    byte ax = (byte)(color & 0xFF);
    byte rx = (byte)((color >> 16) & 0xFF);
    byte gx = (byte)((color >> 8) & 0xFF);
    byte bx = (byte)((color >> 24) & 0xFF);
    return new (rx, gx, bx, ax);
  }
}

public class ColorEqualityComparer : IEqualityComparer<ColorData>
{
  private readonly double _tolerance;

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
    double maxDifference = Math.Sqrt(255 * 255 * 4); // Max possible difference

    return totalDifference <= maxDifference * _tolerance;
  }

  public int GetHashCode(ColorData color)
  {
    return color.GetHashCode();
  }
}