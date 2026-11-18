Shader "DoubleOrDead/TreasureAura"
{
    Properties
    {
        [HDR] _GlowColor ("Glow Colour", Color) = (1, 0.72, 0.3, 1)
        _Strength ("Strength", Range(0, 3)) = 0.45
        _Period ("Pulse Duration", Float) = 3
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float phase : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                float _Strength;
                float _Period;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                float3 centre = TransformObjectToWorld(float3(0, 0, 0));

                output.phase = frac(
                    sin(dot(centre, float3(12.9898, 78.233, 37.719)))
                    * 43758.5453
                ) * 6.283185;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float radius = length((input.uv - 0.5) * 2);
                float glow = pow(saturate(1 - radius), 2.5);

                float pulse = 0.5 + 0.5 * sin(
                    _Time.y * 6.283185 / max(_Period, 0.1)
                    + input.phase
                );

                pulse *= pulse;

                return half4(
                    _GlowColor.rgb,
                    glow * pulse * _Strength * _GlowColor.a
                );
            }
            ENDHLSL
        }
    }
}