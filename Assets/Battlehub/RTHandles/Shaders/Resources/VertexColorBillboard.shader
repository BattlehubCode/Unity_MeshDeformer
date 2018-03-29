// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Battlehub/RTHandles/VertexColorBillboard"
{
	Properties{
	}
	SubShader{
		Cull Off
		ZTest Off
		ZWrite Off

		Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		Pass{
		CGPROGRAM

		#include "UnityCG.cginc"
		#pragma vertex vert  
		#pragma fragment frag 

		struct vertexInput {
			float4 vertex : POSITION;
			float4 color: COLOR;
		};
		struct vertexOutput {
			float4 pos : SV_POSITION;
			float4 color: COLOR;
		};

		vertexOutput vert(vertexInput input)
		{
			vertexOutput output;
			float scaleX = length(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 0.0)));
			float scaleY = length(mul(unity_ObjectToWorld, float4(0.0, 1.0, 0.0, 0.0)));

			output.pos = mul(UNITY_MATRIX_P,
				float4(UnityObjectToViewPos(float3(0.0, 0.0, 0.0)), 1.0) - float4(input.vertex.x * scaleX, input.vertex.y * scaleY, 0.0, 0.0));
			output.color = input.color;
			return output;
		}

		float4 frag(vertexOutput input) : COLOR
		{
			return input.color;
		}

		ENDCG
		}
	}
}
