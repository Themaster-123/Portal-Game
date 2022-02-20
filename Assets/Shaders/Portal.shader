Shader "Portals/Portal" 
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_MaskID ("Mask ID", Int) = 1
	}

	SubShader
	{
		Cull Off

		Tags
		{ 
			"RenderType" = "Opaque" 
			"Queue" = "Overlay+1"
		}

		Stencil 
		{
			Ref [_MaskID]
			Comp Equal
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

			v2f vert (appdata_base v) 
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target 
			{
				fixed4 col = tex2D(_MainTex, i.uv.xy);
				return col;
			}

			ENDCG
		}
	}
}