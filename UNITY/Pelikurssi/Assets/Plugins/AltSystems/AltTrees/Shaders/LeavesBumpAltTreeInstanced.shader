// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

Shader "AltTrees/Instanced/Leaves Bumped"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}

		_Cutoff("Alpha cutoff", Range(0,1)) = 0.3
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2

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
		Tags{ "RenderType" = "LeavesAltTree" "IgnoreProjector" = "True" }
		Cull[_Cull]

		CGPROGRAM

			#pragma surface surf StandardSpecular nolightmap fullforwardshadows addshadow
			#pragma target 3.0
			#pragma multi_compile __ CROSSFADE
			#pragma multi_compile_instancing
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			half _Cutoff;
			
			UNITY_INSTANCING_BUFFER_START(MyProperties)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HueVariationLeave)
#define _HueVariationLeave_arr MyProperties
				UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
#define _Alpha_arr MyProperties
				UNITY_DEFINE_INSTANCED_PROP(float, _Ind)
#define _Ind_arr MyProperties
			UNITY_INSTANCING_BUFFER_END(MyProperties)

			struct Input {
				float2 uv_MainTex;
				#ifdef CROSSFADE
					float4 screenPos;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			fixed4 _Color;

			fixed3 _SpecularColor;
			half _Glossiness;

			void surf(Input IN, inout SurfaceOutputStandardSpecular o)
			{
				#ifdef CROSSFADE
					CrossFadeUV(IN.screenPos, UNITY_ACCESS_INSTANCED_PROP(_Alpha_arr, _Alpha), UNITY_ACCESS_INSTANCED_PROP(_Ind_arr, _Ind));
				#endif

				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				if (c.a <= _Cutoff)
					discard;

				o.Albedo = GetHue(c, UNITY_ACCESS_INSTANCED_PROP(_HueVariationLeave_arr, _HueVariationLeave)) * _Color.rgb;
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
				#pragma multi_compile_instancing
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"

				sampler2D _MainTex;
				half _Cutoff;

				UNITY_INSTANCING_BUFFER_START(MyProperties)
					UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
#define _Alpha_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(float, _Ind)
#define _Ind_arr MyProperties
				UNITY_INSTANCING_BUFFER_END(MyProperties)

				struct v2f
				{
					V2F_SHADOW_CASTER;
					half2 uv : TEXCOORD1;
					#ifdef CROSSFADE
						float4 screenPos : TEXCOORD3;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				v2f vert(appdata_full v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

					#ifdef CROSSFADE
						o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
					#endif
					o.uv = v.texcoord.xy;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					fixed4 c = tex2D(_MainTex, i.uv);
					if (c.a <= _Cutoff)
						discard;

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
		Tags{ "RenderType" = "LeavesAltTree" "IgnoreProjector" = "True" }
		Cull[_Cull]

		CGPROGRAM

			#pragma surface surf Lambert nolightmap fullforwardshadows
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			half _Cutoff;
			float _Alpha;
			float _Ind;
			half4 _HueVariationLeave;

			struct Input {
				float2 uv_MainTex;
			};

			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				if (c.a <= _Cutoff)
					discard;

				o.Albedo = GetHue(c, _HueVariationLeave) * _Color.rgb;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			}

		ENDCG


		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

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
					fixed4 c = tex2D(_MainTex, i.uv);
					if (c.a <= _Cutoff)
						discard;

					SHADOW_CASTER_FRAGMENT(i)
				}

			ENDCG
		}
	}
	FallBack "Transparent/Cutout/VertexLit"
}
