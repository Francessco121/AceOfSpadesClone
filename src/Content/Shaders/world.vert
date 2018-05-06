#version 400 core

/* 	File: world.vert
	Author: Ethan Lafrenais
	Type: vertex shader
*/

in vec4 position;
in vec4 color;
in vec3 normal;

out vec4 fragPosition;
out vec4 fragSkyPosition;
out vec4 fragColor;
out vec3 fragSurfaceNormal;
out float fragAO;
out vec4 fragShadowPosition;

uniform mat4 transformationMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 lightSpaceMatrix;

/*
 * Entry point.
*/
void main()
{
	vec4 vertexPosition = vec4(position.xyz, 1.0);
	vec4 worldPosition = transformationMatrix * vertexPosition;
	vec4 viewPosition = viewMatrix * worldPosition;
	
	// Pass the world position to the fragment shader
	fragPosition = worldPosition;
	// Calculate the final vertex position
	gl_Position = projectionMatrix * viewPosition;
	
	// Calculate the sky position
	fragSkyPosition = projectionMatrix * viewMatrix * normalize(transformationMatrix * vertexPosition);
	// Calculate the shadow position
	fragShadowPosition = lightSpaceMatrix * transformationMatrix * vertexPosition;
	
	// Pass on the vertex color and vertex AO to 
	// the fragment shader
	fragColor = color;
	fragAO = position.w;
	
	// Calculate the normal of the vertex
	mat3 viewPos3 = inverse(mat3(transformationMatrix));
	fragSurfaceNormal = normalize(normal * viewPos3);
}