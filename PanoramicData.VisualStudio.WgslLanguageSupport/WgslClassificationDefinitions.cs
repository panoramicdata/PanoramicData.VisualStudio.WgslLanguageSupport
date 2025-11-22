#nullable enable
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PanoramicData.VisualStudio.WgslLanguageSupport;

/// <summary>
/// Defines classification types and formats for WGSL syntax highlighting.
/// </summary>
internal static class WgslClassificationDefinitions
{
	#region Classification Type Definitions

	[Export(typeof(ClassificationTypeDefinition))]
	[Name("wgsl.keyword")]
	internal static ClassificationTypeDefinition? WgslKeywordType { get; set; }

	[Export(typeof(ClassificationTypeDefinition))]
	[Name("wgsl.type")]
	internal static ClassificationTypeDefinition? WgslTypeType { get; set; }

	[Export(typeof(ClassificationTypeDefinition))]
	[Name("wgsl.attribute")]
	internal static ClassificationTypeDefinition? WgslAttributeType { get; set; }

	[Export(typeof(ClassificationTypeDefinition))]
	[Name("wgsl.comment")]
	internal static ClassificationTypeDefinition? WgslCommentType { get; set; }

	[Export(typeof(ClassificationTypeDefinition))]
	[Name("wgsl.number")]
	internal static ClassificationTypeDefinition? WgslNumberType { get; set; }

	[Export(typeof(ClassificationTypeDefinition))]
	[Name("wgsl.function")]
	internal static ClassificationTypeDefinition? WgslFunctionType { get; set; }

	#endregion

	#region Classification Format Definitions

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "wgsl.keyword")]
	[Name("wgsl.keyword.format")]
	[UserVisible(true)]
	[Order(Before = "Default")]
	internal sealed class WgslKeywordFormat : ClassificationFormatDefinition
	{
		public WgslKeywordFormat()
		{
			DisplayName = "WGSL Keyword";
			ForegroundColor = Color.FromRgb(86, 156, 214); // VS Blue
			IsBold = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "wgsl.type")]
	[Name("wgsl.type.format")]
	[UserVisible(true)]
	[Order(Before = "Default")]
	internal sealed class WgslTypeFormat : ClassificationFormatDefinition
	{
		public WgslTypeFormat()
		{
			DisplayName = "WGSL Type";
			ForegroundColor = Color.FromRgb(78, 201, 176); // VS Teal
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "wgsl.attribute")]
	[Name("wgsl.attribute.format")]
	[UserVisible(true)]
	[Order(Before = "Default")]
	internal sealed class WgslAttributeFormat : ClassificationFormatDefinition
	{
		public WgslAttributeFormat()
		{
			DisplayName = "WGSL Attribute";
			ForegroundColor = Color.FromRgb(156, 220, 254); // VS Light Blue
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "wgsl.comment")]
	[Name("wgsl.comment.format")]
	[UserVisible(true)]
	[Order(Before = "Default")]
	internal sealed class WgslCommentFormat : ClassificationFormatDefinition
	{
		public WgslCommentFormat()
		{
			DisplayName = "WGSL Comment";
			ForegroundColor = Color.FromRgb(87, 166, 74); // VS Green
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "wgsl.number")]
	[Name("wgsl.number.format")]
	[UserVisible(true)]
	[Order(Before = "Default")]
	internal sealed class WgslNumberFormat : ClassificationFormatDefinition
	{
		public WgslNumberFormat()
		{
			DisplayName = "WGSL Number";
			ForegroundColor = Color.FromRgb(181, 206, 168); // VS Light Green
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "wgsl.function")]
	[Name("wgsl.function.format")]
	[UserVisible(true)]
	[Order(Before = "Default")]
	internal sealed class WgslFunctionFormat : ClassificationFormatDefinition
	{
		public WgslFunctionFormat()
		{
			DisplayName = "WGSL Function";
			ForegroundColor = Color.FromRgb(220, 220, 170); // VS Yellow
		}
	}

	#endregion
}
