#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

int2 tiles[];
int4 tileRegionXYWH;

sampler TextureSampler : register(s0);

struct VertexShaderOutput
{
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    for
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODELMainPS();
    }
};