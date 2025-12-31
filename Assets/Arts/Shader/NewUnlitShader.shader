// UniversalOutline.shader
Shader "Universal/UniversalOutline"
{
    Properties
    {
        [Header(Outline)]
        _Color           ("Color", Color) = (1,1,1,1)
        _Width           ("Width", Range(0, 0.1)) = 0.02
        _SpriteExpand    ("Sprite Expand (world)", Range(0, 0.05)) = 0.01

        [Space]
        [Toggle(_SPRITE)]  _Sprite("Is Sprite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+50"
        }

        //================ 3D 外扩 Pass ================
        Pass
        {
            Name "Outline3D"
            Cull Front
            ZWrite Off
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _SPRITE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };
            float4 _Color;
            float  _Width;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                #if !_SPRITE
                    float3 worldP   = TransformObjectToWorld(IN.positionOS.xyz);
                    float3 worldN   = TransformObjectToWorldNormal(IN.normalOS);
                    worldP         += worldN * _Width;
                    OUT.positionHCS = TransformWorldToHClip(worldP);
                #else
                    OUT.positionHCS = 0; // Sprite 不走这里
                #endif
                return OUT;
            }
            half4 frag () : SV_Target { return _Color; }
            ENDHLSL
        }

        //================ Sprite 边缘检测 Pass ================
        Pass
        {
            Name "OutlineSprite"
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _SPRITE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };
            TEXTURE2D(_MainTex);      SAMPLER(sampler_MainTex);
            float4 _Color;
            float  _SpriteExpand;
            float4 _MainTex_TexelSize;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }
            half SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }
            half4 frag (Varyings IN) : SV_Target
            {
                #if _SPRITE
                    half2 step = _MainTex_TexelSize.xy * _SpriteExpand;
                    half a = SampleAlpha(IN.uv);
                    half left = SampleAlpha(IN.uv - half2(step.x, 0));
                    half right= SampleAlpha(IN.uv + half2(step.x, 0));
                    half down = SampleAlpha(IN.uv - half2(0, step.y));
                    half up   = SampleAlpha(IN.uv + half2(0, step.y));
                    half outline = saturate(1 - a) * max(max(left, right), max(down, up));
                    return lerp(half4(0,0,0,0), _Color, outline);
                #else
                    return half4(0,0,0,0);
                #endif
            }
            ENDHLSL
        }
    }

    Fallback Off
}