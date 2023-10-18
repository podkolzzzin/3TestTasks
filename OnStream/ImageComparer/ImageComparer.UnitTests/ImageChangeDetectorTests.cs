using FluentAssertions;

namespace ImageComparer.UnitTests;

public class ImageChangeDetectorTests
{
  private List<Rectangle> Act(int[,] matrix1, int[,] matrix2)
  {
    var comparer = new ImageChangeDetector(EqualityComparer<ColorData>.Default);
    var bmp1 = new MatrixAccessor(matrix1);
    var bmp2 = new MatrixAccessor(matrix2);
    return comparer.DetectChanges(bmp1, bmp2);
  }
  
  [Fact]
  public void When_SecondImageIsWidder_Should_ReturnOneRectDiff()
  {
    var comparer = new ImageChangeDetector(EqualityComparer<ColorData>.Default);
    var result = Act(new int[,]
    {
      { 0, 0 },
      { 0, 0 }
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 }
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 2 && x.Top == 0 && x.Width == 1 && x.Height == 2);
  }

  [Fact]
  public void When_SecondImageIsHeigher_Should_ReturnOneRectDiff()
  {
    var result = Act(new int[,]
    {
      { 0, 0 },
      { 0, 0 }
    }, new int[,]
    {
      { 0, 0 },
      { 0, 0 },
      { 0, 0 },
    });

    result.Should().HaveCount(1)
      .And.ContainSingle(x => x.Left == 0 && x.Top == 2 && x.Width == 2 && x.Height == 1);
  }

  [Fact]
  public void When_SecondImageIsBigger_Should_ReturnTwoRectDiff()
  {
    var result = Act(new int[,]
    {
      { 0, 0 },
      { 0, 0 }
    }, new int[,]
    {
      { 0, 0, 0 },
      { 0, 0, 0 },
      { 0, 0, 0 },
    });

    result.Should().HaveCount(2)
      .And.ContainSingle(x => x.Left == 0 && x.Top == 2 && x.Width == 3 && x.Height == 1)
      .And.ContainSingle(x => x.Left == 2 && x.Top == 0 && x.Width == 1 && x.Height == 2);
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

  // [Fact]
  // public void When_SingleDiff_Detect_ShouldReturnSingle2x3Rect()
  // {
  //   var result = Act(new int[,]
  //   {
  //     { 0, 0, 0 },
  //     { 0, 0, 0 },
  //     { 0, 0, 0 },
  //   }, new int[,]
  //   {
  //     { 0, 0, 1 },
  //     { 0, 1, 1 },
  //     { 0, 0, 1 },
  //   });
  //
  //   result.Should().HaveCount(1)
  //     .And.ContainSingle(x => x.Left == 1 && x.Top == 0 && x.Width == 2 && x.Height == 3);
  // }
  //
  // [Fact]
  // public void When_SingleDiff_Detect_ShouldReturnSingle2x2Rect2()
  // {
  //   var result = Act(new int[,]
  //   {
  //     { 0, 0, 0 },
  //     { 0, 0, 0 },
  //     { 0, 0, 0 },
  //   }, new int[,]
  //   {
  //     { 0, 0, 0 },
  //     { 0, 1, 1 },
  //     { 0, 0, 1 },
  //   });
  //
  //   result.Should().HaveCount(1)
  //     .And.ContainSingle(x => x.Left == 1 && x.Top == 1 && x.Width == 2 && x.Height == 2);
  // }
}