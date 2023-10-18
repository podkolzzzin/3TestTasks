public class LongColorEqualityComparer : IEqualityComparer<int>
{
  private readonly double tolerance;

  public LongColorEqualityComparer(double tolerance)
  {
    if (tolerance < 0.0 || tolerance > 1.0)
    {
      throw new ArgumentException("Tolerance must be between 0.0 and 1.0.");
    }

    this.tolerance = tolerance;
  }

  public bool Equals(int x, int y)
  {
    // Extract the RGB components from the long value
    byte ax = (byte)(x & 0xFF);
    byte ay = (byte)(y & 0xFF);
    byte rx = (byte)((x >> 16) & 0xFF);
    byte ry = (byte)((y >> 16) & 0xFF);
    byte gx = (byte)((x >> 8) & 0xFF);
    byte gy = (byte)((y >> 8) & 0xFF);
    byte bx = (byte)((x >> 24) & 0xFF);
    byte by = (byte)((y >> 24) & 0xFF);

    // Calculate the color difference using Euclidean distance
    double difference = Math.Sqrt(
      Math.Pow(ax - ay, 2) +
      Math.Pow(rx - ry, 2) +
      Math.Pow(gx - gy, 2) +
      Math.Pow(bx - by, 2)
    );

    // Compare the difference to the tolerance
    return difference / Math.Sqrt(255 * 255 * 4) <= tolerance;
  }

  public int GetHashCode(int obj)
  {
    return obj.GetHashCode();
  }
}