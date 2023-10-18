using FluentAssertions;
using ImageComparisonV2.BitmapAccessors;

namespace ImageComparisonV2.UnitTests;

public class ImageDetectorTests
{
  private readonly Detector _detector = new(EqualityComparer<int>.Default);

  private List<Rectangle> Act(int[,] a, int[,] b)
  {
    var bmp1 = new MatrixAccessor(a);
    var bmp2 = new MatrixAccessor(b);
    return new ImageComparer(EqualityComparer<ColorData>.Default).DetectChanges(bmp1, bmp2);
  }
  
  [Fact]
  public void When_SingleDiff_Detect_ShouldReturnSingle1x1Rect()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 1, 0 },
      { 0, 0, 0 },
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 1 && x.Top == 1 && x.Width == 1 && x.Height == 1);
  }
  
  [Fact]
  public void When_SingleDiff_Detect_ShouldReturnSingle2x1Rect()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 1, 1 },
      { 0, 0, 0 },
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 1 && x.Top == 1 && x.Width == 2 && x.Height == 1);
  }
  
  [Fact]
  public void When_SingleDiff_Detect_ShouldReturnSingle1x2Rect()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 1, 0 },
      { 0, 1, 0 },
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 1 && x.Top == 1 && x.Width == 1 && x.Height == 2);
  }
  
  [Fact]
  public void When_SingleDiff_Detect_ShouldReturnSingle2x2Rect()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 1, 1 },
      { 0, 1, 1 },
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 1 && x.Top == 1 && x.Width == 2 && x.Height == 2);
  }
  
  [Fact]
  public void When_SingleDiff_Detect_ShouldReturn2Rects()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 1, 0, 1 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    });

    result.Should().HaveCount(2)
      .And.ContainSingle(x => x.Left == 0 && x.Top == 0 && x.Width == 1 && x.Height == 1)
      .And.ContainSingle(x => x.Left == 2 && x.Top == 0 && x.Width == 1 && x.Height == 1);
  }
  
  [Fact]
  public void When_SingleDiff_Detect_ShouldReturnSingle2x3Rect()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 1 },
      { 0, 1, 1 },
      { 0, 0, 1 },
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 1 && x.Top == 0 && x.Width == 2 && x.Height == 3);
  }
  
  [Fact]
  public void When_SingleDiff_Detect_ShouldReturnSingle2x2Rect2()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 1, 1 },
      { 0, 0, 1 },
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 1 && x.Top == 1 && x.Width == 2 && x.Height == 2);
  }
  
  [Fact]
  public void When_SecondImageIsBigger_Detect_ShouldReturn2RectsDiff()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
    });


    result.Should().BeEquivalentTo(new List<Rectangle>()
    {
      new(3, 0, 1, 3),
      new(0, 3, 4, 1)
    });
  }
  
  [Fact]
  public void When_FirstImageIsBigger_Detect_ShouldReturnEmptyDiff()
  {
    var result = Act(new int[,]
    {
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    });


    result.Should().BeEmpty();
  }
}