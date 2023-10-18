// See https://aka.ms/new-console-template for more information

using System.Drawing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ImageComparer;

BenchmarkRunner.Run<Benchmark>();

[MaxWarmupCount(8)] // Set the number of warm-up iterations
[MaxIterationCount(16)] // Set the number of benchmark iterations
[MemoryDiagnoser]
public class Benchmark
{
  private readonly Bitmap _bmp1, _bmp2;
  private readonly ImageComparerApplication
    _matrixApp, _directApp, _directUnsafeApp,
    _matrixAppVectorized, _directUnsageAppVectorized;
  
  public Benchmark()
  {
    var img1 = Image.FromFile(@"C:\Users\andriipodkolzin\source\repos\3TestTasks\ImageComparison\ImageComparison.IntegrationTests\TestData\ImageComparisonApplicationTests\WhenImagesAreEqual_Should_NoRectanglesAreDrawn\image1.png");
    var img2 = Image.FromFile(@"C:\Users\andriipodkolzin\source\repos\3TestTasks\ImageComparison\ImageComparison.IntegrationTests\TestData\ImageComparisonApplicationTests\WhenImagesAreEqual_Should_NoRectanglesAreDrawn\image2.png");
    _bmp1 = new Bitmap(img1);
    _bmp2 = new Bitmap(img2);
    var comparer = new ColorEqualityComparer(0.1);
    _matrixApp = new ImageComparerApplication(new MatrixAccessorFactory(), new ImageChangeDetector(comparer));
    _directApp = new ImageComparerApplication(new DirectAccessorFactory(), new ImageChangeDetector(comparer));
    _directUnsafeApp = new ImageComparerApplication(new PointerAccessorFactory(), new ImageChangeDetector(comparer));
    
    _directUnsageAppVectorized = new ImageComparerApplication(new PointerAccessorFactory(), new ImageComparerVectorized(comparer));
    _matrixAppVectorized = new ImageComparerApplication(new MatrixAccessorFactory(), new ImageComparerVectorized(comparer));
  }

  [Benchmark]
  public void Direct() => _directApp.Run(_bmp1, _bmp2);

  [Benchmark(Baseline = true)]
  public void Matrix() => _matrixApp.Run(_bmp1, _bmp2);
  
  [Benchmark]
  public void DirectUnsafe() => _directUnsafeApp.Run(_bmp1, _bmp2);
  
  [Benchmark]
  public void MatrixVectorized() => _matrixAppVectorized.Run(_bmp1, _bmp2);
  
  [Benchmark]
  public void DirectUnsafeVectorized() => _directUnsageAppVectorized.Run(_bmp1, _bmp2);
}