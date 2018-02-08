Shader "AltTrees/TreeCreatorLeaves"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_Shininess("Shininess", Range(0.01, 1)) = 0.078125
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_GlossMap("Gloss (A)", 2D) = "black" {}
		_TranslucencyMap("Translucency (A)", 2D) = "white" {}
		_ShadowOffset("Shadow Offset (A)", 2D) = "black" {}


		[PerRendererData]_HueVariationLeave("Hue Variation Leave", Color) = (1.0,0.5,0.0,0.0)
		[PerRendererData]_Alpha("Alpha", Range(0,1)) = 1
		[PerRendererData]_Ind("Ind", Range(0,1)) = 0



		_Cutoff("Alpha cutoff", Range(0,1)) = 0.3
		[HideInInspector] _TreeInstanceColor("TreeInstanceColor", Vector) = (1,1,1,1)
		[HideInInspector] _TreeInstanceScale("TreeInstanceScale", Vector) = (1,1,1,1)
		[HideInInspector] _SquashAmount("Squash", Float) = 1
			

		_SpecularColor("Specular", Color) = (0.2,0.2,0.2)
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.0
	}


	//  Unity 5 Render

	//  SM 3.0+
	SubShader
	{
		Tags
		{
			"IgnoreProjector" = "True" "RenderType" = "TreeCreatorLeafAltTree"
		}

		CGPROGRAM
			#pragma surface surf StandardSpecular vertex:TreeVertLeaf nolightmap fullforwardshadows
			#pragma target 3.0
			#pragma multi_compile __ CROSSFADE
			#pragma multi_compile __ ENABLE_ALTWIND
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/UnityBuiltin3xTreeLibraryAltTree.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _GlossMap;
			sampler2D _TranslucencyMap;
			half _Cutoff;
			float _Alpha;
			float _Ind;
			half4 _HueVariationLeave;

			fixed3 _SpecularColor;
			half _Glossiness;

			struct Input
			{
				float2 uv_MainTex;
				fixed4 color : COLOR;
				#ifdef CROSSFADE
					float4 screenPos;
				#endif
			};

			void surf(Input IN, inout SurfaceOutputStandardSpecular o)
			{
                #ifdef CROSSFADE
					CrossFadeUV(IN.screenPos, _Alpha, _Ind);
				#endif
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				if (c.a <= _Cutoff)
					discard;

				o.Albedo = GetHue(c, _HueVariationLeave)/* * IN.color.rgb*/;
				//o.Gloss = tex2D(_GlossMap, IN.uv_MainTex).a;
				o.Alpha = c.a;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

				o.Specular = _SpecularColor;
				o.Smoothness = _Glossiness;
			}
		ENDCG

			
		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM


				#pragma vertex TreeVertLeaf2
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile __ CROSSFADE
				#pragma multi_compile __ ENABLE_ALTWIND
				#pragma multi_compile_shadowcaster
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/UnityBuiltin3xTreeLibraryAltTree.cginc"
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					#ifdef CROSSFADE
						float4 screenPos : TEXCOORD3;
					#endif
					half2 uv : TEXCOORD1;
				};

				sampler2D _MainTex;
				float _Alpha;
				float _Ind;
				float _Cutoff;

				v2f TreeVertLeaf2(appdata_full v)
				{
					v2f o;
					#ifdef CROSSFADE
						o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
					#endif

					ExpandBillboard(UNITY_MATRIX_IT_MV, v.vertex, v.normal, v.tangent);
					//v.vertex.xyz *= _TreeInstanceScale.xyz;
					v.vertex = AnimateVertex(v.vertex, v.normal, float4(v.color.xy, v.texcoord1.xy));

					v.vertex = Squash(v.vertex);

					o.uv = v.texcoord.xy;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					#ifdef CROSSFADE
						CrossFadeUV(i.screenPos, _Alpha, _Ind);
					#endif

					clip(tex2D(_MainTex, i.uv).a - _Cutoff);

					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	}

	//  SM 2.0
	SubShader
	{
		Tags
		{
			"IgnoreProjector" = "True" "RenderType" = "TreeCreatorLeafAltTree"
		}

		CGPROGRAM
			#pragma surface surf Lambert vertex:TreeVertLeaf nolightmap fullforwardshadows
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/UnityBuiltin3xTreeLibraryAltTree.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
			#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _GlossMap;
			sampler2D _TranslucencyMap;
			half _Shininess;
			half _Cutoff;
			float _Alpha;
			float _Ind;
			half4 _HueVariationLeave;

			struct Input
			{
				float2 uv_MainTex;
				fixed4 color : COLOR;
			};

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				if (c.a <= _Cutoff)
					discard;

				o.Albedo = GetHue(c, _HueVariationLeave)/* * IN.color.rgb*/;
				o.Gloss = tex2D(_GlossMap, IN.uv_MainTex).a;
				o.Alpha = c.a;
				o.Specular = _Shininess;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			}
		ENDCG

			
		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM


				#pragma vertex TreeVertLeaf2
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/UnityBuiltin3xTreeLibraryAltTree.cginc"
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					half2 uv : TEXCOORD1;
				};

				sampler2D _MainTex;
				float _Alpha;
				float _Ind;
				float _Cutoff;

				v2f TreeVertLeaf2(appdata_full v)
				{
					v2f o;

					ExpandBillboard(UNITY_MATRIX_IT_MV, v.vertex, v.normal, v.tangent);
					//v.vertex.xyz *= _TreeInstanceScale.xyz;
					v.vertex = AnimateVertex(v.vertex, v.normal, float4(v.color.xy, v.texcoord1.xy));

					v.vertex = Squash(v.vertex);

					o.uv = v.texcoord.xy;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					clip(tex2D(_MainTex, i.uv).a - _Cutoff);

					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	}
	FallBack "Transparent/Cutout/VertexLit"
}
