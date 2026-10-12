Shader "Custom/PixelWater"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}

        [IntRange] _PixelSize ("Pixel Size", Range(1,16)) = 4
        _Seed ("Seed", Range(0,100)) = 3
        _Speed ("Speed", Range(0,1)) = 0.125
        _Scale ("Scale", Range(0.5,8)) = 2
        _FlowAngle ("Flow Angle", Range(-3.14,3.14)) = 0.5
        _Turbulence ("Turbulence", Range(0,2)) = 0.8
        _Depth ("Depth", Range(-0.5,0.5)) = 0
        _CrestAmount ("Crest Amount", Range(0,1)) = 0.5
        _LandAmount ("Land Amount", Range(0,1)) = 0.35
        _FoamWidth ("Foam Width", Range(0,1)) = 0.5
        _ShallowWidth ("Shallow Width", Range(0,1)) = 0.5
        _Swell ("Swell", Range(0,1)) = 0.5
        _CloudShadow ("Cloud Shadow", Range(0,1)) = 0.5
        _SparkleAmount ("Sparkle Amount", Range(0,1)) = 0.5

        // Used by the baker: when on, time is taken from _ManualPhase (0..1 = one full loop)
        [Toggle] _UseManualPhase ("Use Manual Phase", Float) = 0
        _ManualPhase ("Manual Phase", Range(0,1)) = 0

        [Enum(Custom_Ocean,0,Tropical,1,Lagoon,2,Swamp,3,Night,4)]
        _Preset ("Color Preset", Float) = 0

        _ColorDeep ("Deep Water", Color) = (0.067,0.188,0.384,1)
        _ColorMid ("Mid Water", Color) = (0.114,0.318,0.553,1)
        _ColorLight ("Light Water", Color) = (0.208,0.482,0.718,1)
        _ColorCrest ("Wave Crest", Color) = (0.714,0.89,0.953,1)
        _ColorShallow ("Shallow Water", Color) = (0.412,0.682,0.8,1)
        _ColorFoam ("Foam", Color) = (0.941,0.973,0.984,1)
        _ColorSand ("Sand", Color) = (0.867,0.784,0.545,1)
        _ColorGrass ("Grass", Color) = (0.384,0.612,0.286,1)
        _ColorForest ("Forest", Color) = (0.259,0.439,0.239,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            float _PixelSize;
            float _Seed, _Speed, _Scale, _FlowAngle;
            float _Turbulence, _Depth, _CrestAmount;
            float _LandAmount, _FoamWidth, _ShallowWidth;
            float _Swell, _CloudShadow, _SparkleAmount;
            float _Preset;
            float _UseManualPhase, _ManualPhase;

            float4 _ColorDeep, _ColorMid, _ColorLight;
            float4 _ColorCrest, _ColorShallow, _ColorFoam;
            float4 _ColorSand, _ColorGrass, _ColorForest;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Renamed from "hash" -> "hash21" to avoid any name clashes
            float hash21(float2 p)
            {
                p = frac(p * float2(0.1031, 0.1030) + _Seed * 0.013);
                p += dot(p, p.yx + 33.33);
                return frac((p.x + p.y) * p.x);
            }

            // Renamed from "noise" -> "vnoise": "noise" is a reserved HLSL intrinsic
            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1,0));
                float c = hash21(i + float2(0,1));
                float d = hash21(i + float2(1,1));

                return lerp(lerp(a,b,f.x), lerp(c,d,f.x), f.y);
            }

            float flowNoise(float2 p, float2 dir, float phase)
            {
                float f = frac(phase);
                float a = vnoise(p - dir * f);
                float b = vnoise(p - dir * (f - 1.0));
                return lerp(a,b,f);
            }

            float bayer4(float2 c)
            {
                int x = (int)fmod(abs(c.x), 4.0);
                int y = (int)fmod(abs(c.y), 4.0);
                int idx = y * 4 + x;

                if (idx == 0)  return 0.5/16;
                if (idx == 1)  return 8.5/16;
                if (idx == 2)  return 2.5/16;
                if (idx == 3)  return 10.5/16;
                if (idx == 4)  return 12.5/16;
                if (idx == 5)  return 4.5/16;
                if (idx == 6)  return 14.5/16;
                if (idx == 7)  return 6.5/16;
                if (idx == 8)  return 3.5/16;
                if (idx == 9)  return 11.5/16;
                if (idx == 10) return 1.5/16;
                if (idx == 11) return 9.5/16;
                if (idx == 12) return 15.5/16;
                if (idx == 13) return 7.5/16;
                if (idx == 14) return 13.5/16;
                return 5.5/16;
            }

            // k: 0 deep, 1 mid, 2 light, 3 shallow,
            // 4 crest, 5 foam, 6 sand, 7 grass, 8 forest.
            float3 palette(int k)
            {
                if (_Preset < 0.5)
                {
                    if (k==0) return _ColorDeep.rgb;
                    if (k==1) return _ColorMid.rgb;
                    if (k==2) return _ColorLight.rgb;
                    if (k==3) return _ColorShallow.rgb;
                    if (k==4) return _ColorCrest.rgb;
                    if (k==5) return _ColorFoam.rgb;
                    if (k==6) return _ColorSand.rgb;
                    if (k==7) return _ColorGrass.rgb;
                    return _ColorForest.rgb;
                }

                // Tropical
                if (_Preset < 1.5)
                {
                    if (k==0) return float3(.039,.290,.541);
                    if (k==1) return float3(.059,.482,.710);
                    if (k==2) return float3(.098,.690,.788);
                    if (k==3) return float3(.435,.890,.839);
                    if (k==4) return float3(.824,.984,.949);
                    if (k==5) return float3(1,1,1);
                    if (k==6) return float3(.949,.886,.659);
                    if (k==7) return float3(.498,.761,.290);
                    return float3(.247,.569,.251);
                }

                // Lagoon
                if (_Preset < 2.5)
                {
                    if (k==0) return float3(.059,.239,.251);
                    if (k==1) return float3(.102,.400,.420);
                    if (k==2) return float3(.200,.600,.580);
                    if (k==3) return float3(.450,.800,.700);
                    if (k==4) return float3(.800,.970,.900);
                    if (k==5) return float3(.941,.973,.984);
                    if (k==6) return float3(.930,.850,.600);
                    if (k==7) return float3(.384,.612,.286);
                    return float3(.259,.439,.239);
                }

                // Swamp
                if (_Preset < 3.5)
                {
                    if (k==0) return float3(.110,.165,.110);
                    if (k==1) return float3(.184,.290,.165);
                    if (k==2) return float3(.310,.420,.210);
                    if (k==3) return float3(.490,.540,.270);
                    if (k==4) return float3(.660,.690,.420);
                    if (k==5) return float3(.790,.770,.540);
                    if (k==6) return float3(.420,.350,.230);
                    if (k==7) return float3(.290,.420,.165);
                    return float3(.180,.290,.130);
                }

                // Night
                if (k==0) return float3(.020,.030,.100);
                if (k==1) return float3(.043,.078,.200);
                if (k==2) return float3(.090,.150,.310);
                if (k==3) return float3(.160,.250,.470);
                if (k==4) return float3(.500,.620,.880);
                if (k==5) return float3(.780,.840,1.000);
                if (k==6) return float3(.230,.230,.360);
                if (k==7) return float3(.120,.230,.290);
                return float3(.080,.150,.230);
            }

            float4 frag(v2f i) : SV_Target
            {
                // Virtual 128x128 pixel-art canvas.
                float2 localPos = i.uv * 128.0;
                float px = max(1.0, floor(_PixelSize + 0.5));

                float2 pos =
                    (floor(localPos / px) + 0.5) / 128.0
                    * px * _Scale;

                float phase = _UseManualPhase > 0.5
                    ? _ManualPhase
                    : _Time.y * _Speed;
                float2 dir = float2(cos(_FlowAngle), sin(_FlowAngle));

                float3 deep    = palette(0);
                float3 mid     = palette(1);
                float3 lightC  = palette(2);
                float3 shallow = palette(3);
                float3 crest   = palette(4);
                float3 foam    = palette(5);
                float3 sand    = palette(6);
                float3 grass   = palette(7);
                float3 forest  = palette(8);

                float warp = flowNoise(pos * 0.5 + 5.0, dir, phase) - 0.5;

                float2 bent = pos
                    + float2(warp, -warp) * _Turbulence * 1.5;

                float h =
                    flowNoise(bent * 0.8, dir, phase) * 0.75
                    + flowNoise(bent * 1.7 + 9.0, dir * 2.0, phase) * 0.25;

                h += _Swell * 0.09 * sin(
                    dot(pos, dir) * 4.0
                    - phase * 6.2831853 + warp * 4.0);

                h += _Depth;

                float3 col = deep;
                if (h > 0.42) col = mid;
                if (h > 0.54) col = lightC;

                float level = _LandAmount < 0.01
                    ? 9.0
                    : lerp(0.8, 0.4, _LandAmount);

                float l = vnoise(pos * 0.45 + 40.0) * 0.7
                        + vnoise(pos * 1.1 + 70.0) * 0.3;

                float d = level - l;

                if (d > 0.0)
                {
                    float2 cell = floor(localPos / px);
                    float bayer = bayer4(cell);

                    float dd = d * 2.2
                        / max(_ShallowWidth, 0.05)
                        + (bayer - 0.5) * 0.12;

                    float3 shoreCol = dd < 0.12 ? shallow :
                                      dd < 0.30 ? lightC :
                                      dd < 0.55 ? mid : deep;

                    if (dd < 0.30) col = shoreCol;

                    float wavePos = frac(
                        d * 9.0 / max(_FoamWidth, 0.05) * 0.5
                        - phase * 2.0 + l * 3.0);

                    float thin = clamp(d / 0.05, 0.35, 1.0);

                    float lap = step(wavePos, 0.17 * thin)
                        * step(d, 0.09) * step(0.012, d);

                    if (lap > 0.5) col = foam;

                    float breathe = 0.5 + 0.5 * sin(
                        phase * 6.2831853 + l * 30.0);

                    float foamW = (0.010 + 0.014 * breathe)
                        * _FoamWidth * 2.0;

                    if (d < foamW) col = foam;

                    // "line" is a reserved HLSL keyword -> renamed
                    float crestLine = step(
                        abs(flowNoise(bent * 1.6 + 21.0,
                                      dir * 3.0, phase) - 0.5),
                        0.035 * _CrestAmount * 2.0)
                        * step(0.45, h);

                    if (dd > 0.55)
                        col = lerp(col, crest, crestLine);
                }
                else
                {
                    col = -d < 0.02 ? sand :
                          -d < 0.08 ? grass : forest;
                }

                float cs = flowNoise(
                    pos * 0.28 + 60.0, dir * 0.5, phase);

                float shade = step(
                    lerp(0.85, 0.45, _CloudShadow), cs);

                col = lerp(col,
                    col * float3(0.78, 0.82, 0.92), shade);

                if (d > 0.0 && _SparkleAmount > 0.0)
                {
                    float2 sc = floor(localPos / px);
                    float t = frac(phase * 3.0 + hash21(sc));

                    float on = step(
                        hash21(sc + 17.0),
                        0.012 * _SparkleAmount * 4.0)
                        * step(t, 0.12);

                    col = lerp(col, foam, on);
                }

                return float4(col, 1.0);
            }
            ENDCG
        }
    }

    FallBack Off
}
