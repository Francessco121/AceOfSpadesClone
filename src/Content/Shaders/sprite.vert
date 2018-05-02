#version 400 core

/* 	File: sprite.vert
	Author: Ethan Lafrenais
	Type: vertex shader
*/

in vec2 position;
in vec2 uv;
in vec4 color;

out vec2 fragUVCoords;
out vec4 overlayColor;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 transformationMatrix;

void main()
{
	fragUVCoords = vec2(uv.x, 1.0 - uv.y);
	overlayColor = color;
	
	gl_Position = projectionMatrix * viewMatrix * transformationMatrix * vec4(position.xy, 0.0, 1.0);
}