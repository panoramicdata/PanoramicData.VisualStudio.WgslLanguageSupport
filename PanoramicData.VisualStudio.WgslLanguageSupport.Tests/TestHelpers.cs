using Microsoft.VisualStudio.Text;
using NSubstitute;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests;

/// <summary>
/// Helper methods for unit tests.
/// </summary>
internal static class TestHelpers
{
	/// <summary>
	/// Creates a mock ITextBuffer with the specified content.
	/// </summary>
	public static ITextBuffer CreateMockTextBuffer(string content)
	{
		var buffer = Substitute.For<ITextBuffer>();
		var snapshot = Substitute.For<ITextSnapshot>();
		
		snapshot.GetText().Returns(content);
		snapshot.Length.Returns(content.Length);
		snapshot.GetText(Arg.Any<int>(), Arg.Any<int>()).Returns(ci => 
		{
			var start = ci.ArgAt<int>(0);
			var length = ci.ArgAt<int>(1);
			return content.Substring(start, Math.Min(length, content.Length - start));
		});
		
		buffer.CurrentSnapshot.Returns(snapshot);
		buffer.Properties.Returns(new Microsoft.VisualStudio.Utilities.PropertyCollection());
		
		return buffer;
	}

	/// <summary>
	/// Creates a NormalizedSnapshotSpanCollection for the entire buffer.
	/// </summary>
	public static NormalizedSnapshotSpanCollection CreateSpanCollection(ITextBuffer buffer)
	{
		var snapshot = buffer.CurrentSnapshot;
		var span = new SnapshotSpan(snapshot, 0, snapshot.Length);
		return new NormalizedSnapshotSpanCollection(span);
	}

	/// <summary>
	/// Creates a mock TextContentChangedEventArgs for testing buffer changes.
	/// </summary>
	public static TextContentChangedEventArgs CreateTextContentChangedEventArgs(ITextBuffer buffer)
	{
		var before = Substitute.For<ITextSnapshot>();
		var after = buffer.CurrentSnapshot;
		
		var args = Substitute.For<TextContentChangedEventArgs>(
			before,
			after,
			EditOptions.None,
			null);
		
		args.After.Returns(after);
		
		return args;
	}
}
