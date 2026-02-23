Shader "Custom/ProjectedTexture" {
    Properties {
        _MainTex ("Input Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            Cull Front // Render back faces to see the inside of the cube
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4x4 _ViewProjection; // Combined view-projection matrix
//              float4x4 unity_ObjectToWorld; // Built-in matrix for object-to-world transformation

            struct appdata_t {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD0; // Projected texture coordinates
            };

            v2f vert (appdata_t v) {
                v2f o;

                // Standard vertex position for rendering
                o.pos = UnityObjectToClipPos(v.vertex);

                // Transform vertex to world space
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                // Apply the view-projection matrix to world-space vertex
                o.projPos = mul(_ViewProjection, worldPos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {

                if (i.projPos.z > 0.3) discard;

                // Convert clip space to UV space
                float2 uv = i.projPos.xy / i.projPos.w;

                
                uv = uv * 0.5 + 0.5; // Transform from [-1,1] to [0,1]
                uv.y = 1.0 - uv.y;

                // Sample the texture
                fixed4 color = tex2D(_MainTex, uv);

                // Discard pixels outside the texture bounds
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    discard;
                if (uv.x < 0.15 || uv.x > 0.85 ||
                    uv.y < 0.15|| uv.y > 0.85)
                {
                    discard; // Discard pixels outside the region
                }

                // Return the texture color for pixels within the region
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
