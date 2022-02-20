Shader "Portals/Portal Mask" 
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MaskID ("Mask ID", Int) = 1
	}

	SubShader
	{
		Cull Off

		ColorMask 0

		Offset -1, -1

		Tags
		{ 
			"RenderType" = "Opaque" 
			"Queue" = "Overlay"
		}

		//ColorMask 0

		Stencil 
		{
			Ref [_MaskID]
			Comp Always
			Pass Replace
		}

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;

			v2f vert (appdata_base v) 
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target 
			{
				return _Color;
			}

			ENDCG
		}
	}
}