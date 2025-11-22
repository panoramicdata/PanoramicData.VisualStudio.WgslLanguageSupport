#nullable enable
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
	/// <summary>
	/// Gets or sets the classification type registry service.
	/// This property is set by MEF composition.
	/// </summary>
	[Import]
	internal IClassificationTypeRegistryService ClassificationRegistry { get; set; } = null!;

	/// <summary>
	/// Gets a classifier for the specified text buffer.
	/// </summary>
	/// <param name="textBuffer">The text buffer to classify.</param>
	/// <returns>A classifier instance, or null if the classifier cannot be created.</returns>
	public IClassifier? GetClassifier(ITextBuffer textBuffer)
	{
		if (textBuffer == null)
		{
			return null;
		}

		// ClassificationRegistry is guaranteed to be set by MEF, but check defensively
		if (ClassificationRegistry == null)
		{
			return null;
		}

		return textBuffer.Properties.GetOrCreateSingletonProperty(
			() => new WgslClassifier(ClassificationRegistry));
	}
}
