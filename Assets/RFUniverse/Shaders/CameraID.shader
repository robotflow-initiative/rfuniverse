// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RFUniverse/CameraID"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			half filler;
		};

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			uint currInstanceId = 0;
			#ifdef UNITY_INSTANCING_ENABLED
			currInstanceId = unity_InstanceID;
			#endif
			float2 appendResult35 = (float2((float)currInstanceId , 0.0));
			float dotResult4_g1 = dot( appendResult35 , float2( 12.9898,78.233 ) );
			float lerpResult10_g1 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g1 ) * 43758.55 ) ));
			float2 appendResult29 = (float2((float)currInstanceId , 1.0));
			float dotResult4_g2 = dot( appendResult29 , float2( 12.9898,78.233 ) );
			float lerpResult10_g2 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g2 ) * 43758.55 ) ));
			float2 appendResult34 = (float2((float)currInstanceId , 2.0));
			float dotResult4_g3 = dot( appendResult34 , float2( 12.9898,78.233 ) );
			float lerpResult10_g3 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g3 ) * 43758.55 ) ));
			float4 appendResult31 = (float4(lerpResult10_g1 , lerpResult10_g2 , lerpResult10_g3 , 0.0));
			o.Emission = appendResult31.xyz;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
72;64;2488;1355;2216.731;408.915;1;True;False
Node;AmplifyShaderEditor.InstanceIdNode;38;-1250.731,52.08502;Inherit;False;False;0;1;INT;0
Node;AmplifyShaderEditor.DynamicAppendNode;29;-898.0764,73.103;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;34;-898.0764,244.1036;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;35;-888.0763,-110.8974;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;30;-657.7661,-174.4725;Inherit;True;Random Range;-1;;1;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;32;-661.7661,54.52779;Inherit;True;Random Range;-1;;2;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;33;-660.7661,291.5284;Inherit;True;Random Range;-1;;3;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;31;-375.7659,8.52763;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;27;-49,-49;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;RFUniverse/CameraID;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;0;38;0
WireConnection;34;0;38;0
WireConnection;35;0;38;0
WireConnection;30;1;35;0
WireConnection;32;1;29;0
WireConnection;33;1;34;0
WireConnection;31;0;30;0
WireConnection;31;1;32;0
WireConnection;31;2;33;0
WireConnection;27;2;31;0
ASEEND*/
//CHKSM=99E3AD696D2DC60F45F632EBFCFAFA82D20647E4