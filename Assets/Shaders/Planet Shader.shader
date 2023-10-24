Shader "Custom/Planet Shader"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _ElevationMin ("_ElevationMin", float) = 0
        _ElevationMax ("_ElevationMax", float) = 1
                [Header(Lighting Parameters)]
        [HDR]
        _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        [HDR]
        _SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
        [HDR]
        _ShadowColor("Shadow Color", Color) = (0,0,0,1)
        // Controls the size of the specular reflection.
        _Glossiness("Glossiness", Float) = 32
        [HDR]
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        // Control how smoothly the rim blends when approaching unlit
        // parts of the surface.
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1	

        _ShadowFuzz("Shadow Fuzz", Range(0.01, 1)) = 0.01

        _LightPos("Light Pos", vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
        //LOD 200
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            // Files below include macros and functions to assist
            // with lighting and shadows.
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : NORMAL;
                float height : TEXCOORD1;
                // Macro found in Autolight.cginc. Declares a vector4
                // into the TEXCOORD2 semantic with varying precision 
                // depending on platform target.
                SHADOW_COORDS(2)

                float3 viewDir : TEXCOORD3;	

                float3 lightDir : TexCoord4;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ElevationMax;
            float _ElevationMin;
            float3 _LightPos;
            
            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);		
                o.viewDir = WorldSpaceViewDir(v.vertex);
                //float4 normal = float4(v.normal * 0.5 + 0.5, 1);
                float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;
                float3 objOrigin = mul (unity_ObjectToWorld, float4(0, 0, 0, 1));
                float worldDistance = distance(worldPos, objOrigin);
                float maxSize = _ElevationMax  - _ElevationMin;
                float localPos = saturate((worldDistance - _ElevationMin) / maxSize);
                o.height = localPos;
                
                o.lightDir = normalize(_LightPos - worldPos);

                return o;
            }

            float4 _AmbientColor;
            float4 _ShadowColor;

            float4 _SpecularColor;
            float _Glossiness;		

            float4 _RimColor;
            float _RimAmount;
            float _RimThreshold;	
            float _ShadowFuzz;
            
            float4 frag (v2f i) : SV_Target
            {
                //return i.height;
                float3 fakeLightDir = i.lightDir;


                float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				// Lighting below is calculated using Blinn-Phong,
				// with values thresholded to creat the "toon" look.
				// https://en.wikipedia.org/wiki/Blinn-Phong_shading_model

				// Calculate illumination from directional light.
				// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
				// direction of the main directional light.
				//float NdotL = dot(_WorldSpaceLightPos0, normal);
				float NdotL = dot(fakeLightDir, normal);

				// Samples the shadow map, returning a value in the 0...1 range,
				// where 0 is in the shadow, and 1 is not.
				//float shadow = SHADOW_ATTENUATION(i);
				float shadow = 1;
				// Partition the intensity into light and dark, smoothly interpolated
				// between the two to avoid a jagged break.
				float lightIntensity = smoothstep(0, _ShadowFuzz, NdotL * shadow);	
				// Multiply by the main directional light's intensity and color.
				float4 light = lightIntensity; //* _LightColor0;

                float4 color = lerp(_ShadowColor, _AmbientColor, lightIntensity);

				// Calculate specular reflection.
				//float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float3 halfVector = normalize(fakeLightDir + viewDir);
				float NdotH = dot(normal, halfVector);
				// Multiply _Glossiness by itself to allow artist to use smaller
				// glossiness values in the inspector.
				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;				

				// Calculate rim lighting.
				float rimDot = 1 - dot(viewDir, normal);
				// We only want rim to appear on the lit side of the surface,
				// so multiply it by NdotL, raised to a power to smoothly blend it.
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;


                float biome = i.uv.x;
                float4 c = tex2D (_MainTex, float2(i.height, biome));

				return (light + color + specular + rim) * c;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
