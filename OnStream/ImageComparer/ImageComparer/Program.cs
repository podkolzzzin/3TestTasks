// See https://aka.ms/new-console-template for more information

using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using ImageComparer;
using Rectangle = System.Drawing.Rectangle;


Console.WriteLine("Hello, World!");

public class ImageComparerApplication
{
  private readonly IBitmapAccessorFactory _factory;
  private readonly IImageChangeDetector _detector;
  public ImageComparerApplication(IBitmapAccessorFactory factory, IImageChangeDetector detector)
  {
    _factory = factory;
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
    List<ImageComparer.Rectangle> rectangles = null!;
    using (var ac1 = _factory.Create(bmp1))
    using (var ac2 = _factory.Create(bmp2))
    {
      rectangles = _detector.DetectChanges(ac1, ac2);
    }

    return DrawDifference(bmp2, rectangles);
  }
  
  private Bitmap DrawDifference(Bitmap image, List<ImageComparer.Rectangle> result)
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

public interface IBitmapAccessorFactory
{
  IBitmapAccessor Create(Bitmap bitmap);
}