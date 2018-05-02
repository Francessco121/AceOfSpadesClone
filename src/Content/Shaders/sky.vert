#version 400 core

/* 	File: sky.vert
	Author: Ethan Lafrenais
	Type: vertex shader
*/

in vec3 position;
out vec3 fragPosition;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main()
{
	gl_Position = projectionMatrix * viewMatrix * vec4(position, 1.0);
	fragPosition = position;
}