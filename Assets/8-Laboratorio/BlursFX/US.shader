Shader "Custom/UI/AAA_UI_Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}

        [Header(BLUR)]
        _BlurSize ("Blur Size", Range(0, 20)) = 5
        _BlurQuality ("Blur Quality", Range(1, 3)) = 2
        _BlurIntensity ("Blur Intensity", Range(0, 1)) = 1

        [Header(APPEARANCE)]
        _Opacity ("Opacity", Range(0, 1)) = 1
        _TintColor ("Tint Color", Color) = (1,1,1,1)

        [Header(FROSTED GLASS)]
        _Frost ("Frost", Range(0, 1)) = 0.25
        _FrostPower ("Frost Power", Range(0, 2)) = 1

        [Header(DISTORTION)]
        _Distortion ("Distortion", Range(0, 0.1)) = 0
        _DistortionSpeed ("Distortion Speed", Range(0, 10)) = 1

        [Header(EDGE)]
        _EdgeDarkness ("Edge Darkness", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "AAA_UI_BLUR"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            float _BlurSize;
            float _BlurQuality;
            float _BlurIntensity;

            float _Opacity;
            float4 _TintColor;

            float _Frost;
            float _FrostPower;

            float _Distortion;
            float _DistortionSpeed;

            float _EdgeDarkness;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }

            half4 SampleBlur(
                float2 uv,
                float2 direction,
                float radius
            )
            {
                half4 result = 0;
                float weight = 0;

                // Centro
                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv
                ) * 0.20;

                weight += 0.20;

                // 1
                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv + direction * radius * 0.25
                ) * 0.15;

                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv - direction * radius * 0.25
                ) * 0.15;

                weight += 0.30;

                // 2
                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv + direction * radius * 0.50
                ) * 0.10;

                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv - direction * radius * 0.50
                ) * 0.10;

                weight += 0.20;

                // 3
                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv + direction * radius * 0.75
                ) * 0.05;

                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv - direction * radius * 0.75
                ) * 0.05;

                weight += 0.10;

                // 4
                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv + direction * radius
                ) * 0.025;

                result += SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv - direction * radius
                ) * 0.025;

                weight += 0.05;

                return result / weight;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float2 texel = _MainTex_TexelSize.xy;

                // -------------------------------------------------
                // DISTORTION
                // -------------------------------------------------

                float time = _Time.y * _DistortionSpeed;

                float noiseA = Noise(
                    uv * 8.0 + time * 0.15
                );

                float noiseB = Noise(
                    uv * 12.0 - time * 0.12
                );

                float2 distortion;

                distortion.x =
                    (noiseA - 0.5) * _Distortion;

                distortion.y =
                    (noiseB - 0.5) * _Distortion;

                uv += distortion;

                // -------------------------------------------------
                // BLUR
                // -------------------------------------------------

                float radius =
                    _BlurSize * 0.01;

                half4 blur = 0;

                // 4 direcciones
                blur += SampleBlur(
                    uv,
                    float2(1, 0) * texel,
                    radius
                );

                blur += SampleBlur(
                    uv,
                    float2(0, 1) * texel,
                    radius
                );

                // Diagonales
                if (_BlurQuality >= 2)
                {
                    blur += SampleBlur(
                        uv,
                        normalize(float2(1, 1)) * texel,
                        radius
                    );

                    blur += SampleBlur(
                        uv,
                        normalize(float2(-1, 1)) * texel,
                        radius
                    );
                }

                // Más direcciones para calidad alta
                if (_BlurQuality >= 3)
                {
                    blur += SampleBlur(
                        uv,
                        normalize(float2(1, 0.5)) * texel,
                        radius
                    );

                    blur += SampleBlur(
                        uv,
                        normalize(float2(-1, 0.5)) * texel,
                        radius
                    );

                    blur += SampleBlur(
                        uv,
                        normalize(float2(0.5, 1)) * texel,
                        radius
                    );

                    blur += SampleBlur(
                        uv,
                        normalize(float2(-0.5, 1)) * texel,
                        radius
                    );
                }

                float divisor = 4.0;

                if (_BlurQuality >= 2)
                    divisor = 6.0;

                if (_BlurQuality >= 3)
                    divisor = 10.0;

                blur /= divisor;

                // -------------------------------------------------
                // ORIGINAL
                // -------------------------------------------------

                half4 original =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        uv
                    );

                // -------------------------------------------------
                // BLUR MIX
                // -------------------------------------------------

                half4 finalColor =
                    lerp(
                        original,
                        blur,
                        _BlurIntensity
                    );

                // -------------------------------------------------
                // FROSTED GLASS
                // -------------------------------------------------

                float frostNoise =
                    Noise(uv * 45.0);

                frostNoise =
                    (frostNoise - 0.5) * _Frost;

                finalColor.rgb +=
                    frostNoise * _FrostPower;

                // -------------------------------------------------
                // TINT
                // -------------------------------------------------

                finalColor.rgb *= _TintColor.rgb;

                // -------------------------------------------------
                // EDGE DARKENING
                // -------------------------------------------------

                float2 edge =
                    abs(uv - 0.5) * 2.0;

                float edgeFactor =
                    saturate(max(edge.x, edge.y));

                float edgeFade =
                    lerp(
                        1.0,
                        1.0 - edgeFactor,
                        _EdgeDarkness
                    );

                finalColor.rgb *= edgeFade;

                // -------------------------------------------------
                // ALPHA
                // -------------------------------------------------

                finalColor.a *=
                    _Opacity *
                    input.color.a;

                finalColor *= input.color;

                return finalColor;
            }

            ENDHLSL
        }
    }
}