// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Axis"
{
	Properties
	{
		_lower("lower", Float) = -30
		_upper("upper", Float) = 90
		_Color("Color", Color) = (1,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform float _upper;
		uniform float _lower;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _Color.rgb;
			float ifLocalVar46 = 0;
			if( _upper >= 0.0 )
				ifLocalVar46 = (float)0;
			else
				ifLocalVar46 = ( abs( floor( ( _upper / 360.0 ) ) ) + 1.0 );
			float ifLocalVar49 = 0;
			if( _lower >= 0.0 )
				ifLocalVar49 = (float)0;
			else
				ifLocalVar49 = ( abs( floor( ( _lower / 360.0 ) ) ) + 1.0 );
			float temp_output_56_0 = ( max( ifLocalVar46 , ifLocalVar49 ) * 360.0 );
			float temp_output_57_0 = ( _upper + temp_output_56_0 );
			float2 break15 = ( i.uv_texcoord - float2( 0.5,0.5 ) );
			float temp_output_10_0 = degrees( atan2( break15.y , break15.x ) );
			float ifLocalVar19 = 0;
			if( temp_output_10_0 >= 0.0 )
				ifLocalVar19 = temp_output_10_0;
			else
				ifLocalVar19 = ( temp_output_10_0 + 360.0 );
			float temp_output_72_0 = ( 360.0 - ifLocalVar19 );
			float temp_output_59_0 = ( _lower + temp_output_56_0 );
			o.Alpha = ( ( 1.0 - pow( 0.3 , ( ( floor( ( temp_output_57_0 / 360.0 ) ) + ( temp_output_72_0 >= fmod( temp_output_57_0 , 360.0 ) ? 0.0 : 1.0 ) ) - ( floor( ( temp_output_59_0 / 360.0 ) ) + ( temp_output_72_0 < fmod( temp_output_59_0 , 360.0 ) ? 1.0 : 0.0 ) ) ) ) ) * step( distance( i.uv_texcoord , float2( 0.5,0.5 ) ) , 0.5 ) );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
72;119;1310;1113;953.4673;2435.716;2.969548;True;False
Node;AmplifyShaderEditor.RangedFloatNode;4;-1000.214,-1285.334;Inherit;False;Property;_lower;lower;0;0;Create;True;0;0;0;False;0;False;-30;120;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1009.389,-1479.789;Inherit;False;Property;_upper;upper;1;0;Create;True;0;0;0;False;0;False;90;150;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;31;-737.4783,-1808.904;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-706.2294,-1479.131;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;39;-554.4465,-1472.107;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;34;-562.159,-1788.373;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-790.6,-150.7001;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.AbsOpNode;53;-439.6675,-1835.411;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;52;-422.5302,-1500.22;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;12;-518.6,-198.7001;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.IntNode;66;-325.9706,-1664.628;Inherit;False;Constant;_Int0;Int 0;2;0;Create;True;0;0;0;False;0;False;0;0;False;0;1;INT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-277.3589,-1886.328;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;15;-357.6,-213.7001;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;50;-266.9791,-1529.107;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;49;-66.7352,-1701.94;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;INT;0;False;3;INT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;46;-75.41344,-1920.332;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;INT;0;False;3;INT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ATan2OpNode;9;-224.6,-230.7001;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;51;123.8816,-1773.48;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DegreesOpNode;10;-7.599976,-230.7001;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;376.3819,-1699.825;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;194.4,-128.7001;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;19;336.4,-247.7001;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;530.6195,-1410.027;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;57;550.078,-1583.608;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;60;1020.356,-1345.397;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;38;836.8378,-586.7903;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;72;713.0741,-336.3934;Inherit;False;2;0;FLOAT;360;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;33;820.1114,-1072.778;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;62;1062.638,-1194.917;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;360;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;61;1195.677,-1324.866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;63;1213.958,-1172.386;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;29;991.9883,-945.1093;Inherit;True;3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;36;1001.621,-559.6957;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;1474.337,-1157.823;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;1473.64,-900.8785;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;41;1812.202,-1057.478;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;6;-405.6,88.29993;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;70;2177.49,-785.8668;Inherit;False;False;2;0;FLOAT;0.3;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;8;-93.31531,173.5731;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;71;2377.429,-747.6464;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;68;1658.958,-260.4073;Inherit;False;Property;_Color;Color;2;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,1,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;2223.425,30.75568;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2442.766,-71.35037;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Axis;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;31;0;30;0
WireConnection;37;0;4;0
WireConnection;39;0;37;0
WireConnection;34;0;31;0
WireConnection;53;0;34;0
WireConnection;52;0;39;0
WireConnection;12;0;5;0
WireConnection;48;0;53;0
WireConnection;15;0;12;0
WireConnection;50;0;52;0
WireConnection;49;0;4;0
WireConnection;49;2;66;0
WireConnection;49;3;66;0
WireConnection;49;4;50;0
WireConnection;46;0;30;0
WireConnection;46;2;66;0
WireConnection;46;3;66;0
WireConnection;46;4;48;0
WireConnection;9;0;15;1
WireConnection;9;1;15;0
WireConnection;51;0;46;0
WireConnection;51;1;49;0
WireConnection;10;0;9;0
WireConnection;56;0;51;0
WireConnection;20;0;10;0
WireConnection;19;0;10;0
WireConnection;19;2;10;0
WireConnection;19;3;10;0
WireConnection;19;4;20;0
WireConnection;59;0;4;0
WireConnection;59;1;56;0
WireConnection;57;0;30;0
WireConnection;57;1;56;0
WireConnection;60;0;57;0
WireConnection;38;0;59;0
WireConnection;72;1;19;0
WireConnection;33;0;57;0
WireConnection;62;0;59;0
WireConnection;61;0;60;0
WireConnection;63;0;62;0
WireConnection;29;0;72;0
WireConnection;29;1;33;0
WireConnection;36;0;72;0
WireConnection;36;1;38;0
WireConnection;35;0;61;0
WireConnection;35;1;29;0
WireConnection;40;0;63;0
WireConnection;40;1;36;0
WireConnection;41;0;35;0
WireConnection;41;1;40;0
WireConnection;6;0;5;0
WireConnection;70;1;41;0
WireConnection;8;0;6;0
WireConnection;71;0;70;0
WireConnection;69;0;71;0
WireConnection;69;1;8;0
WireConnection;0;2;68;0
WireConnection;0;9;69;0
ASEEND*/
//CHKSM=95041DAF460989016D3EFFD162E244831CD6A74D