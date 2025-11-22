using AwesomeAssertions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger attribute validation.
/// </summary>
public class WgslErrorTaggerAttributeTests
{
	[Fact]
	public void GetTags_InvalidAttribute_ShouldReturnError()
	{
		// Arrange
		var code = "@invalid fn test() {}";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unknown attribute '@invalid'") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_ValidVertexAttribute_ShouldNotReturnAttributeError()
	{
		// Arrange
		var code = "@vertex fn main() -> @builtin(position) vec4f { return vec4f(); }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var attrErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unknown attribute") == true).ToList();
		_ = attrErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_ValidFragmentAttribute_ShouldNotReturnAttributeError()
	{
		// Arrange
		var code = "@fragment fn main() -> @location(0) vec4f { return vec4f(); }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var attrErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unknown attribute") == true).ToList();
		_ = attrErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_ValidComputeAttribute_ShouldNotReturnAttributeError()
	{
		// Arrange
		var code = "@compute @workgroup_size(8, 8, 1) fn main() {}";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var attrErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unknown attribute") == true).ToList();
		_ = attrErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_ValidBindingGroupAttributes_ShouldNotReturnAttributeError()
	{
		// Arrange
		var code = "@binding(0) @group(0) var<uniform> myUniform: vec4f;";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var attrErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unknown attribute") == true).ToList();
		_ = attrErrors.Should().BeEmpty();
	}
}
