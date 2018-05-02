#version 400 core

/* 	File: gui.vert
	Author: Ethan Lafrenais
	Type: vertex shader
*/

in vec2 position;

out vec2 texCoords;

uniform mat4 transformationMatrix;
uniform bool flipY;

void main()
{
	gl_Position = transformationMatrix * vec4(position, 0.0, 1.0);
	texCoords = flipY
		? vec2((position.x + 1.0) / 2.0, 1.0 - (position.y + 1.0) / 2.0)
		: vec2((position.x + 1.0) / 2.0, (position.y + 1.0) / 2.0);
}