// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Portals/PortalSlice"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _PortalThickness ("Portal Thickness", float) = 0.25
    }


    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200


        CGPROGRAM
        #pragma surface surf Standard addshadow
        #include "UnityCG.cginc"

        #pragma target 3.0

        static const int PORTAL_AMOUNT = 2;

        sampler2D _MainTex;
        fixed4 _Color;
        float4x4 _PortalInverseRotations[PORTAL_AMOUNT];
        float4 _PortalPositions[PORTAL_AMOUNT];
        float4 _PortalSizes[PORTAL_AMOUNT];
        float _PortalThickness;

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
            for (int i = 0; i < PORTAL_AMOUNT; i++) {
                float3 rotatedPos = mul(_PortalInverseRotations[i], IN.worldPos - _PortalPositions[i]) + _PortalPositions[i];
                float3 extents = float3(_PortalSizes[i].x, _PortalSizes[i].y, _PortalThickness) / 2;
                float3 min = _PortalPositions[i] - extents;
                float3 max = _PortalPositions[i] + extents;
                if (min.x <= rotatedPos.x && max.x >= rotatedPos.x && min.y <= rotatedPos.y && max.y >= rotatedPos.y && min.z <= rotatedPos.z && max.z >= rotatedPos.z) {
                    discard;
                }

            }
            //float portalDot = dot(_PortalDirection, (_PortalPosition + _PortalDirection * _SliceOffset) - IN.worldPos);
            //clip(portalDot);

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            //o.Alpha = portalDot;
        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
