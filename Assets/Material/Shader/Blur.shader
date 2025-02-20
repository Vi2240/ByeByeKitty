Shader "UI/Blur" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Radius", Range(0, 0.1)) = 0.02
        _Intensity ("Blur Quality", Range(1, 15)) = 5
    }
    
    SubShader {
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
                float sigma = _Intensity * 0.5;
                float twoSigma2 = 2.0 * sigma * sigma;
                float kernelSize = ceil(2.0 * sigma);
                float weightSum = 0.0;
                fixed4 col = fixed4(0,0,0,0);

                // Combined 2D sampling
                for(float x = -kernelSize; x <= kernelSize; x++) {
                    for(float y = -kernelSize; y <= kernelSize; y++) {
                        float distance = sqrt(x*x + y*y);
                        float weight = exp(-(distance * distance) / twoSigma2);
                        float2 offset = float2(x, y) * _BlurSize;
                        col += tex2D(_MainTex, i.uv + offset) * weight;
                        weightSum += weight;
                    }
                }

                return col / weightSum;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}