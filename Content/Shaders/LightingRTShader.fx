#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Textures from your render targets
sampler SceneColor : register(s0);
sampler Lighting : register(s1);
sampler Sky : register(s2);


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 scenePixel = tex2D(SceneColor, input.TexCoord);
    float4 lightPixel = tex2D(Lighting, input.TexCoord);
    float4 skyPixel = tex2D(Sky, input.TexCoord);
    return lerp(skyPixel, scenePixel * lightPixel, scenePixel.a);
}

technique CombineTextures
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}