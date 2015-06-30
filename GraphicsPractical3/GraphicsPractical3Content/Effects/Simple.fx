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
};

//------------------------------------------ Functions ------------------------------------------

// PhongLighting implementation
float4 PhongLighting(VertexShaderOutput input)
{		
		//calculate inversedtransposed normal
		float3 normal = mul(input.Normal, InversedTransposedWorld);

		//lambertian calculation (2.1)
		float DiffuseIntensity = 0;
		float SpecPow = 0;
	
		//calculate intensity using all the lights
		for (int i = 0; i < MAX_LIGHTS; i++)
		{
			//calculate diffuse intensity and add it to the last intensity
			DiffuseIntensity += max(0, saturate(dot(input.Normal, normalize(PointLight[i] - normal))));
			
			//calculate specular
			float3 lightVector = normalize(PointLight[i] - input.Position3D);
			float3 viewVector = normalize(CameraPosition - input.Position3D);
			float3 halfVector = normalize(lightVector + viewVector);
			SpecPow += pow(saturate(dot(input.Normal, halfVector)), SpecularPower);
		}
		//diffusecolor calculation
		float4 diffColor = DiffuseColor * DiffuseIntensity;
		
			//ambientcolor calculation 
		float4 ambColor = AmbientColor * AmbientIntensity;
		
		//specular calculation 		
		float specColor = SpecularColor * SpecPow * SpecularIntensity;
		
		//return color
		return diffColor + ambColor + specColor;
}

// Cell Shading
float4 CellShading(VertexShaderOutput input)
{
	//Calculate normal
	float3 normal = mul(input.Normal, InversedTransposedWorld);

	//Calulate DiffuseIntensity
	float DiffuseIntensity = max(0, saturate(dot(input.Normal, normalize(PointLight[0] - normal))));
	
	//Change intensity so 
	if (DiffuseIntensity >= 0 && DiffuseIntensity <= 0.25)
	{
		DiffuseIntensity = 0;
	}		
	else if (DiffuseIntensity > 0.25 && DiffuseIntensity <= 0.50)
	{
		DiffuseIntensity = 0.25;
	}		
	else if (DiffuseIntensity > 0.50 && DiffuseIntensity <= 0.75)
	{
		DiffuseIntensity = 0.50;
	}		
	else if (DiffuseIntensity > 0.75 && DiffuseIntensity <= 1)
	{
		DiffuseIntensity = 0.75;
	}

	float4 diffColor = DiffuseColor * DiffuseIntensity;
		float4 ambColor = AmbientColor * AmbientIntensity;

		

	//float gradientLength = length(float2(derivX, derivY));
		//float thresholdWidth = 2.0 * gradientLength;
		//color = lerp(diffColor, ambColor, aliasColor);
		//float colorcheck = ddx(diffColor + ambColor) + ddy(diffColor + ambColor);

		/*if (colorcheck != 0)
		{
		diffColor = DiffuseColor * max(0, saturate(dot(input.Normal, normalize(PointLight[0] - normal))));
		ambColor = AmbientColor * AmbientIntensity;
		color = diffColor + ambColor;
		}*/

		return diffColor + ambColor;
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

	//add normal values to the output, so it can be used for coloring
	output.Normal = input.Normal3D;

	//add pixel coordinates
	output.Position3D = input.Position3D;

	return output;
}

float4 SimplePixelShader(VertexShaderOutput input) : COLOR0
{
	return PhongLighting(input);
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
	
	//add normal values to the output, so it can be used for coloring
	output.Normal = input.Normal3D;

	//add pixel coordinates
	output.Position3D = input.Position3D;

	return output;
}

float4 CellShaderPixelShader(VertexShaderOutput input) : COLOR0
{
	return CellShading(input);
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
	//get color from image
	float4 color = tex2D(TextureSampler, TextureCoordinate);

	//greyvalues
	float3 greyValues = (0.3, 0.59, 0.11);
	
	//return the changedcolor in greyvalues = dot product of colors and greyvalues
	return dot(color, greyValues);;
}

technique ColorFilter
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 ColorFilterPixelShader();
	}
}

//---------------------------------------- Technique: GaussianBlurTechnique ----------------------------------------

float weights[11] =
{

	0.0093, 0.028002, 0.065984, 0.121703, 0.175713, 0.198596, 0.175713, 0.121703, 0.065984, 0.028002, 0.0093
	//sigma 1
	//0.000003, 0.000229, 0.005977, 0.060598, 0.24173, 0.382925, 0.24173, 0.060598, 0.005977, 0.000229, 0.000003
	//0.00598, 0.060626, 0.241843, 0.383103, 0.241843, 0.060626, 0.00598
};
float offset[11] =
{
	//-3,-2,-1,0,1,2,3
	-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5
};

float4 GaussianBlurPixelShader(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float2 coord;
	coord.y = TextureCoordinate.y;
	float4 color = float4(0.0, 0.0, 0.0, 0.0);

		for (int i = 0; i < 11; ++i)
		{
		coord.x = TextureCoordinate.x + offset[i] / 800.0f;
		color += tex2D(TextureSampler, coord) * weights[i];
		}

	return color;

}

technique GaussianBlur
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 GaussianBlurPixelShader();
	}
}