//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// Resources:
// http://library.nd.edu/documents/arch-lib/Unity/SB/Assets/SampleScenes/Shaders/Skybox-Procedural.shader
//

Shader "WeatherMaker/WeatherMakerSkySphereShader"
{
	Properties
	{
		_MainTex ("Day Texture", 2D) = "blue" {}
		_DawnDuskTex ("Dawn/Dusk Texture", 2D) = "orange" {}
		_NightTex ("Night Texture", 2D) = "black" {}
		_DayMultiplier ("Day Multiplier", Range(0, 3)) = 1
		_DawnDuskMultiplier ("Dawn/Dusk Multiplier", Range(0, 1)) = 0
		_NightMultiplier ("Night Multiplier", Range(0, 3)) = 0
		_NightVisibilityThreshold ("Night Visibility Threshold", Range(0, 1)) = 0
		_NightIntensity("Night Intensity", Range(0, 32)) = 2
		_NightTwinkleSpeed("Night Twinkle Speed", Range(0, 100)) = 16
		_NightTwinkleVariance("Night Twinkle Variance", Range(0, 10)) = 1
		_NightTwinkleMinimum("Night Twinkle Minimum Color", Range(0, 1)) = 0.02
		_NightTwinkleRandomness("Night Twinkle Randomness", Range(0, 5)) = 0.15
		_SunNormal ("Sun Normal pointing to 0,0,0", Vector) = (0, 0, 0, 0)
		_SunColor ("Sun Color", Color) = (1, 1, 1, 1)
		_SunSize ("Sun Size", Range(0.01, 10)) = 0.02
		_SkyTintColor ("Sky tint color, procedural only", Color) = (0.5, 0.5, 0.5, 1)
		_SkyAtmosphereThickness ("Sky atmosphere thickness, procedural only", Range(0, 5)) = 1
		_GroundTintColor("Ground tint color, procedural only", Color) = (0.4, 0.4, 0.4)
		_FogColor("Fog Color", Color) = (1,1,1,1)
		_FogNoise("Fog Noise", 2D) = "white" {}
		_FogNoiseScale("Fog Noise Scale", Range(0.0, 1.0)) = 0.02
		_FogNoiseMultiplier("Fog Noise Multiplier", Range(0.01, 4.0)) = 1
		_FogNoiseVelocity("Fog Noise Velocity", Vector) = (0.1, 0.2, 0, 0)
		_FogCover("Fog Cover", Range(0.0, 1.0)) = 0.25
		_FogSharpness("Fog Sharpness", Range(0.0, 1.0)) = 0.015
		_FogNoiseHeightScale("Fog Noise Height Scale", Range(0.0, 1.0)) = 0.001
		_FogNoiseHeightMultiplier("Fog Noise Height Multiplier", Range(0.0, 5000.0)) = 200.0
		_FogWhispiness("Fog Whispiness", Range(0.0, 3.0)) = 1.0
		_FogWhispinessChangeFactor("Fog Whispiness Change Factor", Range(0.0, 1.0)) = 0.03
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient light multiplier", Range(0, 4)) = 1
	}
	SubShader
	{
		Tags{ "RenderType" = "Background" "IgnoreProjector" = "True" "Queue" = "Background" }
		Cull Off Lighting Off ZWrite Off

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
			#pragma multi_compile __ ENABLE_SUN_HIGH_QUALITY ENABLE_SUN_FAST
			#pragma multi_compile __ ENABLE_PROCEDURAL_TEXTURED_SKY ENABLE_PROCEDURAL_SKY
			#pragma multi_compile __ ENABLE_CLOUDS
			#pragma multi_compile __ ENABLE_NIGHT_TWINKLE
			#pragma multi_compile __ PER_PIXEL_LIGHTING
			
			#include "WeatherMakerFogShader.cginc"

			// controls how clouds look at horizon
			#define CLOUD_RAY_OFFSET 0.05

			// reduce scale of cloud noise
			#define SCALE_REDUCER 0.1

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 ray : NORMAL;

#if defined(ENABLE_PROCEDURAL_TEXTURED_SKY) || defined(ENABLE_PROCEDURAL_SKY)

				fixed4 vertexColor : COLOR0;

#endif

#if defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

				fixed4 sunColor : TEXCOORD1;

#endif

			};

			sampler2D _DawnDuskTex;
			float4 _DawnDuskTex_ST;
			sampler2D _NightTex;
			float4 _NightTex_ST;
			fixed _DayMultiplier;
			fixed _DawnDuskMultiplier;
			fixed _NightMultiplier;
			fixed _NightVisibilityThreshold;
			fixed _NightIntensity;
			fixed _NightTwinkleSpeed;
			fixed _NightTwinkleVariance;
			fixed _NightTwinkleMinimum;
			fixed _NightTwinkleRandomness;
			float3 _SunNormal;
			fixed _SunSize;
			fixed3 _SkyTintColor;
			float _SkyAtmosphereThickness;
			fixed3 _GroundTintColor;
			float _FogCover;
			float _FogSharpness;
			float _FogWhispiness;
			float _FogWhispinessChangeFactor;

			inline float hash(float n)
			{
				return frac(sin(n) * 43758.5453);
			}

			inline float CloudNoise(float2 x, float c)
			{
				/*
				x *= _FogNoiseScale;
				float2 p = floor(x);
				float2 f = frac(x);
				f = f * f * (3.0 - 2.0 * f);
				float n = p.x + p.y * 57.0;
				float res = lerp(lerp(hash(n + 0.0), hash(n + 1.0),f.x), lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y);
				return res;
				*/
				return tex2Dlod(_FogNoise, float4(x * _FogNoiseScale, 0, 0)) + c;
			}

			inline float CloudFBM(float2 p, float c)
			{
				/*
				float f = 0.0;
				f += 0.50000 * CloudNoise(p);
				p *= 2.02;
				f += 0.25000 * CloudNoise(p);
				p *= 2.03;
				f += 0.12500 * CloudNoise(p);
				p *= 2.01;
				f += 0.06250 * CloudNoise(p);
				p *= 2.04;
				f += 0.03125 * CloudNoise(p);
				return f * _FogNoiseMultiplier;
				*/

				// these two should add up to 1
				const float CloudMainWeight = 0.8;
				const float CloudWhispinessWeight = 0.2;

				// controls random deformation / shape of the clouds over time
				return
				(
					(CloudNoise(p, c) * CloudMainWeight) +
					((CloudNoise(p * _FogWhispinessChangeFactor * _Time.x, c) * _FogWhispiness * CloudWhispinessWeight))
				) * _FogNoiseMultiplier;
			}

#if defined(ENABLE_CLOUDS)

			fixed4 ComputeCloudColor(float3 ray, fixed3 lightColor, out float3 worldPos)
			{
				float3 normal = float3(0, 1, 0);
				float denom = dot(normal, ray);
				float3 cameraPos = float3(_WorldSpaceCameraPos.x, -10.0, _WorldSpaceCameraPos.z) * (4.0 / _FogHeight);

				// get base plane intersection
				float3 pos = float3(0, _FogHeight + 10.0, 0);
				if (denom < 0.00001)
				{
					// early exit, don't draw the bottom half of the clouds - performance gain of not calculating these pixels with the branch is greater than the below processing code
					// the fragment shader will render the bottom half in batches and will branch the same way in all of the GPU processors
					worldPos = float3(0.0, 0.0, 0.0);
					return fixed4(0.0, 0.0, 0.0, 0.0);
				}

				float t = dot(pos, normal) / denom;
				float2 velocity = (_FogNoiseVelocity * _Time.x);
				worldPos = cameraPos + (ray * t);
				float noise = tex2Dlod(_FogNoise, float4(((SCALE_REDUCER * worldPos.xz) + velocity) * _FogNoiseHeightScale, 0.0, 0.0));
				float randomHeight = lerp(-_FogNoiseHeightMultiplier, _FogNoiseHeightMultiplier, noise);

				// re-cast against the base height + random value to make the clouds look more realistic
				pos = float3(0, _FogHeight + randomHeight - cameraPos.y, 0);
				t = dot(pos, normal) / denom;

				// recalculate worldPos with new t value
				worldPos = cameraPos + (ray * t * SCALE_REDUCER);

				// calculate cloud values
				float2 xz = velocity + worldPos.xz;
				float c = lerp(0.19, 0.5, _FogCover);
				float f = CloudFBM(xz, c);
				c = f - (1.0 - c);
				f = saturate(1.0 - (pow(_FogSharpness, c)));

				fixed4 cloudColor;

				// light the clouds and blend clouds on top of sky
				lightColor += CalculateVertexColorWorldSpace(worldPos, 1.0 - weatherMaker_LightPosition[0].w).rgb;
				cloudColor.rgb = _FogColor.rgb * lightColor * (1.0 - (_FogDensity * f));
				cloudColor.a = saturate(f * (1.0 + (_FogDensity * 4.0)));

				return cloudColor;
			}

#endif

			fixed4 GetSunColor(v2f i)
			{

#if defined(ENABLE_SUN_HIGH_QUALITY)

#if defined(ENABLE_CLOUDS)

				return saturate(GetSunColorHighQuality(_SunNormal, i.sunColor, _SunSize, i.ray));

#else

				return GetSunColorHighQuality(_SunNormal, i.sunColor, _SunSize, i.ray);

#endif

#elif defined(ENABLE_SUN_FAST)

#if defined(ENABLE_CLOUDS)

				return saturate(GetSunColorFast(_SunNormal, i.sunColor, _SunSize, i.ray));

#else

				return GetSunColorFast(_SunNormal, i.sunColor, _SunSize, i.ray);

#endif

#else

				return fixed4(0, 0, 0, 0);

#endif

			}

			inline fixed4 GetNightColor(v2f i, fixed4 dayColor)
			{
				fixed4 nightColor = (tex2D(_NightTex, i.uv) * _NightMultiplier);
				fixed maxValue = max(nightColor.r, max(nightColor.g, nightColor.b));
				fixed dayMultiplier = (1.0 - max(dayColor.r, max(dayColor.g, dayColor.b)));
				dayMultiplier = pow(dayMultiplier, 64);

#if defined(ENABLE_NIGHT_TWINKLE)

				fixed twinkleRandom = _NightTwinkleRandomness * RandomFloat(i.ray * _Time.y);
				fixed twinkle = 1.0 + twinkleRandom + (_NightTwinkleVariance * sin(_NightTwinkleSpeed * _Time.w * maxValue));
				fixed noTwinkle = saturate(ceil(maxValue - _NightTwinkleMinimum));
				twinkle *= noTwinkle;
				twinkle += (1.0 - noTwinkle);
				nightColor *= twinkle;

#endif

				nightColor *= ceil(maxValue - _NightVisibilityThreshold) * _NightIntensity * dayMultiplier;

				return saturate(nightColor);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.vertex = UnityObjectToClipPos(float4(v.vertex.xyz, 0));
				//o.vertex.z = o.vertex.w;
				o.uv = v.uv; // TRANSFORM_TEX not supported

#if defined(ENABLE_PROCEDURAL_TEXTURED_SKY) || defined(ENABLE_PROCEDURAL_SKY)

				procedural_sky_vertex psv = CalculateSkyVertex(_SunNormal, _SunColor, _GroundTintColor, v.vertex.xyz, _SkyTintColor, _SkyAtmosphereThickness);
				o.ray = psv.ray;

#if defined(ENABLE_CLOUDS)

				o.vertexColor = psv.vertexColor * (1.0 - _FogDensity);

#else

				o.vertexColor = psv.vertexColor;

#endif

#if defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

				o.sunColor = psv.sunColor;

#endif

#elif defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

				o.ray = v.normal;
				o.sunColor = fixed4(_SunColor.rgb, GetSunLightSkyMultiplier(-_SunNormal, v.vertex.xyz));

#else

				o.ray = v.normal;

#endif

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 result;

#if defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

#if defined(ENABLE_CLOUDS)

			fixed sunMultiplier = i.sunColor.w * (1.0 - _FogDensity);

#else

			fixed sunMultiplier = i.sunColor.w;

#endif

#else

				fixed sunMultiplier = 1.0;

#endif

#if defined(ENABLE_PROCEDURAL_TEXTURED_SKY)

				fixed4 skyColor = i.vertexColor + GetSunColor(i);
				skyColor.a = 1.0 - _NightMultiplier;
				fixed4 dayColor = tex2D(_MainTex, i.uv) * _DayMultiplier;
				dayColor.rgb *= sunMultiplier;
				fixed4 dawnDuskColor = tex2D(_DawnDuskTex, i.uv);
				fixed4 dawnDuskColor2 = dawnDuskColor * _DawnDuskMultiplier;
				dawnDuskColor2.rgb *= sunMultiplier;
				dayColor += dawnDuskColor2;
				fixed4 nightColor = GetNightColor(i, dayColor);

				// hide night texture wherever dawn/dusk is opaque
				nightColor.rgb *= (1.0 - dawnDuskColor.a);

				// blend texture on top of sky
				result = ((dayColor * dayColor.a) + (skyColor * (1.0 - dayColor.a)));

				// blend previous result on top of night
				result = ((result * result.a) + (nightColor * (1.0 - result.a)));

#elif defined(ENABLE_PROCEDURAL_SKY)

				fixed4 nightColor = GetNightColor(i, i.vertexColor);
				result = (i.vertexColor * sunMultiplier) + nightColor + GetSunColor(i);

#else

				fixed4 dayColor = tex2D(_MainTex, i.uv) * sunMultiplier * _DayMultiplier;
				fixed4 dawnDuskColor = (tex2D(_DawnDuskTex, i.uv) * _DawnDuskMultiplier);
				fixed4 nightColor = GetNightColor(i, dayColor);
				result = (dayColor + dawnDuskColor + nightColor) + GetSunColor(i);

#endif

#if defined(ENABLE_CLOUDS)

				fixed3 lightColor;
				fixed3 sunColor = ((1.0 - weatherMaker_LightPosition[0].w) * weatherMaker_LightColor[0]).rgb;
				fixed4 cloudColor;
				float3 worldPos;

#if defined(ENABLE_PROCEDURAL_TEXTURED_SKY) || defined(ENABLE_PROCEDURAL_SKY)

				fixed skyColorWeight = saturate(_DawnDuskMultiplier * 0.8 + _NightMultiplier * 1.5);
				lightColor = lerp(sunColor * sunMultiplier, i.vertexColor, skyColorWeight);
				cloudColor = ComputeCloudColor(-float3(i.ray.x, i.ray.y - CLOUD_RAY_OFFSET, i.ray.z), lightColor, worldPos);

#else

				lightColor = sunColor * sunMultiplier;
				cloudColor = ComputeCloudColor(float3(i.ray.x, i.ray.y + CLOUD_RAY_OFFSET, i.ray.z), lightColor, worldPos);

#endif

				// blend clouds on top of sky
				result = ((cloudColor * cloudColor.a) + (result * (1.0 - cloudColor.a)));

#endif

				const fixed3 magic = fixed3(0.06711056, 100.00583715, 452.9829189);
				fixed gradient = frac(magic.z * frac(dot(i.uv * 100, magic.xy))) * 0.005;
				result.rgb -= gradient.xxx;

				return result;
			}

			ENDCG
		}
	}
}
