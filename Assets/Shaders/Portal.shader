Shader "Portals/Portal" 
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Cull Off

		Offset -1, -1

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
			int _DisplayMask;

			v2f vert (appdata_base v) 
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeScreenPos(o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target 
			{
				fixed4 col = tex2D(_MainTex, i.uv.xy / i.uv.w);
				if (_DisplayMask == 0) 
				{
					return float4(0, 0, 0, 0);
				} else 
				{
					return col;
				}
			}

			ENDCG
		}
	}
}