// WGSL Linting Test File
// This file demonstrates all linting rules

// ===== VALID CODE =====

struct VertexInput {
    @location(0) position: vec3<f32>,
    @location(1) normal: vec3<f32>,
};

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) worldPos: vec3<f32>,
};

@vertex
fn vs_main(input: VertexInput) -> VertexOutput {
    var output: VertexOutput;
    output.position = vec4<f32>(input.position, 1.0);
    output.worldPos = input.position;
    return output;
}

@fragment
fn fs_main(input: VertexOutput) -> @location(0) vec4<f32> {
    return vec4<f32>(1.0, 0.0, 0.0, 1.0);
}

@compute @workgroup_size(8, 8, 1)
fn cs_main(@builtin(global_invocation_id) global_id: vec3<u32>) {
    // Compute shader code
    let index: u32 = global_id.x;
}

// ===== ERROR EXAMPLES (Uncomment to see linting) =====

// 1. Missing Semicolon
// fn test_semicolon() {
//     var x: f32 = 1.0
//     let y: i32 = 2
//     return
// }

// 2. Unmatched Braces
// fn test_braces() {
//     if (true) {
//         var x = 1.0;
//     // Missing closing brace
// }

// 3. Invalid Attribute
// @invalid_attribute
// fn test_invalid() {}

// 4. Undefined Type
// fn test_undefined_type() {
//     var x: InvalidType = 0;
// }

// 5. Duplicate Bindings
// @group(0) @binding(0)
// var<uniform> data1: f32;
// @group(0) @binding(0)  // Error: Duplicate!
// var<uniform> data2: f32;

// 6. Invalid Workgroup Size
// @compute @workgroup_size(300, 1, 1)  // Error: > 256
// fn test_workgroup() {}

// @compute @workgroup_size(16, 16, 2)  // Warning: 16*16*2 = 512 > 256
// fn test_workgroup2() {}

// 7. Variable Without Type or Initializer
// fn test_var() {
//     var x;  // Error: needs type or initializer
// }

// 8. Stage Function Issues
// @vertex
// fn test_vertex() -> vec4<f32> {  // Warning: should return @builtin(position)
//     return vec4<f32>(0.0);
// }

// @fragment  
// fn test_fragment() -> vec4<f32> {  // Warning: should have @location
//     return vec4<f32>(0.0);
// }

// ===== MORE VALID EXAMPLES =====

// All built-in types recognized
fn test_types() {
    var a: bool = true;
    var b: i32 = 1;
    var c: u32 = 1u;
    var d: f32 = 1.0;
    var e: f16 = 1.0h;
    
    var v2: vec2<f32> = vec2<f32>(1.0, 2.0);
    var v3: vec3<f32> = vec3<f32>(1.0, 2.0, 3.0);
    var v4: vec4<f32> = vec4<f32>(1.0, 2.0, 3.0, 4.0);
    
    var m2: mat2x2<f32> = mat2x2<f32>(1.0, 0.0, 0.0, 1.0);
    var m3: mat3x3<f32> = mat3x3<f32>();
    var m4: mat4x4<f32> = mat4x4<f32>();
}

// Proper bindings
@group(0) @binding(0)
var<uniform> uniforms: vec4<f32>;

@group(0) @binding(1)
var texSampler: sampler;

@group(0) @binding(2)
var tex: texture_2d<f32>;

// Proper workgroup sizes
@compute @workgroup_size(64)
fn compute1() {}

@compute @workgroup_size(8, 8)
fn compute2() {}

@compute @workgroup_size(4, 4, 4)
fn compute3() {}
