Shader "Unlit/worldPoints"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

			struct WorldNode {
				float3 position;	// = 12
				float occupied;		// = 4
			};

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

			half4 _Color;
			StructuredBuffer<WorldNode> world;


            v2f vert (appdata v, uint instance_id : SV_InstanceID)
            {
                v2f output = (v2f)0;

				WorldNode node = world[instance_id];
				output.vertex = UnityObjectToClipPos(node.position);
				
                return output;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
