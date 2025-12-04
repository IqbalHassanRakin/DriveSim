//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Sky modes
    /// </summary>
    public enum WeatherMakeSkyMode
    {
        /// <summary>
        /// Textured - day, dawn/dusk and night are all done via textures
        /// </summary>
        Textured = 0,

        /// <summary>
        /// Procedural sky - day and dawn/dusk textures are overlaid on top of procedural sky, night texture is used as is
        /// </summary>
        ProceduralTextured,

        /// <summary>
        /// Procedural sky - day, dawn/dusk textures are ignored, night texture is used as is
        /// </summary>
        Procedural
    }

    /// <summary>
    /// Sun modes
    /// </summary>
    public enum WeatherMakerSunMode
    {
        /// <summary>
        /// No sun
        /// </summary>
        Disabled,

        /// <summary>
        /// High quality sun
        /// </summary>
        HighQuality,

        /// <summary>
        /// Fast to render sun
        /// </summary>
        Fast
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WeatherMakerSkySphereScript : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Weather maker script")]
        public WeatherMakerScript WeatherScript;

        [Tooltip("The camera the sky sphere should follow")]
        public Camera _camera;

        [Tooltip("The sky mode. 'Textured' uses a texture for day, dawn/dusk and night. " +
            "'Procedural textured' combines a procedural sky with the day and dawn/dusk textures using alpha, and uses the night texture as is. " +
            "'Procedural' uses the night texture as is and does everything else procedurally.")]
        public WeatherMakeSkyMode SkyMode = WeatherMakeSkyMode.Textured;

        [Tooltip("The sun mode. Disabled, HighQuality or Fast.")]
        public WeatherMakerSunMode SunMode = WeatherMakerSunMode.HighQuality;

        [Header("Sun")]
        [Tooltip("Sun")]
        public Light Sun;

        [Range(0.0f, 10.0f)]
        [Tooltip("The size of the sun in the sky, 0 to disable the sun.")]
        public float SunSize = 0.02f;

        [Header("Positioning")]
        [Range(-0.5f, 0.5f)]
        [Tooltip("Offset the sky this amount from the camera y. This value is multiplied by the height of the sky sphere.")]
        public float YOffsetMultiplier = 0.0f;

        [Range(0.1f, 0.99f)]
        [Tooltip("Place the sky sphere at this amount of the far clip plane")]
        public float FarClipPlaneMultiplier = 0.8f;

        [Header("Textures - dawn/dusk not used in procedural sky.")]
        [Tooltip("The daytime texture")]
        public Texture2D DayTexture;

        [Tooltip("The dawn / dusk texture (not used for procedural sky) - this MUST be set if DawnDuskFadeDegrees is not 0, otherwise things will look funny.")]
        public Texture2D DawnDuskTexture;

        [Tooltip("The night time texture")]
        public Texture2D NightTexture;

        [Range(0.0f, 180.0f)]
        [Tooltip("It is fully day (i.e. no more night) when the sun x is at least this degrees above 0. 180 is high noon.")]
        public float DayDegrees = 95.0f;

        [Range(0.0f, 30.0f)]
        [Tooltip("The number of degrees that it fades from day to dawn/dusk before starting to fade to night. Set to 0 to fade from day and night directly. " +
            "For equal transitions from day to dusk and night, set this equal to NightFadeDegrees, but this is not required.")]
        public float DawnDuskFadeDegrees = 0.0f;

        [Range(0.0f, 90.0f)]
        [Tooltip("The number of degrees that it fades from day or dawn/dusk to night before becoming fully night")]
        public float NightFadeDegrees = 30.0f;

        [Header("Ambient Colors")]
        [Tooltip("Day ambient color")]
        public Color DayAmbientColor = Color.black;

        [Tooltip("Day ambient intensity")]
        [Range(0.0f, 1.0f)]
        public float DayAmbientIntensity = 0.0f;

        [Tooltip("Dawn/Dusk ambient color")]
        public Color DawnDuskAmbientColor = Color.black;

        [Tooltip("Dawn/Dusk ambient intensity")]
        [Range(0.0f, 1.0f)]
        public float DawnDuskAmbientIntensity = 0.0f;

        [Tooltip("Night ambient color")]
        public Color NightAmbientColor = Color.black;

        [Tooltip("Night ambient intensity")]
        [Range(0.0f, 1.0f)]
        public float NightAmbientIntensity = 0.0f;

        [Header("Night Sky")]
        [Range(0.0f, 1.0f)]
        [Tooltip("Night pixels must have an R, G or B value greater than or equal to this to be visible. Raise this value if you want to hide dimmer elements " +
            "of your night texture or there is a lot of light pollution, i.e. a city.")]
        public float NightVisibilityThreshold = 0.0f;

        [Range(0.0f, 32.0f)]
        [Tooltip("Intensity of night sky. Pixels that don't meet the NightVisibilityThreshold will still be invisible.")]
        public float NightIntensity = 2.0f;

        [Range(0.0f, 100.0f)]
        [Tooltip("How fast the twinkle pulsates")]
        public float NightTwinkleSpeed = 16.0f;

        [Tooltip("The variance of the night twinkle. The higher the value, the more variance.")]
        [Range(0.0f, 10.0f)]
        public float NightTwinkleVariance = 1.0f;

        [Tooltip("The minimum of the max rgb component for the night pixel to twinkle")]
        [Range(0.0f, 1.0f)]
        public float NightTwinkleMinimum = 0.02f;

        [Tooltip("The amount of randomness in the night sky twinkle")]
        [Range(0.0f, 5.0f)]
        public float NightTwinkleRandomness = 0.15f;

        [Header("Clouds (Play Mode Only)")]
        [Tooltip("Texture for cloud noise")]
        public Texture2D CloudNoise;

        [Tooltip("Cloud noise scale")]
        [Range(0.000001f, 1.0f)]
        public float CloudNoiseScale = 0.02f;

        [Tooltip("Multiplier for cloud noise")]
        [Range(0.0f, 4.0f)]
        public float CloudNoiseMultiplier = 1.0f;

        [Tooltip("Cloud noise height scale")]
        [Range(0.0f, 1.0f)]
        public float CloudNoiseHeightScale = 0.001f;

        [Tooltip("Cloud noise height multiplier")]
        [Range(0.0f, 5000.0f)]
        public float CloudNoiseHeightMultiplier = 200.0f;

        [Tooltip("Cloud velocity (xz)")]
        public Vector2 CloudVelocity;

        [Tooltip("Cloud color")]
        public Color CloudColor = Color.white;

        [Tooltip("Cloud height - only affects where the clouds stop at the horizon")]
        public float CloudHeight = 500;

        [Tooltip("Cloud cover, controls how many clouds there are")]
        [Range(0.0f, 1.0f)]
        public float CloudCover = 0.85f;

        [Tooltip("Cloud density, controls how opaque the clouds are")]
        [Range(0.0f, 1.0f)]
        public float CloudDensity = 0.0f;

        [Tooltip("Cloud sharpness, controls how distinct the clouds are")]
        [Range(0.0f, 1.0f)]
        public float CloudSharpness = 0.015f;

        [Tooltip("Cloud whispiness, controls how thin / small particles the clouds get as they change over time.")]
        [Range(0.0f, 3.0f)]
        public float CloudWhispiness = 1.0f;

        [Tooltip("Changes the whispiness of the clouds over time")]
        [Range(0.0f, 1.0f)]
        public float CloudWhispinessChangeFactor = 0.03f;

#if UNITY_EDITOR

        [Header("Generation of Sphere")]
        [Range(2, 6)]
        [Tooltip("Resolution of sphere. The higher the more triangles.")]
        public int Resolution = 4;

        [UnityEngine.HideInInspector]
        [UnityEngine.SerializeField]
        private int lastResolution = -1;

        [Tooltip("UV mode for sphere generation")]
        public UVMode UVMode = UVMode.PanoramaMirrorDown;

        [UnityEngine.HideInInspector]
        [UnityEngine.SerializeField]
        private UVMode lastUVMode = (UVMode)int.MaxValue;

#endif

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public Material Material { get { return meshRenderer.sharedMaterial; } }

        private void DestroyMesh()
        {
            if (meshFilter.sharedMesh != null)
            {
                GameObject.DestroyImmediate(meshFilter.sharedMesh, true);
                meshFilter.sharedMesh = null;
            }
        }

        private void SetShaderSunParameters()
        {
            if (SkyMode == WeatherMakeSkyMode.Textured)
            {
                meshRenderer.sharedMaterial.SetVector("_SunNormal", Sun.transform.forward);
            }
            else
            {
                meshRenderer.sharedMaterial.SetVector("_SunNormal", -Sun.transform.forward);
            }
            meshRenderer.sharedMaterial.SetColor("_SunColor", Sun.color);
            meshRenderer.sharedMaterial.SetFloat("_SunSize", SunSize);

			if (Sun.gameObject.activeInHierarchy && Sun.intensity > 0.0f && SunSize > 0.0f)
            {
                if (SunMode == WeatherMakerSunMode.HighQuality)
                {
                    meshRenderer.sharedMaterial.EnableKeyword("ENABLE_SUN_HIGH_QUALITY");
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_FAST");
                }
                else if (SunMode == WeatherMakerSunMode.Fast)
                {
                    meshRenderer.sharedMaterial.EnableKeyword("ENABLE_SUN_FAST");
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_HIGH_QUALITY");
                }
                else
                {
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_HIGH_QUALITY");
                    meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_FAST");
                }
            }
            else
            {
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_HIGH_QUALITY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_SUN_FAST");
            }
        }

        private void SetShaderSkyParameters()
        {
            meshRenderer.sharedMaterial.mainTexture = DayTexture;
            meshRenderer.sharedMaterial.SetTexture("_DawnDuskTex", DawnDuskTexture);
            meshRenderer.sharedMaterial.SetTexture("_NightTex", NightTexture);
            if (SkyMode == WeatherMakeSkyMode.Textured)
            {
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_SKY");
            }
            else if (SkyMode == WeatherMakeSkyMode.Procedural)
            {
                meshRenderer.sharedMaterial.EnableKeyword("ENABLE_PROCEDURAL_SKY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
            }
            else if (SkyMode == WeatherMakeSkyMode.ProceduralTextured)
            {
                meshRenderer.sharedMaterial.EnableKeyword("ENABLE_PROCEDURAL_TEXTURED_SKY");
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_PROCEDURAL_SKY");
            }
        }

        private void SetShaderLightParameters()
        {
            // always fully day between these two values
            float dayMaximum = 360.0f - DayDegrees;
            float dayMultiplier;
            float dawnDuskMultiplier;
            float nightMultiplier;
            float nightIntensity = 0.0f;
            float sunRotation = (Sun == null ? 90.0f : Sun.transform.eulerAngles.x) + 90.0f;
            
            // fade to night faster for procedural sky
            float dawnDuskFadeDegrees = (SkyMode == WeatherMakeSkyMode.Textured ? DawnDuskFadeDegrees : DawnDuskFadeDegrees * 0.5f);

            if (sunRotation > 360.0f)
            {
                sunRotation -= 360.0f;
            }

            if (sunRotation >= DayDegrees && sunRotation <= dayMaximum)
            {
                dayMultiplier = 1.0f;

                // fully day, these are 0
                dawnDuskMultiplier = nightMultiplier = 0.0f;
                RenderSettings.ambientLight = DayAmbientColor * DayAmbientIntensity;
            }
            else if (sunRotation < (DayDegrees - NightFadeDegrees - dawnDuskFadeDegrees) || sunRotation > (dayMaximum + NightFadeDegrees + dawnDuskFadeDegrees))
            {
                nightMultiplier = 1.0f;

                // fully night, these are 0
                dayMultiplier = dawnDuskMultiplier = 0.0f;
                nightIntensity = NightIntensity;

                RenderSettings.ambientLight = NightAmbientColor * NightAmbientIntensity;
            }
            else if (sunRotation < DayDegrees)
            {
                if (dawnDuskFadeDegrees == 0.0f)
                {
                    // fade from day to night
                    dawnDuskMultiplier = 0.0f;
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (DayDegrees - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dayMultiplier;
                    nightIntensity = Mathf.Lerp(0.0f, NightIntensity, Mathf.Pow(nightMultiplier, 4.0f));

                    RenderSettings.ambientLight = Color.Lerp(DayAmbientColor * DayAmbientIntensity, NightAmbientColor * NightAmbientIntensity, nightMultiplier);
                }
                else if (sunRotation < DayDegrees - dawnDuskFadeDegrees)
                {
                    dayMultiplier = 0.0f;

                    // fade from dawn/dusk to night
                    dawnDuskMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (DayDegrees - dawnDuskFadeDegrees - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dawnDuskMultiplier;
                    nightIntensity = Mathf.Lerp(0.0f, NightIntensity, Mathf.Pow(nightMultiplier, 4.0f));

                    RenderSettings.ambientLight = Color.Lerp(DawnDuskAmbientColor * DawnDuskAmbientIntensity, NightAmbientColor * NightAmbientIntensity, nightMultiplier);
                }
                else
                {
                    nightMultiplier = 0.0f;

                    // fade from day to dawn/dusk
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (DayDegrees - dawnDuskFadeDegrees)) / dawnDuskFadeDegrees));
                    dawnDuskMultiplier = 1.0f - dayMultiplier;

                    RenderSettings.ambientLight = Color.Lerp(DayAmbientColor * DayAmbientIntensity, DawnDuskAmbientColor * DawnDuskAmbientIntensity, dawnDuskMultiplier);
                }
            }
            else
            {
                if (dawnDuskFadeDegrees == 0.0f)
                {
                    // fade from day to night
                    dawnDuskMultiplier = 0.0f;
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (360.0f - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dayMultiplier;
                    nightIntensity = Mathf.Lerp(0.0f, NightIntensity, Mathf.Pow(nightMultiplier, 4.0f));

                    RenderSettings.ambientLight = Color.Lerp(DayAmbientColor * DayAmbientIntensity,  NightAmbientColor * NightAmbientIntensity, nightMultiplier);
                }
                else if (sunRotation > (dayMaximum + dawnDuskFadeDegrees))
                {
                    dayMultiplier = 0.0f;

                    // fade from dawn/dusk to night
                    dawnDuskMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (360.0f - dawnDuskFadeDegrees - NightFadeDegrees)) / NightFadeDegrees));
                    nightMultiplier = 1.0f - dawnDuskMultiplier;
                    nightIntensity = Mathf.Lerp(0.0f, NightIntensity, Mathf.Pow(nightMultiplier, 4.0f));

                    RenderSettings.ambientLight = Color.Lerp(DawnDuskAmbientColor * DawnDuskAmbientIntensity, NightAmbientColor * NightAmbientIntensity, nightMultiplier);
                }
                else
                {
                    nightMultiplier = 0.0f;

                    // fade from day to dawn/dusk
                    dayMultiplier = Mathf.Lerp(1.0f, 0.0f, 1.0f - ((sunRotation - (360.0f - dawnDuskFadeDegrees)) / dawnDuskFadeDegrees));
                    dawnDuskMultiplier = 1.0f - dayMultiplier;

                    RenderSettings.ambientLight = Color.Lerp(DayAmbientColor * DayAmbientIntensity, DawnDuskAmbientColor * DawnDuskAmbientIntensity, dawnDuskMultiplier);
                }
            }
            // Debug.LogFormat("Day: {0}, Dawn: {1}, Night: {2}", dayMultiplier, dawnDuskMultiplier, nightMultiplier);

            meshRenderer.sharedMaterial.SetFloat("_DayMultiplier", dayMultiplier);
            meshRenderer.sharedMaterial.SetFloat("_DawnDuskMultiplier", dawnDuskMultiplier);
            meshRenderer.sharedMaterial.SetFloat("_NightMultiplier", nightMultiplier);
            meshRenderer.sharedMaterial.SetFloat("_NightVisibilityThreshold", NightVisibilityThreshold);
            meshRenderer.sharedMaterial.SetFloat("_NightIntensity", nightIntensity);

            if (NightTwinkleRandomness > 0.0f || (NightTwinkleVariance > 0.0f && NightTwinkleSpeed > 0.0f))
            {
                meshRenderer.sharedMaterial.SetFloat("_NightTwinkleSpeed", NightTwinkleSpeed);
                meshRenderer.sharedMaterial.SetFloat("_NightTwinkleVariance", NightTwinkleVariance);
                meshRenderer.sharedMaterial.SetFloat("_NightTwinkleMinimum", NightTwinkleMinimum);
                meshRenderer.sharedMaterial.SetFloat("_NightTwinkleRandomness", NightTwinkleRandomness);
                meshRenderer.sharedMaterial.EnableKeyword("ENABLE_NIGHT_TWINKLE");
            }
            else
            {
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_NIGHT_TWINKLE");
            }

#if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                WeatherScript.DayNightScript.SunIntensityMultipliers["WeatherMakerSkySphereScript"] = 1.0f;
                WeatherScript.DayNightScript.SunShadowIntensityMultipliers["WeatherMakerSkySphereScript"] = 1.0f;
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_CLOUDS");
            }
            else 

#endif

            if (CloudNoise != null && CloudColor.a > 0.0f && CloudNoiseMultiplier > 0.0f && CloudCover > 0.0f)
            {
                meshRenderer.sharedMaterial.EnableKeyword("ENABLE_CLOUDS");
                meshRenderer.sharedMaterial.SetTexture("_FogNoise", CloudNoise);
                meshRenderer.sharedMaterial.SetColor("_FogColor", CloudColor);
                meshRenderer.sharedMaterial.SetVector("_FogNoiseVelocity", CloudVelocity);
                meshRenderer.sharedMaterial.SetFloat("_FogNoiseScale", CloudNoiseScale);
                meshRenderer.sharedMaterial.SetFloat("_FogNoiseMultiplier", CloudNoiseMultiplier);
                meshRenderer.sharedMaterial.SetFloat("_FogNoiseHeightScale", CloudNoiseHeightScale);
                meshRenderer.sharedMaterial.SetFloat("_FogNoiseHeightMultiplier", CloudNoiseHeightMultiplier);
                meshRenderer.sharedMaterial.SetFloat("_FogHeight", CloudHeight);
                meshRenderer.sharedMaterial.SetFloat("_FogCover", CloudCover);
                meshRenderer.sharedMaterial.SetFloat("_FogDensity", CloudDensity);
                meshRenderer.sharedMaterial.SetFloat("_FogSharpness", CloudSharpness);
                meshRenderer.sharedMaterial.SetFloat("_FogWhispiness", CloudWhispiness);
                meshRenderer.sharedMaterial.SetFloat("_FogWhispinessChangeFactor", CloudWhispinessChangeFactor);
                float sunIntensityMultiplier = Mathf.Clamp(1.0f - (CloudDensity * 0.5f), 0.0f, 1.0f);
                WeatherScript.DayNightScript.SunIntensityMultipliers["WeatherMakerSkySphereScript"] = sunIntensityMultiplier;
                float sunShadowMultiplier = 1.0f - Mathf.Clamp(((CloudCover + CloudDensity) - 0.25f) * 5.0f, 0.0f, 1.0f);
                WeatherScript.DayNightScript.SunShadowIntensityMultipliers["WeatherMakerSkySphereScript"] = sunShadowMultiplier;
            }
            else
            {
                WeatherScript.DayNightScript.SunIntensityMultipliers["WeatherMakerSkySphereScript"] = 1.0f;
                WeatherScript.DayNightScript.SunShadowIntensityMultipliers["WeatherMakerSkySphereScript"] = 1.0f;
                meshRenderer.sharedMaterial.DisableKeyword("ENABLE_CLOUDS");
            }
        }

        private void SetupSkySphere()
        {

#if UNITY_EDITOR

            if (Resolution != lastResolution)
            {
                lastResolution = Resolution;
                DestroyMesh();
            }
            if (UVMode != lastUVMode)
            {
                lastUVMode = UVMode;
                DestroyMesh();
            }
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                meshFilter.sharedMesh = WeatherMakerSphereCreator.Create(Resolution, UVMode);
            }

#endif

            float farPlane = FarClipPlaneMultiplier * Camera.farClipPlane;
            float yOffset = farPlane * YOffsetMultiplier;
            gameObject.transform.position = Camera.transform.position + new Vector3(0.0f, yOffset, 0.0f);
            float scale = farPlane * ((Camera.farClipPlane - Mathf.Abs(yOffset)) / Camera.farClipPlane);
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void UpdateSkySphere()
        {

#if UNITY_EDITOR

            if (meshRenderer.sharedMaterial == null)
            {
                Debug.LogError("Sky sphere material not set");
                return;
            }

#endif

            SetupSkySphere();
            SetShaderSunParameters();
            SetShaderSkyParameters();
            SetShaderLightParameters();            
        }

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            UpdateSkySphere();
        }

        private void Update()
        {
            UpdateSkySphere();
        }

        /// <summary>
        /// Enable / disable lens flare
        /// </summary>
        /// <param name="enable">Enable or disable</param>
        public void SetFlareEnabled(bool enable)
        {
            if (Sun == null)
            {
                return;
            }
            LensFlare flare = Sun.GetComponent<LensFlare>();
            if (flare == null)
            {
                return;
            }
            Color startColor = (enable ? Color.black : Color.white);
            Color endColor = (enable ? Color.white : Color.black);
            TweenFactory.Tween("WeatherMakerLensFlare", startColor, endColor, 3.0f, TweenScaleFunctions.Linear, (ITween<Color> c) =>
            {
                flare.color = c.CurrentValue;
            }, null);
            WeatherScript.FogScript.SunEnabled = enable;
        }

        /// <summary>
        /// Show cloud animated
        /// </summary>
        /// <param name="duration">How long until cloud fully visible</param>
        /// <param name="cover">Cloud cover, 0 to 1</param>
        public void ShowCloudsAnimated(float duration, float cover)
        {
            ShowCloudsAnimated(duration, cover, 0.0f, 1.0f, Color.white);
        }

        /// <summary>
        /// Show cloud animated
        /// </summary>
        /// <param name="duration">How long until cloud fully visible</param>
        /// <param name="cover">Cloud cover, 0 to 1</param>
        /// <param name="density">Cloud density, 0 to 1</param>
        /// <param name="whispiness">Cloud whispiness, 0 to 3</param>
        /// <param name="color">Cloud color</param>
        public void ShowCloudsAnimated(float duration, float cover, float density, float whispiness, Color color)
        {
            SetFlareEnabled(false);
            float startCover = CloudCover;
            float startDensity = CloudDensity;
            float startWhispiness = CloudWhispiness;
            Color startColor = CloudColor;
            TweenFactory.Tween("WeatherMakerClouds", 0.0f, 1.0f, duration, TweenScaleFunctions.Linear, (ITween<float> c) =>
            {
                CloudCover = Mathf.Lerp(startCover, cover, c.CurrentValue);
                CloudDensity = Mathf.Lerp(startDensity, density, c.CurrentValue);
                CloudWhispiness = Mathf.Lerp(startWhispiness, whispiness, c.CurrentValue);
                CloudColor = Color.Lerp(startColor, color, c.CurrentValue);
            }, null);
        }

        public void HideCloudsAnimated(float duration)
        {
            SetFlareEnabled(true);
            float cover = CloudCover;
            float density = CloudDensity;
            TweenFactory.Tween("WeatherMakerClouds", 1.0f, 0.0f, duration, TweenScaleFunctions.Linear, (ITween<float> c) =>
            {
                CloudCover = c.CurrentValue * cover;
                CloudDensity = c.CurrentValue * density;
            }, null);
        }

        public Camera Camera
        {
            get { return (_camera == null ? Camera.main : _camera); }
            set { _camera = value; }
        }
    }
}