using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;

namespace ImageComparer;

public unsafe class PointerAccessor : IBitmapAccessor
{
  private readonly int* _matrix;
  private readonly Bitmap _bitmap;
  private readonly BitmapData _bitmapData;
  public PointerAccessor(int* matrix, Bitmap bitmap, BitmapData bitmapData)
  {
    _matrix = matrix;
    _bitmap = bitmap;
    _bitmapData = bitmapData;
    Width = bitmap.Width;
    Height = bitmap.Height;
  }
  public int Width { get; }
  public int Height { get; }
  public ColorData GetPixel(int x, int y)
  {
    byte* p = (byte*)(_matrix + x + y * Width);
    // Get the ARGB values
    byte blue = p[0];
    byte green = p[1];
    byte red = p[2];
    byte alpha = p[3];

    return new(red, green, blue, alpha);
  }
  public Vector<int> GetVector(int x, int y)
  {
    return new Vector<int>(new Span<int>(_matrix + x + y * Width, Vector<int>.Count));
  }
  
  public void Dispose() => _bitmap.UnlockBits(_bitmapData);
}

public class PointerAccessorFactory : IBitmapAccessorFactory
{
  public unsafe IBitmapAccessor Create(Bitmap bitmap)
  {
    var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
      ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var pointer = (int*)data.Scan0.ToPointer();
    return new PointerAccessor(pointer, bitmap, data);
  }
}