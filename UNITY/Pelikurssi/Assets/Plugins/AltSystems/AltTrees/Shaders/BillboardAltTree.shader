// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

Shader "AltTrees/Billboard"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		[NoScaleOffset]_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset]_BumpMap ("Normal Map", 2D) = "bump" {}
		_Metallic ("Metallic", Range(0,1)) = 0.4

		[PerRendererData]_Alpha ("Alpha", Range(0,1)) = 1
		[PerRendererData]_HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.0)
		[PerRendererData]_Width("Width", Range(0,1)) = 1
		[PerRendererData]_Height("Height", Range(0,1)) = 1
		[PerRendererData]_Rotation("Rotation", Range(0,1)) = 1

		
		_SpecularColor("Specular", Color) = (0.2,0.2,0.2)
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.0
	}



	//  Unity 5 Render 

	//  SM 3.0+
	SubShader
    {
		Tags {"Queue"="AlphaTest" "RenderType"="BillboardAltTree" "DisableBatching"="True" "IgnoreProjector"="True"}
		
		CGPROGRAM
		
		#pragma surface surf StandardSpecular vertex:vert nolightmap fullforwardshadows
		#pragma shader_feature DEBUG_ON
		#pragma multi_compile __ CROSSFADE
		#pragma target 3.0
		#pragma multi_compile_instancing
	    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
	    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

		sampler2D _MainTex;
        sampler2D _BumpMap;

		struct Input {
			#ifdef CROSSFADE
				float4 screenPos;
			#endif
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float4 color;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		UNITY_INSTANCING_BUFFER_START(MyProperties)
			UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
#define _Alpha_arr MyProperties
			UNITY_DEFINE_INSTANCED_PROP(float4, _HueVariation)
#define _HueVariation_arr MyProperties
			UNITY_DEFINE_INSTANCED_PROP(float, _Width)
#define _Width_arr MyProperties
			UNITY_DEFINE_INSTANCED_PROP(float, _Height)
#define _Height_arr MyProperties
			UNITY_DEFINE_INSTANCED_PROP(float, _Rotation)
#define _Rotation_arr MyProperties
		UNITY_INSTANCING_BUFFER_END(MyProperties)


		fixed3 _SpecularColor;
		half _Glossiness;
		fixed4 _Color;
        /*fixed4 _HueVariation;
        float _Alpha;
		float _Width;
		float _Height;
		float _Rotation;*/

		void vert (inout appdata_full v, out Input o)
		{
            UNITY_INITIALIZE_OUTPUT(Input,o);

			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);
		    
			o.color = UNITY_ACCESS_INSTANCED_PROP(_HueVariation_arr, _HueVariation);

			BillboardVert(v, UNITY_ACCESS_INSTANCED_PROP(_Rotation_arr, _Rotation), UNITY_ACCESS_INSTANCED_PROP(_Width_arr, _Width), UNITY_ACCESS_INSTANCED_PROP(_Height_arr, _Height));
		}

		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
            #ifdef CROSSFADE
				CrossFadeUV(IN.screenPos, UNITY_ACCESS_INSTANCED_PROP(_Alpha_arr, _Alpha), 1.0);
			#endif
            
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex.xy);

			clip(c.a - 0.2);

			o.Albedo = GetHue(c, IN.color) * _Color.rgb;
			
			#ifdef DEBUG_ON
				o.Albedo = IN.color.rgb;
			#endif

            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap.xy));
			o.Alpha = c.a;
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
				#pragma multi_compile_instancing
				#include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"



                
                sampler2D _MainTex;
				/*
				float _Alpha;
				float _Width;
				float _Height;
				float _Rotation;*/

				struct Input
				{
					V2F_SHADOW_CASTER;
					float2 uv : TEXCOORD1;
					#ifdef CROSSFADE
						float4 screenPos : TEXCOORD3;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				UNITY_INSTANCING_BUFFER_START(MyProperties)
					UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
#define _Alpha_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(float4, _HueVariation)
#define _HueVariation_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(float, _Width)
#define _Width_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(float, _Height)
#define _Height_arr MyProperties
					UNITY_DEFINE_INSTANCED_PROP(float, _Rotation)
#define _Rotation_arr MyProperties
				UNITY_INSTANCING_BUFFER_END(MyProperties)

				Input vert(appdata_full v)
				{
					Input o;
					UNITY_INITIALIZE_OUTPUT(Input,o);

					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

					float3 eyeVector;

					#ifdef SHADOWS_DEPTH // might be directional shadow or screen depth
						if (unity_LightShadowBias.z != 0.0)
						{
							eyeVector  = ObjSpaceLightDir(v.vertex) * -1;
						}
						else
						{
							eyeVector = ObjSpaceViewDir(v.vertex);
						}
					#else
						#ifdef UNITY_INSTANCING_ENABLED
							float3 objSpaceLightPos = mul(unity_WorldToObjectArray[unity_InstanceID], _WorldSpaceLightPos0).xyz;
							eyeVector  = objSpaceLightPos.xyz - v.vertex.xyz;
						#else
							float3 objSpaceLightPos = mul(unity_WorldToObject, _WorldSpaceLightPos0).xyz;
							eyeVector  = objSpaceLightPos.xyz - v.vertex.xyz;
						#endif
					#endif

					float3 upVector = float3(0,1,0);
					float3 sideVector = normalize(cross(eyeVector,upVector));
			
					float3 finalposition = v.vertex.xyz;
					finalposition += (v.texcoord1.x) * UNITY_ACCESS_INSTANCED_PROP(_Width_arr, _Width) * sideVector;
					finalposition += (v.texcoord1.y) * UNITY_ACCESS_INSTANCED_PROP(_Height_arr, _Height) * upVector;
				    
					float4 pos = float4(finalposition, 1);
		    
					float3 dir = normalize(cross(sideVector, upVector) * -1);

					float _float = (atan2(dir.x,dir.z)*180.0)/3.1415+180.0;
					
					_float -= UNITY_ACCESS_INSTANCED_PROP(_Rotation_arr, _Rotation);
					
					if(_float<0)
  						_float = 360.0 + _float;

					float _intY = 3.0-floor(_float/120.0);
  					float _intX = floor((_float - 120.0*_intY)/40.0);
					
					v.texcoord.x += _intX * 0.3333333;
					v.texcoord.y += _intY * 0.3333333;

					v.normal = dir;
					v.vertex = pos;

					#ifdef CROSSFADE
						o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
					#endif
					
					o.uv = v.texcoord.xy;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				float4 frag(Input i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					
					#ifdef CROSSFADE
						CrossFadeUV(i.screenPos, UNITY_ACCESS_INSTANCED_PROP(_Alpha_arr, _Alpha), 1.0);
					#endif
					clip(tex2D(_MainTex, i.uv).a - 0.2);
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	}

	//  SM 2.0
	SubShader
    {
		Tags {"Queue"="AlphaTest" "RenderType"="BillboardAltTree" "DisableBatching"="True" "IgnoreProjector"="True"}
		
		CGPROGRAM
		
		#pragma surface surf Lambert vertex:vert nolightmap fullforwardshadows
		#pragma shader_feature DEBUG_ON
	    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/AltTrees.cginc"
	    #include "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc"

		sampler2D _MainTex;
        sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float4 color;
		};

		fixed4 _Color;
        fixed4 _HueVariation;
        float _Alpha;
		float _Width;
		float _Height;
		float _Rotation;

		void vert (inout appdata_full v, out Input o)
		{
            UNITY_INITIALIZE_OUTPUT(Input,o);
		    
			o.color = _HueVariation;
            

			BillboardVert(v, _Rotation, _Width, _Height);
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex.xy);
			if(c.a <= 1.0 - _Alpha)
				discard;

			o.Albedo = GetHue(c, IN.color) * _Color.rgb;
			
			#ifdef DEBUG_ON
				o.Albedo = IN.color.rgb;
			#endif

            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap.xy));
			o.Alpha = c.a;

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



                
                float _Alpha;
				sampler2D _MainTex;
				float _Width;
				float _Height;
				float _Rotation;

				struct Input
				{
					V2F_SHADOW_CASTER;
					float2 uv : TEXCOORD1;
				};

				Input vert(appdata_full v)
				{
					Input o;
					UNITY_INITIALIZE_OUTPUT(Input,o);

					float3 eyeVector;

					#ifdef SHADOWS_DEPTH // might be directional shadow or screen depth
						if (unity_LightShadowBias.z != 0.0)
						{
							eyeVector  = ObjSpaceLightDir(v.vertex) * -1;
						}
						else
						{
							eyeVector = ObjSpaceViewDir(v.vertex);
						}
					#else
						float3 objSpaceLightPos = mul(unity_WorldToObject, _WorldSpaceLightPos0).xyz;
						eyeVector  = objSpaceLightPos.xyz - v.vertex.xyz;
					#endif

					float3 upVector = float3(0,1,0);
					float3 sideVector = normalize(cross(eyeVector,upVector));
			
					float3 finalposition = v.vertex.xyz;
					finalposition += (v.texcoord1.x) * _Width * sideVector;
					finalposition += (v.texcoord1.y) * _Height * upVector;
				    
					float4 pos = float4(finalposition, 1);
		    
					float3 dir = normalize(cross(sideVector, upVector) * -1);

					float _float = (atan2(dir.x,dir.z)*180.0)/3.1415+180.0;
					
					_float -= _Rotation;
					
					if(_float<0)
  						_float = 360.0 + _float;

					float _intY = 3.0-floor(_float/120.0);
  					float _intX = floor((_float - 120.0*_intY)/40.0);
					
					v.texcoord.x += _intX * 0.3333333;
					v.texcoord.y += _intY * 0.3333333;

					v.normal = dir;
					v.vertex = pos;

					o.uv = v.texcoord.xy;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				float4 frag(Input i) : SV_Target
				{
					clip(tex2D(_MainTex, i.uv).a - 0.2);
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	}
	FallBack "Transparent/Cutout/Diffuse"
}
