using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ImageComparer;

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

public class MatrixAccessorFactory : IBitmapAccessorFactory
{
  public IBitmapAccessor Create(Bitmap bitmap)
  {
    var arr = AsMatrix(bitmap);
    return new MatrixAccessor(arr);
  }
  
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
}