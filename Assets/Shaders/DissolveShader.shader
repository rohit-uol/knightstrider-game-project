// Custom/SpriteDissolve
// Built-in Render Pipeline - Unlit, transparent, sprite-compatible dissolve shader.
// Assign a greyscale noise texture to _NoiseTex; pixels whose noise value falls
// below _DissolveAmount are clipped, creating a progressive shatter/erode effect.

Shader "Custom/SpriteDissolve"
{
    Properties
    {
        _MainTex      ("Sprite Texture", 2D) = "white" {}
        _NoiseTex     ("Noise Texture",  2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _Color        ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;        // per-vertex sprite tint/alpha
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
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
                fixed4 col   = tex2D(_MainTex,  i.uv) * i.color;

                // Sample the red channel of the noise texture as the cutoff value
                float  noise = tex2D(_NoiseTex, i.uv).r;

                // Discard pixels whose noise value is below the dissolve threshold.
                // A hard edge (sharp noise texture) = shatter; soft noise = erode/sand.
                clip(noise - _DissolveAmount);

                return col;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
