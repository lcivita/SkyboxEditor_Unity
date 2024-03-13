Shader "Custom/SkyboxProcedural"
{
    Properties
    {
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _GradientRes ("Gradient Resolution", Float) = 50
    }
    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _GradientTex;
            float _GradientRes;

            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // vertex pos to 0-1 range
                o.uv = v.vertex.xyz * 0.5 + 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_GradientTex, i.uv);
                
                return color;
            }
            ENDCG
        }
    }
}
