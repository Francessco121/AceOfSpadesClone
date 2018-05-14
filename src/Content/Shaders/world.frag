#version 400 core

#define MAX_LIGHTS 64
#define FOG_LOW 0
#define FOG_MED 1
#define FOG_HIGH 2
#define LIGHT_DIRECTIONAL 0
#define LIGHT_POINT 1
#define LIGHT_SPOT 2

/* 	File: world.frag
	Author: Ethan Lafrenais
	Type: fragment shader
*/

in vec4 fragPosition;
in vec4 fragSkyPosition;
in vec4 fragColor;
in vec3 fragSurfaceNormal;
in vec2 fragLighting;
in vec4 fragShadowPosition;

layout(location = 0) out vec4 finalColor;

// Lighting
uniform bool skipLight;
uniform vec3 lightPosition[MAX_LIGHTS];
uniform vec3 lightDirection[MAX_LIGHTS];
uniform vec3 lightColor[MAX_LIGHTS];
uniform vec3 attenuation[MAX_LIGHTS];
uniform float lightPower[MAX_LIGHTS];
uniform float lightRadius[MAX_LIGHTS];
uniform int lightTypes[MAX_LIGHTS];
uniform int numLights;
uniform vec3 cameraPosition;
uniform float ambientIntensity;
uniform float lightFalloff;
uniform float specularPower;
uniform float specularIntensity;
uniform vec4 colorOverlay;

// Shadows
uniform int pcfSamples;
uniform float shadowTexelMultiplier;
uniform float shadowBias;
uniform bool renderShadows;
uniform float shadowVisibility;

// Fog
uniform bool fogEnabled;
uniform vec3 fogColor;
uniform int fogQuality;
uniform float fogDensity;
uniform float fogGradient;
uniform float fogMin;
uniform float fogMax;
uniform float skyMapOffset;

// Matrices
uniform mat4 invProjectionMatrix;
uniform mat4 invViewMatrix;
uniform mat4 viewMatrix;

// Textures
uniform sampler2D shadowMap;
uniform sampler2D skyMap;

/*
 * Applies gamma correction to the
 * specified color.
*/
vec4 gamma(in vec4 color)
{
    return pow(color, vec4(0.5));
}

/*
 * Calculates the current fragments position
 * projected onto the sky sphere.
*/
vec2 calculateSkyFragCoords()
{
	vec3 projCoords = fragSkyPosition.xyz / fragSkyPosition.w;
	projCoords = projCoords * 0.5 + 0.5;
	
	return projCoords.xy;
}

/*
 * Calculates the amount of shadowing for
 * the current fragment.
*/
float calculateShadow()
{
	// Get the projected coords of the shadow
	vec3 projCoords = fragShadowPosition.xyz / fragShadowPosition.w;
	projCoords = projCoords * 0.5 + 0.5;
	
	// If the fragment isn't on the shadow map
	// don't apply any shadowing
	if (projCoords.z > 1.0)
		return 0.0;
	
	// Calculate the current depth of the fragment
	float currentDepth = projCoords.z - shadowBias;
	
	if (pcfSamples < 2)
	{
		// Calculate shadow normally, without any PCF
		float sampleDepth = texture(shadowMap, projCoords.xy).r;
		return (currentDepth < sampleDepth ? 0.0 : 1.0) * shadowVisibility;
	}
	else
	{
		// Calculate shadow using PCF, this will sample the area around
		// the current texture coordinate on the shadowmap and combine
		// them for a smooth effect.
		int sampleRadius = pcfSamples / 2;
		int axisSamples = pcfSamples + 1;
		
		float shadowVis = 0.0;
		vec2 texelSize = shadowTexelMultiplier / textureSize(shadowMap, 0);
		
		for (int x = -sampleRadius; x <= sampleRadius; ++x)
		{
			for (int y = -sampleRadius; y <= sampleRadius; ++y)
			{
				float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
				shadowVis += currentDepth < pcfDepth ? 0.0 : 1.0;
			}
		}
			
		return (shadowVis / (axisSamples * axisSamples)) * shadowVisibility;
	}
}

/*
 * Calculates the specular light from a given light
 * to the current fragment.
*/
vec3 calculateSpecular(vec3 unitLightVector, vec3 unitVectorToCamera, vec3 specularColor, float specularPower, float specularIntensity)
{
	// Reflect the light across the surface normal
	vec3 reflectedLightDirection = normalize(reflect(-unitLightVector, fragSurfaceNormal));

	// Calculate specular
	float specularFactor = max(dot(unitVectorToCamera, reflectedLightDirection), 0.0);
	
	if (specularFactor > 0.0)
	{
		specularFactor = pow(specularFactor, specularPower);
		return specularColor * specularIntensity * specularFactor;
	}
	else
		return vec3(0.0);
}

/*
 * Applies the light to the given color, brightness, shading, and specular values.
*/
void applyLight(in int lightI, in vec3 unitVectorToCamera, inout vec3 color, inout float brightness, inout float shading,
	inout vec3 totalSpecular)
{
	// Get the type of light
	int lightType = lightTypes[lightI];

	// Calculate the vector from this vertex to the light
	vec3 lightVector = lightType != LIGHT_DIRECTIONAL ? lightPosition[lightI] - fragPosition.xyz : lightPosition[lightI];
	vec3 unitLightVector = normalize(lightVector);

	float lightBrightness = 0.0;

	// Handle positional lights
	if (lightType != LIGHT_DIRECTIONAL)
	{
		// Default brightness from light to the light's power
		lightBrightness = lightPower[lightI];

		// Handle individual light types
		if (lightType == LIGHT_SPOT)
		{
			vec3 lightDir = lightDirection[lightI];
			float radius = lightRadius[lightI];
			
			float ddot = dot(lightDir, unitLightVector);
			
			if (ddot >= 0.0)
			{
				ddot = clamp(ddot, -1.0, 1.0);
				float angle = acos(ddot);
				
				lightBrightness *= max(1.0 - (angle / radius), 0.0);
			}
			else
			{
				lightBrightness = 0;
			}
		}

		// Calculate positional-light attenuation
		float distToLight = length(lightVector);
		float attFactor = attenuation[lightI].x + (attenuation[lightI].y * distToLight) + (attenuation[lightI].z * distToLight * distToLight);

		lightBrightness /= attFactor;
	}
	// Handle directional lights
	else
	{
		// Multiply the intensity based on the angle of the light (normals facing light directly should get full brightness)
		float nDotl = dot(fragSurfaceNormal, unitLightVector);
		shading *= (nDotl + 1.0) * 0.5; // nDotl is [-1, 1], convert to [0, 2] and cut in half to go to [0, 1]

		// Calculate shadows
		float shadowFactor = renderShadows && nDotl > 0.0 ? calculateShadow() : 0.0;

		if (nDotl > 0.0 && nDotl < 0.1)
			shadowFactor *= (nDotl * 0.1);

		// For the upper half of lighting, use shadows to darken any normal lighting
		lightBrightness = fragLighting.y < 0.5
			? fragLighting.y
			: fragLighting.y * (1.0 - (shadowFactor * 0.25));

		// Apply global light falloff
		lightBrightness *= lightFalloff;
	}

	// Apply light color and brightness
	color = mix(color, color * lightColor[lightI], min(lightBrightness, 1.0));
	brightness += lightBrightness;

	// Apply specular highlights
	if (specularPower > 0.0 && specularIntensity > 0.0)
		totalSpecular += calculateSpecular(unitLightVector, unitVectorToCamera, lightColor[lightI], specularPower, specularIntensity);
}

/*
 * Calculates the visibilty of the current fragment.
*/
float calculateFogVisibility(vec4 worldPosition)
{
	// Get the distance of the vertex to the camera
	vec4 positionRelativeToCamera = viewMatrix * worldPosition;
	float dist = length(positionRelativeToCamera.xyz);
	
	// Calculate its visibility
	float visibility = exp(-pow((dist * fogDensity), fogGradient));
	visibility = clamp(visibility, 1.0 - fogMax, 1.0 - fogMin);
	
	return visibility;
}

/*
 * Blends the specified color with the current fog
 * based on the current fragment's position.
*/
vec4 applyFog(vec4 currentColor)
{
	// Calculate the visibility of the current fragment
	float visibility = calculateFogVisibility(fragPosition);

	// Apply the fog based on quality
	if (fogQuality == FOG_HIGH)
	{
		// Blend directly into snapshot of the current sky texture
		vec4 sky = texture(skyMap, calculateSkyFragCoords());
		return mix(sky, currentColor, visibility);
	}
	else if (fogQuality == FOG_MED)
	{
		// Attempt to blend into sky based on skyMap
		vec4 zenith = texture(skyMap, vec2(1.0, skyMapOffset));
		vec4 horizon = texture(skyMap, vec2(0.0, skyMapOffset));
		
		vec3 pointOnSphere = normalize(fragPosition.xyz);
		float a = pointOnSphere.y;
		
		return mix(mix(horizon, zenith, a), currentColor, visibility);
	}
	else
		// Apply basic fog
		return mix(vec4(fogColor, 1.0), currentColor, visibility);
}

/*
 * Entry point.
*/
void main()
{
	// Only process the texture if this is a legit part of the geometry
	if (fragColor.a == 0.0)
		discard;
		
	// Calculate the vector from the vertex to the camera
	vec3 unitVectorToCamera = normalize(cameraPosition - fragPosition.xyz);
	
	// Setup lighting and color variables
	float brightness = 0.0;
	vec3 color = fragColor.rgb * colorOverlay.rgb; // Default to emmisive color
	float alpha = fragColor.a * colorOverlay.a;
	float shading = 1.0;
	vec3 totalSpecular = vec3(0.0);

	// Calculate lighting
	if (skipLight)
	{
		brightness = 1.0;
	}
	else
	{
		// Apply each light
		for (int i = 0; i < numLights; i++)
		{
			applyLight(i, unitVectorToCamera, color, brightness, shading, totalSpecular);
		}

		// Apply final shading
		brightness *= 1.0 - (shading * 0.2);
	}

	// Apply ambient lighting
	brightness = max(brightness, ambientIntensity);

	// Apply AO
	brightness *= 1.0 - fragLighting.x;

	// Blend color, brightness, specular + attach alpha
	finalColor = vec4((color * brightness) + totalSpecular, alpha);
	
	// Apply fog if enabled
	if (fogEnabled) 
		finalColor = applyFog(finalColor);
}