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
        private Vector3[] light;

        // Model
        private Model model;
        private Material modelMaterial;
          
        //Variable Declerations
        string sEffect;
        int teller;
        int iSwitch;
        bool b;
        float size;
        bool colorFilter;
        bool postProcess;

        //used for PostProcessing
        RenderTarget2D renderTarget;
        Rectangle rectangle;
        PresentationParameters presParameters;

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
            this.model = this.Content.Load<Model>("Models/femalehead");
            
            this.model.Meshes[0].MeshParts[0].Effect = effect;

            // Set Diffuse- & ambientcolor, ambient intensity, light direction, and specular color and intensity
            this.modelMaterial.DiffuseColor = Color.Red;
            this.modelMaterial.AmbientColor = Color.Red;
            this.modelMaterial.AmbientIntensity = 0.2f;
            this.modelMaterial.SpecularColor = Color.White;
            this.modelMaterial.SpecularIntensity = 2.0f;
            this.modelMaterial.SpecularPower = 25.0f;
            this.light = new Vector3[5];
            this.light[0] = new Vector3(50, 50, 50);
            this.light[1] = new Vector3(1000, 0, 50);
            this.light[2] = new Vector3(0, 50, 1000);
            this.light[3] = new Vector3(-50, 0, -50);
            this.light[4] = new Vector3(-100, -100, 0);

            //Start Values
            sEffect = "Simple";
            teller = 0;
            iSwitch = 0;
            b = true;
            size = 2.0f;
            colorFilter = false;
            postProcess = false;
            presParameters = this.GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(this.GraphicsDevice, 800, 600, true, presParameters.BackBufferFormat, presParameters.DepthStencilFormat);
            rectangle = new Rectangle(0, 0, 800, 600);
                     
        }
        
        protected override void Update(GameTime gameTime)
        {
            //float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f;
            float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float deltaAngleY = 0;
            float deltaAngleX = 0;
            KeyboardState kbState = Keyboard.GetState();
                        
            //counter goes up if space is pressed
            if (kbState.IsKeyDown(Keys.Space))
            {      
                if(b==true)
                {
                    teller++;
                    b = false;
                    iSwitch = teller % 4;
                }

                //effect file
                Effect effect = this.Content.Load<Effect>("Effects/Simple");               

                //switch that changes for each effect on spacebar
                switch(iSwitch)
                {
                        //Multiple Lightsources
                    case 0:
                        colorFilter = false;
                        postProcess = false;
                        sEffect = "Simple";
                        this.model = this.Content.Load<Model>("Models/femalehead");
                        this.model.Meshes[0].MeshParts[0].Effect = effect;
                        size = 2.0f;
                        break;

                        //Cell Shader
                    case 1:
                        colorFilter = false;
                        postProcess = false;
                        sEffect = "CellShader";
                        this.model = this.Content.Load<Model>("Models/femalehead");
                        this.model.Meshes[0].MeshParts[0].Effect = effect;
                        size = 2.0f;
                        break;

                        //Simple Color Filter
                    case 2:
                        colorFilter = true;
                        postProcess = true;
                        sEffect = "Simple";
                        this.model = this.Content.Load<Model>("Models/femalehead");
                        this.model.Meshes[0].MeshParts[0].Effect = effect;
                        size = 2.0f;
                        break;

                        //Gaussain Blur
                    case 3:
                        colorFilter = false;
                        postProcess = true;
                        sEffect = "Simple";
                        this.model = this.Content.Load<Model>("Models/femalehead");
                        this.model.Meshes[0].MeshParts[0].Effect = effect;
                        size = 2.0f;
                        break;

                        //Default
                    default:
                        colorFilter = false;
                        postProcess = false;
                        sEffect = "Simple";                        
                        this.model = this.Content.Load<Model>("Models/femalehead");
                        this.model.Meshes[0].MeshParts[0].Effect = effect;
                        size = 1.0f;
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

            if (kbState.IsKeyDown(Keys.Up))
                deltaAngleX += -3 * timeStep;
            if (kbState.IsKeyDown(Keys.Down))
                deltaAngleX += 3 * timeStep;
            if (deltaAngleX != 0)
                this.camera.Eye = Vector3.Transform(this.camera.Eye, Matrix.CreateRotationX(deltaAngleX));

            // Update the window title
            this.Window.Title = "XNA Renderer | FPS: " + this.frameRateCounter.FrameRate;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen in a predetermined color and clear the depth buffer
            this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DeepSkyBlue, 1.0f, 0);
                  
            //new render target for postprocessing
            if(postProcess)
            {
                this.GraphicsDevice.SetRenderTarget(renderTarget);
            }           

            // Get the model's only mesh
            ModelMesh mesh = this.model.Meshes[0];
            Effect effect = mesh.Effects[0];
            
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

            Matrix World = Matrix.CreateScale(size);

            Matrix InversedTransposedWorld = Matrix.Invert(Matrix.Transpose(World));

            effect.Parameters["World"].SetValue(World);
            effect.Parameters["InversedTransposedWorld"].SetValue(InversedTransposedWorld);
                      
            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.GraphicsDevice.BlendState = BlendState.Opaque;

            // Draw the model
            mesh.Draw();

            if(postProcess)
            {
                //set rendertarget to screen
                this.GraphicsDevice.SetRenderTarget(null);

                //What postprocess effect is used
                if(colorFilter)
                {
                    effect.CurrentTechnique = effect.Techniques["ColorFilter"];
                }
                else
                {
                    effect.CurrentTechnique = effect.Techniques["GaussianBlur"];
                }

                spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, effect);
                spriteBatch.Draw(renderTarget, rectangle, Color.White);
                spriteBatch.End();
            }
            
              
            base.Draw(gameTime);
        }
    }
}