Shader "Custom/NewUnlitUniversalRenderPipelineShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _HashMultiplier ("Hash Multiplier", Vector) = (123.34, 456.21, 0, 0)
        _HashOffset ("Hash Offset", Float) = 45.32
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"   
            float4 _HashMultiplier;
            float _HashOffset;         
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
               
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            Varyings vert (appdata_base v)
            {
                Varyings o;
                o.positionHCS = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            half2 hash21(float2 p){
                p = frac(p * _HashMultiplier.xy);
                p += dot(p, p + _HashOffset);
                return frac(p.x * p.y);
            }
            float vnoise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3 - 2 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            float fbm(float2 p)
            {
                float v = 0, a = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += a * vnoise(p);
                    p *= 2;
                    a *= 0.5;
                }
                return v;
            }
            half4 frag(Varyings IN) : SV_Target
            {
                float gridSize = 8.0;
                float2 p = IN.worldPos.xy * gridSize;
                float n = hash21(floor(p));
                float n1 = vnoise(p);
                float f = fbm(n1);
                return float4(f,f,f,1);
            }
            ENDHLSL
        }
    }
}
