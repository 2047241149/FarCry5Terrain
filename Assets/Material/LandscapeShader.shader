Shader "Landscape/RenderLandscape"
{
    Properties
    {
        _HeightTex ("Texture", 2D) = "white" {}
        _HeightScale ("HeightScale", float) = 1.0
        _bPureWhite("bPureWhite", int) = 0
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
                float3 worldNormal : NORMAL; 
            };

            sampler2D _HeightTex;
            float _HeightScale;
            int _bPureWhite;

            v2f vert (appdata v)
            {
                v2f o;
                float4 heightNormal = tex2Dlod(_HeightTex, float4(v.uv, 0, 0)).rgba;
                float heightSampleValue = heightNormal.r;
                float3 normal = heightNormal.gba;
                float height = heightSampleValue * _HeightScale;
                float4 landscapeVertex = float4(v.vertex.x, height, v.vertex.z, 1.0f);
                o.vertex = UnityObjectToClipPos(landscapeVertex);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(1.0f, 1.0f, 1.0f, 1.0f);
                if(_bPureWhite == 0)
                {
                    // sample the texture
                    float3 normalizedLightDir = normalize(_WorldSpaceLightPos0.xyz);
                    fixed halfLambert = dot(i.worldNormal, normalizedLightDir) * 0.5 + 0.5;
                    col = fixed4(halfLambert, halfLambert, halfLambert, 1.0f);
                }

                return col;
            }
            ENDCG
        }
    }
}
