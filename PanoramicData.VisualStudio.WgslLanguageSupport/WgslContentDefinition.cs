#nullable enable
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace PanoramicData.VisualStudio.WgslLanguageSupport;

/// <summary>
/// Defines the "wgsl" content type and associates it with the .wgsl file extension.
/// </summary>
internal static class WgslContentDefinition
{
	/// <summary>
	/// Exports the WGSL content type definition.
	/// </summary>
	[Export]
	[Name("wgsl")]
	[BaseDefinition("code")]
	public static ContentTypeDefinition? WgslContentTypeDefinition { get; set; }

	/// <summary>
	/// Associates the .wgsl file extension with the WGSL content type.
	/// </summary>
	[Export]
	[FileExtension(".wgsl")]
	[ContentType("wgsl")]
	public static FileExtensionToContentTypeDefinition? WgslFileExtensionDefinition { get; set; }
}
