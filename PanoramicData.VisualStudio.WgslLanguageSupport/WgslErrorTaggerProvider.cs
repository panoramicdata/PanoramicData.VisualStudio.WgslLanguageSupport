using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace PanoramicData.VisualStudio.WgslLanguageSupport;

/// <summary>
/// Provides WgslErrorTagger instances for WGSL text buffers.
/// </summary>
[Export(typeof(ITaggerProvider))]
[ContentType("wgsl")]
[TagType(typeof(IErrorTag))]
internal class WgslErrorTaggerProvider : ITaggerProvider
{
	/// <summary>
	/// Creates a tagger for the specified buffer.
	/// </summary>
	public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
	{
		if (buffer == null)
		{
			return null;
		}

		// Only create one tagger per buffer
		return buffer.Properties.GetOrCreateSingletonProperty(
			() => new WgslErrorTagger(buffer)) as ITagger<T>;
	}
}
