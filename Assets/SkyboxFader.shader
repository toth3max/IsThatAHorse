Shader "Skybox/6 Sided (w Fade - Custom)" {
Properties {
	_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
	[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
	_Rotation ("Rotation", Range(0, 360)) = 0
	_Fade ("Fade", Range(0,1)) = 0
	[NoScaleOffset] _FrontTex ("Front [+Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _BackTex ("Back [-Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _LeftTex ("Left [+X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _RightTex ("Right [-X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _UpTex ("Up [+Y]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _DownTex ("Down [-Y]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _FrontTex2 ("Front2 [+Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _BackTex2 ("Back2 [-Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _LeftTex2 ("Left2 [+X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _RightTex2 ("Right2 [-X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _UpTex2 ("Up2 [+Y]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _DownTex2 ("Down2 [-Y]   (HDR)", 2D) = "grey" {}
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off
	
	CGINCLUDE
	#include "UnityCG.cginc"

	half4 _Tint;
	half _Exposure;
	float _Rotation;
	float _Fade;

	float4 RotateAroundYInDegrees (float4 vertex, float degrees)
	{
		float alpha = degrees * UNITY_PI / 180.0;
		float sina, cosa;
		sincos(alpha, sina, cosa);
		float2x2 m = float2x2(cosa, -sina, sina, cosa);
		return float4(mul(m, vertex.xz), vertex.yw).xzyw;
	}
	
	struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};
	struct v2f {
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
	};
	v2f vert (appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, RotateAroundYInDegrees(v.vertex, _Rotation));
		o.texcoord = v.texcoord;
		return o;
	}
	half4 skybox_frag (v2f i, sampler2D smp, half4 smpDecode, sampler2D smp2, half4 smpDecode2)
	{
		half4 tex = tex2D (smp, i.texcoord);
		half3 c1 = DecodeHDR (tex, smpDecode);
		
		half4 tex2 = tex2D (smp2, i.texcoord);
		half3 c2 = DecodeHDR (tex2, smpDecode2);
		
		half3 c = lerp(c1, c2, _Fade);
		
		c = c * _Tint.rgb * unity_ColorSpaceDouble;
		c *= _Exposure;
		return half4(c, 1);
	}
	ENDCG
	
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _FrontTex;
		half4 _FrontTex_HDR;
		sampler2D _FrontTex2;
		half4 _FrontTex2_HDR;
		half4 frag (v2f i) : SV_Target { return skybox_frag(i,_FrontTex, _FrontTex_HDR, _FrontTex2, _FrontTex2_HDR); }
		ENDCG 
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _BackTex;
		half4 _BackTex_HDR;
		sampler2D _BackTex2;
		half4 _BackTex2_HDR;
		half4 frag (v2f i) : SV_Target { return skybox_frag(i,_BackTex, _BackTex_HDR, _BackTex2, _BackTex2_HDR); }
		ENDCG 
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _LeftTex;
		half4 _LeftTex_HDR;
		sampler2D _LeftTex2;
		half4 _LeftTex2_HDR;
		half4 frag (v2f i) : SV_Target { return skybox_frag(i,_LeftTex, _LeftTex_HDR, _LeftTex2, _LeftTex2_HDR); }
		ENDCG
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _RightTex;
		half4 _RightTex_HDR;
		sampler2D _RightTex2;
		half4 _RightTex2_HDR;
		half4 frag (v2f i) : SV_Target { return skybox_frag(i,_RightTex, _RightTex_HDR, _RightTex2, _RightTex2_HDR); }
		ENDCG
	}	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _UpTex;
		half4 _UpTex_HDR;
		sampler2D _UpTex2;
		half4 _UpTex2_HDR;
		half4 frag (v2f i) : SV_Target { return skybox_frag(i,_UpTex, _UpTex_HDR, _UpTex2, _UpTex2_HDR); }
		ENDCG
	}	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _DownTex;
		half4 _DownTex_HDR;
		sampler2D _DownTex2;
		half4 _DownTex2_HDR;
		half4 frag (v2f i) : SV_Target { return skybox_frag(i,_DownTex, _DownTex_HDR, _DownTex2, _DownTex2_HDR); }
		ENDCG
	}
}
}
