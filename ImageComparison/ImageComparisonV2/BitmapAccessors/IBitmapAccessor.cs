using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ImageComparisonV2.BitmapAccessors;

public interface IBitmapAccessor : IDisposable
{
  int Width { get; }
  int Height { get; }
  ColorData GetPixel(int x, int y);
  Vector<int> GetVector(int col, int row);
}

public interface IBitmapAccessorFactory
{
  IBitmapAccessor GetBitmapAccessor(Bitmap bitmap);
}

public class BitmapAccessorFactory : IBitmapAccessorFactory
{
  public IBitmapAccessor GetBitmapAccessor(Bitmap bitmap)
  {
    return new BitmapAccessor(bitmap);
  }
}

public class MatrixAccessorFactory : IBitmapAccessorFactory
{
  private int[,] AsMatrix(Bitmap bitmap)
  {
    int width = bitmap.Width;
    int height = bitmap.Height;

    // Create a long[,] array to store the pixel values
    int[,] result = new int[height, width];

    // Lock the bitmap data to access pixel values
    BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
      ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try
    {

      // Get the stride (number of bytes per row)
      int stride = bitmapData.Stride;

      // Get the pointer to the pixel data
      IntPtr scan0 = bitmapData.Scan0;

      // Iterate through the pixels and convert to long values
      unsafe
      {
        byte* p = (byte*)(void*)scan0;
        
        for (int y = 0; y < height; y++)
        {
          for (int x = 0; x < width; x++)
          {
            // Get the ARGB values
            int blue = p[0];
            int green = p[1];
            int red = p[2];
            int alpha = p[3];

            // Convert to a long value (assuming little-endian byte order)
            int pixelValue = ((alpha << 24) | (red << 16) | (green << 8) | blue);

            // Store the pixel value in the result array
            result[y, x] = pixelValue;

            // Move to the next pixel (4 bytes per pixel)
            p += 4;
          }

          // Move to the next row
          p += stride - (width * 4);
        }
      }
    }
    finally
    {
      bitmap.UnlockBits(bitmapData);
    }
    return result;
  }
  
  public IBitmapAccessor GetBitmapAccessor(Bitmap bitmap)
  {
    return new MatrixAccessor(AsMatrix(bitmap));
  }
}

public class UnsafeBitmapAccessorFactory : IBitmapAccessorFactory
{
  private class UnsafeBitmapAccessor : PointerAccessor
  {
    private readonly BitmapData _bitmapData;
    private readonly Bitmap _bitmap;
    public unsafe UnsafeBitmapAccessor(int* matrix, int width, int height, BitmapData bitmapData, Bitmap bitmap) : base(matrix, width, height)
    {
      _bitmapData = bitmapData;
      _bitmap = bitmap;
    }

    public override void Dispose()
    {
      _bitmap.UnlockBits(_bitmapData);
    }
  }

  public unsafe IBitmapAccessor GetBitmapAccessor(Bitmap bitmap)
  {
    var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
      ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var pointer = (int*)data.Scan0.ToPointer();
    return new UnsafeBitmapAccessor(pointer, bitmap.Width, bitmap.Height, data, bitmap);
  }
}