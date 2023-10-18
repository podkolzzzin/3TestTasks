using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace ImageComparison;


public class VisitedDirectUnsafeVectorized
{
    public static List<Rectangle> DetectChanges(Bitmap bitmap1, Bitmap bitmap2, IEqualityComparer<int> comparer)
    {
        int cols = Math.Min(bitmap1.Width, bitmap2.Width);
        int rows = Math.Min(bitmap1.Height, bitmap2.Height);

        List<Rectangle> rectangles = new List<Rectangle>();

        unsafe
        {
            var bits1 = bitmap1.LockBits(new System.Drawing.Rectangle(0, 0, cols, rows),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bits2 = bitmap2.LockBits(new System.Drawing.Rectangle(0, 0, cols, rows),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var p1 = (int*)bits1.Scan0.ToPointer();
            var p2 = (int*)bits2.Scan0.ToPointer();
            var fullSpan1 = new Span<int>(p1, rows * cols);
            var fullSpan2 = new Span<int>(p2, rows * cols);
            bool[,] visited = new bool[rows, cols];
            
            
            try
            {
                Rectangle? rectangle = null;
                for (int row = 0; row < rows; row++)
                {
                    int col = 0;
                    for (; col + Vector<int>.Count < cols; col += Vector<long>.Count)
                    {
                        var vector1 = new Vector<int>(fullSpan1.Slice(row * cols + col, Vector<int>.Count));
                        var vector2 = new Vector<int>(fullSpan2.Slice(row * cols + col, Vector<int>.Count));
                        if (!Vector.EqualsAll(vector1, vector2))
                        {
                            Detect(p1, p2, row, col, cols, rows, visited, comparer, rectangles);
                        }
                    }

                    Detect(p1, p2, row, col, cols, rows, visited, comparer, rectangles);
                }
            }
            finally
            {
                bitmap1.UnlockBits(bits1);
                bitmap2.UnlockBits(bits2);   
            }
        }
        
        return rectangles;
    }

    private static unsafe long GetPixel(byte* p)
    {
        // Get the ARGB values
        int blue = p[0];
        int green = p[1];
        int red = p[2];
        int alpha = p[3];

        // Convert to a long value (assuming little-endian byte order)
        return (long)((alpha << 24) | (red << 16) | (green << 8) | blue);
    }

    private static unsafe int GetPixel(int* pointer, int x, int y, int w)
    {
      return pointer[y * w + x];
    }

  private static unsafe Rectangle? Detect(int* matrix1, int* matrix2, int row, int col, int cols, int rows, bool[,] visited, IEqualityComparer<int> comparer, List<Rectangle> rectangles)
  {
    for (; col < cols; col++)
    {
      if (!visited[row, col] && !comparer.Equals(GetPixel(matrix1, col, row, cols), GetPixel(matrix2, col, row, cols)))
      {
        // Change detected, start growing the rectangle
        Rectangle rectangle = GrowRectangle(matrix1, matrix2, visited, row, col, rows, cols, comparer); 
        rectangles.Add(rectangle);
      }
    }
    return null;
  }

  private static unsafe Rectangle GrowRectangle(int* matrix1, int* matrix2, bool[,] visited, int startRow, int startCol, int rows, int cols, IEqualityComparer<int> comparer)
  {
    int endRow = startRow;
    int endCol = startCol;
    visited[startRow, endCol] = true;

    // Expand horizontally
    while (endCol + 1 < cols && !visited[startRow, endCol + 1] && !comparer.Equals(GetPixel(matrix1, endCol + 1, startRow, cols), GetPixel(matrix2, endCol + 1, startRow, cols)))
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
        if (visited[endRow + 1, col] || comparer.Equals(GetPixel(matrix1, col, endRow + 1, cols), GetPixel(matrix2, col, endRow + 1, cols)))
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
