// Pickup highlight: rotating striped (dashed) outline for SpriteRenderer (URP 2D, unlit).
// Ported from a CC0 Godot canvas_item shader by shadecore_dev.
//
// Differences from the Godot original:
//   - "_ShowSprite" (default ON) also draws the sprite itself, so a single
//     SpriteRenderer is enough. Turn it OFF to reproduce the original
//     outline-only behaviour (e.g. on a duplicated sprite behind the pickup).
//
// IMPORTANT (sprite import settings):
//   - Mesh Type must be "Full Rect" (Tight meshes clip away the transparent
//     pixels where the outline is drawn).
//   - The texture needs at least 1-2 transparent pixels of padding around
//     the art, otherwise there is no room for the outline.
Shader "SeasonSlime/PickupHighlight"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        [HDR] _OutlineColor("Outline Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RotationSpeed("Rotation Speed", Range(-4.0, 4.0)) = 0.3
        _Stripes("Stripes", Range(1.0, 32.0)) = 16.0
        _StripeWidth("Stripe Width", Range(0.0, 1.0)) = 0.5
        [Toggle] _ShowSprite("Show Sprite", Float) = 1
        // UV rect of the current sprite frame inside the texture: xy = min, zw = size.
        // Updated automatically by the PickupHighlight.cs component (needed for
        // sprite-sheet / flipbook sprites). Leave (0,0,1,1) for single sprites.
        _SpriteRect("Sprite UV Rect", Vector) = (0.0, 0.0, 1.0, 1.0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "PickupHighlight"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                half4 _OutlineColor;
                float _RotationSpeed;
                float _Stripes;
                float _StripeWidth;
                float _ShowSprite;
                float4 _SpriteRect;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color       : COLOR;
                float2 uv         : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            float SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            // Sample alpha, clamped inside the current frame rect so the
            // outline never bleeds in from neighbouring sprite-sheet frames.
            float SampleAlphaClamped(float2 uv, float2 pixelSize)
            {
                float2 rectMin = _SpriteRect.xy + pixelSize * 0.5;
                float2 rectMax = _SpriteRect.xy + _SpriteRect.zw - pixelSize * 0.5;
                return SampleAlpha(clamp(uv, rectMin, rectMax));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 pixelSize = _MainTex_TexelSize.xy;

                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;

                // 4-neighbour check: is any adjacent texel opaque?
                bool within = SampleAlphaClamped(uv + pixelSize * float2( 1.0,  0.0), pixelSize) > 0.0;
                within = within || SampleAlphaClamped(uv + pixelSize * float2( 0.0, -1.0), pixelSize) > 0.0;
                within = within || SampleAlphaClamped(uv + pixelSize * float2( 0.0,  1.0), pixelSize) > 0.0;
                within = within || SampleAlphaClamped(uv + pixelSize * float2(-1.0,  0.0), pixelSize) > 0.0;

                bool outline = within && (SampleAlpha(uv) == 0.0);

                float rotation = _Time.y * _RotationSpeed;

                // Rotating stripe pattern around the centre of the CURRENT FRAME
                // (not the whole texture), computed on integer pixel coordinates
                // for a crisp pixel-art look.
                float2 frameCentre = _SpriteRect.xy + _SpriteRect.zw * 0.5;
                float px = floor((uv.x - frameCentre.x) / pixelSize.x);
                float py = floor((uv.y - frameCentre.y) / pixelSize.y);
                float angle01 = (atan2(px * cos(rotation) + py * sin(rotation),
                                       py * cos(rotation) - px * sin(rotation)) - PI) / -TWO_PI;
                float fill = fmod(angle01, 1.0 / _Stripes) < (_StripeWidth / _Stripes) ? 1.0 : 0.0;

                half4 outlineCol = outline ? _OutlineColor * fill : half4(0.0, 0.0, 0.0, 0.0);

                // Premultiplied-alpha composition (Blend One OneMinusSrcAlpha).
                // Outline exists only where the sprite is transparent, so the
                // two contributions never overlap.
                half4 output;
                if (_ShowSprite > 0.5)
                {
                    output.rgb = c.rgb * c.a + outlineCol.rgb * outlineCol.a;
                    output.a = saturate(c.a + outlineCol.a);
                }
                else
                {
                    output.rgb = outlineCol.rgb * outlineCol.a;
                    output.a = outlineCol.a;
                }
                return output;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
