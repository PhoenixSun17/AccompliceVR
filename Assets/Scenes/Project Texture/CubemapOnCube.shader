Shader "Custom/CubemapOnCube" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            Cull Front // Cull the front faces to render inside of the cube
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _MainTex;

            struct appdata_t {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 localPos : TEXCOORD0; // Local position of the vertex
            };

            v2f vert (appdata_t v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz; // Pass local vertex position
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float3 dir = normalize(i.localPos); // Normalize direction
                return texCUBE(_MainTex, dir); // Sample the cubemap
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
