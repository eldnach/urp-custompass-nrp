Shader "Unlit/BlitShader1"
{
    Properties
    {
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite On ZTest LEqual

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenpos : TEXCOORD1;
            };

            UNITY_DECLARE_FRAMEBUFFER_INPUT_FLOAT(0);

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                o.screenpos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = UNITY_READ_FRAMEBUFFER_INPUT(0, i.screenpos.xy);
                col += fixed4(0.0, 0.5, 0.0, 1.0);
                return col;
            }
            ENDCG
        }
    }
}
