// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RFUniverse/BoundingBox"
{
	Properties
	{
		_Size("Size", Vector) = (0,0,0,0)
		_Color("Color", Color) = (0,0,0,0)
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd vertex:vertexDataFunc 
		struct Input
		{
			half filler;
		};

		uniform float3 _Size;
		uniform float4 _Color;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float4 appendResult20 = (float4(( ( (( _Size.x / -2.0 )).x / ase_objectScale.x ) * ( v.color.r > 0.5 ? 1.0 : -1.0 ) ) , ( ( (( _Size.y / 2.0 )).x / ase_objectScale.x ) * ( v.color.g > 0.5 ? 1.0 : -1.0 ) ) , ( ( (( _Size.z / 2.0 )).x / ase_objectScale.x ) * ( v.color.b > 0.5 ? 1.0 : -1.0 ) ) , 0.0));
			v.vertex.xyz += appendResult20.xyz;
			v.vertex.w = 1;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _Color.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
0;73;1559;1155;1421.206;841.8861;1;True;True
Node;AmplifyShaderEditor.Vector3Node;8;-971.5078,-623.3356;Inherit;False;Property;_Size;Size;0;0;Create;True;0;0;0;False;0;False;0,0,0;-1.35,1.23,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;-487.5396,-589.5528;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;30;-488.5396,-681.5527;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;29;-487.5396,-496.5528;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1;-536.5,-106.5;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;16;-299.5078,-495.3356;Inherit;False;FLOAT;2;1;2;3;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;15;-298.5078,-584.3356;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;14;-294.5078,-666.3356;Inherit;False;FLOAT;0;1;2;3;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectScaleNode;11;-524.0077,-321.7355;Inherit;False;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;18;-77.50781,-356.3356;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;3;-317.5,-44.5;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;3;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;12;-78.50781,-545.3356;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;2;-318.5,-187.5;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;3;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-78.50781,-453.3356;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;4;-317.5,104.5;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;3;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;188.4922,-17.33557;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;195.4922,-268.3356;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;195.4922,-140.3356;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;152.3922,-687.7357;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,1,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;20;484.4922,-68.33557;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;596.9232,-672.1539;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;RFUniverse/BoundingBox;False;False;False;False;True;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;28;0;8;2
WireConnection;30;0;8;1
WireConnection;29;0;8;3
WireConnection;16;0;29;0
WireConnection;15;0;28;0
WireConnection;14;0;30;0
WireConnection;18;0;16;0
WireConnection;18;1;11;1
WireConnection;3;0;1;2
WireConnection;12;0;14;0
WireConnection;12;1;11;1
WireConnection;2;0;1;1
WireConnection;17;0;15;0
WireConnection;17;1;11;1
WireConnection;4;0;1;3
WireConnection;23;0;18;0
WireConnection;23;1;4;0
WireConnection;21;0;12;0
WireConnection;21;1;2;0
WireConnection;22;0;17;0
WireConnection;22;1;3;0
WireConnection;20;0;21;0
WireConnection;20;1;22;0
WireConnection;20;2;23;0
WireConnection;0;2;13;0
WireConnection;0;11;20;0
ASEEND*/
//CHKSM=0EC024125A1C40257B86B9B3F2DB40BAEFF75F2D