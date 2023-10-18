using System.Numerics;

namespace ImageComparison;

public class VisitedVectorized
{
  public static List<Rectangle> DetectChanges(int[,] matrix1, int[,] matrix2, IEqualityComparer<int> comparer)
  {
    int rows = Math.Min(matrix1.GetLength(0), matrix2.GetLength(0));
    int cols = Math.Min(matrix1.GetLength(1), matrix2.GetLength(1));


    bool[,] visited = new bool[rows, cols];
    List<Rectangle> rectangles = new List<Rectangle>();
    if (matrix2.GetLength(0) > matrix1.GetLength(0))
    {
      rectangles.Add(new Rectangle(0, matrix1.GetLength(0), matrix2.GetLength(1), matrix2.GetLength(0) - matrix1.GetLength(0)));
    }
    if (matrix2.GetLength(1) > matrix1.GetLength(1))
    {
      rectangles.Add(new Rectangle(matrix1.GetLength(1), 0, matrix2.GetLength(1) - matrix1.GetLength(1), matrix1.GetLength(0)));
    }

    unsafe
    {
      fixed (int* pointer1 = matrix1)
      fixed (int* pointer2 = matrix2)
      {
        var fullSpan1 = new Span<int>(pointer1, matrix1.GetLength(0) * matrix1.GetLength(1));
        var fullSpan2 = new Span<int>(pointer2, matrix2.GetLength(0) * matrix2.GetLength(1));
        Rectangle? rectangle = null;
        for (int row = 0; row < rows; row++)
        {
          int col = 0;
          for (; col + Vector<long>.Count < cols; col += Vector<long>.Count)
          {
            var vector1 = new Vector<int>(fullSpan1.Slice(matrix1.GetLength(0) * row + col, Vector<int>.Count));
            var vector2 = new Vector<int>(fullSpan2.Slice(matrix2.GetLength(0) * row + col, Vector<int>.Count));
            if (!Vector.EqualsAll(vector1, vector2))
            {
              Detect(matrix1, matrix2, row, col, cols, visited, comparer, rectangles);
            }
          }

          Detect(matrix1, matrix2, row, col, cols, visited, comparer, rectangles);
        }
      }
    }

    return rectangles;
  }
  private static Rectangle? Detect(int[,] matrix1, int[,] matrix2, int row, int col, int cols, bool[,] visited, IEqualityComparer<int> comparer, List<Rectangle> rectangles)
  {
    for (; col < cols; col++)
    {
      if (!visited[row, col] && !comparer.Equals(matrix1[row, col], matrix2[row, col]))
      {
        // Change detected, start growing the rectangle
        Rectangle rectangle = GrowRectangle(matrix1, matrix2, visited, row, col, comparer); 
        rectangles.Add(rectangle);
      }
    }
    return null;
  }

  private static Rectangle GrowRectangle(int[,] matrix1, int[,] matrix2, bool[,] visited, int startRow, int startCol, IEqualityComparer<int> comparer)
  {
    int rows = matrix1.GetLength(0);
    int cols = matrix1.GetLength(1);

    int endRow = startRow;
    int endCol = startCol;
    visited[startRow, endCol] = true;

    // Expand horizontally
    while (endCol + 1 < cols && !visited[startRow, endCol + 1] && !comparer.Equals(matrix1[startRow, endCol + 1], matrix2[startRow, endCol + 1]))
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
        if (visited[endRow + 1, col] || comparer.Equals(matrix1[endRow + 1, col], matrix2[endRow + 1, col]))
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