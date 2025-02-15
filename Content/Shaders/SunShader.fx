#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 time24;
uint screenDimX;
static const float PI = 3.14159265f;
//matrix WorldViewProjection;


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

sampler2D SpriteTextureSampler : register(s0);

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    float2 offset;
    offset.x = ((time24 - 12.0f) / 12.0f) * screenDimX;
    offset.y = sin((time24 * PI) / 24.0f) * 0.0f;
    
    VertexShaderOutput output = (VertexShaderOutput)0;
    
    input.Position.xy += offset;
    
    // Transform the vertex position
    output.Position = input.Position;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}