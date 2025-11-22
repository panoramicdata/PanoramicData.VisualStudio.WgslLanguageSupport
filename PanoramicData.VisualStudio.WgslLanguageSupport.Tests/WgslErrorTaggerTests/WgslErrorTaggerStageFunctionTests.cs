using AwesomeAssertions;
using Microsoft.VisualStudio.Text.Adornments;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger stage function validation.
/// </summary>
public class WgslErrorTaggerStageFunctionTests
{
	[Fact]
	public void GetTags_VertexShaderWithoutPosition_ShouldReturnWarning()
	{
		// Arrange
		var code = "@vertex fn main() -> vec4f { return vec4f(); }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => 
			t.Tag.ErrorType == PredefinedErrorTypeNames.Warning &&
			t.Tag.ToolTipContent?.ToString()?.Contains("@builtin(position)") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_VertexShaderWithPosition_ShouldNotReturnWarning()
	{
		// Arrange
		var code = "@vertex fn main() -> @builtin(position) vec4f { return vec4f(); }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var warnings = tags.Where(t => 
			t.Tag.ErrorType == PredefinedErrorTypeNames.Warning &&
			t.Tag.ToolTipContent?.ToString()?.Contains("@builtin(position)") == true).ToList();
		_ = warnings.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_FragmentShaderWithoutLocation_ShouldReturnWarning()
	{
		// Arrange
		var code = "@fragment fn main() -> vec4f { return vec4f(); }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => 
			t.Tag.ErrorType == PredefinedErrorTypeNames.Warning &&
			t.Tag.ToolTipContent?.ToString()?.Contains("@location") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_FragmentShaderWithLocation_ShouldNotReturnWarning()
	{
		// Arrange
		var code = "@fragment fn main() -> @location(0) vec4f { return vec4f(); }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var warnings = tags.Where(t => 
			t.Tag.ErrorType == PredefinedErrorTypeNames.Warning &&
			t.Tag.ToolTipContent?.ToString()?.Contains("@location") == true).ToList();
		_ = warnings.Should().BeEmpty();
	}
}
