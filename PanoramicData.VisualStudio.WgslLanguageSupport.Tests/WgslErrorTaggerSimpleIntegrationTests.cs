using AwesomeAssertions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests;

/// <summary>
/// Simple integration test without AwesomeAssertions to avoid formatting issues
/// </summary>
public class WgslErrorTaggerSimpleIntegrationTests
{
	[Fact]
	public void GetTags_ValidWgslShader_ShouldReturnNoErrors_SimpleAssertion()
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

		// Assert - use simple xUnit assertions to avoid AwesomeAssertions formatting issues
		tags.Should().NotBeNull();
		tags.Should().BeEmpty();
	}
}
