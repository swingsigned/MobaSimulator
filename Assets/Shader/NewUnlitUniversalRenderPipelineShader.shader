Shader "Custom/PixelGrass"
{
    Properties
    {
        _ColDark  ("Dark",  Color) = (0.25, 0.55, 0.22, 1)
        _ColMid   ("Mid",   Color) = (0.55, 0.75, 0.22, 1)
        _ColLight ("Light", Color) = (0.72, 0.85, 0.30, 1)
        _ColSpark ("Spark", Color) = (0.95, 0.92, 0.55, 1)
        _Pixels   ("Pixel Grid (xy)", Vector) = (64, 180, 0, 0)
        _Scale    ("Noise Scale", Float) = 6
        _Edge     ("Edge Roughness", Float) = 0.12
        _SparkAmt ("Spark Density", Range(0.9, 1)) = 0.985
        _Scroll   ("Scroll", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _ColDark, _ColMid, _ColLight, _ColSpark;
            float4 _Pixels, _Scroll;
            float _Scale, _Edge, _SparkAmt;

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
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
                    p *= 2.0; a *= 0.5;
                }
                return v;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Pixelate
                float2 grid = _Pixels.xy;
                float2 cell = floor(i.uv * grid);
                float2 uv = (cell + 0.5) / grid;

                // 2. Noise nền (mảng lớn)
                float2 p = (uv + _Scroll.xy * _Time.y) * _Scale;
                float n = fbm(p);

                // 3. Nhiễu tần số cao để viền rách
                n += (hash21(cell) - 0.5) * _Edge;

                // 4. Chia 3 bậc màu
                fixed3 col = _ColDark.rgb;
                if (n > 0.42) col = _ColMid.rgb;
                if (n > 0.58) col = _ColLight.rgb;

                // 5. Chấm sáng hiếm
                if (hash21(cell * 1.7 + 11.3) > _SparkAmt && n > 0.5)
                    col = _ColSpark.rgb;

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}