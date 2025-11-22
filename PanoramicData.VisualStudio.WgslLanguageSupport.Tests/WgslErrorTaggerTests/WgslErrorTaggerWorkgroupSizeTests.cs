using AwesomeAssertions;
using Microsoft.VisualStudio.Text.Adornments;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger workgroup size validation.
/// </summary>
public class WgslErrorTaggerWorkgroupSizeTests
{
	[Fact]
	public void GetTags_WorkgroupSizeTooLarge_ShouldReturnError()
	{
		// Arrange
		var code = "@compute @workgroup_size(300) fn main() {}";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("between 1 and 256") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_WorkgroupSizeZero_ShouldReturnError()
	{
		// Arrange
		var code = "@compute @workgroup_size(0) fn main() {}";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("between 1 and 256") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_WorkgroupSizeTotalTooLarge_ShouldReturnWarning()
	{
		// Arrange
		var code = "@compute @workgroup_size(16, 16, 2) fn main() {}";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => 
			t.Tag.ErrorType == PredefinedErrorTypeNames.Warning &&
			t.Tag.ToolTipContent?.ToString()?.Contains("exceeds recommended limit") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_WorkgroupSizeValid_ShouldNotReturnError()
	{
		// Arrange
		var code = "@compute @workgroup_size(8, 8, 1) fn main() {}";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var workgroupErrors = tags.Where(t => 
			t.Tag.ToolTipContent?.ToString()?.Contains("Workgroup size") == true).ToList();
		_ = workgroupErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_WorkgroupSizeSingleDimension_ShouldNotReturnError()
	{
		// Arrange
		var code = "@compute @workgroup_size(64) fn main() {}";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var workgroupErrors = tags.Where(t => 
			t.Tag.ToolTipContent?.ToString()?.Contains("Workgroup size") == true).ToList();
		_ = workgroupErrors.Should().BeEmpty();
	}
}
