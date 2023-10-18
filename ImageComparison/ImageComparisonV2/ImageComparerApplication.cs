using System.Drawing;
using ImageComparisonV2.BitmapAccessors;

namespace ImageComparisonV2;

public class ImageComparerApplication
{
  private readonly IImageComparer _imageComparer;
  private readonly IBitmapAccessorFactory _factory;
  public ImageComparerApplication(IImageComparer imageComparer, IBitmapAccessorFactory factory)
  {
    _imageComparer = imageComparer;
    _factory = factory;
  }
  
  public Bitmap Run(string img1, string img2)
  {
    using var bmp1 = (Bitmap)Image.FromFile(img1);
    using var bmp2 = (Bitmap)Image.FromFile(img2);

    return Run(bmp1, bmp2);
  }

  public Bitmap Run(Bitmap bmp1, Bitmap bmp2)
  {
    List<Rectangle> result = null!;
    using (var ac1 = _factory.GetBitmapAccessor(bmp1))
    using (var ac2 = _factory.GetBitmapAccessor(bmp2))
    {
      result = _imageComparer.DetectChanges(ac1, ac2);
    }
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
