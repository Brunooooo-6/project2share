Shader "Hidden/ScreenSpaceOutline_DepthOnly"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1,1,0,1)
        _Thickness("Thickness (px)", Float) = 2
        _DepthThreshold("Depth Threshold", Float) = 0.0025
    }

    SubShader
    {
        Tags{ "RenderPipeline"="HDRenderPipeline" }
        Pass
        {
            Name "FullScreenOutline"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            TEXTURE2D_X(_SourceTex);
            SAMPLER(sampler_SourceTex);

            TEXTURE2D_X(_OutlineMask);
            SAMPLER(sampler_OutlineMask);

            TEXTURE2D_X(_DepthTex);          // ✅ 新增
            SAMPLER(sampler_DepthTex);       // ✅ 新增

            float4 _OutlineColor;
            float  _Thickness;
            float  _DepthThreshold;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes a)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(a.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(a.vertexID);
                return o;
            }

            float LinearEyeDepthFromRaw(float rawDepth)
            {
                return rcp(_ZBufferParams.x * rawDepth + _ZBufferParams.y);
            }

            float SampleLinearEyeDepth(float2 uv)
            {
                float raw = SAMPLE_TEXTURE2D_X(_DepthTex, sampler_DepthTex, uv).r; // ✅ 用我们自己传的
                return LinearEyeDepthFromRaw(raw);
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                float4 col  = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex, uv);
                float  mask = SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_OutlineMask, uv).r;

                // 不在mask区域：不处理
                if (mask < 0.5) return col;

                // 像素大小 * thickness
                float2 texel = _ScreenSize.zw * _Thickness;

                // 深度差分（右、上）
                float dC = SampleLinearEyeDepth(uv);
                float dR = SampleLinearEyeDepth(uv + float2(texel.x, 0));
                float dU = SampleLinearEyeDepth(uv + float2(0, texel.y));

                float edge = max(abs(dC - dR), abs(dC - dU));