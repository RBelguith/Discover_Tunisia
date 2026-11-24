// Additive glow around the globe: a rim over the surface, or a halo past the edge when Outer is on.
Shader "DiscoverTunisia/Atmosphere"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.35, 0.7, 1, 1)
        _Strength ("Strength", Float) = 1
        _Power ("Falloff", Float) = 3
        _Scale ("Halo width", Float) = 2
        [Toggle] _Outer ("Halo past the edge", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Atmosphere"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Strength;
                half _Power;
                half _Scale;
                half _Outer;
                half _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewWS = GetWorldSpaceViewDir(position.positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float facing = dot(normalize(input.normalWS), normalize(input.viewWS));
                float rim = _Outer > 0.5 ? saturate(-facing * _Scale) : 1.0 - saturate(facing);
                half glow = pow(rim, _Power) * _Strength;
                return half4(_Color.rgb * glow, 0);
            }
            ENDHLSL
        }
    }
}
