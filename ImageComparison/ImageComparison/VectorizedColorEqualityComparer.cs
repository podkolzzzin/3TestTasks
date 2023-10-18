namespace ImageComparison;


using System;
using System.Collections.Generic;
using System.Numerics;

public class VectorizedColorEqualityComparer : IEqualityComparer<long>
{
  private readonly double tolerance;

  public VectorizedColorEqualityComparer(double tolerance)
  {
    if (tolerance < 0.0 || tolerance > 1.0)
    {
      throw new ArgumentException("Tolerance must be between 0.0 and 1.0.");
    }

    this.tolerance = tolerance;
  }

  public bool Equals(long x, long y)
  {
    // Create vectors from the input values
    Vector<byte> vectorX = new Vector<byte>(new byte[]
    {
      (byte)(x & 0xFF),
      (byte)((x >> 8) & 0xFF),
      (byte)((x >> 16) & 0xFF),
      (byte)((x >> 24) & 0xFF)
    });

    Vector<byte> vectorY = new Vector<byte>(new byte[]
    {
      (byte)(y & 0xFF),
      (byte)((y >> 8) & 0xFF),
      (byte)((y >> 16) & 0xFF),
      (byte)((y >> 24) & 0xFF)
    });

    // Calculate the color difference using SIMD operations
    Vector<byte> difference = Vector.Abs(vectorX - vectorY);

    // Extract elements from the vector and convert them to an array
    byte[] differenceArray = new byte[Vector<byte>.Count];
    difference.CopyTo(differenceArray);

    int sumSquaredDifference = 0;
    foreach (var element in differenceArray)
    {
      sumSquaredDifference += element * element;
    }

    double euclideanDistance = Math.Sqrt(sumSquaredDifference);

    // Compare the difference to the tolerance
    double maxDifference = Math.Sqrt(255 * 255 * 4);
    return euclideanDistance / maxDifference <= tolerance;
  }
  
  public int GetHashCode(long obj)
  {
    return obj.GetHashCode();
  }
}
