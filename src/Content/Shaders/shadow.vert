#version 400 core

/* 	File: shadow.vert
	Author: Ethan Lafrenais
	Type: vertex shader
*/

in vec3 position;

uniform mat4 transformationMatrix;
uniform mat4 lightSpaceMatrix;

void main()
{
	gl_Position = lightSpaceMatrix * transformationMatrix * vec4(position, 1.0);
}