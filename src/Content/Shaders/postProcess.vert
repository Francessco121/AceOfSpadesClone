#version 400 core

/* 	File: postProcess.vert
	Author: Ethan Lafrenais
	Type: vertex shader
*/

in vec2 position;

out vec2 fragTexCoords;

uniform mat4 transformationMatrix;

void main()
{
	gl_Position = transformationMatrix * vec4(position, 0.0, 1.0) * 2.0 - 1.0;
	gl_Position.z = 0.0;
	
	fragTexCoords = position;
}