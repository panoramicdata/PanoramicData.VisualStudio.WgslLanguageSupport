using AwesomeAssertions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger brace matching detection.
/// </summary>
public class WgslErrorTaggerBraceTests
{
	[Fact]
	public void GetTags_UnclosedCurlyBrace_ShouldReturnError()
	{
		// Arrange
		var code = "fn test() {";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unclosed '{'") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_UnclosedParenthesis_ShouldReturnError()
	{
		// Arrange
		var code = "fn test(x: i32";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unclosed '('") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_UnclosedBracket_ShouldReturnError()
	{
		// Arrange
		var code = "var arr: array<i32, 5> = [1, 2";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unclosed '['") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_UnmatchedClosingBrace_ShouldReturnError()
	{
		// Arrange
		var code = "fn test() }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Unmatched closing '}'") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_MismatchedBraces_ShouldReturnError()
	{
		// Arrange
		var code = "fn test() { return (x] }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Mismatched brace") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_MatchedBraces_ShouldNotReturnError()
	{
		// Arrange
		var code = "fn test() { return (x); }";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var braceErrors = tags.Where(t => 
			t.Tag.ToolTipContent?.ToString()?.Contains("Unclosed") == true ||
			t.Tag.ToolTipContent?.ToString()?.Contains("Unmatched") == true ||
			t.Tag.ToolTipContent?.ToString()?.Contains("Mismatched") == true).ToList();
		_ = braceErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_BracesInLineComment_ShouldNotReturnError()
	{
		// Arrange
		var code = "// unclosed brace {";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_BracesInBlockComment_ShouldNotReturnError()
	{
		// Arrange
		var code = "/* unclosed brace { */";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_BracesInString_ShouldNotReturnError()
	{
		// Arrange
		var code = @"var str = ""unclosed brace {"";";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var braceErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("brace") == true).ToList();
		_ = braceErrors.Should().BeEmpty();
	}
}
