#version 400 core

/* 	File: depthDebug.frag
	Author: Ethan Lafrenais
	Type: fragment shader
*/

in vec2 fragTexCoords;

out vec4 color;

uniform sampler2D depthMap;
uniform bool linearize;
uniform float nearPlane;
uniform float farPlane;

float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // Back to NDC 
    return (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - z * (farPlane - nearPlane));
}

void main()
{
	float depth = texture(depthMap, fragTexCoords).r;
	if (linearize)
		color = vec4(vec3(LinearizeDepth(depth) / farPlane), 1.0);
	else
		color = vec4(vec3(depth), 1.0);
}