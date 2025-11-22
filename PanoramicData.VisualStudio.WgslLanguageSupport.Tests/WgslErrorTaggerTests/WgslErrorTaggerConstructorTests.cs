using AwesomeAssertions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger constructor.
/// </summary>
public class WgslErrorTaggerConstructorTests
{
	[Fact]
	public void Constructor_WithValidBuffer_ShouldCreateInstance()
	{
		// Arrange
		var buffer = TestHelpers.CreateMockTextBuffer("");

		// Act
		var tagger = new WgslErrorTagger(buffer);

		// Assert
		_ = tagger.Should().NotBeNull();
	}

	[Fact]
	public void Constructor_WithNullBuffer_ShouldThrowArgumentNullException()
	{
		// Act & Assert
		Action act = () => new WgslErrorTagger(null!);
		_ = act.Should().ThrowExactly<ArgumentNullException>();
	}

	[Fact]
	public void GetTags_EmptySpanCollection_ShouldReturnEmpty()
	{
		// Arrange
		var buffer = TestHelpers.CreateMockTextBuffer("var x: i32");
		var tagger = new WgslErrorTagger(buffer);
		var emptySpans = new NormalizedSnapshotSpanCollection();

		// Act
		var tags = tagger.GetTags(emptySpans).ToList();

		// Assert
		_ = tags.Should().BeEmpty();
	}

	[Fact]
	public void BufferChanged_ShouldRaiseTagsChangedEvent()
	{
		// Arrange
		var buffer = TestHelpers.CreateMockTextBuffer("var x: i32;");
		var tagger = new WgslErrorTagger(buffer);
		var eventRaised = false;
		tagger.TagsChanged += (sender, args) => eventRaised = true;

		// Act
		buffer.Changed += NSubstitute.Raise.Event<EventHandler<TextContentChangedEventArgs>>(
			buffer,
			TestHelpers.CreateTextContentChangedEventArgs(buffer));

		// Assert
		_ = eventRaised.Should().BeTrue();
	}
}
