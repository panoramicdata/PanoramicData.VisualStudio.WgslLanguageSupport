using AwesomeAssertions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger binding validation.
/// </summary>
public class WgslErrorTaggerBindingTests
{
	[Fact]
	public void GetTags_DuplicateBindings_ShouldReturnError()
	{
		// Arrange
		var code = @"
@binding(0) @group(0) var tex1: texture_2d<f32>;
@binding(0) @group(0) var tex2: texture_2d<f32>;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Duplicate binding") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_DifferentBindingsSameGroup_ShouldNotReturnError()
	{
		// Arrange
		var code = @"
@binding(0) @group(0) var tex1: texture_2d<f32>;
@binding(1) @group(0) var tex2: texture_2d<f32>;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var bindingErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Duplicate binding") == true).ToList();
		_ = bindingErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_SameBindingDifferentGroups_ShouldNotReturnError()
	{
		// Arrange
		var code = @"
@binding(0) @group(0) var tex1: texture_2d<f32>;
@binding(0) @group(1) var tex2: texture_2d<f32>;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var bindingErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Duplicate binding") == true).ToList();
		_ = bindingErrors.Should().BeEmpty();
	}
}
