Shader "Custom/GridTransition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Transition Progress", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Progress;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            float2 rotate(float2 uv, float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float2(
                    c * uv.x - s * uv.y,
                    s * uv.x + c * uv.y
                );
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 resolution = _ScreenParams.xy;
                float aspect = resolution.y / resolution.x;

                // screen UV
                float2 fragCoord = i.screenPos.xy / i.screenPos.w;
                fragCoord *= resolution;

                float2 uv = fragCoord / resolution.x;

                uv -= float2(0.5, 0.5 * aspect);

                float rot = radians(45.0);
                uv = rotate(uv, rot);

                uv += float2(0.5, 0.5 * aspect);
                uv.y += 0.5 * (1.0 - aspect);

                float2 pos = 10.0 * uv;
                float2 rep = frac(pos);

                float dist = 2.0 * min(
                    min(rep.x, 1.0 - rep.x),
                    min(rep.y, 1.0 - rep.y)
                );

                float squareDist = length(floor(pos) + 0.5 - float2(5.0, 5.0));

                float t = _Time.y;

                float edge = (t - squareDist * 0.5) * 0.5;
                edge = 2.0 * frac(edge * 0.5);
                edge = pow(abs(1.0 - edge), 2.0);

                float value = frac(dist * 2.0);
                value = smoothstep(edge - 0.05, edge, 0.95 * value);

                value += squareDist * 0.1;

                float3 col = lerp(
                    float3(1,1,1),
                    float3(0.5,0.75,1),
                    value
                );

                float alpha = 0.25 * saturate(value);

                return float4(col, alpha);
            }

            ENDHLSL
        }
    }
}