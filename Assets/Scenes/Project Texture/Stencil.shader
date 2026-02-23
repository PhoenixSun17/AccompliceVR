Shader "Custom/Stencil"
{
Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            // Stencil setup for writing
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
            ZWrite Off
            ColorMask 0
        }

        Pass
        {
            // Use stencil buffer to cut the hole
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float2 _HoleCenter; // Center of the hole in UV space
            float _HoleRadius;  // Radius of the hole in UV space

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate distance from the hole center
                float dist = distance(i.uv, _HoleCenter);

                // If inside the hole radius, discard the fragment
                if (dist < _HoleRadius)
                {
                    discard;
                }

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
