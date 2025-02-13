#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

int light;
float4 frame;
float screenScale;

sampler TextureSampler : register(s0);

struct VertexShaderOutput
{
    float2 TILE_SCREEN : TS;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 tileOffset = float2(frame.x * frame.z, frame.y * frame.w);
    
    float2 tileCoords = float2(input.TextureCoordinates.x * frame.x, input.TextureCoordinates.y * frame.y);
	
    float2 finalUV = (tileCoords + tileOffset);
    
    return tex2D(TextureSampler, finalUV) * (light / 16.0f);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};