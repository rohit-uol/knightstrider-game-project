Shader "Custom/Knight"
{
	Properties
	{
		[HideInInspector] _MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue"="Overlay" "RenderType"="Transparent" }
		
		Stencil
		{
			Ref 1
			Comp Always
			Pass IncrSat
		}

		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always

			HLSLPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			struct appdata 
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex: SV_POSITION;
				float2 uv: TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag(v2f i): SV_TARGET
			{
				float4 color = tex2D(_MainTex, i.uv);
				return color;
			}
			ENDHLSL
		}
	}
	Fallback "Sprites/Default"
}