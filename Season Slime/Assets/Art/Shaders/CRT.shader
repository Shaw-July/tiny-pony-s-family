// CRT post-process shader for URP Full Screen Pass Renderer Feature.
// Ported from a Godot canvas_item CRT shader (scanlines, grille, roll,
// noise, aberration, warp, vignette, discolor).
Shader "SeasonSlime/CRT"
{
    Properties
    {
        _ScanlinesOpacity("Scanlines Opacity", Range(0.0, 1.0)) = 0.4
        _ScanlinesWidth("Scanlines Width", Range(0.0, 0.5)) = 0.25
        _GrilleOpacity("Grille Opacity", Range(0.0, 1.0)) = 0.3
        _Resolution("Resolution (x, y)", Vector) = (640.0, 480.0, 0.0, 0.0)

        [Toggle] _Pixelate("Pixelate", Float) = 1

        [Toggle] _Roll("Roll", Float) = 1
        _RollSpeed("Roll Speed", Float) = 8.0
        _RollSize("Roll Size", Range(0.0, 100.0)) = 15.0
        _RollVariation("Roll Variation", Range(0.1, 5.0)) = 1.8
        _DistortIntensity("Distort Intensity", Range(0.0, 0.2)) = 0.05

        _NoiseOpacity("Noise Opacity", Range(0.0, 1.0)) = 0.4
        _NoiseSpeed("Noise Speed", Float) = 5.0
        _StaticNoiseIntensity("Static Noise Intensity", Range(0.0, 1.0)) = 0.06

        _Aberration("Aberration", Range(-1.0, 1.0)) = 0.03
        _Brightness("Brightness", Float) = 1.4
        [Toggle] _Discolor("Discolor", Float) = 1

        _WarpAmount("Warp Amount", Range(0.0, 5.0)) = 1.0

        _VignetteIntensity("Vignette Intensity", Float) = 0.4
        _VignetteOpacity("Vignette Opacity", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "CRT"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _ScanlinesOpacity;
                float _ScanlinesWidth;
                float _GrilleOpacity;
                float4 _Resolution;
                float _Pixelate;
                float _Roll;
                float _RollSpeed;
                float _RollSize;
                float _RollVariation;
                float _DistortIntensity;
                float _NoiseOpacity;
                float _NoiseSpeed;
                float _StaticNoiseIntensity;
                float _Aberration;
                float _Brightness;
                float _Discolor;
                float _WarpAmount;
                float _VignetteIntensity;
                float _VignetteOpacity;
            CBUFFER_END

            #define CRT_PI 3.14159265

            float2 Random2(float2 uv)
            {
                uv = float2(dot(uv, float2(127.1, 311.7)), dot(uv, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(uv) * 43758.5453123);
            }

            float GradientNoise(float2 uv)
            {
                float2 uvIndex = floor(uv);
                float2 uvFract = frac(uv);
                float2 blur = smoothstep(0.0, 1.0, uvFract);

                return lerp(
                    lerp(dot(Random2(uvIndex + float2(0.0, 0.0)), uvFract - float2(0.0, 0.0)),
                         dot(Random2(uvIndex + float2(1.0, 0.0)), uvFract - float2(1.0, 0.0)), blur.x),
                    lerp(dot(Random2(uvIndex + float2(0.0, 1.0)), uvFract - float2(0.0, 1.0)),
                         dot(Random2(uvIndex + float2(1.0, 1.0)), uvFract - float2(1.0, 1.0)), blur.x),
                    blur.y) * 0.5 + 0.5;
            }

            float2 Warp(float2 uv)
            {
                float2 delta = uv - 0.5;
                float delta2 = dot(delta, delta);
                float delta4 = delta2 * delta2;
                float deltaOffset = delta4 * _WarpAmount;
                return uv + delta * deltaOffset;
            }

            float Border(float2 uv)
            {
                float radius = min(_WarpAmount, 0.08);
                radius = max(min(min(abs(radius * 2.0), 1.0), 1.0), 1e-5);
                float2 absUv = abs(uv * 2.0 - 1.0) - float2(1.0, 1.0) + radius;
                float dist = length(max(float2(0.0, 0.0), absUv)) / radius;
                float square = smoothstep(0.96, 1.0, dist);
                return clamp(1.0 - square, 0.0, 1.0);
            }

            float Vignette(float2 uv)
            {
                uv *= 1.0 - uv.xy;
                float vignette = uv.x * uv.y * 15.0;
                return pow(abs(vignette), _VignetteIntensity * _VignetteOpacity);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = input.texcoord;
                float2 uv = Warp(screenUV);
                float2 textUv = uv;
                float2 rollUv = float2(0.0, 0.0);
                float time = _Roll > 0.5 ? _Time.y : 0.0;

                if (_Pixelate > 0.5)
                {
                    textUv = ceil(uv * _Resolution.xy) / _Resolution.xy;
                }

                float rollLine = 0.0;
                if (_Roll > 0.5 || _NoiseOpacity > 0.0)
                {
                    rollLine = smoothstep(0.3, 0.9, sin(uv.y * _RollSize - (time * _RollSpeed)));
                    rollLine *= rollLine * smoothstep(0.3, 0.9, sin(uv.y * _RollSize * _RollVariation - (time * _RollSpeed * _RollVariation)));
                    rollUv = float2(rollLine * _DistortIntensity * (1.0 - screenUV.x), 0.0);
                }

                half4 text;
                if (_Roll > 0.5)
                {
                    text.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, textUv + rollUv * 0.8 + float2(_Aberration, 0.0) * 0.1).r;
                    text.g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, textUv + rollUv * 1.2 - float2(_Aberration, 0.0) * 0.1).g;
                    text.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, textUv + rollUv).b;
                    text.a = 1.0;
                }
                else
                {
                    text.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, textUv + float2(_Aberration, 0.0) * 0.1).r;
                    text.g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, textUv - float2(_Aberration, 0.0) * 0.1).g;
                    text.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, textUv).b;
                    text.a = 1.0;
                }

                float r = text.r;
                float g = text.g;
                float b = text.b;

                uv = Warp(screenUV);

                if (_GrilleOpacity > 0.0)
                {
                    float gR = smoothstep(0.85, 0.95, abs(sin(uv.x * (_Resolution.x * CRT_PI))));
                    r = lerp(r, r * gR, _GrilleOpacity);

                    float gG = smoothstep(0.85, 0.95, abs(sin(1.05 + uv.x * (_Resolution.x * CRT_PI))));
                    g = lerp(g, g * gG, _GrilleOpacity);

                    float gB = smoothstep(0.85, 0.95, abs(sin(2.1 + uv.x * (_Resolution.x * CRT_PI))));
                    b = lerp(b, b * gB, _GrilleOpacity);
                }

                text.r = clamp(r * _Brightness, 0.0, 1.0);
                text.g = clamp(g * _Brightness, 0.0, 1.0);
                text.b = clamp(b * _Brightness, 0.0, 1.0);

                float scanlines = 0.5;
                if (_ScanlinesOpacity > 0.0)
                {
                    scanlines = smoothstep(_ScanlinesWidth, _ScanlinesWidth + 0.5, abs(sin(uv.y * (_Resolution.y * CRT_PI))));
                    text.rgb = lerp(text.rgb, text.rgb * scanlines, _ScanlinesOpacity);
                }

                if (_NoiseOpacity > 0.0)
                {
                    float n = smoothstep(0.4, 0.5, GradientNoise(uv * float2(2.0, 200.0) + float2(10.0, _Time.y * _NoiseSpeed)));
                    rollLine *= n * scanlines * clamp(Random2((ceil(uv * _Resolution.xy) / _Resolution.xy) + float2(_Time.y * 0.8, 0.0)).x + 0.8, 0.0, 1.0);
                    text.rgb = clamp(lerp(text.rgb, text.rgb + rollLine, _NoiseOpacity), 0.0, 1.0);
                }

                if (_StaticNoiseIntensity > 0.0)
                {
                    text.rgb += clamp(Random2((ceil(uv * _Resolution.xy) / _Resolution.xy) + frac(_Time.y)).x, 0.0, 1.0) * _StaticNoiseIntensity;
                }

                text.rgb *= Border(uv);
                text.rgb *= Vignette(uv);

                if (_Discolor > 0.5)
                {
                    const float saturation = 0.5;
                    const float contrast = 1.2;
                    float3 greyscale = ((text.r + text.g + text.b) / 3.0).xxx;
                    text.rgb = lerp(text.rgb, greyscale, saturation);

                    float midpoint = pow(0.5, 2.2);
                    text.rgb = (text.rgb - midpoint) * contrast + midpoint;
                }

                return text;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
