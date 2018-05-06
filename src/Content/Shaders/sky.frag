#version 400
//#define PI 3.1415926535897932384626433832795

/* 	File: sky.frag
	Author: Ethan Lafrenais
	Type: fragment shader
*/

in vec3 fragPosition;

out vec4 finalColor;

uniform sampler2D skyMap;

uniform vec3 fogColor;
uniform float skyMapOffset;
uniform float skyMapFade;

uniform vec3 sunPosition;
uniform bool renderSun;

//const float lowerLimit = 0.0;
//const float upperLimit = 0.0;

//const vec4 skytop = vec4(0.0, 0.0, 1.0, 1.0);
//const vec4 skyhorizon = vec4(0.3294, 0.92157, 1.0, 1.0);

void main()
{
	vec3 pointOnSphere = normalize(fragPosition);
	float a = pointOnSphere.y;
	
	float sunVisibility = 0.0;
	if (renderSun)
	{
		vec3 unitSunPos = normalize(sunPosition);
		float distToSun = distance(pointOnSphere, unitSunPos);
		float invSunDist = 1.05 - distToSun;
		
		float sunHeight = unitSunPos.y;
		float invSunHeight = 1.0 - sunHeight;
		float sunPower = 20.0 - 5.0 * invSunHeight;

		if (invSunDist > 0.0)
			sunVisibility = pow(invSunDist, clamp(sunPower, 15.0, 40.0)) * clamp((sunHeight + 0.5) / 1.5, 0.0, 1.0);
			// sunVisibility = pow(invSunDist, 15) * clamp((a + 0.5) / 1.5, 0, 1);
	}
	
	vec4 zenith = texture(skyMap, vec2(1.0, skyMapOffset));
	vec4 horizon = texture(skyMap, vec2(0.0, skyMapOffset));
	
	finalColor = mix(mix(horizon, zenith, a), vec4(1.0, 1.0, 0.0, 1.0), sunVisibility);
	finalColor.a = skyMapFade;
}