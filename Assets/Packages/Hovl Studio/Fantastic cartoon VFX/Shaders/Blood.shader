// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hovl/Particles/Blood"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0.1,0.05,0,0)
		_Color("Color", Color) = (1,0,0,1)
		_Normalmaplightning("Normal map lightning", Float) = 0.5
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Gloss("Gloss", Range( 0 , 1)) = 0.7
		_NormapMap("Normap Map", 2D) = "bump" {}
		_Emission("Emission", Float) = 1
		[HideInInspector] _tex3coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float3 uv_tex3coord;
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
		};

		uniform sampler2D _NormapMap;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _SpeedMainTexUVNoiseZW;
		uniform float _Normalmaplightning;
		uniform float4 _Color;
		uniform float _Emission;
		uniform float _Metallic;
		uniform float _Gloss;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 uv0_MainTex = i.uv_tex3coord;
			uv0_MainTex.xy = i.uv_tex3coord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 appendResult3 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			float3 temp_output_6_0 = ( uv0_MainTex + uv0_MainTex.z + float3( ( appendResult3 * _Time.y ) ,  0.0 ) );
			float3 normalizeResult41 = normalize( ( UnpackNormal( tex2D( _NormapMap, temp_output_6_0.xy ) ) * float3(1,1,0.2) ) );
			float vertexalpha31 = i.vertexColor.a;
			float2 temp_output_17_0 = ( i.uv_texcoord + -0.5 );
			float2 break22 = ( temp_output_17_0 * float2( 4,4 ) * temp_output_17_0 );
			float clampResult24 = clamp( ( break22.x + break22.y ) , 0.0 , 1.0 );
			float temp_output_14_0 = ( ( tex2D( _MainTex, temp_output_6_0.xy ).r + 0.49 ) * vertexalpha31 * ( 1.0 - clampResult24 ) );
			float smoothstepResult28 = smoothstep( ( 0.5 + _Normalmaplightning ) , temp_output_14_0 , ( 0.5 - _Normalmaplightning ));
			float3 lerpResult42 = lerp( normalizeResult41 , float3(0,0,1) , smoothstepResult28);
			o.Normal = lerpResult42;
			o.Albedo = ( _Color * i.vertexColor * smoothstepResult28 ).rgb;
			o.Emission = ( _Emission * _Color * temp_output_14_0 * i.vertexColor ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Gloss;
			o.Alpha = temp_output_14_0;
			clip( temp_output_14_0 - _Cutoff );
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=17000
7;29;1906;1004;1618.376;575.1864;1.087638;True;False
Node;AmplifyShaderEditor.RangedFloatNode;18;-2095.204,647.8109;Float;False;Constant;_float;float;2;0;Create;True;0;0;False;0;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-2159.844,533.1396;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;2;-2152.645,316.9153;Float;False;Property;_SpeedMainTexUVNoiseZW;Speed MainTex U/V + Noise Z/W;2;0;Create;True;0;0;False;0;0.1,0.05,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-1895.058,531.624;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1719.978,531.1615;Float;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;4,4;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;3;-1849.01,322.4724;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;7;-1872.83,438.9077;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;22;-1576.612,530.161;Float;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-1679.872,365.4686;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-1881.217,172.1678;Float;False;0;11;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;15;-920.0991,-319.2267;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;6;-1507.075,296.3493;Float;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-1310.53,529.2073;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;24;-1164.698,530.8914;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-740.6383,-240.0798;Float;False;vertexalpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;11;-1343.723,273.0449;Float;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;40;-1061.245,144.5487;Float;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;False;0;1,1,0.2;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-1021.802,295.4052;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.49;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-992.7676,529.4811;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;38;-1347.343,76.72295;Float;True;Property;_NormapMap;Normap Map;7;0;Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;32;-1054.576,390.9305;Float;False;31;vertexalpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-1049.58,-41.85022;Float;False;Property;_Normalmaplightning;Normal map lightning;4;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-970.1381,-131.4119;Float;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-716.8071,-38.83339;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-875.2448,82.54868;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-763.123,294.5472;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;29;-730.2061,-133.7695;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;43;-559.1693,151.1464;Float;False;Constant;_Vector1;Vector 1;6;0;Create;True;0;0;False;0;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SmoothstepOpNode;28;-548.8362,-112.7129;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;26;-941.7217,-510.7526;Float;False;Property;_Color;Color;3;0;Create;True;0;0;False;0;1,0,0,1;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;41;-746.4447,82.54868;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-530.2065,-487.0622;Float;False;Property;_Emission;Emission;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-346.4664,-212.371;Float;False;Property;_Metallic;Metallic;5;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;42;-332.8448,82.6487;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-351.5021,-463.8107;Float;False;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-348.6047,-326.5255;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;44;-146.8738,201.8851;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;45;-103.6937,216.832;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-349.5701,-139.3021;Float;False;Property;_Gloss;Gloss;6;0;Create;True;0;0;False;0;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;35;68.11432,-322.0563;Float;False;True;2;Float;;0;0;Standard;Hovl/Particles/Blood;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;2;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;19;0
WireConnection;17;1;18;0
WireConnection;20;0;17;0
WireConnection;20;2;17;0
WireConnection;3;0;2;1
WireConnection;3;1;2;2
WireConnection;22;0;20;0
WireConnection;9;0;3;0
WireConnection;9;1;7;0
WireConnection;6;0;4;0
WireConnection;6;1;4;3
WireConnection;6;2;9;0
WireConnection;23;0;22;0
WireConnection;23;1;22;1
WireConnection;24;0;23;0
WireConnection;31;0;15;4
WireConnection;11;1;6;0
WireConnection;13;0;11;1
WireConnection;16;0;24;0
WireConnection;38;1;6;0
WireConnection;30;0;33;0
WireConnection;30;1;34;0
WireConnection;39;0;38;0
WireConnection;39;1;40;0
WireConnection;14;0;13;0
WireConnection;14;1;32;0
WireConnection;14;2;16;0
WireConnection;29;0;33;0
WireConnection;29;1;34;0
WireConnection;28;0;29;0
WireConnection;28;1;30;0
WireConnection;28;2;14;0
WireConnection;41;0;39;0
WireConnection;42;0;41;0
WireConnection;42;1;43;0
WireConnection;42;2;28;0
WireConnection;46;0;47;0
WireConnection;46;1;26;0
WireConnection;46;2;14;0
WireConnection;46;3;15;0
WireConnection;27;0;26;0
WireConnection;27;1;15;0
WireConnection;27;2;28;0
WireConnection;44;0;14;0
WireConnection;45;0;14;0
WireConnection;35;0;27;0
WireConnection;35;1;42;0
WireConnection;35;2;46;0
WireConnection;35;3;36;0
WireConnection;35;4;37;0
WireConnection;35;9;44;0
WireConnection;35;10;45;0
ASEEND*/
//CHKSM=33EC55A9D358B9481CA5B2DA4689639816A0D76A