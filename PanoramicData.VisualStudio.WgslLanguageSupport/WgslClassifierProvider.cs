using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PanoramicData.VisualStudio.WgslLanguageSupport;

/// <summary>
/// Provides WgslClassifier instances for WGSL content.
/// </summary>
[Export(typeof(IClassifierProvider))]
[ContentType("wgsl")]
internal class WgslClassifierProvider : IClassifierProvider
{
	[Import]
	internal IClassificationTypeRegistryService? ClassificationRegistry { get; set; }

	public IClassifier? GetClassifier(ITextBuffer textBuffer)
	{
		if (ClassificationRegistry == null)
			return null;

		return textBuffer.Properties.GetOrCreateSingletonProperty(
			() => new WgslClassifier(ClassificationRegistry));
	}
}
