Shader "Custom/Unlit/SkyBox"
{
    Properties
    {
        _SkyBoxTex ("Texture", Cube) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            TextureCube _SkyBoxTex;
            SamplerState sampler_SkyBoxTex;
            float4 _Corners[4];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                float3 worldPos = _Corners[v.uv.x + v.uv.y * 2].xyz;
                o.viewDir = normalize(worldPos - _WorldSpaceCameraPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _SkyBoxTex.Sample(sampler_SkyBoxTex, i.viewDir);
            }
            ENDCG
        }
    }
}
