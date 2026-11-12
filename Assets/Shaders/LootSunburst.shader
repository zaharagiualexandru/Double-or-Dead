Shader "DoubleOrDead/LootSunburst"
{
    Properties
    {
        [HDR] _GlowColor ("Glow Colour", Color) = (1, 0.8, 0.35, 1)
        _Intensity ("Intensity", Range(0, 4)) = 1
        _HaloStrength ("Halo Strength", Range(0, 2)) = 0.5
        _RayStrength ("Ray Strength", Range(0, 2)) = 0.7
        _RayCount ("Ray Count", Range(3, 16)) = 7
        _RaySharpness ("Ray Sharpness", Range(2, 30)) = 12
        _RotationSpeed ("Rotation Speed", Range(-1, 1)) = 0.12
        _PulseAmount ("Pulse Amount", Range(0, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _GlowColor;
                float _Intensity;
                float _HaloStrength;
                float _RayStrength;
                float _RayCount;
                float _RaySharpness;
                float _RotationSpeed;
                float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 position = (input.uv - 0.5) * 2.0;
                float radius = length(position);

                float angle = atan2(
                    position.y,
                    position.x + 0.00001
                );

                float rotatingAngle =
                    angle + _Time.y * _RotationSpeed;

                float rayCount = floor(_RayCount + 0.5);

                // Narrow angular bands widen as they extend outward.
                float rayPattern =
                    0.5 + 0.5 * cos(rotatingAngle * rayCount);

                float rays = pow(
                    saturate(rayPattern),
                    _RaySharpness
                );

                // Vary the ray lengths while keeping the pattern seamless.
                float rayLength =
                    0.72 + 0.23 * sin(rotatingAngle * 3.0 + 1.3);

                float rayFade =
                    1.0 - smoothstep(0.12, rayLength, radius);

                float halo = exp(-radius * radius * 12.0);

                float edgeFade =
                    1.0 - smoothstep(0.85, 1.0, radius);

                float pulse =
                    1.0 + sin(_Time.y * 2.0) * _PulseAmount;

                float brightness =
                    halo * _HaloStrength +
                    rays * rayFade * _RayStrength;

                float alpha = saturate(
                    brightness * edgeFade * _GlowColor.a
                );

                return half4(
                    _GlowColor.rgb * _Intensity * pulse,
                    alpha
                );
            }

            ENDHLSL
        }
    }
}