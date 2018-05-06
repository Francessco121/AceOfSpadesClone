#version 400 core

/* 	File: postProcess.frag
	Author: Ethan Lafrenais
	Type: fragment shader
*/

in vec2 fragTexCoords;

out vec4 finalColor;

uniform sampler2D colorSampler;

uniform vec2 resolution;
uniform bool apply_fxaa;

const float FXAA_SPAN_MAX = 4.0;
const float FXAA_REDUCE_MUL = 1.0 / 8.0;
const float FXAA_REDUCE_MIN = 1.0 / 128.0;

vec4 textureSample(in vec2 coords)
{
	return texture(colorSampler, coords);
}

vec4 fxaa(in vec4 baseColor)
{
	// Get colors from texture
	vec2 inverse_resolution = vec2(1.0 / resolution.x, 1.0 / resolution.y);
	
	vec3 rgbNW = textureSample(fragTexCoords + (vec2(-1.0, -1.0)) * inverse_resolution).xyz;
	vec3 rgbNE = textureSample(fragTexCoords + (vec2(1.0, -1.0)) * inverse_resolution).xyz;
	vec3 rgbSW = textureSample(fragTexCoords + (vec2(-1.0, 1.0)) * inverse_resolution).xyz;
	vec3 rgbSE = textureSample(fragTexCoords + (vec2(1.0, 1.0)) * inverse_resolution).xyz;
	vec3 rgbM  = baseColor.xyz;
	
	// Calculate luma
	vec3 luma = vec3(0.299, 0.587, 0.114);
	float lumaNW = dot(rgbNW, luma);
	float lumaNE = dot(rgbNE, luma);
	float lumaSW = dot(rgbSW, luma);
	float lumaSE = dot(rgbSE, luma);
	float lumaM  = dot(rgbM,  luma);
	float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
	float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE))); 
	
	// Calculate dir
	vec2 dir;
	dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
	dir.y =  ((lumaNW + lumaSW) - (lumaNE + lumaSE));
	float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
	float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
	dir = min(
		vec2(FXAA_SPAN_MAX, FXAA_SPAN_MAX),
		max(vec2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX), dir * rcpDirMin)) 
		* inverse_resolution;
	
	// Calculate the final colors
	vec3 rgbA = 0.5 * (textureSample(fragTexCoords + dir * (1.0 / 3.0 - 0.5)).xyz 
		+ textureSample(fragTexCoords + dir * (2.0 / 3.0 - 0.5)).xyz);
	vec3 rgbB = rgbA * 0.5 + 0.25 * (textureSample(fragTexCoords + dir * -0.5).xyz 
		+ textureSample(fragTexCoords + dir * 0.5).xyz);
		
	float lumaB = dot(rgbB, luma);
	
	if((lumaB < lumaMin) || (lumaB > lumaMax))
		return vec4(rgbA, 1.0);
	else
		return vec4(rgbB, 1.0);
}

void main()
{
	// Get the texColor from the colorTexSampler
	vec4 texColor = textureSample(fragTexCoords);
	
	if (texColor.a == 0)
		discard;
		
	// FXAA
	if (apply_fxaa)
		texColor = fxaa(texColor);
	
	finalColor = texColor;
}