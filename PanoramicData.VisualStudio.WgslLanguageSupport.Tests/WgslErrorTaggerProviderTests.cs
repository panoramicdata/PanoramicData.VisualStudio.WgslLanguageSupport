using AwesomeAssertions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests;

/// <summary>
/// Tests for the WGSL error tagger provider.
/// </summary>
public class WgslErrorTaggerProviderTests
{
	[Fact]
	public void CreateTagger_WithValidBuffer_ShouldReturnTagger()
	{
		// Arrange
		var provider = new WgslErrorTaggerProvider();
		var buffer = TestHelpers.CreateMockTextBuffer("");

		// Act
		var tagger = provider.CreateTagger<IErrorTag>(buffer);

		// Assert
		_ = tagger.Should().NotBeNull();
	}

	[Fact]
	public void CreateTagger_WithNullBuffer_ShouldReturnNull()
	{
		// Arrange
		var provider = new WgslErrorTaggerProvider();

		// Act
		var tagger = provider.CreateTagger<IErrorTag>(null!);

		// Assert
		_ = tagger.Should().BeNull();
	}

	[Fact]
	public void CreateTagger_CalledTwiceWithSameBuffer_ShouldReturnSameTaggerInstance()
	{
		// Arrange
		var provider = new WgslErrorTaggerProvider();
		var buffer = TestHelpers.CreateMockTextBuffer("");

		// Act
		var tagger1 = provider.CreateTagger<IErrorTag>(buffer);
		var tagger2 = provider.CreateTagger<IErrorTag>(buffer);

		// Assert
		_ = tagger1.Should().NotBeNull();
		_ = tagger2.Should().NotBeNull();
		_ = ReferenceEquals(tagger1, tagger2).Should().BeTrue();
	}

	[Fact]
	public void CreateTagger_WithWrongTagType_ShouldReturnNull()
	{
		// Arrange
		var provider = new WgslErrorTaggerProvider();
		var buffer = TestHelpers.CreateMockTextBuffer("");

		// Act
		var tagger = provider.CreateTagger<IClassificationTag>(buffer);

		// Assert
		_ = tagger.Should().BeNull();
	}

	[Fact]
	public void CreateTagger_WithDifferentBuffers_ShouldReturnDifferentTaggers()
	{
		// Arrange
		var provider = new WgslErrorTaggerProvider();
		var buffer1 = TestHelpers.CreateMockTextBuffer("");
		var buffer2 = TestHelpers.CreateMockTextBuffer("");

		// Act
		var tagger1 = provider.CreateTagger<IErrorTag>(buffer1);
		var tagger2 = provider.CreateTagger<IErrorTag>(buffer2);

		// Assert
		_ = tagger1.Should().NotBeNull();
		_ = tagger2.Should().NotBeNull();
		_ = ReferenceEquals(tagger1, tagger2).Should().BeFalse();
	}
}
