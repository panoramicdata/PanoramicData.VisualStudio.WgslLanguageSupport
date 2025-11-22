using AwesomeAssertions;

namespace PanoramicData.VisualStudio.WgslLanguageSupport.Tests.WgslErrorTaggerTests;

/// <summary>
/// Tests for WgslErrorTagger type validation.
/// </summary>
public class WgslErrorTaggerTypeValidationTests
{
	[Fact]
	public void GetTags_UndefinedType_ShouldReturnError()
	{
		// Arrange
		var code = "var x: UnknownType;";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		_ = tags.Should().NotBeEmpty();
		_ = tags.Any(t => t.Tag.ToolTipContent?.ToString()?.Contains("Undefined type 'UnknownType'") == true).Should().BeTrue();
	}

	[Fact]
	public void GetTags_BuiltInScalarTypes_ShouldNotReturnError()
	{
		// Arrange
		var code = @"
var a: bool;
var b: i32;
var c: u32;
var d: f32;
var e: f16;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var typeErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Undefined type") == true).ToList();
		_ = typeErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_BuiltInVectorTypes_ShouldNotReturnError()
	{
		// Arrange
		var code = @"
var a: vec2f;
var b: vec3i;
var c: vec4u;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var typeErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Undefined type") == true).ToList();
		_ = typeErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_BuiltInMatrixTypes_ShouldNotReturnError()
	{
		// Arrange
		var code = @"
var a: mat2x2f;
var b: mat3x3f;
var c: mat4x4f;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var typeErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Undefined type") == true).ToList();
		_ = typeErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_TextureTypes_ShouldNotReturnError()
	{
		// Arrange
		var code = @"
var a: texture_2d<f32>;
var b: texture_cube<f32>;
var c: sampler;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var typeErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Undefined type") == true).ToList();
		_ = typeErrors.Should().BeEmpty();
	}

	[Fact]
	public void GetTags_UserDefinedStruct_ShouldNotReturnError()
	{
		// Arrange
		var code = @"
struct MyStruct {
	x: f32,
	y: f32
}

var a: MyStruct;
";
		var buffer = TestHelpers.CreateMockTextBuffer(code);
		var tagger = new WgslErrorTagger(buffer);
		var spans = TestHelpers.CreateSpanCollection(buffer);

		// Act
		var tags = tagger.GetTags(spans).ToList();

		// Assert
		var typeErrors = tags.Where(t => t.Tag.ToolTipContent?.ToString()?.Contains("Undefined type 'MyStruct'") == true).ToList();
		_ = typeErrors.Should().BeEmpty();
	}
}
