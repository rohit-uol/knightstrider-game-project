// Custom/SpriteDissolve
// Built-in Render Pipeline - Unlit, transparent, sprite-compatible dissolve shader.
// Assign a greyscale noise texture to _NoiseTex; pixels whose noise value falls
// below _DissolveAmount are clipped, creating a progressive shatter/erode effect.
// Pixels near the dissolve edge receive a two-tone fire glow (hot core -> orange rim)
// with a time-based flicker so the cracks look like burning/glowing fire.

Shader "Custom/SpriteDissolve"
{
    Properties
    {
        _MainTex        ("Sprite Texture",         2D)           = "white" {}
        _NoiseTex       ("Noise Texture",          2D)           = "white" {}
        _DissolveAmount ("Dissolve Amount",         Range(0, 1))  = 0
        _Color          ("Tint",                   Color)        = (1, 1, 1, 1)

        [Header(Fire Glow Edge)]
        _EdgeWidth      ("Edge Glow Width",        Range(0, 0.4)) = 0.12
        _GlowIntensity  ("Glow Intensity",         Range(1, 6))   = 2.5
        _CoreColor      ("Edge Core Color (inner)",Color)        = (1, 0.95, 0.4, 1)
        _RimColor       ("Edge Rim Color (outer)", Color)        = (1, 0.15, 0, 1)
        _FlickerSpeed   ("Flicker Speed",          Range(0, 30))  = 12
        _FlickerAmount  ("Flicker Amount",         Range(0, 0.5)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float     _DissolveAmount;
            fixed4    _Color;

            float  _EdgeWidth;
            float  _GlowIntensity;
            fixed4 _CoreColor;
            fixed4 _RimColor;
            float  _FlickerSpeed;
            float  _FlickerAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col  = tex2D(_MainTex,  i.uv) * i.color;
                float  noise = tex2D(_NoiseTex, i.uv).r;

                // Distance above the dissolve threshold (0 = right at the clip edge)
                float edgeDist = noise - _DissolveAmount;

                // Clip pixels that have dissolved away
                clip(edgeDist);

                // --- Fire glow band ---
                if (_EdgeWidth > 0.0 && edgeDist < _EdgeWidth)
                {
                    // t = 0 at the inner clip edge, 1 at the outer edge of the band
                    float t = edgeDist / _EdgeWidth;

                    // Per-pixel flicker: varies with UV position + time
                    float flicker = 1.0 + _FlickerAmount *
                        sin(_Time.y * _FlickerSpeed + noise * 25.0 + i.uv.y * 15.0);

                    // Two-tone gradient: hot white-yellow core -> orange-red rim
                    fixed4 glowColor = lerp(_CoreColor, _RimColor, t);

                    // Intensity falls off toward the outer edge (smoothstep keeps it crisp near clip)
                    float glowStrength = (1.0 - smoothstep(0.0, 1.0, t)) * _GlowIntensity * flicker;

                    // Blend glow over sprite; fully glowing at inner edge, fading to sprite at outer
                    col.rgb = lerp(col.rgb, glowColor.rgb * glowStrength, 1.0 - t);
                    col.a   = 1.0;
                }

                return col;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
