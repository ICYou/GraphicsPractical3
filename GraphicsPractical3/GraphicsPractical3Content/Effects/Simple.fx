//------------------------------------------- Defines -------------------------------------------

#define Pi 3.14159265
#define MAX_LIGHTS 5

//------------------------------------- Top Level Variables -------------------------------------

// Top level variables can and have to be set at runtime

// Matrices for 3D perspective projection 
float4x4 View, Projection, World, InversedTransposedWorld;
float4 DiffuseColor, AmbientColor, SpecularColor;
float AmbientIntensity, SpecularIntensity, SpecularPower;
float3 CameraPosition;
float3 PointLight[MAX_LIGHTS];

texture Texture;
sampler TextureSampler : register(s0)
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

//---------------------------------- Input / Output structures ----------------------------------

// Each member of the struct has to be given a "semantic", to indicate what kind of data should go in
// here and how it should be treated. Read more about the POSITION0 and the many other semantics in 
// the MSDN library
struct VertexShaderInput
{
	float4 Position3D : POSITION0;
	float4 Normal3D : NORMAL0;
	//float2 TextureCoordinate : TEXCOORD0;

};

// The output of the vertex shader. After being passed through the interpolator/rasterizer it is also 
// the input of the pixel shader. 
// Note 1: The values that you pass into this struct in the vertex shader are not the same as what 
// you get as input for the pixel shader. A vertex shader has a single vertex as input, the pixel 
// shader has 3 vertices as input, and lets you determine the color of each pixel in the triangle 
// defined by these three vertices. Therefor, all the values in the struct that you get as input for 
// the pixel shaders have been linearly interpolated between there three vertices!
// Note 2: You cannot use the data with the POSITION0 semantic in the pixel shader.
struct VertexShaderOutput
{
	float4 Position2D : POSITION0;
	float4 Normal : TEXCOORD0;
	float4 Position3D : TEXCOORD1;
	//float2 TextureCoordinate : TEXCOORD3;
};

//------------------------------------------ Functions ------------------------------------------

// PhongLighting implementation
float4 PhongLighting(VertexShaderOutput input)
{		
		//2.4
		float3 normal = mul(input.Normal, InversedTransposedWorld);

		//lambertian calculation (2.1)
		float DiffuseIntensity = 0;
		float SpecPow = 0;
	
		for (int i = 0; i < MAX_LIGHTS; i++)
		{
			DiffuseIntensity += max(0, saturate(dot(input.Normal, normalize(PointLight[i] - normal))));
		
			float3 lightVector = normalize(PointLight[i] - input.Position3D);
			float3 viewVector = normalize(CameraPosition - input.Position3D);
			float3 halfVector = normalize(lightVector + viewVector);
			SpecPow += pow(saturate(dot(input.Normal, halfVector)), SpecularPower);
		}
		float4 diffColor = DiffuseColor * DiffuseIntensity;
		//ambientcolor calculation (2.2)
		float4 ambColor = AmbientColor * AmbientIntensity;
		//specular calculation (2.3)
		
		float specColor = SpecularColor * SpecPow * SpecularIntensity;
		
		return diffColor + ambColor + specColor;
}

// Cell Shading
float4 CellShading(VertexShaderOutput input)
{
	
	float3 normal = mul(input.Normal, InversedTransposedWorld);

		//lambertian calculation (2.1)
		float DiffuseIntensity = max(0, saturate(dot(input.Normal, normalize(PointLight[0] - normal))));
	if (DiffuseIntensity >= 0 && DiffuseIntensity <= 0.20)
		DiffuseIntensity = 0;
	else if (DiffuseIntensity > 0.3 && DiffuseIntensity <= 0.45)
		DiffuseIntensity = 0.3;
	else if (DiffuseIntensity > 0.55 && DiffuseIntensity <= 0.70)
		DiffuseIntensity = 0.55;
	else if (DiffuseIntensity > 0.80 && DiffuseIntensity <= 1)
		DiffuseIntensity = 0.80;

	float4 diffColor = DiffuseColor * DiffuseIntensity;

		float4 ambColor = AmbientColor * AmbientIntensity;

		float4 color = diffColor + ambColor;
		//float colorcheck = ddx(diffColor + ambColor) + ddy(diffColor + ambColor);

		/*if (colorcheck != 0)
		{
		diffColor = DiffuseColor * max(0, saturate(dot(input.Normal, normalize(PointLight[0] - normal))));
		ambColor = AmbientColor * AmbientIntensity;
		color = diffColor + ambColor;
		}*/

		return color;
}

//---------------------------------------- Technique: Simple ----------------------------------------

VertexShaderOutput SimpleVertexShader(VertexShaderInput input)
{
	// Allocate an empty output struct
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Do the matrix multiplications for perspective projection and the world transform
	float4 worldPosition = mul(input.Position3D, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position2D = mul(viewPosition, Projection);

	//1.1 Coloring using normals (add normal values to the output, so it can be used for coloring)
	output.Normal = input.Normal3D;

	//1.2 Checkerboard pattern (add pixel coordinates)
	output.Position3D = input.Position3D;

	return output;
}

float4 SimplePixelShader(VertexShaderOutput input) : COLOR0
{
	//Uncomment the one you want to see
	//float4 color = NormalColor(input); //1.1
	//float4 color = ProceduralColor(input); //1.2
	//float4 color = LambertianLighting(input); //(2.1 + 2.2)
	float4 color = PhongLighting(input);	//2.3,2.4
	//float4 color = CellShading(input);
	return color;
}

technique Simple
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 SimpleVertexShader();
		PixelShader = compile ps_3_0 SimplePixelShader();
	}
}

//---------------------------------------- Technique: CellShading ----------------------------------------

VertexShaderOutput CellShaderVertexShader(VertexShaderInput input)
{
	// Allocate an empty output struct
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Do the matrix multiplications for perspective projection and the world transform
	float4 worldPosition = mul(input.Position3D, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position2D = mul(viewPosition, Projection);

	output.Normal = input.Normal3D;
	output.Position3D = input.Position3D;

	return output;
}

float4 CellShaderPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = CellShading(input);
	return color;
}

technique CellShader
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 CellShaderVertexShader();
		PixelShader = compile ps_3_0 CellShaderPixelShader();
	}
}
//---------------------------------------- Technique: ColorFilterTechnique ----------------------------------------


float4 ColorFilterPixelShader(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{	
	float4 color = tex2D(TextureSampler, TextureCoordinate);

	float3 greyValues = (0.3, 0.59, 0.11);
	color = dot(color, greyValues);
	
	return color;
}

technique ColorFilter
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 ColorFilterPixelShader();
	}
}

//---------------------------------------- Technique: GaussianBlurTechnique ----------------------------------------


float4 GaussianBlurPixelShader(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(TextureSampler, TextureCoordinate);

	float3 greyValues = (0.3, 0.59, 0.11);
	color = dot(color.rgb, greyValues);

	return color;
}

technique GaussianBlur
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 GaussianBlurPixelShader();
	}
}


//---------------------------------------- Technique: TextureTechnique ----------------------------------------
/*VertexShaderOutput TextureVertexShader(VertexShaderInput input)
{
	// Allocate an empty output struct
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Do the matrix multiplications for perspective projection and the world transform
	
	float4 worldPosition = mul(input.Position3D, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position2D = mul(viewPosition, Projection);

	//3.1
	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

//Texture
float4 TextureColor(VertexShaderOutput input)
{
	return tex2D(TextureSampler, input.TextureCoordinate);
}

float4 TexturePixelShader(VertexShaderOutput input) : COLOR0
{	
	float4 color = TextureColor(input);

	return color;
}


technique TextureTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 TextureVertexShader();
		PixelShader = compile ps_2_0 TexturePixelShader();
	}
}*/