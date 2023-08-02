Shader "Universal Render Pipeline/Custom/SimpleAnimationUnlit"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _PositionMap("Position Map", 2DArray) = "" {}
        _AnimationData("Animation Data", Vector) = (0, 0, 0, 0)
        _MaxFrames("Max Frames", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Geometry"
        }
        Pass
        {
            Name "Forward"
            Tags
            {
                "LightMode"="UniversalForward"
            }
            Cull Back

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma require 2darray
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "HLSL/VertexAnimation.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv3 : TEXCOORD2;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _AnimationData;
            float _MaxFrames;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                    UNITY_DOTS_INSTANCED_PROP(float4, _AnimationData)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
                #define _AnimationData UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _AnimationData)
            #endif

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D_ARRAY(_PositionMap);
            SAMPLER(sampler_PositionMap);

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 pos;
                float alpha;
                VA_ARRAY_float(input.uv3, sampler_PositionMap, _PositionMap, _AnimationData.g, _AnimationData.r, _MaxFrames, pos, alpha);

                float3 posNext;
                float alphaNext;
                VA_ARRAY_float(input.uv3, sampler_PositionMap, _PositionMap, _AnimationData.a, _AnimationData.b, _MaxFrames, posNext, alphaNext);

                float3 alphaNormals;
                float3 alphaNormalsNext;

                FloatToFloat3_float(alpha, alphaNormals); 
                FloatToFloat3_float(alphaNext, alphaNormalsNext);

                float timeFraction = frac(_AnimationData.r * _MaxFrames);
                float3 normals = lerp(alphaNormals, alphaNormalsNext, timeFraction);
                float3 position = lerp(pos, posNext, timeFraction);

                input.positionOS = float4(position.xyz, 1.0f);
                input.normalOS = normals; 
                
                const VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                const VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = input.color;
                return output;
            }
            
            half4 UnlitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 baseMap = half4(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv));
                return baseMap * _BaseColor * input.color;
            }
            ENDHLSL
        }
    }
}
