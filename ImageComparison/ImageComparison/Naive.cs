namespace ImageComparison;

public class Naive
{
    public static List<Rectangle> DetectChanges(long[,] matrix1, long[,] matrix2, long threshold)
    {
      int rows = matrix1.GetLength(0);
      int cols = matrix1.GetLength(1);

      List<Rectangle> rectangles = new List<Rectangle>();

      for (int row = 0; row < rows; row++)
      {
        for (int col = 0; col < cols; col++)
        {
          if (Math.Abs(matrix1[row, col] - matrix2[row, col]) > threshold)
          {
            // Change detected, start growing the rectangle
            Rectangle rectangle = GrowRectangle(matrix1, matrix2, row, col, threshold);
            rectangles.Add(rectangle);
          }
        }
      }

      return rectangles;
    }

    private static Rectangle GrowRectangle(long[,] matrix1, long[,] matrix2, int startRow, int startCol, long threshold)
    {
      int rows = matrix1.GetLength(0);
      int cols = matrix1.GetLength(1);

      int endRow = startRow;
      int endCol = startCol;

      // Expand horizontally
      while (endCol + 1 < cols && Math.Abs(matrix1[startRow, endCol + 1] - matrix2[startRow, endCol + 1]) > threshold)
      {
        endCol++;
      }

      // Expand vertically
      while (endRow + 1 < rows)
      {
        bool canExpand = false;
        for (int col = startCol; col <= endCol; col++)
        {
          if (Math.Abs(matrix1[endRow + 1, col] - matrix2[endRow + 1, col]) <= threshold)
          {
            canExpand = false;
            break;
          }
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