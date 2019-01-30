// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Battlehub/RTHandles/Billboard"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture Image", 2D) = "white" {}
	}
	SubShader{
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZTest LEqual
		ZWrite Off


		Tags{ "Queue" = "Transparent+20" "IgnoreProjector" = "True" "RenderType" = "Transparent" }


		Pass{
		CGPROGRAM

		#include "UnityCG.cginc"
		#pragma vertex vert  
		#pragma fragment frag 

		// User-specified uniforms            
		uniform sampler2D _MainTex;
		fixed4 _Color;

		struct vertexInput {
			float4 vertex : POSITION;
			float4 tex : TEXCOORD0;
		};
		struct vertexOutput {
			float4 pos : SV_POSITION;
			float4 tex : TEXCOORD0;
		};

		vertexOutput vert(vertexInput input)
		{


			vertexOutput output;
			float scaleX = length(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 0.0)));
			float scaleY = length(mul(unity_ObjectToWorld, float4(0.0, 1.0, 0.0, 0.0)));

			float4 mv = float4(UnityObjectToViewPos(float3(0.0, 0.0, 0.0)), 1.0);
			output.pos = mul(UNITY_MATRIX_P, mv
				- float4(input.vertex.x * scaleX, input.vertex.y * scaleY, 0.0, 0.0));
			output.tex = input.tex;
			return output;
		}

		float4 frag(vertexOutput input) : COLOR
		{
			return _Color * tex2D(_MainTex, float2(input.tex.xy));
		}

		ENDCG
		}
	}
}


