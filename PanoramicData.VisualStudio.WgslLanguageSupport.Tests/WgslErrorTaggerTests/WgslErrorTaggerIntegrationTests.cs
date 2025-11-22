using AwesomeAssertions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger integration scenarios.
/// </summary>
public class WgslErrorTaggerIntegrationTests
{
	[Fact]
	public void GetTags_MultipleErrors_ShouldReturnAllErrors()
	{
		// Arrange
		var code = @"
var x
@invalid fn test(
struct MyStruct
@binding(0) @group(0) var tex1: texture_2d<f32>;
@binding(0) @group(0) var tex2: texture_2d<f32>;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = (tags.Count > 1).Should().BeTrue();
	}

	[Fact]
	public void GetTags_ValidWgslShader_ShouldReturnNoErrors()
	{
		// Arrange
		var code = @"
struct VertexOutput {
	@builtin(position) position: vec4f,
	@location(0) color: vec4f
}

@vertex
fn vs_main(@builtin(vertex_index) vertexIndex: u32) -> VertexOutput {
	var output: VertexOutput;
	output.position = vec4f(0.0, 0.0, 0.0, 1.0);
	output.color = vec4f(1.0, 0.0, 0.0, 1.0);
	return output;
}

@fragment
fn fs_main(input: VertexOutput) -> @location(0) vec4f {
	return input.color;
}
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		// This valid shader should have no errors or warnings
		// Use Assert.Empty instead of Assert.Equal to comply with xUnit analyzer
		tags.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_BlockCommentSpanningMultipleLines_ShouldNotReturnErrors()
	{
		// Arrange
		var code = @"
/*
var unclosed
@invalid
{{{
*/
var valid: i32;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().BeEmpty();
	}
}
