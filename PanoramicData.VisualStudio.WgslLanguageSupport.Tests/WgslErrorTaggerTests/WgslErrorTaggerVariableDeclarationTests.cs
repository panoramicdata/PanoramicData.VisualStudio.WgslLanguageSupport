using AwesomeAssertions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger variable declaration validation.
/// </summary>
public class WgslErrorTaggerVariableDeclarationTests
{
	[Fact]
	public void GetTags_VarWithoutTypeOrInitializer_ShouldReturnError()
	{
		// Arrange
		var code = "var x;";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("type annotation or an initializer") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_VarWithType_ShouldNotReturnError()
	{
		// Arrange
		var code = "var x: i32;";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var declErrors = tags.Where(t => 
			t.Tag.ToolTipContent?.ToString()?.Contains("type annotation or an initializer") == true).ToList();
		_ = declErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_VarWithInitializer_ShouldNotReturnError()
	{
		// Arrange
		var code = "var x = 42;";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var declErrors = tags.Where(t => 
			t.Tag.ToolTipContent?.ToString()?.Contains("type annotation or an initializer") == true).ToList();
		_ = declErrors.Should().BeEmpty();
	}
}
