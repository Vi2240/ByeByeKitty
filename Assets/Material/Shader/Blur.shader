Shader "UI/Blur" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Radius", Range(0, 0.15)) = 0.02
        _Intensity ("Blur Intensity", Range(1, 30)) = 15 // Increased max to 30
    }
    
    SubShader {
        Tags { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Off
        Cull Off
        ZWrite Off

        CGINCLUDE
        #include "UnityCG.cginc"
        
        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };
        ENDCG

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_horizontal

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

            fixed4 frag_horizontal (v2f i) : SV_Target {
                float sigma = _Intensity * 0.25; // Adjusted for better high-intensity control
                float twoSigma2 = 2.0 * sigma * sigma;
                float sqrtTwoPiSigma2 = sqrt(6.283185307 * twoSigma2);
                
                fixed4 col = (fixed4)0;
                float weightSum = 0.0;
                int kernelSize = int(ceil(2.5 * sigma)); // Optimized kernel size
                
                [loop]
                for(int j = -kernelSize; j <= kernelSize; j++) {
                    float distance = abs(j);
                    float weight = exp(-(distance * distance) / twoSigma2) / sqrtTwoPiSigma2;
                    float2 uv = i.uv + float2(_BlurSize * j * 0.1, 0); // Fine-tuned offset
                    col += tex2D(_MainTex, uv) * weight;
                    weightSum += weight;
                }
                
                return weightSum > 0.001 ? col / weightSum : tex2D(_MainTex, i.uv);
            }
            ENDCG
        }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_vertical

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

            fixed4 frag_vertical (v2f i) : SV_Target {
                float sigma = _Intensity * 0.25;
                float twoSigma2 = 2.0 * sigma * sigma;
                float sqrtTwoPiSigma2 = sqrt(6.283185307 * twoSigma2);
                
                fixed4 col = (fixed4)0;
                float weightSum = 0.0;
                int kernelSize = int(ceil(2.5 * sigma));
                
                [loop]
                for(int j = -kernelSize; j <= kernelSize; j++) {
                    float distance = abs(j);
                    float weight = exp(-(distance * distance) / twoSigma2) / sqrtTwoPiSigma2;
                    float2 uv = i.uv + float2(0, _BlurSize * j * 0.1);
                    col += tex2D(_MainTex, uv) * weight;
                    weightSum += weight;
                }
                
                return weightSum > 0.001 ? col / weightSum : tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}