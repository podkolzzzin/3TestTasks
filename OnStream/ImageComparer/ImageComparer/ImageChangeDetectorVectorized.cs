using System.Numerics;

namespace ImageComparer;

public interface IImageChangeDetector
{
  List<Rectangle> DetectChanges(IBitmapAccessor bitmap1, IBitmapAccessor bitmap2);
}

public class ImageComparerVectorized : IImageChangeDetector
{
  private readonly IEqualityComparer<ColorData> _comparer;
  public ImageComparerVectorized(IEqualityComparer<ColorData> comparer)
  {
    _comparer = comparer;
  }
  
  public List<Rectangle> DetectChanges(IBitmapAccessor bitmap1, IBitmapAccessor bitmap2)
  {
    int width = Math.Min(bitmap1.Width, bitmap2.Width);
    int height = Math.Min(bitmap1.Height, bitmap2.Height);

    List<Rectangle> rectangles = new List<Rectangle>();
    bool[,] visited = new bool[height, width];

    if (bitmap2.Width > bitmap1.Width)
    {
      rectangles.Add(new Rectangle(bitmap1.Width, 0, bitmap2.Width - bitmap1.Width, bitmap2.Height));
    }
    if (bitmap2.Height > bitmap1.Height)
    {
      rectangles.Add(new Rectangle(0, bitmap1.Height, bitmap1.Width, bitmap2.Height - bitmap1.Height));
    }

    for (int row = 0; row < height; row++)
    {
      int col = 0;
      for (; col + Vector<int>.Count < width; col += Vector<int>.Count)
      {
        var vector1 = bitmap1.GetVector(col, row);
        var vector2 = bitmap2.GetVector(col, row);
        if (!Vector.EqualsAll(vector1, vector2))
        {
          Detect(bitmap1, bitmap2, row, col, width, visited, rectangles);
        }
      }

      Detect(bitmap1, bitmap2, row, col, width, visited, rectangles);
    }

    return rectangles;
  }
  
  
  private Rectangle? Detect(IBitmapAccessor bitmap1, IBitmapAccessor bitmap2, int row, int col, int cols, bool[,] visited, List<Rectangle> rectangles)
  {
    for (; col < cols; col++)
    {
      if (!visited[row, col] && !_comparer.Equals(bitmap1.GetPixel(col, row), bitmap2.GetPixel(col, row)))
      {
        // Change detected, start growing the rectangle
        Rectangle rectangle = GrowRectangle(bitmap1, bitmap2, visited, row, col);
        rectangles.Add(rectangle);
      }
    }
    return null;
  }

  private Rectangle GrowRectangle(IBitmapAccessor bitmap1, IBitmapAccessor bitmap2, bool[,] visited, int startRow, int startCol)
  {
    int rows = bitmap1.Height;
    int cols = bitmap1.Width;

    int endRow = startRow;
    int endCol = startCol;
    visited[startRow, endCol] = true;

    // Expand horizontally
    while (endCol + 1 < cols && !visited[startRow, endCol + 1] && !_comparer.Equals(bitmap1.GetPixel(endCol + 1, startRow), bitmap2.GetPixel(endCol + 1, startRow)))
    {
      endCol++;
      visited[startRow, endCol] = true;
    }

    // Expand vertically
    while (endRow + 1 < rows)
    {
      bool canExpand = true;
      for (int col = startCol; col <= endCol; col++)
      {
        if (visited[endRow + 1, col] || _comparer.Equals(bitmap1.GetPixel(col, endRow + 1), bitmap2.GetPixel(col, endRow + 1)))
        {
          canExpand = false;
          break;
        }
        visited[endRow + 1, col] = true;
      }

      if (canExpand)
      {
        endRow++;
      }
      else
      {
        break;
      }
    }

    return new Rectangle(startCol, startRow, endCol - startCol + 1, endRow - startRow + 1);
  }
}