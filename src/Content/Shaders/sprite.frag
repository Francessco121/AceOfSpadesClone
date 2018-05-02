#version 400 core

/* 	File: sprite.frag
	Author: Ethan Lafrenais
	Type: fragment shader
*/

in vec2 fragUVCoords;
in vec4 overlayColor;

out vec4 color;

uniform sampler2D spriteTex;

//uniform float outline;
//uniform float offsetDamp;

/*vec4 outline()
{
	vec2 Offset = (1.0 / (textureSize(spriteTex, 0) * 0.5f)) * 0.002;

	vec4 n = texture(spriteTex, vec2(fragUVCoords.x, fragUVCoords.y - Offset.y));
	vec4 e = texture(spriteTex, vec2(fragUVCoords.x + Offset.x, fragUVCoords.y));
	vec4 s = texture(spriteTex, vec2(fragUVCoords.x, fragUVCoords.y + Offset.y));
	vec4 w = texture(spriteTex, vec2(fragUVCoords.x - Offset.x, fragUVCoords.y));

	vec4 nw = texture(spriteTex, vec2(fragUVCoords.x - Offset.x, fragUVCoords.y - Offset.y));
	vec4 ne = texture(spriteTex, vec2(fragUVCoords.x + Offset.x, fragUVCoords.y - Offset.y));
	vec4 se = texture(spriteTex, vec2(fragUVCoords.x + Offset.x, fragUVCoords.y + Offset.y));
	vec4 sw = texture(spriteTex, vec2(fragUVCoords.x - Offset.x, fragUVCoords.y + Offset.y));

	float GrowedAlpha = overlayColor.a;
	GrowedAlpha = mix(GrowedAlpha, 1.0, s.a);
	GrowedAlpha = mix(GrowedAlpha, 1.0, w.a);
	GrowedAlpha = mix(GrowedAlpha, 1.0, n.a);
	GrowedAlpha = mix(GrowedAlpha, 1.0, e.a);
	GrowedAlpha = mix(GrowedAlpha, 1.0, nw.a);
	GrowedAlpha = mix(GrowedAlpha, 1.0, ne.a);
	GrowedAlpha = mix(GrowedAlpha, 1.0, se.a);
	GrowedAlpha = mix(GrowedAlpha, 1.0, sw.a);

	vec4 OutlineColorWithNewAlpha = vec4(0, 0, 0, 1);
	OutlineColorWithNewAlpha.a = GrowedAlpha;
	vec4 CharColor = overlayColor * texture(spriteTex, fragUVCoords);
	
	return mix(OutlineColorWithNewAlpha, CharColor, CharColor.a);
}*/

void main()
{
	color = overlayColor * texture(spriteTex, fragUVCoords);
}