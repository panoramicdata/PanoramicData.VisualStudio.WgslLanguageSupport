using AwesomeAssertions;
using Microsoft.VisualStudio.Text.Adornments;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger missing semicolon detection.
/// </summary>
public class WgslErrorTaggerSemicolonTests
{
	[Fact]
	public void GetTags_VarWithoutSemicolon_ShouldReturnError()
	{
		// Arrange
		var code = "var x: i32";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Count.Should().Be(1);
		_ = tags[0].Tag.ErrorType.Should().Be(PredefinedErrorTypeNames.SyntaxError);
		_ = tags[0].Tag.ToolTipContent.Should().Be("Missing semicolon at end of statement");
	}

	[Fact]
	public void GetTags_LetWithoutSemicolon_ShouldReturnError()
	{
		// Arrange
		var code = "let x = 42";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Count.Should().Be(1);
		_ = tags[0].Tag.ErrorType.Should().Be(PredefinedErrorTypeNames.SyntaxError);
	}

	[Fact]
	public void GetTags_ReturnWithoutSemicolon_ShouldReturnError()
	{
		// Arrange
		var code = "fn test() { return x }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("semicolon") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_VarWithSemicolon_ShouldNotReturnError()
	{
		// Arrange
		var code = "var x: i32;";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_VarInComment_ShouldNotReturnError()
	{
		// Arrange
		var code = "// var x: i32";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().BeEmpty();
	}
}
