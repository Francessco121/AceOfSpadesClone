#version 400 core

/* 	File: forwardSprite.frag
	Author: Ethan Lafrenais
	Type: fragment shader
*/

in vec2 fragUVCoords;

out vec4 finalColor;

uniform vec4 overlayColor;
uniform sampler2D spriteTex; 

void main()
{
	finalColor = overlayColor * texture(spriteTex, fragUVCoords);
	
	// Alpha test
	if (finalColor.a < 0.95)
		discard;
}