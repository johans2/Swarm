Shader "Unlit/swarmers"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"

			struct Swarmer {
				float3 position;		// = 12
				float3 previousPosition;// = 12
				float3 velocity;		// = 12
				float life;				// = 4
				float startDelay;		// = 4
				float3 color;			// = 12
			};

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 color : TEXCOORD1;
			};

			half4 _Color;
			StructuredBuffer<Swarmer> swarmers;


			v2f vert(appdata v, uint instance_id : SV_InstanceID)
			{
				v2f output = (v2f)0;

				Swarmer swarmer = swarmers[instance_id];
				output.vertex = UnityObjectToClipPos(swarmer.position);
				output.color = swarmer.color;

				return output;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = _Color;
				col.rgb += i.color;
				return col;
			}
			ENDCG
		}
	}
}
