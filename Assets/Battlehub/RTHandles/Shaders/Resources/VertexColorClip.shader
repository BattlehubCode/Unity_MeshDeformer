// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Battlehub/RTHandles/VertexColorClip" {
	Properties
	{
		_ZWrite("ZWrite", Float) = 0.0
		_ZTest("ZTest", Float) = 0.0
		_Cull("Cull", Float) = 0.0
	}
	SubShader
	{
		
		Tags{ "Queue" = "Geometry+5" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		Pass
		{
			Cull[_Cull]
			ZTest Off
			ZWrite Off
		
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
				float3 norm : TEXCOORD0;
				float4 color: COLOR;
			};


			inline float4 GammaToLinearSpace(float4 sRGB)
			{
				if (IsGammaSpace())
				{
					return sRGB;
				}
				return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
			}

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				output.norm = normalize(mul(UNITY_MATRIX_IT_MV, float4(input.vertex.xyz, 0)));
				output.color = GammaToLinearSpace(input.color);
				output.color.a = input.color.a;
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				clip(dot(input.norm, float3(0, 0, 1)));
				return  input.color;
			}	

			ENDCG
		}
	}
}