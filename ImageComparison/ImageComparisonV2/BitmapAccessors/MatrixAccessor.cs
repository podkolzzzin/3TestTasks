using System.Numerics;
using System.Runtime.CompilerServices;

namespace ImageComparisonV2.BitmapAccessors;

public class MatrixAccessor : IBitmapAccessor
{
  private readonly int[,] _matrix;
  public MatrixAccessor(int[,] matrix)
  {
    _matrix = matrix;
    Width = matrix.GetLength(1);
    Height = matrix.GetLength(0);
  }
  
  public int Width { get; }
  public int Height { get; }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ColorData GetPixel(int x, int y) => ColorData.FromInt(_matrix[y, x]);
  public Vector<int> GetVector(int col, int row)
  {
    unsafe
    {
      fixed (int* pointer1 = _matrix)
      {
        var span = new Span<int>(pointer1 + col + row * Width, Vector<int>.Count);
        return new Vector<int>(span);
      }
    }
  }
  public void Dispose()
  {
  }
}