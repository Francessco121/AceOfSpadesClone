#version 400 core

/* 	File: fullscreenQuad.vert
	Author: Ethan Lafrenais
	Type: vertex shader
	Desc: Simple full screen quad vertex shader
*/

in vec2 position;

out vec2 fragTexCoords;

uniform mat4 transformationMatrix;

void main()
{
	gl_Position = transformationMatrix * vec4(position, 0.0, 1.0);
	fragTexCoords = vec2((position.x + 1.0) / 2.0, (position.y + 1.0) / 2.0);
}