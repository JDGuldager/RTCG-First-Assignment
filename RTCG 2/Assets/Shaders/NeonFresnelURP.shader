Shader "Custom/NeonFresnelURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 1, 1, 1)
        _EmissionStrength ("Emission Strength", Range(0, 25)) = 8
        _FresnelPower ("Fresnel Power", Range(0.1, 12)) = 4
        _PulseSpeed ("Pulse Speed", Range(0, 20)) = 2
        _PulsePhaseScale ("Pulse Phase (World Y)", Range(0, 10)) = 0.5
        _PulseMin ("Pulse Min", Range(0, 1)) = 0.2
        _PulseMax ("Pulse Max", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Geometry"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // Core URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _EmissionStrength;
                float _FresnelPower;
                float _PulseSpeed;
                float _PulsePhaseScale;
                float _PulseMin;
                float _PulseMax;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   norInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = NormalizeNormalPerVertex(norInputs.normalWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = SafeNormalize(GetWorldSpaceViewDir(IN.positionWS));

                // Fresnel rim: stronger at grazing angles
                float ndv = saturate(dot(normalWS, viewDirWS));
                float fresnel = pow(1.0 - ndv, _FresnelPower);

                // Pulse: time + world-space Y phase offset (gives spatial variation across grid)
                float t = _Time.y * _PulseSpeed + (IN.positionWS.y * _PulsePhaseScale);
                float s = sin(t);
                float pulse01 = (s + 1.0) * 0.5;                 // 0..1
                float pulse = lerp(_PulseMin, _PulseMax, pulse01); // clamp to nice range

                float3 emission = _BaseColor.rgb * fresnel * pulse * _EmissionStrength;

                // Unlit output (emission-driven). Bloom will pick this up if post-processing is enabled.
                return half4(emission, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}