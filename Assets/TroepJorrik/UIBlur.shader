Shader "Custom/UIBlurFixed"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 0.05)) = 0.01
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [comp]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _BlurAmount;

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                float4 col = 0;
                float d = _BlurAmount;

                // 9-tap sampling of the SPRITE
                col += tex2D(_MainTex, IN.texcoord + float2(-d, -d));
                col += tex2D(_MainTex, IN.texcoord + float2(0, -d));
                col += tex2D(_MainTex, IN.texcoord + float2(d, -d));
                col += tex2D(_MainTex, IN.texcoord + float2(-d, 0));
                col += tex2D(_MainTex, IN.texcoord + float2(0, 0));
                col += tex2D(_MainTex, IN.texcoord + float2(d, 0));
                col += tex2D(_MainTex, IN.texcoord + float2(-d, d));
                col += tex2D(_MainTex, IN.texcoord + float2(0, d));
                col += tex2D(_MainTex, IN.texcoord + float2(d, d));

                return (col / 9) * IN.color;
            }
            ENDCG
        }
    }
}