Shader "Outline/Highlight"
{
    Properties{ _OutlineColor("Color", Color) = (1, 0.5, 0, 1) }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry+100" }
        Pass
        {
            Cull Front
            ZWrite Off
            HLSLPROGRAM
            float4 _OutlineColor;
            float4 vert(float4 pos : POSITION) : SV_POSITION { return UnityObjectToClipPos(pos * 1.05); }
            float4 frag() : SV_Target { return _OutlineColor; }
            ENDHLSL
        }
    }
}