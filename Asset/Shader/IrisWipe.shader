Shader "Custom/IrisWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)

        _Radius ("Radius", Float) = 2

        _Center ("Center", Vector) = (0.5,0.5,0,0)

        _Softness ("Softness", Range(0.001,0.2)) = 0.03

        _Aspect ("Aspect", Float) = 1.777777
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;

            float _Radius;
            float4 _Center;
            float _Softness;
            float _Aspect;

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.positionHCS =
                    TransformObjectToHClip(v.positionOS.xyz);

                o.uv = v.uv;

                o.color = v.color * _Color;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                float2 center = _Center.xy;

                float2 delta = uv - center;

                // 保证圆不会因为屏幕比例变椭圆
                delta.x *= _Aspect;

                float dist = length(delta);

                float alpha =
                    smoothstep(
                        _Radius - _Softness,
                        _Radius + _Softness,
                        dist);

                half4 col =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        i.uv);

                col *= i.color;

                // 黑色遮罩
                col.rgb = 0;

                col.a *= alpha;

                return col;
            }

            ENDHLSL
        }
    }
}