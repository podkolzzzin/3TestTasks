using System.Drawing;

namespace ImageComparison;


public class ColorEqualityComparer : IEqualityComparer<Color>
{
    private readonly double _tolerance;

    public ColorEqualityComparer(double tolerance)
    {
        if (tolerance < 0.0 || tolerance > 1.0)
        {
            throw new ArgumentException("Tolerance must be between 0.0 and 1.0.");
        }

        this._tolerance = tolerance;
    }

    public bool Equals(Color color1, Color color2)
    {
        int diffA = Math.Abs(color1.A - color2.A);
        int diffR = Math.Abs(color1.R - color2.R);
        int diffG = Math.Abs(color1.G - color2.G);
        int diffB = Math.Abs(color1.B - color2.B);

        double totalDifference = Math.Sqrt(diffA * diffA + diffR * diffR + diffG * diffG + diffB * diffB);
        double maxDifference = Math.Sqrt(255 * 255 * 4); // Max possible difference

        return totalDifference <= maxDifference * _tolerance;
    }

    public int GetHashCode(Color color)
    {
        return color.GetHashCode();
    }
}

public class VisitedDirect
{
    public static List<Rectangle> DetectChanges(Bitmap bitmap1, Bitmap bitmap2, IEqualityComparer<Color> comparer)
    {
        int width = Math.Min(bitmap1.Width, bitmap2.Width);
        int height = Math.Min(bitmap1.Height, bitmap2.Height);

        List<Rectangle> rectangles = new List<Rectangle>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!comparer.Equals(bitmap1.GetPixel(x, y), bitmap2.GetPixel(x, y)))
                {
                    // Change detected, start growing the rectangle
                    Rectangle rectangle = GrowRectangle(bitmap1, bitmap2, comparer, x, y);
                    rectangles.Add(rectangle);
                }
            }
        }

        return rectangles;
    }

    private static Rectangle GrowRectangle(Bitmap bitmap1, Bitmap bitmap2, IEqualityComparer<Color> comparer, int startX, int startY)
    {
        int width = bitmap1.Width;
        int height = bitmap1.Height;

        int endX = startX;
        int endY = startY;

        // Expand horizontally
        while (endX + 1 < width && !comparer.Equals(bitmap1.GetPixel(endX + 1, startY), bitmap2.GetPixel(endX + 1, startY)))
        {
            endX++;
        }

        // Expand vertically
        while (endY + 1 < height)
        {
            bool canExpand = true;
            for (int x = startX; x <= endX; x++)
            {
                if (!comparer.Equals(bitmap1.GetPixel(x, endY + 1), bitmap2.GetPixel(x, endY + 1)))
                {
                    canExpand = false;
                    break;
                }
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
