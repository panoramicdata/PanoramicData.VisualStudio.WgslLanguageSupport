#nullable enable
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport;

/// <summary>
/// Classifies WGSL code for syntax highlighting.
/// </summary>
internal class WgslClassifier : IClassifier
{
	private readonly IClassificationType? _keywordType;
	private readonly IClassificationType? _typeType;
	private readonly IClassificationType? _attributeType;
	private readonly IClassificationType? _commentType;
	private readonly IClassificationType? _numberType;
	private readonly IClassificationType? _functionType;

	public WgslClassifier(IClassificationTypeRegistryService registry)
	{
		if (registry == null)
		{
			throw new ArgumentNullException(nameof(registry));
		}

		// Get classification types - these may be null if not yet registered
		_keywordType = registry.GetClassificationType("wgsl.keyword");
		_typeType = registry.GetClassificationType("wgsl.type");
		_attributeType = registry.GetClassificationType("wgsl.attribute");
		_commentType = registry.GetClassificationType("wgsl.comment");
		_numberType = registry.GetClassificationType("wgsl.number");
		_functionType = registry.GetClassificationType("wgsl.function");
	}

	// WGSL Keywords
	private static readonly HashSet<string> Keywords =
	[
		"fn", "var", "let", "const", "struct", "if", "else", "for", "while", "loop",
		"return", "break", "continue", "discard", "switch", "case", "default",
		"true", "false", "override", "enable", "requires", "diagnostic",
		"alias", "continuing", "fallthrough"
	];

	// WGSL Built-in Types
	private static readonly HashSet<string> Types =
	[
		"bool", "i32", "u32", "f32", "f16",
		"vec2", "vec3", "vec4",
		"vec2i", "vec3i", "vec4i", "vec2u", "vec3u", "vec4u",
		"vec2f", "vec3f", "vec4f", "vec2h", "vec3h", "vec4h",
		"mat2x2", "mat2x3", "mat2x4", "mat3x2", "mat3x3", "mat3x4",
		"mat4x2", "mat4x3", "mat4x4",
		"mat2x2f", "mat2x3f", "mat2x4f", "mat3x2f", "mat3x3f", "mat3x4f",
		"mat4x2f", "mat4x3f", "mat4x4f",
		"mat2x2h", "mat2x3h", "mat2x4h", "mat3x2h", "mat3x3h", "mat3x4h",
		"mat4x2h", "mat4x3h", "mat4x4h",
		"array", "ptr", "atomic", "sampler", "sampler_comparison",
		"texture_1d", "texture_2d", "texture_2d_array", "texture_3d",
		"texture_cube", "texture_cube_array", "texture_multisampled_2d",
		"texture_storage_1d", "texture_storage_2d", "texture_storage_2d_array", "texture_storage_3d",
		"texture_depth_2d", "texture_depth_2d_array", "texture_depth_cube",
		"texture_depth_cube_array", "texture_depth_multisampled_2d",
		"texture_external"
	];

	/// <summary>
	/// Occurs when the classification of a span of text has changed.
	/// This event is required by IClassifier but not currently used as classifications are stateless.
	/// </summary>
#pragma warning disable CS0067 // Event is never used
	public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;
#pragma warning restore CS0067

	public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
	{
		var classifications = new List<ClassificationSpan>();
		var text = span.GetText();
		var startPosition = span.Start.Position;

		// 1. Classify comments first (they take precedence)
		ClassifyComments(text, startPosition, span.Snapshot, classifications);

		// 2. Classify attributes (@vertex, @location, etc.)
		ClassifyAttributes(text, startPosition, span.Snapshot, classifications);

		// 3. Classify numbers
		ClassifyNumbers(text, startPosition, span.Snapshot, classifications);

		// 4. Classify function calls
		ClassifyFunctions(text, startPosition, span.Snapshot, classifications);

		// 5. Classify keywords and types
		ClassifyKeywordsAndTypes(text, startPosition, span.Snapshot, classifications);

		return classifications;
	}

	private void ClassifyComments(string text, int startPosition, ITextSnapshot snapshot, List<ClassificationSpan> classifications)
	{
		if (_commentType == null) return;

		// Line comments: //
		var lineCommentPattern = @"//[^\r\n]*";
		foreach (Match match in Regex.Matches(text, lineCommentPattern))
		{
			var span = new SnapshotSpan(snapshot, startPosition + match.Index, match.Length);
			classifications.Add(new ClassificationSpan(span, _commentType));
		}

		// Block comments: /* */
		var blockCommentPattern = @"/\*[\s\S]*?\*/";
		foreach (Match match in Regex.Matches(text, blockCommentPattern))
		{
			var span = new SnapshotSpan(snapshot, startPosition + match.Index, match.Length);
			classifications.Add(new ClassificationSpan(span, _commentType));
		}
	}

	private void ClassifyAttributes(string text, int startPosition, ITextSnapshot snapshot, List<ClassificationSpan> classifications)
	{
		if (_attributeType == null) return;

		// Match @attribute or @attribute(...)
		var attributePattern = @"@\w+(?:\([^)]*\))?";
		foreach (Match match in Regex.Matches(text, attributePattern))
		{
			// Skip if inside a comment
			if (IsInsideComment(text, match.Index))
				continue;

			var span = new SnapshotSpan(snapshot, startPosition + match.Index, match.Length);
			classifications.Add(new ClassificationSpan(span, _attributeType));
		}
	}

	private void ClassifyNumbers(string text, int startPosition, ITextSnapshot snapshot, List<ClassificationSpan> classifications)
	{
		if (_numberType == null) return;

		// Match numbers: integers, floats, hex
		var numberPattern = @"\b(?:0x[0-9a-fA-F]+[iu]?|(?:[0-9]+\.?[0-9]*|\.[0-9]+)(?:[eE][+-]?[0-9]+)?[fh]?)\b";
		foreach (Match match in Regex.Matches(text, numberPattern))
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var span = new SnapshotSpan(snapshot, startPosition + match.Index, match.Length);
			classifications.Add(new ClassificationSpan(span, _numberType));
		}
	}

	private void ClassifyFunctions(string text, int startPosition, ITextSnapshot snapshot, List<ClassificationSpan> classifications)
	{
		if (_functionType == null) return;

		// Match function calls: identifier(
		var functionPattern = @"\b([a-zA-Z_]\w*)\s*\(";
		foreach (Match match in Regex.Matches(text, functionPattern))
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var functionName = match.Groups[1].Value;
			if (Keywords.Contains(functionName) || Types.Contains(functionName))
				continue;

			var span = new SnapshotSpan(snapshot, startPosition + match.Groups[1].Index, functionName.Length);
			classifications.Add(new ClassificationSpan(span, _functionType));
		}
	}

	private void ClassifyKeywordsAndTypes(string text, int startPosition, ITextSnapshot snapshot, List<ClassificationSpan> classifications)
	{
		// Match identifiers
		var wordPattern = @"\b[a-zA-Z_]\w*\b";
		foreach (Match match in Regex.Matches(text, wordPattern))
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var word = match.Value;

			if (Keywords.Contains(word) && _keywordType != null)
			{
				var span = new SnapshotSpan(snapshot, startPosition + match.Index, match.Length);
				classifications.Add(new ClassificationSpan(span, _keywordType));
			}
			else if (Types.Contains(word) && _typeType != null)
			{
				var span = new SnapshotSpan(snapshot, startPosition + match.Index, match.Length);
				classifications.Add(new ClassificationSpan(span, _typeType));
			}
		}
	}

	private bool IsInsideComment(string text, int position)
	{
		var beforeText = text.Substring(0, position);
		var lastLineComment = beforeText.LastIndexOf("//");
		var lastNewLine = beforeText.LastIndexOf('\n');

		// Check line comment
		if (lastLineComment > lastNewLine)
			return true;

		// Check block comment
		var blockCommentStarts = Regex.Matches(beforeText, @"/\*").Count;
		var blockCommentEnds = Regex.Matches(beforeText, @"\*/").Count;
		return blockCommentStarts > blockCommentEnds;
	}
}
