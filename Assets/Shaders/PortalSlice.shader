Shader "Portals/PortalSlice"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SliceOffset ("Slice Offset", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200


        CGPROGRAM
        #pragma surface surf Standard addshadow
        #include "UnityCG.cginc"

        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float3 _PortalDirection;
        float3 _PortalPosition;
        float _SliceOffset;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };


        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float portalDot = dot(_PortalDirection, (_PortalPosition + _PortalDirection * _SliceOffset) - IN.worldPos);
            clip(portalDot);

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            //o.Alpha = portalDot;
        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
