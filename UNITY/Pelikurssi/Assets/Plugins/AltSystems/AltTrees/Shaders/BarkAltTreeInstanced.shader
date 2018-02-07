// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

Shader "AltTrees/Instanced/Bark" {
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2

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
		Tags { "RenderType"="BarkAltTree" }
		Cull [_Cull]
		
		CGPROGRAM
		#pragma surface surf StandardSpecular nolightmap fullforwardshadows
		#pragma target 3.0
		#pragma multi_compile __ CROSSFADE
		#pragma multi_compile_instancing
		#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
	    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

		sampler2D _MainTex;


		UNITY_INSTANCING_BUFFER_START(MyProperties)
			UNITY_DEFINE_INSTANCED_PROP(float4, _HueVariationBark)
#define _HueVariationBark_arr MyProperties
			UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
#define _Alpha_arr MyProperties
			UNITY_DEFINE_INSTANCED_PROP(float, _Ind)
#define _Ind_arr MyProperties
		UNITY_INSTANCING_BUFFER_END(MyProperties)

		struct Input
		{
			float2 uv_MainTex;
			#ifdef CROSSFADE
				float4 screenPos;
			#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		fixed4 _Color;

		fixed3 _SpecularColor;
		half _Glossiness;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			#ifdef CROSSFADE
				CrossFadeUV(IN.screenPos, UNITY_ACCESS_INSTANCED_PROP(_Alpha_arr, _Alpha), UNITY_ACCESS_INSTANCED_PROP(_Ind_arr, _Ind));
			#endif
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = GetHue(c, UNITY_ACCESS_INSTANCED_PROP(_HueVariationBark_arr, _HueVariationBark)) * _Color.rgb;

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
				#pragma multi_compile_instancing
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					#ifdef CROSSFADE
						float4 screenPos : TEXCOORD3;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				sampler2D _MainTex;

				UNITY_INSTANCING_BUFFER_START(MyProperties)
					UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
#define _Alpha_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(float, _Ind)
#define _Ind_arr MyProperties
				UNITY_INSTANCING_BUFFER_END(MyProperties)

				v2f vert(appdata_full v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					
					#ifdef CROSSFADE
						o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
					#endif

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					#ifdef CROSSFADE
						CrossFadeUV(i.screenPos, UNITY_ACCESS_INSTANCED_PROP(_Alpha_arr, _Alpha), UNITY_ACCESS_INSTANCED_PROP(_Ind_arr, _Ind));
					#endif
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	}
	
	//  SM 2.0
	SubShader
	{
		Tags { "RenderType"="BarkAltTree" }
		Cull [_Cull]
		
		CGPROGRAM
		#pragma surface surf StandardSpecular nolightmap fullforwardshadows
		#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
	    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

		sampler2D _MainTex;
		float _Alpha;
		float _Ind;
		half4 _HueVariationBark;

		struct Input
		{
			float2 uv_MainTex;
		};

		fixed4 _Color;

		fixed3 _SpecularColor;
		half _Glossiness;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = GetHue(c, _HueVariationBark) * _Color.rgb;

			o.Specular = _SpecularColor;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
