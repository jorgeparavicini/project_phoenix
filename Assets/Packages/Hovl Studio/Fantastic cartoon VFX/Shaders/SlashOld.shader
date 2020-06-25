Shader "Hovl/Particles/SlashOld"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_EmissionRGB("Emission/R/G/B", Vector) = (1,1,0.5,0.5)
		_Startopacity("Start opacity", Float) = 40
		[Toggle]_Sideopacity("Side opacity", Float) = 0
		_Sideopacitypower("Side opacity power", Float) = 40
		_Finalopacity("Final opacity", Range( 0 , 1)) = 1
		_LenghtSet1ifyouuseinPS("Lenght(Set 1 if you use in PS)", Range( 0 , 1)) = 1
		_PathSet0ifyouuseinPS("Path(Set 0 if you use in PS)", Range( 0 , 1)) = 0
		[MaterialToggle] _Usedepth ("Use depth?", Float ) = 0
        _Depthpower ("Depth power", Float ) = 1
		[Enum(Cull Off,0, Cull Front,1, Cull Back,2)] _CullMode("Culling", Float) = 0
	}

	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull[_CullMode]
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog			
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID	
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float _PathSet0ifyouuseinPS;
				uniform float _LenghtSet1ifyouuseinPS;
				uniform float4 _EmissionRGB;
				uniform float4 _Color;
				uniform float _Startopacity;
				uniform float _Sideopacity;
				uniform float _Sideopacitypower;
				uniform float _Finalopacity;
				uniform fixed _Usedepth;
				uniform float _Depthpower;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);	

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					float lp = 1;
					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate ((sceneZ-partZ) / _Depthpower);
						lp *= lerp(1, fade, _Usedepth);
						i.color.a *= lp;
					#endif

					float4 uv05 = i.texcoord;
					uv05.xy = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float temp_output_7_0 = (2.5 + (( _PathSet0ifyouuseinPS + uv05.z ) - 0.0) * (1.0 - 2.5) / (1.0 - 0.0));
					float clampResult76 = clamp( uv05.w , 0.0 , 1.0 );
					float temp_output_10_0 = (1.0 + (( _LenghtSet1ifyouuseinPS * clampResult76 ) - 0.0) * (0.0 - 1.0) / (1.0 - 0.0));
					float clampResult16 = clamp( ( ( ( temp_output_7_0 * temp_output_7_0 * temp_output_7_0 * temp_output_7_0 * temp_output_7_0 ) * uv05.x ) - temp_output_10_0 ) , 0.0 , 1.0 );
					float V66 = uv05.y;
					float2 appendResult18 = (float2(( clampResult16 * ( 1.0 / (1.0 + (temp_output_10_0 - 0.0) * (0.001 - 1.0) / (1.0 - 0.0)) ) ) , V66));
					float2 clampResult19 = clamp( appendResult18 , float2( 0,0 ) , float2( 1,1 ) );
					float4 tex2DNode21 = tex2D( _MainTex, clampResult19 );
					float temp_output_54_0 = (clampResult19).x;
					float clampResult59 = clamp( ( (1.0 + (temp_output_54_0 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) * _Startopacity ) , 0.0 , 1.0 );
					float clampResult60 = clamp( ( _Startopacity * temp_output_54_0 ) , 0.0 , 1.0 );
					float clampResult62 = clamp( ( (1.0 + (V66 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) * _Sideopacitypower ) , 0.0 , 1.0 );
					float clampResult28 = clamp( ( tex2DNode21.a * i.color.a * _Color.a * ( clampResult59 * clampResult60 * lerp(1.0,clampResult62,_Sideopacity) ) * _Finalopacity ) , 0.0 , 1.0 );
					float4 appendResult86 = (float4(( ( ( tex2DNode21.r * _EmissionRGB.y ) + ( tex2DNode21.g * _EmissionRGB.z ) + ( tex2DNode21.b * _EmissionRGB.w ) ) * (i.color).rgb * (_Color).rgb * _EmissionRGB.x ) , clampResult28));					
					fixed4 col = appendResult86;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
}
/*ASEBEGIN
Version=16900
296;124;1213;909;1714.463;328.5306;2.045301;True;False
Node;AmplifyShaderEditor.CommentaryNode;53;-4490.048,-27.91685;Float;False;2282.792;622.2435;Slash UV;19;2;3;4;7;8;5;12;9;10;13;14;16;17;18;19;66;75;76;85;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-4476.143,294.932;Float;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;2;-4429.932,40.22149;Float;False;Property;_PathSet0ifyouuseinPS;Path(Set 0 if you use in PS);8;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-3981.993,43.66797;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;7;-3854.058,34.69856;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;2.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;76;-4017.877,250.086;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-4440.049,160.7987;Float;False;Property;_LenghtSet1ifyouuseinPS;Lenght(Set 1 if you use in PS);7;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;85;-3577.827,308.1031;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-3659.728,201.2546;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-3648.495,29.86452;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-3486.968,28.89672;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;10;-3491.504,199.5061;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;13;-3258.908,28.0522;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;14;-3256.3,187.6072;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0.001;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;16;-3012.983,23.27694;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;75;-3007.203,166.0433;Float;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;66;-3735.759,395.456;Float;False;V;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-2818.393,22.08316;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;83;-2398.328,730.9493;Float;False;1396.689;645.0528;Side opacity;14;67;65;54;64;56;55;63;57;58;62;61;60;59;25;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;-2595.536,197.6426;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;19;-2402.201,197.1262;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-2348.328,1079.222;Float;False;66;V;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;54;-2149.501,982.7732;Float;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;65;-2168.895,1085.672;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-2207.309,1261.002;Float;False;Property;_Sideopacitypower;Side opacity power;5;0;Create;True;0;0;False;0;40;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1897.029,942.9186;Float;False;Property;_Startopacity;Start opacity;3;0;Create;True;0;0;False;0;40;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-1927.908,1114.902;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;55;-1901.338,780.9493;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-1678.167,967.9112;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-1668.055,800.1371;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;62;-1733.804,1077.494;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;22;-1145.052,142.3896;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;61;-1582.211,1068.601;Float;False;Property;_Sideopacity;Side opacity;4;0;Create;True;0;0;False;0;0;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;21;-1055.309,-73.36416;Float;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-1224.055,331.3597;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;24;-1027.797,526.1595;Float;False;Property;_EmissionRGB;Emission/R/G/B;2;0;Create;True;0;0;False;0;1,1,0.5,0.5;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;59;-1519.85,799.2576;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;60;-1523.831,946.5353;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-1170.639,820.9329;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;37;-847.563,492.4088;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-906.1818,853.6483;Float;False;Property;_Finalopacity;Final opacity;6;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-645.4576,-54.47713;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-647.9492,35.22533;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-650.4408,129.9109;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;36;-852.9051,312.1135;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-592.9656,567.994;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;35;-949.0871,181.2063;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;38;-997.1663,372.1856;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-470.7111,16.05663;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-196.5317,159.0522;Float;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;28;-405.2354,569.4601;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;86;20.87549,331.3223;Float;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;87;288.8726,328.3717;Float;False;True;2;Float;;0;7;Hovl/Particles/SlashOld;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;4;0;2;0
WireConnection;4;1;5;3
WireConnection;7;0;4;0
WireConnection;76;0;5;4
WireConnection;85;0;5;1
WireConnection;9;0;3;0
WireConnection;9;1;76;0
WireConnection;8;0;7;0
WireConnection;8;1;7;0
WireConnection;8;2;7;0
WireConnection;8;3;7;0
WireConnection;8;4;7;0
WireConnection;12;0;8;0
WireConnection;12;1;85;0
WireConnection;10;0;9;0
WireConnection;13;0;12;0
WireConnection;13;1;10;0
WireConnection;14;0;10;0
WireConnection;16;0;13;0
WireConnection;75;1;14;0
WireConnection;66;0;5;2
WireConnection;17;0;16;0
WireConnection;17;1;75;0
WireConnection;18;0;17;0
WireConnection;18;1;66;0
WireConnection;19;0;18;0
WireConnection;54;0;19;0
WireConnection;65;0;67;0
WireConnection;63;0;65;0
WireConnection;63;1;64;0
WireConnection;55;0;54;0
WireConnection;58;0;56;0
WireConnection;58;1;54;0
WireConnection;57;0;55;0
WireConnection;57;1;56;0
WireConnection;62;0;63;0
WireConnection;61;1;62;0
WireConnection;21;1;19;0
WireConnection;59;0;57;0
WireConnection;60;0;58;0
WireConnection;25;0;59;0
WireConnection;25;1;60;0
WireConnection;25;2;61;0
WireConnection;37;0;23;4
WireConnection;29;0;21;1
WireConnection;29;1;24;2
WireConnection;30;0;21;2
WireConnection;30;1;24;3
WireConnection;31;0;21;3
WireConnection;31;1;24;4
WireConnection;36;0;22;4
WireConnection;27;0;21;4
WireConnection;27;1;36;0
WireConnection;27;2;37;0
WireConnection;27;3;25;0
WireConnection;27;4;26;0
WireConnection;35;0;22;0
WireConnection;38;0;23;0
WireConnection;33;0;29;0
WireConnection;33;1;30;0
WireConnection;33;2;31;0
WireConnection;34;0;33;0
WireConnection;34;1;35;0
WireConnection;34;2;38;0
WireConnection;34;3;24;1
WireConnection;28;0;27;0
WireConnection;86;0;34;0
WireConnection;86;3;28;0
WireConnection;87;0;86;0
ASEEND*/
//CHKSM=FA4D31BA2FF15DE956650A7BF9441DF0209E874D