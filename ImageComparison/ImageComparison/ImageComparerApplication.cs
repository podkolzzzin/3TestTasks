using System.Drawing;
using System.Drawing.Imaging;
using ImageComparison;
using ColorEqualityComparer = ImageComparison.ColorEqualityComparer;

public interface IChangeDetector
{
  List<Rectangle> Detect(Bitmap bmp1, Bitmap bmp2);
}

public class MatrixChangeDetector : IChangeDetector
{
  private readonly IDetector _detector;
  public MatrixChangeDetector(IDetector detector)
  {
    _detector = detector;
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
  
  public List<Rectangle> Detect(Bitmap bmp1, Bitmap bmp2)
  {
    return _detector.Detect(AsMatrix(bmp1), AsMatrix(bmp2));
  }
}

public class BitmapChangeDetector : IChangeDetector
{
  private readonly Func<Bitmap, Bitmap, List<Rectangle>> _changeDetector;
  public BitmapChangeDetector(Func<Bitmap, Bitmap, List<Rectangle>> changeDetector)
  {
    _changeDetector = changeDetector;

  }
  
  public List<Rectangle> Detect(Bitmap bmp1, Bitmap bmp2)
  {
    return _changeDetector(bmp1, bmp2);
  }
}

public class ImageComparerApplication
{
  private readonly IChangeDetector _detector;
  public ImageComparerApplication(IChangeDetector detector)
  {
    _detector = detector;
  }
  
  public Bitmap Run(string img1, string img2)
  {
    using var bmp1 = (Bitmap)Image.FromFile(img1);
    using var bmp2 = (Bitmap)Image.FromFile(img2);

    return Run(bmp1, bmp2);
  }

  public Bitmap Run(Bitmap bmp1, Bitmap bmp2)
  {
    var result = _detector.Detect(bmp1, bmp2);

    return DrawDifference(bmp2, result);
  }
  
  private Bitmap DrawDifference(Bitmap image, List<Rectangle> result)
  {
    var bmp = new Bitmap(image);
    using (Graphics gfx = Graphics.FromImage(bmp))
    {
      using Pen pen = new Pen(Color.Red, 1);
      foreach (var rect in result)
      {
        gfx.DrawRectangle(pen, rect.Left, rect.Top, rect.Width, rect.Height);
      }
    }
    return bmp;
  }
}