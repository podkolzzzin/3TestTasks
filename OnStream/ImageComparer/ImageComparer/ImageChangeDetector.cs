using System.Numerics;
using System.Runtime.CompilerServices;

namespace ImageComparer;

public class ImageChangeDetector : IImageChangeDetector
{
  private readonly IEqualityComparer<ColorData> _comparer;
  public ImageChangeDetector(IEqualityComparer<ColorData> comparer)
  {
    _comparer = comparer;
  }
  
  public List<Rectangle> DetectChanges(IBitmapAccessor bitmap1, IBitmapAccessor bitmap2)
  {
    int width = Math.Min(bitmap1.Width, bitmap2.Width);
    int height = Math.Min(bitmap1.Height, bitmap2.Height);

    List<Rectangle> rectangles = new List<Rectangle>();

    if (bitmap2.Height > bitmap1.Height)
    {
      rectangles.Add(new Rectangle(0, bitmap1.Height, bitmap2.Width, bitmap2.Height - bitmap1.Height));
    }
    if (bitmap2.Width > bitmap1.Width)
    {
      rectangles.Add(new Rectangle(bitmap1.Width, 0, bitmap2.Width - bitmap1.Width, bitmap1.Height));
    }

    // FF FF FF FF
    // FF 00 FF FF
    bool[,] visited = new bool[height, width];
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        if (!visited[y, x] && !_comparer.Equals(bitmap1.GetPixel(x, y), bitmap2.GetPixel(x, y)))
        {
          // Change detected, start growing the rectangle
          Rectangle rectangle = GrowRectangle(bitmap1, bitmap2, visited, x, y);
          rectangles.Add(rectangle);
        }
      }
    }

    return rectangles;
  }

  private Rectangle GrowRectangle(IBitmapAccessor bitmap1, IBitmapAccessor bitmap2, bool[,] visited, int startX, int startY)
  {
    int width = bitmap1.Width;
    int height = bitmap1.Height;

    int endX = startX;
    int endY = startY;

    // Expand horizontally
    while (endX + 1 < width && !visited[startY, endX + 1] && !_comparer.Equals(bitmap1.GetPixel(endX + 1, startY), bitmap2.GetPixel(endX + 1, startY)))
    {
      endX++;
      visited[startY, endX] = true;
    }

    // Expand vertically
    while (endY + 1 < height)
    {
      bool canExpand = true;
      for (int x = startX; x <= endX; x++)
      {
        if (visited[endY + 1, x] || _comparer.Equals(bitmap1.GetPixel(x, endY + 1), bitmap2.GetPixel(x, endY + 1)))
        {
          canExpand = false;
          break;
        }
        visited[endY + 1, x] = true;
      }

      if (canExpand)
      {
        endY++;
      }
      else
      {
        break;
      }
    }

    return new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);
  }
}
public record Rectangle(int Left, int Top, int Width, int Height);

public interface IBitmapAccessor : IDisposable
{
  int Width { get; }
  int Height { get; }
  ColorData GetPixel(int x, int y);
  
  Vector<int> GetVector(int col, int row);
}

public readonly record struct ColorData(byte R, byte G, byte B, byte A)
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ColorData FromInt(int color)
  {
    byte ax = (byte)(color & 0xFF);
    byte rx = (byte)((color >> 16) & 0xFF);
    byte gx = (byte)((color >> 8) & 0xFF);
    byte bx = (byte)((color >> 24) & 0xFF);
    return new (rx, gx, bx, ax);
  }
}
