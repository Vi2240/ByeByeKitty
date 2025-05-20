Shader "Custom/Charred"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1) // Standard sprite tint
        _BurnAmount ("Burn Amount (0=Normal, 1=Burned)", Range(0, 1)) = 0
        _BurnColor ("Burn Color", Color) = (0.1, 0.1, 0.1, 1) // Dark grey/ash color
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane" // Makes it preview correctly on a quad in material inspector
        }
        LOD 100

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color; // Overall tint from SpriteRenderer
            float _BurnAmount;
            fixed4 _BurnColor;
            float4 _MainTex_ST; // For texture tiling/offset

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color; // Apply SpriteRenderer's color and vertex color
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.texcoord) * i.color;

                // If original pixel is transparent, keep it transparent
                if (originalColor.a < 0.01) {
                    discard; // or return fixed4(0,0,0,0);
                }

                // Lerp the RGB values towards the burn color based on _BurnAmount
                // Keep the original alpha value
                fixed3 burnedRgb = lerp(originalColor.rgb, _BurnColor.rgb, _BurnAmount);
                
                return fixed4(burnedRgb, originalColor.a);
            }
            ENDCG
        }
    }
}