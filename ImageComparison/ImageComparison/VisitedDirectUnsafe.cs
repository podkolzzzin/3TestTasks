using System.Drawing;
using System.Drawing.Imaging;

namespace ImageComparison;


public class VisitedDirectUnsafe
{
    public static List<Rectangle> DetectChanges(Bitmap bitmap1, Bitmap bitmap2, IEqualityComparer<int> comparer)
    {
        int width = Math.Min(bitmap1.Width, bitmap2.Width);
        int height = Math.Min(bitmap1.Height, bitmap2.Height);

        List<Rectangle> rectangles = new List<Rectangle>();

        unsafe
        {
            var bits1 = bitmap1.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bits2 = bitmap2.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            
            try
            {
                byte* p1 = (byte*)(void*)bits1.Scan0;
                byte* p2 = (byte*)(void*)bits2.Scan0;
                
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (!comparer.Equals(GetPixel(p1), GetPixel(p2)))
                        {
                            // Change detected, start growing the rectangle
                            Rectangle rectangle = GrowRectangle(p1, p2, width, height, comparer, x, y);
                            rectangles.Add(rectangle);
                        }
                    }
                }
                p1 += 4;
                p2 += 4;
            }
            finally
            {
                bitmap1.UnlockBits(bits1);
                bitmap2.UnlockBits(bits2);   
            }
        }
        
        return rectangles;
    }

    private static unsafe int GetPixel(byte* p)
    {
        // Get the ARGB values
        int blue = p[0];
        int green = p[1];
        int red = p[2];
        int alpha = p[3];

        // Convert to a long value (assuming little-endian byte order)
        return ((alpha << 24) | (red << 16) | (green << 8) | blue);
    }

    private static unsafe Rectangle GrowRectangle(byte* bitmap1, byte* bitmap2, int width, int height, IEqualityComparer<int> comparer, int startX, int startY)
    {

        int endX = startX;
        int endY = startY;

        // Expand horizontally
        while (endX + 1 < width && !comparer.Equals(GetPixel(bitmap1), GetPixel(bitmap2)))
        {
            endX++;
        }

        // Expand vertically
        while (endY + 1 < height)
        {
            bool canExpand = true;
            for (int x = startX; x <= endX; x++)
            {
                
                if (!comparer.Equals(GetPixel(bitmap1 + width), GetPixel(bitmap2 + width)))
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
