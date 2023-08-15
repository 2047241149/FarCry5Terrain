Shader "Landscape/RenderLandscape"
{
    Properties
    {
        _HeightTex ("Texture", 2D) = "white" {}
        _HeightScale ("HeightScale", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _HeightTex;
            float _HeightScale;

            v2f vert (appdata v)
            {
                v2f o;
                float heightSampleValue = tex2Dlod(_HeightTex, float4(v.uv, 0, 0)).r;
                float height = heightSampleValue * _HeightScale;
                float4 landscapeVertex = float4(v.vertex.x, height, v.vertex.z, 1.0f);
                o.vertex = UnityObjectToClipPos(landscapeVertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(1.0f, 1.0f, 1.0f, 1.0f);

                return col;
            }
            ENDCG
        }
    }
}
