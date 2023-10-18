// See https://aka.ms/new-console-template for more information

using ImageComparison;

Console.WriteLine("Hello, World!");



public interface IDetector
{
  List<Rectangle> Detect(int[,] img1, int[,] img2);
}

public class Detector : IDetector
{
  private readonly IEqualityComparer<int> _comparer;
  public Detector(IEqualityComparer<int> comparer)
  {
    _comparer = comparer;
  }
  
  public List<Rectangle> Detect(int[,] img1, int[,] img2) => Visited.DetectChanges(img1, img2, _comparer);
}

public class SimdDetector : IDetector
{
  private readonly IEqualityComparer<int> _comparer;
  public SimdDetector(IEqualityComparer<int> comparer)
  {
    _comparer = comparer;
  }
  
  public List<Rectangle> Detect(int[,] img1, int[,] img2) => VisitedVectorized.DetectChanges(img1, img2, _comparer);
}

public record Rectangle(int Left, int Top, int Width, int Height);
