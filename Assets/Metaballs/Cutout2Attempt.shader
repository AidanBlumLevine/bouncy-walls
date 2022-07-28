Shader "Custom/Transparect Cutout 2" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _GColor ("Green color", Color) = (1,1,1,1)
        _BColor ("Blue color", Color) = (1,1,1,1)
        _RColor ("Red color", Color) = (1,1,1,1)
        _SplatColor ("Splat color", Color) = (1,1,1,1)
        _Stroke ("Stroke alpha", Range(0,1)) = 0.1
        _StrokeColor ("Stroke color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
        LOD 100

        Lighting Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _Cutoff;
            half4 _RColor;
            half4 _GColor;
            half4 _BColor;
            fixed _Stroke;
            half4 _StrokeColor;
            half4 _SplatColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                clip(col.a - _Cutoff);
                if(col.r > col.g && col.r > col.b){
                    //clip(col.r - _Cutoff);
                    if (col.r < _Stroke) {
                        col = _StrokeColor;
                    } else {
                        col = _RColor;
                    }
                }
                if(col.b > col.g && col.b > col.r){
                    //clip(col.b - _Cutoff);
                    if (col.b < _Stroke) {
                        col = _StrokeColor;
                    } else {
                        col = _BColor;
                    }
                }
                if(col.g > col.b && col.g > col.r){
                    //clip(col.g - _Cutoff);
                    if (col.g < _Stroke) {
                        col = _StrokeColor;
                    } else {
                        col = _GColor;    
                    }
                }
                // else {
                    //     if(col.g > col.b){
                        //         if(col.r > col.g){
                            //             col = _RColor;
                        //         }
                        //         else {
                            //             col = _GColor;
                        //         }
                    //     } 
                    //     else {
                        //         if(col.r > col.b){
                            //             col = _RColor;
                        //         } 
                        //         else {
                            //             col = _BColor;
                        //         }
                    //     }
                // }
                // half sp = length(abs(col.rgb - _SplatColor.rgb));
                // if(sp < .02){
                    //     col = _SplatColor;
                // } 
                // else if(sp < .04){
                    //     col = _StrokeColor;
                // }
                return col;
            }
            ENDCG
        }
    }

}
