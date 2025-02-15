#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler TextureSampler : register(s0);

//light multiplier
float timeLerp = 0.0f;

struct VertexShaderOutput
{
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float red = lerp(1.0f, 0.0f, timeLerp);
	
    float4 skyBaseColor = lerp(float4(0.24f, 0.47f, 0.94, 1.0f), 0.0f, 1.0f - timeLerp);

    skyBaseColor.r += red * timeLerp;
    float4 skyHorizonColor = saturate(skyBaseColor * 2.0f);
    return lerp(skyBaseColor, skyHorizonColor, pow(input.TextureCoordinates.y, 2));
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};