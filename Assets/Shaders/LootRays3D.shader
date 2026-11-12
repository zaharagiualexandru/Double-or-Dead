Shader "DoubleOrDead/LootRays3D"
{
    Properties
    {
        [HDR] _Color("Ray Colour", Color) = (1, 0.85, 0.35, 1)
        _Intensity("Intensity", Range(0, 5)) = 1.5
        _Opacity("Opacity", Range(0, 1)) = 0.2
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
                float4 _Color;
                float _Intensity;
                float _Opacity;
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
                float alongRay = input.uv.y;

                // Fade near the origin and towards the outer end.
                float startFade = smoothstep(0, 0.12, alongRay);
                float endFade = 1 - smoothstep(0.3, 1, alongRay);

                float alpha =
                    startFade * endFade * _Opacity * _Color.a;

                return half4(_Color.rgb * _Intensity, alpha);
            }
            ENDHLSL
        }
    }
}