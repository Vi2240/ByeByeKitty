Shader "UI/Blur" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Spread Factor", Range(0.000, 0.01)) = 0.001
        // Increased range for _Intensity. Higher values = more samples, smoother for subtle blurs.
        _Intensity ("Blur Smoothness (Sigma)", Range(0.5, 15.0)) = 2.5 // Increased default and max
    }

    SubShader {
        // ... (rest of the SubShader remains the same as the previous good version) ...
        // Make sure the Pass with the CGPROGRAM block is the one from the previous iteration
        // where _BlurSize was a small UV multiplier and _Intensity controlled sigma/kernelRadius.

        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Off
        Cull Off
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurSize;
            float _Intensity;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float sigma = max(0.1, _Intensity);
                float twoSigma2 = 2.0 * sigma * sigma;
                int kernelRadius = int(ceil(2.5 * sigma));

                float weightSum = 0.0;
                fixed4 col = fixed4(0,0,0,0);

                for(int y = -kernelRadius; y <= kernelRadius; y++) {
                    for(int x = -kernelRadius; x <= kernelRadius; x++) {
                        float distSq = float(x*x + y*y);
                        float weight = exp(-distSq / twoSigma2);
                        float2 uvOffset = float2(x * _BlurSize, y * _BlurSize);
                        col += tex2D(_MainTex, i.uv + uvOffset) * weight;
                        weightSum += weight;
                    }
                }

                if (weightSum == 0.0 || weightSum < 0.00001) {
                    return tex2D(_MainTex, i.uv);
                }
                return col / weightSum;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}