// See https://aka.ms/new-console-template for more information

using System.Drawing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ImageComparisonV2;
using ImageComparisonV2.BitmapAccessors;
using ImageComparerApplication = ImageComparisonV2.ImageComparerApplication;

BenchmarkRunner.Run<Benchmark>();


//[MaxWarmupCount(8)] // Set the number of warm-up iterations
//[MaxIterationCount(16)] // Set the number of benchmark iterations
[MemoryDiagnoser]
public class Benchmark
{
  private const string BasePath = "../../../../ImageComparison.IntegrationTests/TestData/";
  private readonly Bitmap _bmp1, _bmp2;
  private readonly ImageComparerApplication _matrixApp = new (new ImageComparer(new ColorEqualityComparer(0.1)), new MatrixAccessorFactory());
  private readonly ImageComparerApplication _bitmapApp = new (new ImageComparer(new ColorEqualityComparer(0.1)), new BitmapAccessorFactory());
  private readonly ImageComparerApplication _bitmapUnsafeApp = new (new ImageComparer(new ColorEqualityComparer(0.1)), new UnsafeBitmapAccessorFactory());
  private readonly ImageComparerApplication _bitmapUnsafeVectorizedApp = new (new ImageComparerVectorized(new ColorEqualityComparer(0.1)), new UnsafeBitmapAccessorFactory());
  private readonly ImageComparerApplication _simdApp = new (new ImageComparerVectorized(new ColorEqualityComparer(0.1)), new MatrixAccessorFactory());
  
  public Dictionary<string, (Bitmap, Bitmap)> Pairs { get; } = new();
  
  
  [Params("same", "realistic", "large_same")]
  public string PairName { get; set; }
  
  public Benchmark()
  {
    void Add(string folder, string key, string extension = "bmp")
    {
      const string path = @"C:\Users\andriipodkolzin\source\repos\3TestTasks\ImageComparison\ImageComparison.IntegrationTests\TestData\ImageComparisonApplicationTests\{0}\image{1}.{2}";
      var bmp1 = (Bitmap)Image.FromFile(string.Format(path, folder, 1, extension));
      var bmp2 = (Bitmap)Image.FromFile(string.Format(path, folder, 2, extension));

      var copy1 = new Bitmap(bmp1);
      var copy2 = new Bitmap(bmp2);
      
      Pairs.Add(key, (copy1, copy2));
    }
    
    Add("WhenImagesAreEqual_Should_NoRectanglesAreDrawn", "same", "png");
    Add("WhenRealistic_Should_BeLotsOfRectangles", "realistic");
    Add("WhenRealistic_Should_BeLotsOfRectangles", "large_same");
    Pairs["large_same"] = (new Bitmap(Pairs["large_same"].Item1), new Bitmap(Pairs["large_same"].Item1));
  }

  public Bitmap Test(ImageComparerApplication app)
  {
    var (bmp1, bmp2) = Pairs[PairName];
    return app.Run(bmp1, bmp2);
  }

  [Benchmark]
  public void Matrix() => Test(_matrixApp);
  
  [Benchmark]
  public void MatrixSIMD() => Test(_simdApp);

  [Benchmark]
  public void Direct() => Test(_bitmapApp);

  [Benchmark]
  public void DirectUnsafe() => Test(_bitmapUnsafeApp);

  [Benchmark(Baseline = true)]
  public void DirectUnsafeVectorized() => Test(_bitmapUnsafeVectorizedApp);
}