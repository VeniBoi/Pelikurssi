﻿Shader "AltTrees/Leaves"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.3
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2

		[PerRendererData]_HueVariationLeave("Hue Variation Leave", Color) = (1.0,0.5,0.0,0.0)
		[PerRendererData]_Alpha("Alpha", Range(0,1)) = 1
		[PerRendererData]_Ind("Ind", Range(0,1)) = 0
			

		_SpecularColor("Specular", Color) = (0.2,0.2,0.2)
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.0
	}

	//  Unity 5 Render
	
	//  SM 3.0+
	SubShader
	{
		Tags { "RenderType"="LeavesAltTree" "IgnoreProjector" = "True" }
		Cull [_Cull]
		
		CGPROGRAM
			#pragma surface surf StandardSpecular nolightmap fullforwardshadows
			#pragma target 3.0
			#pragma multi_compile __ CROSSFADE
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			half _Cutoff;
			float _Alpha;
			float _Ind;
			half4 _HueVariationLeave;

			struct Input {
				float2 uv_MainTex;
				#ifdef CROSSFADE
					float4 screenPos;
				#endif
			};

			fixed4 _Color;

			fixed3 _SpecularColor;
			half _Glossiness;

			void surf (Input IN, inout SurfaceOutputStandardSpecular o)
			{
				#ifdef CROSSFADE
					CrossFadeUV(IN.screenPos, _Alpha, _Ind);
				#endif

				fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
				if (c.a <= _Cutoff)
					discard;

				o.Albedo = GetHue(c, _HueVariationLeave) * _Color.rgb;

				o.Specular = _SpecularColor;
				o.Smoothness = _Glossiness;
			}
		ENDCG
			

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile __ CROSSFADE
				#pragma multi_compile_shadowcaster
			    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"

				sampler2D _MainTex;
                float _Alpha;
                float _Ind;
				half _Cutoff;

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					half2 uv : TEXCOORD1;
					#ifdef CROSSFADE
						float4 screenPos : TEXCOORD3;
					#endif
				};

				v2f vert(appdata_full v)
				{
					v2f o;
					#ifdef CROSSFADE
						o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
					#endif
					o.uv = v.texcoord.xy;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					fixed4 c = tex2D (_MainTex, i.uv);
					if (c.a <= _Cutoff)
						discard;

					#ifdef CROSSFADE
						CrossFadeUV(i.screenPos, _Alpha, _Ind);
					#endif
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	}
	
	//  SM 2.0
	SubShader
	{
		Tags { "RenderType"="LeavesAltTree" "IgnoreProjector" = "True" }
		Cull [_Cull]
		
		CGPROGRAM
			#pragma surface surf StandardSpecular nolightmap fullforwardshadows
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			half _Cutoff;
			float _Alpha;
			float _Ind;
			half4 _HueVariationLeave;

			struct Input {
				float2 uv_MainTex;
			};

			fixed4 _Color;

			fixed3 _SpecularColor;
			half _Glossiness;

			void surf (Input IN, inout SurfaceOutputStandardSpecular o)
			{
				fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
				if (c.a <= _Cutoff)
					discard;

				o.Albedo = GetHue(c, _HueVariationLeave) * _Color.rgb;

				o.Specular = _SpecularColor;
				o.Smoothness = _Glossiness;
			}
		ENDCG
			

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
			    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"

				sampler2D _MainTex;
                float _Alpha;
                float _Ind;
				half _Cutoff;

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					half2 uv : TEXCOORD1;
				};

				v2f vert(appdata_full v)
				{
					v2f o;
					o.uv = v.texcoord.xy;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					fixed4 c = tex2D (_MainTex, i.uv);
					if (c.a <= _Cutoff)
						discard;

					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	}
	FallBack "Transparent/Cutout/VertexLit"
}
