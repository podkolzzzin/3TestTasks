using System.Numerics;
using System.Runtime.CompilerServices;

namespace ImageComparisonV2.BitmapAccessors;

public unsafe class PointerAccessor : IBitmapAccessor
{
  private readonly int* _matrix;
  public PointerAccessor(int* matrix, int width, int height)
  {
    _matrix = matrix;
    Width = width;
    Height = height;
  }
  public int Width { get; }
  public int Height { get; }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ColorData GetPixel(int x, int y) => ColorData.FromInt(_matrix[x + y * Width]);
  public Vector<int> GetVector(int col, int row)
  {
    var span = new Span<int>(_matrix + col + row * Width, Vector<int>.Count);
    return new Vector<int>(span);
  }
  public virtual void Dispose()
  {
  }
}