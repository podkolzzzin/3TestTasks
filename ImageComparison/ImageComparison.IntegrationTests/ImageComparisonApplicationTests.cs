using System.Drawing;
using FluentAssertions;
using ImageComparisonV2;
using ImageComparisonV2.BitmapAccessors;

namespace ImageComparison.IntegrationTests;

public class ImageComparisonApplicationTests
{
  
  
  [Theory]
  [MemberData(nameof(Data))]
  public void WhenTheImagesAreDifferent_Should_DrawRedRectangles(string caseName, string image1, string image2, string template)
  {
    var application = new ImageComparerApplication(new ImageComparer(new ColorEqualityComparer(0.1)), new MatrixAccessorFactory());
    var result = application.Run(image1, image2);

    if (File.Exists(template))
    {
      var templateImage = (Bitmap)Image.FromFile(template);
      AssertEquals(templateImage, result);
    }
    else
    {
      result.Save(template);
    }
  }
  
  private void AssertEquals(Bitmap expected, Bitmap actual)
  {
    expected.Width.Should().Be(actual.Width);
    expected.Height.Should().Be(actual.Height);

    for (int y = 0; y < expected.Height; y++)
    {
      for (int x = 0; x < expected.Width; x++)
      {
        var expectedPixel = expected.GetPixel(x, y);
        var actualPixel = actual.GetPixel(x, y);

        actualPixel.Should().Be(expectedPixel);
      }
    }
  }

  public static IEnumerable<object[]> Data 
  {
    get
    {
      string FindFolder()
      {
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
          var folder = Path.Combine(current, "TestData");
          if (Directory.Exists(folder))
          {
            return folder;
          }
          current = Directory.GetParent(current)?.FullName;
        }
        throw new Exception("Could not find the Images folder");
      }

      var folder = Path.Combine(FindFolder(), nameof(ImageComparisonApplicationTests));
      foreach (var dir in new DirectoryInfo(folder).GetDirectories())
      {
        var file1 = dir.GetFiles("image1.*").Single();
        var file2 = dir.GetFiles("image2.*").Single();
        yield return new object[] { dir.Name, file1.FullName, file2.FullName, Path.Combine(dir.FullName, "template" + file2.Extension) };
      }
    }
  }
}