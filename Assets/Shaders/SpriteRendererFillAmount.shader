Shader "Hidden/SpriteRendererFillAmount"
{
    Properties
    {
        [NoScaleOffset]
        _MainTex ("Texture", 2D) = "white" {}
        _FillAmount ("FillAmount", Range(0, 1)) = 0
        [MaterialToggle]
        _Reverse ("Reverse", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
 
        Blend SrcAlpha OneMinusSrcAlpha
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color: COLOR;
                float2 uv : TEXCOORD0;
            };
 
            sampler2D _MainTex;
            float _FillAmount;
            float _Reverse;
 
           v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(
                    lerp(
                        -i.uv.y + _FillAmount,
                        i.uv.y - (1 - _FillAmount),
                        _Reverse
                    )
                );
                return col;
            }
            ENDCG
        }
    }
}