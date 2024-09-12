Shader "Custom/TransparentObject"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        _Radius ("Effect Radius", Float) = 0.5
        _Transparency ("Transparency", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        float4 _PlayerPos;
        float _Radius;
        float _Transparency;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Рассчитываем расстояние от игрока до объекта
            float dist = distance(_PlayerPos.xyz, IN.worldPos);
            
            // Если объект в пределах радиуса, то делаем его прозрачным
            if (dist < _Radius)
            {
                c.a = lerp(1, _Transparency, dist / _Radius);
            }

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
