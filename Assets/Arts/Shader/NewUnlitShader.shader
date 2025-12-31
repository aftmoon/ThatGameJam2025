Shader "Hidden/BlackOriginBloom"
{
    Properties { _MainTex ("", 2D) = "white" {} }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
    float4 _MainTex_TexelSize;
    float _Threshold, _Intensity, _Scatter;

    half4 FragExtract(VaryingsDefault i) : SV_Target
    {
        half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
        half  bloom = saturate(c.a - _Threshold);
        return c * bloom;
    }

    half4 FragBlurH(VaryingsDefault i) : SV_Target
    {
        float2 ofs = _MainTex_TexelSize.xy * float2(1,0) * _Scatter;
        half4 s = 0;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - ofs*3) * 0.05;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - ofs*2) * 0.15;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - ofs  ) * 0.25;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord        ) * 0.2;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + ofs  ) * 0.25;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + ofs*2) * 0.15;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + ofs*3) * 0.05;
        return s;
    }

    half4 FragBlurV(VaryingsDefault i) : SV_Target
    {
        float2 ofs = _MainTex_TexelSize.xy * float2(0,1) * _Scatter;
        half4 s = 0;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - ofs*3) * 0.05;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - ofs*2) * 0.15;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - ofs  ) * 0.25;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord        ) * 0.2;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + ofs  ) * 0.25;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + ofs*2) * 0.15;
        s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + ofs*3) * 0.05;
        return s;
    }

    half4 FragComposite(VaryingsDefault i) : SV_Target
    {
        half4 main = SAMPLE_TEXTURE2D(_BlitTexture, sampler_MainTex, i.texcoord);
        half4 blur = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
        return main + blur * _Intensity;
    }
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass { Name "Extract" HLSLPROGRAM vertex VertDefault fragment FragExtract ENDHLSL }
        Pass { Name "BlurH"   HLSLPROGRAM vertex VertDefault fragment FragBlurH   ENDHLSL }
        Pass { Name "BlurV"   HLSLPROGRAM vertex VertDefault fragment FragBlurV   ENDHLSL }
        Pass { Name "Comp"    HLSLPROGRAM vertex VertDefault fragment FragComposite ENDHLSL }
    }
}