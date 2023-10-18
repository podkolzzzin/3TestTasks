using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ImageComparer;

public class DirectAccessor : IBitmapAccessor
{
  private readonly Bitmap _bitmap;
  public DirectAccessor(Bitmap bitmap)
  {
    _bitmap = bitmap;
  }

  public int Width => _bitmap.Height;
  public int Height => _bitmap.Height;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ColorData GetPixel(int x, int y)
  {
    var color = _bitmap.GetPixel(x, y);
    return new(color.R, color.G, color.B, color.A);
  }
  public Vector<int> GetVector(int col, int row)
  {
    throw new NotImplementedException();
  }
  public void Dispose()
  {
  }
}

public class DirectAccessorFactory : IBitmapAccessorFactory
{
  public IBitmapAccessor Create(Bitmap bitmap) => new DirectAccessor(bitmap);
}