Shader "UI/CircleWipe"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Progress("Progress", Range(0,0.6)) = 0.6
        _Center("Center", Vector) = (0.5,0.5,0,0)
        _Color("Tint Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4   _MainTex_ST;
            float    _Progress;
            float2   _Center;
            fixed4   _Color;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                // distância ao centro
                float d = distance(uv, _Center);

                // feather da borda
                float edge = 0.05;

                // Dentro do círculo (d < _Progress - edge): m=0 → transparente
                // Fora do círculo (d > _Progress): m=1 → opaco
                float m = smoothstep(_Progress - edge, _Progress, d);

                // cor base e tint
                fixed4 baseCol = tex2D(_MainTex, uv);
                fixed4 outCol = baseCol * _Color;
                // aplica a máscara no alpha
                outCol.a *= m;
                return outCol;
            }
            ENDCG
        }
    }
}
