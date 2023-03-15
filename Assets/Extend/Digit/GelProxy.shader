// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RFUniverse/GelProxy"
{
	Properties
	{
		_Tessellation("Tessellation", Int) = 0
		_Color("Color", Color) = (0,0,0,0)
		_Scale("Scale", Float) = 0
		_Offset("Offset", 2D) = "black" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			half filler;
		};

		uniform sampler2D _Offset;
		uniform float4 _Offset_ST;
		uniform float _Scale;
		float4 _Offset_TexelSize;
		uniform float4 _Color;
		uniform int _Tessellation;


		void CalculateUVsSmooth46_g11( float2 UV, float4 TexelSize, out float2 UV0, out float2 UV1, out float2 UV2, out float2 UV3, out float2 UV4, out float2 UV5, out float2 UV6, out float2 UV7, out float2 UV8 )
		{
			{
			    float3 pos = float3( TexelSize.xy, 0 );
			    float3 neg = float3( -pos.xy, 0 );
			    UV0 = UV + neg.xy;
			    UV1 = UV + neg.zy;
			    UV2 = UV + float2( pos.x, neg.y );
			    UV3 = UV + neg.xz;
			    UV4 = UV;
			    UV5 = UV + pos.xz;
			    UV6 = UV + float2( neg.x, pos.y );
			    UV7 = UV + pos.zy;
			    UV8 = UV + pos.xy;
			    return;
			}
		}


		float3 CombineSamplesSmooth58_g11( float Strength, float S0, float S1, float S2, float S3, float S4, float S5, float S6, float S7, float S8 )
		{
			{
			    float3 normal;
			    normal.x = Strength * ( S0 - S2 + 2 * S3 - 2 * S5 + S6 - S8 );
			    normal.y = Strength * ( S0 + 2 * S1 + S2 - S6 - 2 * S7 - S8 );
			    normal.z = 1.0;
			    return normalize( normal );
			}
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			float4 temp_cast_1 = _Tessellation;
			return temp_cast_1;
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float2 uv_Offset = v.texcoord.xy * _Offset_ST.xy + _Offset_ST.zw;
			float4 appendResult3 = (float4(0.0 , 0.0 , ( tex2Dlod( _Offset, float4( uv_Offset, 0, 0.0) ).r * _Scale ) , 0.0));
			v.vertex.xyz += appendResult3.xyz;
			v.vertex.w = 1;
			float temp_output_91_0_g11 = 3.0;
			float Strength58_g11 = temp_output_91_0_g11;
			float localCalculateUVsSmooth46_g11 = ( 0.0 );
			float2 temp_output_85_0_g11 = uv_Offset;
			float2 UV46_g11 = temp_output_85_0_g11;
			float4 TexelSize46_g11 = _Offset_TexelSize;
			float2 UV046_g11 = float2( 0,0 );
			float2 UV146_g11 = float2( 0,0 );
			float2 UV246_g11 = float2( 0,0 );
			float2 UV346_g11 = float2( 0,0 );
			float2 UV446_g11 = float2( 0,0 );
			float2 UV546_g11 = float2( 0,0 );
			float2 UV646_g11 = float2( 0,0 );
			float2 UV746_g11 = float2( 0,0 );
			float2 UV846_g11 = float2( 0,0 );
			CalculateUVsSmooth46_g11( UV46_g11 , TexelSize46_g11 , UV046_g11 , UV146_g11 , UV246_g11 , UV346_g11 , UV446_g11 , UV546_g11 , UV646_g11 , UV746_g11 , UV846_g11 );
			float4 break140_g11 = tex2Dlod( _Offset, float4( UV046_g11, 0, 0.0) );
			float S058_g11 = break140_g11.r;
			float4 break142_g11 = tex2Dlod( _Offset, float4( UV146_g11, 0, 0.0) );
			float S158_g11 = break142_g11.r;
			float4 break146_g11 = tex2Dlod( _Offset, float4( UV246_g11, 0, 0.0) );
			float S258_g11 = break146_g11.r;
			float4 break148_g11 = tex2Dlod( _Offset, float4( UV346_g11, 0, 0.0) );
			float S358_g11 = break148_g11.r;
			float4 break150_g11 = tex2Dlod( _Offset, float4( UV446_g11, 0, 0.0) );
			float S458_g11 = break150_g11.r;
			float4 break152_g11 = tex2Dlod( _Offset, float4( UV546_g11, 0, 0.0) );
			float S558_g11 = break152_g11.r;
			float4 break154_g11 = tex2Dlod( _Offset, float4( UV646_g11, 0, 0.0) );
			float S658_g11 = break154_g11.r;
			float4 break156_g11 = tex2Dlod( _Offset, float4( UV746_g11, 0, 0.0) );
			float S758_g11 = break156_g11.r;
			float4 break158_g11 = tex2Dlod( _Offset, float4( UV846_g11, 0, 0.0) );
			float S858_g11 = break158_g11.r;
			float3 localCombineSamplesSmooth58_g11 = CombineSamplesSmooth58_g11( Strength58_g11 , S058_g11 , S158_g11 , S258_g11 , S358_g11 , S458_g11 , S558_g11 , S658_g11 , S758_g11 , S858_g11 );
			v.normal = localCombineSamplesSmooth58_g11;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Color.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
0;73;1738;740;1733.05;347.4039;1;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;24;-1341,-148.5;Inherit;True;Property;_Offset;Offset;3;0;Create;True;0;0;0;False;0;False;None;None;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-968,143.5;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-619,13;Inherit;False;Property;_Scale;Scale;2;0;Create;True;0;0;0;False;0;False;0;0.003;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-676,-260;Inherit;True;Property;_Tex;Tex;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-364,-92;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-179,-373;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5490196,0.5490196,0.5490196,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;3;-5,-112;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.IntNode;1;-51,250;Inherit;False;Property;_Tessellation;Tessellation;0;0;Create;True;0;0;0;False;0;False;0;3;False;0;1;INT;0
Node;AmplifyShaderEditor.FunctionNode;23;-610,128.5;Inherit;False;Normal From Texture;-1;;11;9728ee98a55193249b513caf9a0f1676;13,149,0,147,0,143,0,141,0,139,0,151,0,137,0,153,0,159,0,157,0,155,0,135,0,108,1;4;87;SAMPLER2D;0;False;85;FLOAT2;0,0;False;74;SAMPLERSTATE;0;False;91;FLOAT;3;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;264,-293;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;RFUniverse/GelProxy;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;2;24;0
WireConnection;4;0;24;0
WireConnection;4;1;25;0
WireConnection;7;0;4;1
WireConnection;7;1;8;0
WireConnection;3;2;7;0
WireConnection;23;87;24;0
WireConnection;23;85;25;0
WireConnection;0;0;5;0
WireConnection;0;11;3;0
WireConnection;0;12;23;40
WireConnection;0;14;1;0
ASEEND*/
//CHKSM=0BA28EC775985489B989777ED666EC73F454E628