Shader "Sprites/GaussianBlurSeparable"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0,5)) = 1.0
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200

        Cull Off
        Lighting Off
        ZWrite On
        Blend Off

        // --- Pass 1: Horizontal Blur ---
        Pass
        {
            Name "GaussianBlurHorizontal"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _BlurSize;
            float _AlphaCutoff;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                float2 offset = float2(_MainTex_TexelSize.x * _BlurSize, 0);

                fixed4 col = tex2D(_MainTex, uv) * 0.227027;
                col += tex2D(_MainTex, uv + offset) * 0.316216;
                col += tex2D(_MainTex, uv - offset) * 0.316216;
                col += tex2D(_MainTex, uv + offset * 2.0) * 0.070270;
                col += tex2D(_MainTex, uv - offset * 2.0) * 0.070270;

                if (col.a < _AlphaCutoff)
                    clip(-1);

                return col * i.color;
            }
            ENDCG
        }

        // --- Pass 2: Vertical Blur ---
        Pass
        {
            Name "GaussianBlurVertical"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _BlurSize;
            float _AlphaCutoff;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                float2 offset = float2(0, _MainTex_TexelSize.y * _BlurSize);

                fixed4 col = tex2D(_MainTex, uv) * 0.227027;
                col += tex2D(_MainTex, uv + offset) * 0.316216;
                col += tex2D(_MainTex, uv - offset) * 0.316216;
                col += tex2D(_MainTex, uv + offset * 2.0) * 0.070270;
                col += tex2D(_MainTex, uv - offset * 2.0) * 0.070270;

                if (col.a < _AlphaCutoff)
                    clip(-1);

                return col * i.color;
            }
            ENDCG
        }
    }
}
