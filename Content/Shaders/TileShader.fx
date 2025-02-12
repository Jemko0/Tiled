#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

SamplerState tileSamplerState
{
    Filter = MIN_MAG_MIP_LINEAR; // Linear filtering for minification, magnification, and mipmapping
    AddressU = Wrap; // Wrap texture coordinates in the U direction
    AddressV = Wrap; // Wrap texture coordinates in the V direction
    AddressW = Wrap; // Wrap texture coordinates in the W direction
};

float2 cameraPosition;
Texture2D tileTexture;
int2 tileStarts;
int2 tileEnd;

sampler TextureSampler : register(s0);

struct VertexShaderOutput
{
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return tileTexture.Gather(tileSamplerState, input.TextureCoordinates);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};