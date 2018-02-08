Shader "AltTrees/Bark Bumped"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2

		[PerRendererData]_HueVariationBark("Hue Variation Bark", Color) = (1.0,0.5,0.0,0.0)
		[PerRendererData]_Alpha("Alpha", Range(0,1)) = 1
		[PerRendererData]_Ind("Ind", Range(0,1)) = 0
			

		_SpecularColor("Specular", Color) = (0.2,0.2,0.2)
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.0
	}

	//  Unity 5 Render
		
	//  SM 3.0+
	SubShader
	{
		Tags{ "RenderType" = "BarkAltTree" }
		Cull[_Cull]

		CGPROGRAM
			#pragma surface surf StandardSpecular nolightmap fullforwardshadows
			#pragma target 3.0
			#pragma multi_compile __ CROSSFADE
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			float _Alpha;
			float _Ind;
			half4 _HueVariationBark;

			struct Input
			{
				float2 uv_MainTex;
				#ifdef CROSSFADE
					float4 screenPos;
				#endif
			};

			half _Metallic;
			fixed4 _Color;

			fixed3 _SpecularColor;
			half _Glossiness;

			void surf(Input IN, inout SurfaceOutputStandardSpecular o)
			{
				#ifdef CROSSFADE
					CrossFadeUV(IN.screenPos, _Alpha, _Ind);
				#endif
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

				o.Albedo = GetHue(c, _HueVariationBark) * _Color.rgb;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

				o.Specular = _SpecularColor;
				o.Smoothness = _Glossiness;
			}
		ENDCG

			
		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM


				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile __ CROSSFADE
				#pragma multi_compile_shadowcaster
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					#ifdef CROSSFADE
						float4 screenPos : TEXCOORD3;
					#endif
				};

				sampler2D _MainTex;

				float _Alpha;
				float _Ind;

				v2f vert(appdata_full v)
				{
					v2f o;
					#ifdef CROSSFADE
						o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
					#endif

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
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
		Tags{ "RenderType" = "BarkAltTree" }
		Cull[_Cull]

		CGPROGRAM
			#pragma surface surf Lambert nolightmap fullforwardshadows
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			float _Alpha;
			float _Ind;
			half4 _HueVariationBark;

			struct Input
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

				o.Albedo = GetHue(c, _HueVariationBark) * _Color.rgb;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			}
		ENDCG
	}
	FallBack "Diffuse"
}
