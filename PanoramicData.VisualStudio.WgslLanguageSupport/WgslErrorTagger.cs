using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport;

/// <summary>
/// Implements comprehensive linting for WGSL files by tagging errors in the text buffer.
/// </summary>
internal class WgslErrorTagger : ITagger<IErrorTag>
{
	private readonly ITextBuffer _buffer;
	private ITextSnapshot _currentSnapshot;

	// WGSL Keywords
	private static readonly HashSet<string> Keywords =
	[
		"fn", "var", "let", "const", "struct", "if", "else", "for", "while", "loop",
		"return", "break", "continue", "discard", "switch", "case", "default",
		"true", "false", "override"
	];

	// WGSL Built-in Types
	private static readonly HashSet<string> BuiltInTypes =
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
		"texture_depth_cube_array", "texture_depth_multisampled_2d"
	];

	// WGSL Stage Attributes
	private static readonly HashSet<string> StageAttributes =
	[
		"vertex", "fragment", "compute"
	];

	// WGSL Built-in Attributes
	private static readonly HashSet<string> BuiltInAttributes =
	[
		"position", "vertex_index", "instance_index", "front_facing",
		"frag_depth", "local_invocation_id", "local_invocation_index",
		"global_invocation_id", "workgroup_id", "num_workgroups",
		"sample_index", "sample_mask"
	];

	public WgslErrorTagger(ITextBuffer buffer)
	{
		_buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
		_currentSnapshot = buffer.CurrentSnapshot;
		_buffer.Changed += OnBufferChanged;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
		{
			yield break;
		}

		var snapshot = spans[0].Snapshot;
		var text = snapshot.GetText();

		foreach (var error in FindErrors(text, snapshot))
		{
			yield return error;
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> FindErrors(string text, ITextSnapshot snapshot)
	{
		// 1. Check for missing semicolons after statements
		foreach (var error in CheckMissingSemicolons(text, snapshot))
			yield return error;

		// 2. Check for unmatched braces
		foreach (var error in CheckUnmatchedBraces(text, snapshot))
			yield return error;

		// 3. Check for invalid attribute usage
		foreach (var error in CheckInvalidAttributes(text, snapshot))
			yield return error;

		// 4. Check for invalid stage functions
		foreach (var error in CheckStageFunctions(text, snapshot))
			yield return error;

		// 5. Check for undefined types
		foreach (var error in CheckUndefinedTypes(text, snapshot))
			yield return error;

		// 6. Check for duplicate binding/group attributes
		foreach (var error in CheckDuplicateBindings(text, snapshot))
			yield return error;

		// 7. Check for invalid workgroup_size
		foreach (var error in CheckWorkgroupSize(text, snapshot))
			yield return error;

		// 8. Check for invalid variable declarations
		foreach (var error in CheckVariableDeclarations(text, snapshot))
			yield return error;
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckMissingSemicolons(string text, ITextSnapshot snapshot)
	{
		// Check for statements that should end with semicolons
		// We need to find: var/let/const/return/break/continue/discard followed by content
		// that doesn't end with a semicolon before a newline, closing brace, or EOF
		// NOTE: Struct members use commas, not semicolons, so we must skip checking inside struct definitions

		var statementKeywords = new[] { "var", "let", "const", "return", "break", "continue", "discard" };

		foreach (var keyword in statementKeywords)
		{
			// Find all occurrences of the keyword
			var pattern = $@"\b{keyword}\b";
			var matches = Regex.Matches(text, pattern);

			foreach (Match match in matches)
			{
				// Skip if inside a comment
				if (IsInsideComment(text, match.Index))
					continue;

				// Skip if inside a struct definition (struct members use commas, not semicolons)
				if (IsInsideStruct(text, match.Index))
					continue;

				// Find the end of this statement
				var searchPos = match.Index + match.Length;

				// Skip whitespace after keyword
				while (searchPos < text.Length && char.IsWhiteSpace(text[searchPos]) && text[searchPos] != '\n')
					searchPos++;

				// Find where the statement ends (semicolon, newline, closing brace, opening brace, or EOF)
				var foundSemicolon = false;
				var foundTerminator = false;
				var foundOpeningBrace = false;
				var statementEnd = searchPos;

				while (statementEnd < text.Length)
				{
					var ch = text[statementEnd];

					if (ch == ';')
					{
						foundSemicolon = true;
						break;
					}
					else if (ch == '\n' || ch == '}')
					{
						foundTerminator = true;
						break;
					}
					else if (ch == '{')
					{
						// For struct/fn definitions, this is OK - no semicolon needed
						foundOpeningBrace = true;
						break;
					}

					statementEnd++;
				}

				// If we reached EOF, that's also a terminator
				if (statementEnd >= text.Length)
				{
					foundTerminator = true;
					statementEnd = text.Length;
				}

				// Only report error if:
				// 1. We found a terminator (not a semicolon or opening brace)
				// 2. There was actual content after the keyword
				// 3. The content doesn't end with a semicolon
				if (!foundSemicolon && !foundOpeningBrace && foundTerminator && statementEnd > searchPos)
				{
					// Make sure there's actual content (not just whitespace)
					var contentLength = statementEnd - searchPos;
					if (contentLength > 0 && contentLength <= text.Length - searchPos)
					{
						var content = text.Substring(searchPos, contentLength).Trim();
						if (content.Length > 0)
						{
							// Position error at the last non-whitespace character
							var errorPos = statementEnd - 1;
							while (errorPos > searchPos && errorPos < text.Length && char.IsWhiteSpace(text[errorPos]))
								errorPos--;

							// Safely create span with bounds checking
							var span = TryCreateSpan(snapshot, errorPos, 1);
							if (span.HasValue)
							{
								var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
									"Missing semicolon at end of statement");
								yield return new TagSpan<IErrorTag>(span.Value, errorTag);
							}
						}
					}
				}
			}
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckUnmatchedBraces(string text, ITextSnapshot snapshot)
	{
		var stack = new Stack<(char brace, int position)>();
		var commentDepth = 0;
		var inLineComment = false;
		var inString = false;

		for (var i = 0; i < text.Length; i++)
		{
			var ch = text[i];
			var next = i + 1 < text.Length ? text[i + 1] : '\0';

			// Handle comments
			if (!inString)
			{
				if (ch == '/' && next == '/')
				{
					inLineComment = true;
					i++;
					continue;
				}

				if (ch == '/' && next == '*')
				{
					commentDepth++;
					i++;
					continue;
				}

				if (ch == '*' && next == '/')
				{
					commentDepth = Math.Max(0, commentDepth - 1);
					i++;
					continue;
				}

				if (ch == '\n')
					inLineComment = false;
			}

			if (commentDepth > 0 || inLineComment)
				continue;

			// Handle strings
			if (ch == '"')
			{
				inString = !inString;
				continue;
			}

			if (inString)
				continue;

			// Check braces
			if (ch == '{' || ch == '(' || ch == '[')
			{
				stack.Push((ch, i));
			}
			else if (ch == '}' || ch == ')' || ch == ']')
			{
				var expected = ch switch
				{
					'}' => '{',
					')' => '(',
					']' => '[',
					_ => '\0'
				};

				if (stack.Count == 0)
				{
					var span = TryCreateSpan(snapshot, i, 1);
					if (span.HasValue)
					{
						var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
							$"Unmatched closing '{ch}'");
						yield return new TagSpan<IErrorTag>(span.Value, errorTag);
					}
				}
				else
				{
					var (opening, _) = stack.Pop();
					if (opening != expected)
					{
						var span = TryCreateSpan(snapshot, i, 1);
						if (span.HasValue)
						{
							var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
								$"Mismatched brace: expected '{GetClosingBrace(opening)}' but found '{ch}'");
							yield return new TagSpan<IErrorTag>(span.Value, errorTag);
						}
					}
				}
			}
		}

		// Report unclosed braces
		while (stack.Count > 0)
		{
			var (brace, position) = stack.Pop();
			var span = TryCreateSpan(snapshot, position, 1);
			if (span.HasValue)
			{
				var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
					$"Unclosed '{brace}'");
				yield return new TagSpan<IErrorTag>(span.Value, errorTag);
			}
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckInvalidAttributes(string text, ITextSnapshot snapshot)
	{
		// Check for @attribute usage
		var attributePattern = @"@(\w+)(?:\(([^)]*)\))?";
		var matches = Regex.Matches(text, attributePattern);

		foreach (Match match in matches)
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var attrName = match.Groups[1].Value;
			var validAttributes = new HashSet<string>(StageAttributes);
			validAttributes.UnionWith(BuiltInAttributes);
			validAttributes.UnionWith([ "binding", "group", "location", "builtin",
				"interpolate", "invariant", "workgroup_size", "size", "align" ]);

			if (!validAttributes.Contains(attrName))
			{
				var span = TryCreateSpan(snapshot, match.Index, match.Length);
				if (span.HasValue)
				{
					var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
						$"Unknown attribute '@{attrName}'");
					yield return new TagSpan<IErrorTag>(span.Value, errorTag);
				}
			}
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckStageFunctions(string text, ITextSnapshot snapshot)
	{
		// Check for stage function declarations
		var functionPattern = @"@(vertex|fragment|compute)\s+fn\s+(\w+)";
		var matches = Regex.Matches(text, functionPattern);

		foreach (Match match in matches)
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var stage = match.Groups[1].Value;
			var _ = match.Groups[2].Value;

			// Check for proper return type based on stage
			var functionStart = match.Index;
			var functionEnd = FindMatchingBrace(text, functionStart);

			if (functionEnd > functionStart)
			{
				var functionText = text.Substring(functionStart, functionEnd - functionStart);

				// Vertex functions should return @builtin(position)
				// This can be either directly in the return type or in the function signature
				// Also check if it returns a struct type that has @builtin(position)
				if (stage == "vertex")
				{
					var hasBuiltinPosition = functionText.Contains("@builtin(position)");

					if (!hasBuiltinPosition)
					{
						// Check if the function returns a user-defined type that might have @builtin(position)
						var returnTypeMatch = Regex.Match(functionText, @"->\s*(\w+)");
						if (returnTypeMatch.Success)
						{
							var returnType = returnTypeMatch.Groups[1].Value;
							// Check if this type is a struct defined elsewhere in the file with @builtin(position)
							var structPattern = $@"struct\s+{returnType}\s*{{[^}}]*@builtin\(position\)[^}}]*}}";
							hasBuiltinPosition = Regex.IsMatch(text, structPattern, RegexOptions.Singleline);
						}
					}

					if (!hasBuiltinPosition)
					{
						var span = TryCreateSpan(snapshot, match.Index, match.Length);
						if (span.HasValue)
						{
							var errorTag = new ErrorTag(PredefinedErrorTypeNames.Warning,
								"Vertex shader should return a structure with @builtin(position)");
							yield return new TagSpan<IErrorTag>(span.Value, errorTag);
						}
					}
				}

				// Fragment functions should return a location or no return
				if (stage == "fragment")
				{
					var hasReturn = Regex.IsMatch(functionText, @"->\s*@location\(\d+\)");
					var returnsVoid = Regex.IsMatch(functionText, @"->\s*\(\s*\)") ||
									  !functionText.Contains("->");

					if (!hasReturn && !returnsVoid)
					{
						var span = TryCreateSpan(snapshot, match.Index, match.Length);
						if (span.HasValue)
						{
							var errorTag = new ErrorTag(PredefinedErrorTypeNames.Warning,
								"Fragment shader return type should have @location attribute or be void");
							yield return new TagSpan<IErrorTag>(span.Value, errorTag);
						}
					}
				}
			}
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckUndefinedTypes(string text, ITextSnapshot snapshot)
	{
		// Extract user-defined struct names
		var structPattern = @"struct\s+(\w+)";
		var structMatches = Regex.Matches(text, structPattern);
		var userDefinedTypes = new HashSet<string>(BuiltInTypes);

		foreach (Match match in structMatches)
		{
			if (!IsInsideComment(text, match.Index))
				userDefinedTypes.Add(match.Groups[1].Value);
		}

		// Check type usage in variable declarations
		var typeUsagePattern = @"(?:var|let|const)\s+\w+\s*:\s*(\w+)";
		var typeMatches = Regex.Matches(text, typeUsagePattern);

		foreach (Match match in typeMatches)
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var typeName = match.Groups[1].Value;
			if (!userDefinedTypes.Contains(typeName) && !Keywords.Contains(typeName))
			{
				var typeStart = match.Groups[1].Index;
				var span = TryCreateSpan(snapshot, typeStart, typeName.Length);
				if (span.HasValue)
				{
					var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
						$"Undefined type '{typeName}'");
					yield return new TagSpan<IErrorTag>(span.Value, errorTag);
				}
			}
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckDuplicateBindings(string text, ITextSnapshot snapshot)
	{
		var bindingPattern = @"@binding\((\d+)\)\s*@group\((\d+)\)";
		var matches = Regex.Matches(text, bindingPattern);
		var seenBindings = new HashSet<string>();

		foreach (Match match in matches)
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var binding = match.Groups[1].Value;
			var group = match.Groups[2].Value;
			var key = $"{group}:{binding}";

			if (seenBindings.Contains(key))
			{
				var span = TryCreateSpan(snapshot, match.Index, match.Length);
				if (span.HasValue)
				{
					var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
						$"Duplicate binding: @binding({binding}) @group({group})");
					yield return new TagSpan<IErrorTag>(span.Value, errorTag);
				}
			}
			else
			{
				seenBindings.Add(key);
			}
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckWorkgroupSize(string text, ITextSnapshot snapshot)
	{
		var workgroupPattern = @"@workgroup_size\((\d+)(?:\s*,\s*(\d+))?(?:\s*,\s*(\d+))?\)";
		var matches = Regex.Matches(text, workgroupPattern);

		foreach (Match match in matches)
		{
			if (IsInsideComment(text, match.Index))
				continue;

			var x = int.Parse(match.Groups[1].Value);
			var y = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
			var z = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 1;

			// WGSL limits: each dimension must be >= 1 and <= 256
			// Total invocations must be <= 256
			if (x < 1 || x > 256 || y < 1 || y > 256 || z < 1 || z > 256)
			{
				var span = TryCreateSpan(snapshot, match.Index, match.Length);
				if (span.HasValue)
				{
					var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
						"Workgroup size dimensions must be between 1 and 256");
					yield return new TagSpan<IErrorTag>(span.Value, errorTag);
				}
			}
			else if (x * y * z > 256)
			{
				var span = TryCreateSpan(snapshot, match.Index, match.Length);
				if (span.HasValue)
				{
					var errorTag = new ErrorTag(PredefinedErrorTypeNames.Warning,
						$"Total workgroup invocations ({x * y * z}) exceeds recommended limit of 256");
					yield return new TagSpan<IErrorTag>(span.Value, errorTag);
				}
			}
		}
	}

	private IEnumerable<ITagSpan<IErrorTag>> CheckVariableDeclarations(string text, ITextSnapshot snapshot)
	{
		// Check for var without type or initializer: var <name> followed by ; or newline
		var varPattern = @"\bvar\s+(\w+)\s*;";
		var matches = Regex.Matches(text, varPattern);

		foreach (Match match in matches)
		{
			if (IsInsideComment(text, match.Index))
				continue;

			// Check if this var declaration has a type annotation or initializer
			var varContent = match.Value;
			// If it's just "var name;" without : or =, it's an error
			if (!varContent.Contains(":") && !varContent.Contains("="))
			{
				var spanLength = match.Groups[1].Index + match.Groups[1].Length - match.Index;
				var span = TryCreateSpan(snapshot, match.Index, spanLength);
				if (span.HasValue)
				{
					var errorTag = new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
						"Variable declaration must have either a type annotation or an initializer");
					yield return new TagSpan<IErrorTag>(span.Value, errorTag);
				}
			}
		}
	}

	// Helper methods

	/// <summary>
	/// Safely creates a SnapshotSpan with bounds checking.
	/// Returns null if the span would be invalid.
	/// </summary>
	private SnapshotSpan? TryCreateSpan(ITextSnapshot snapshot, int start, int length)
	{
		// Validate snapshot
		if (snapshot == null)
			return null;

		// Validate all bounds
		if (start < 0 || start >= snapshot.Length)
			return null;

		if (length < 1 || start + length > snapshot.Length)
			return null;

		return new SnapshotSpan(snapshot, start, length);
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

	private bool IsInsideStruct(string text, int position)
	{
		// Check if we're inside a struct definition
		// Look backwards from position to find if we're between "struct" and its closing brace
		var beforeText = text.Substring(0, position);

		// Find the last struct keyword before our position
		var lastStructMatch = Regex.Matches(beforeText, @"\bstruct\b").Cast<Match>().LastOrDefault();
		if (lastStructMatch == null)
			return false;

		// Count braces between the struct keyword and our position
		var textBetween = beforeText.Substring(lastStructMatch.Index);
		var openBraces = textBetween.Count(c => c == '{');
		var closeBraces = textBetween.Count(c => c == '}');

		// If there are more open than close braces, we're inside the struct
		return openBraces > closeBraces;
	}

	private char GetClosingBrace(char opening) => opening switch
	{
		'{' => '}',
		'(' => ')',
		'[' => ']',
		_ => '\0'
	};

	private int FindMatchingBrace(string text, int start)
	{
		var depth = 0;
		var foundFirst = false;

		for (var i = start; i < text.Length; i++)
		{
			if (text[i] == '{')
			{
				foundFirst = true;
				depth++;
			}
			else if (text[i] == '}')
			{
				depth--;
				if (foundFirst && depth == 0)
					return i;
			}
		}

		return -1;
	}

	private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
	{
		_currentSnapshot = e.After;
		var span = new SnapshotSpan(e.After, 0, e.After.Length);
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
	}
}
