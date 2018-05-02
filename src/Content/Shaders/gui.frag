#version 400 core

/* 	File: gui.frag
	Author: Ethan Lafrenais
	Type: fragment shader
*/

in vec2 texCoords;

out vec4 finalColor;

uniform sampler2D guiTexture;
uniform vec4 overlayColor;

void main()
{
	finalColor = overlayColor * texture(guiTexture, texCoords);
}