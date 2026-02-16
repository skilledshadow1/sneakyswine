Shader "Unlit/GlowingOutlineShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 1) // Yellow color
        _OutlineWidth ("Outline Width", Range(0.0, 0.03)) = 0.01
    }

    SubShader
    {
        // Pass to draw the outline
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite On
            ZTest LEqual
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            uniform float _OutlineWidth;
            uniform float4 _OutlineColor;

            v2f vert(appdata v)
            {
                // Expand the vertex position along the normal direction to create the outline
                v2f o;
                float3 norm = normalize(v.normal);
                float4 offset = float4(norm * _OutlineWidth, 0);
                o.pos = UnityObjectToClipPos(v.vertex + offset);
                o.color = _OutlineColor;
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }

        // Main texture pass to draw the original object
        Pass
        {
            Name "Base"
            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask RGB

            SetTexture [_MainTex] { combine texture * primary }
        }
    }
    FallBack "Diffuse"
}