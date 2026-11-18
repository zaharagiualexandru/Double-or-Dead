Shader "DoubleOrDead/TreasureHalo"
{
    Properties
    {
        [HDR] _GlowColor ("Glow Colour", Color) = (1, 0.65, 0.15, 1)
        _Strength ("Strength", Range(0, 1)) = 0.12
        _PulseDuration ("Pulse Duration", Float) = 3
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
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                float _Strength;
                float _PulseDuration;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);

                output.positionCS =
                    TransformWorldToHClip(output.positionWS);

                output.normalWS =
                    TransformObjectToWorldNormal(input.normalOS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 view = normalize(
                    GetCameraPositionWS() - input.positionWS
                );

                float facing = saturate(dot(normal, view));

                // Broad glow near the edge, fading out at the silhouette.
                float halo = pow(1 - facing, 2)
                    * smoothstep(0, 0.22, facing);

                float3 centre = TransformObjectToWorld(float3(0, 0, 0));
                float phase = dot(centre, float3(1.7, 2.3, 0.9));

                float wave = 0.5 + 0.5 * sin(
                    _Time.y * 6.283185 / max(_PulseDuration, 0.1)
                    + phase
                );

                float pulse = lerp(0.15, 1, wave * wave);

                return half4(
                    _GlowColor.rgb,
                    halo * pulse * _Strength * _GlowColor.a
                );
            }
            ENDHLSL
        }
    }
}