using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GraphicsPractical3
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Often used XNA objects
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private FrameRateCounter frameRateCounter;

        // Game objects and variables
        private Camera camera;
        private Vector3 light;

        // Model
        private Model model;
        private Material modelMaterial;

        // Quad
        private VertexPositionNormalTexture[] quadVertices;
        private short[] quadIndices;
        private Matrix quadTransform;
        private Texture2D texture;

        //Effect on Space
        string sEffect;
        int teller;
        int iSwitch;
        bool b;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            // Create and add a frame rate counter
            this.frameRateCounter = new FrameRateCounter(this);
            this.Components.Add(this.frameRateCounter);
        }

        protected override void Initialize()
        {
            // Copy over the device's rasterizer state to change the current fillMode
            this.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
            // Set up the window
            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 600;
            this.graphics.IsFullScreen = false;
            // Let the renderer draw and update as often as possible
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            // Flush the changes to the device parameters to the graphics card
            this.graphics.ApplyChanges();
            // Initialize the camera
            this.camera = new Camera(new Vector3(0, 50, 100), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

            this.IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a SpriteBatch object
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            // Load the "Simple" effect
            Effect effect = this.Content.Load<Effect>("Effects/Simple");
            // Load the model and let it use the "Simple" effect
            this.model = this.Content.Load<Model>("Models/Teapot");
            this.model.Meshes[0].MeshParts[0].Effect = effect;

            // Set Diffuse- & ambientcolor, ambient intensity, light direction, and specular color and intensity
            this.modelMaterial.DiffuseColor = Color.Red;
            this.modelMaterial.AmbientColor = Color.Red;
            this.modelMaterial.AmbientIntensity = 0.2f;
            this.modelMaterial.SpecularColor = Color.White;
            this.modelMaterial.SpecularIntensity = 2.0f;
            this.modelMaterial.SpecularPower = 25.0f;
            this.light = new Vector3(50, 50, 50);

            // Load Texture
            texture = Content.Load<Texture2D>("Textures/CobblestonesDiffuse");

            //Start Effect
            sEffect = "Simple";
            teller = 0;
            iSwitch = 0;
            b = true;
            
            // Setup the quad
            this.setupQuad();
        }

        /// <summary>
        /// Sets up a 2 by 2 quad around the origin.
        /// </summary>
        private void setupQuad()
        {
            float scale = 50.0f;

            // Normal points up
            Vector3 quadNormal = new Vector3(0, 1, 0);

            this.quadVertices = new VertexPositionNormalTexture[4];
            // Top left
            this.quadVertices[0].Position = new Vector3(-1, 0, -1);
            this.quadVertices[0].Normal = quadNormal;
            this.quadVertices[0].TextureCoordinate = new Vector2(-1, -1);
            // Top right
            this.quadVertices[1].Position = new Vector3(1, 0, -1);
            this.quadVertices[1].Normal = quadNormal;
            this.quadVertices[1].TextureCoordinate = new Vector2(1, -1);
            // Bottom left
            this.quadVertices[2].Position = new Vector3(-1, 0, 1);
            this.quadVertices[2].Normal = quadNormal;
            this.quadVertices[2].TextureCoordinate = new Vector2(-1, 1);
            // Bottom right
            this.quadVertices[3].Position = new Vector3(1, 0, 1);
            this.quadVertices[3].Normal = quadNormal;
            this.quadVertices[3].TextureCoordinate = new Vector2(1, 1);

            this.quadIndices = new short[] { 0, 1, 2, 1, 2, 3 };
            this.quadTransform = Matrix.Multiply(Matrix.CreateScale(scale), Matrix.CreateTranslation(0, -15.0f, 0));

        }

        protected override void Update(GameTime gameTime)
        {
            //float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f;
            float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float deltaAngleY = 0;
            float deltaAngleX = 0;
            KeyboardState kbState = Keyboard.GetState();
                        
            if (kbState.IsKeyDown(Keys.Space))
            {      
                if(b==true)
                {
                    teller++;
                    b = false;
                    iSwitch = teller % 4;                    
                }

                switch(iSwitch)
                {
                    case 0:
                        sEffect = "Simple";
                        break;

                    case 1: 
                        sEffect = "Test";
                        break;

                    case 2: 
                        sEffect = "Simple";
                        break;

                    case 3: 
                        sEffect = "Test";
                        break;

                    default:
                        sEffect = "Simple";
                        break;
                }

            }
            if (kbState.IsKeyUp(Keys.Space))
            {
                b = true;
            }

            //CameraPosition Rotations
            if (kbState.IsKeyDown(Keys.Left))
                deltaAngleY += -3 * timeStep;
            if (kbState.IsKeyDown(Keys.Right))
                deltaAngleY += 3 * timeStep;
            if (deltaAngleY != 0)
                this.camera.Eye = Vector3.Transform(this.camera.Eye, Matrix.CreateRotationY(deltaAngleY));

            if (kbState.IsKeyDown(Keys.Down))
                deltaAngleX += -3 * timeStep;
            if (kbState.IsKeyDown(Keys.Up))
                deltaAngleX += 3 * timeStep;
            if (deltaAngleX != 0)
                this.camera.Eye = Vector3.Transform(this.camera.Eye, Matrix.CreateRotationX(deltaAngleX));

            //exit game on backbutton press on controller
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Update the window title
            this.Window.Title = "XNA Renderer | FPS: " + this.frameRateCounter.FrameRate;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen in a predetermined color and clear the depth buffer
            this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DeepSkyBlue, 1.0f, 0);

            // Get the model's only mesh
            ModelMesh mesh = this.model.Meshes[0];
            Effect effect = mesh.Effects[0];
            //Effect TextureEffect = mesh.Effects[0];
           
            // Set the effect parameters, Color, LightSource, Ambient and specular
            effect.Parameters["DiffuseColor"].SetValue(modelMaterial.DiffuseColor.ToVector4());
            effect.Parameters["PointLight"].SetValue(light);
            effect.Parameters["AmbientColor"].SetValue(modelMaterial.AmbientColor.ToVector4());
            effect.Parameters["AmbientIntensity"].SetValue(modelMaterial.AmbientIntensity);
            effect.Parameters["SpecularColor"].SetValue(modelMaterial.SpecularColor.ToVector4());
            effect.Parameters["SpecularPower"].SetValue(modelMaterial.SpecularPower);
            effect.Parameters["SpecularIntensity"].SetValue(modelMaterial.SpecularIntensity);
            effect.CurrentTechnique = effect.Techniques[sEffect];
            // Matrices for 3D perspective projection
            this.camera.SetEffectParameters(effect);

            Matrix World = Matrix.CreateScale(10.0f);

            Matrix InversedTransposedWorld = Matrix.Invert(Matrix.Transpose(World));

            effect.Parameters["World"].SetValue(World);
            effect.Parameters["InversedTransposedWorld"].SetValue(InversedTransposedWorld);

            // Draw the model
            mesh.Draw();

            /*
            //Adding Texture
            TextureEffect.Parameters["Texture"].SetValue(this.texture);
            TextureEffect.Parameters["World"].SetValue(this.quadTransform);
            TextureEffect.CurrentTechnique = TextureEffect.Techniques["TextureTechnique"];
            this.camera.SetEffectParameters(TextureEffect);

            foreach (EffectPass pass in TextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, this.quadVertices, 0, 4, this.quadIndices, 0, 2);
            }*/

            base.Draw(gameTime);
        }
    }
}