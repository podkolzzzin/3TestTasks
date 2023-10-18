using System.Drawing;
using System.Numerics;

namespace ImageComparisonV2.BitmapAccessors;

public class BitmapAccessor : IBitmapAccessor
{
  private readonly Bitmap _bitmap;
  public BitmapAccessor(Bitmap bitmap)
  {
    Width = bitmap.Width;
    Height = bitmap.Height;
    _bitmap = bitmap;
  }
  public int Height { get; }
  public ColorData GetPixel(int x, int y)
  {
    var c = _bitmap.GetPixel(x, y);
    return new(c.R, c.G, c.B, c.A);
  }
  public Vector<int> GetVector(int col, int row)
  {
    throw new NotImplementedException();
  }

  public int Width { get; }
  public void Dispose()
  {
  }
}