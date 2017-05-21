using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SLT_Printer
{
    class GameObject
    {
        public Model Model { get; set; }
        public Vector3 Position { get; set; }
        public bool IsActive { get; set; }
        public BoundingSphere BoundingSphere { get; set; }

        public GameObject()
        {
            Model = null;
            Position = Vector3.Zero;
            IsActive = false;
            BoundingSphere = new BoundingSphere();
        }
    }

    class Camera
    {
        public Vector3 AvatarHeadOffset { get; set; }
        public Vector3 TargetOffset { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        public Camera()
        {
            AvatarHeadOffset = new Vector3(0, 7, -15);
            TargetOffset = new Vector3(0, 5, 0);
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
        }

        public void Update(float avatarYaw, Vector3 position, float aspectRatio)
        {
            Matrix rotationMatrix = Matrix.CreateRotationY(avatarYaw);

            Vector3 transformedheadOffset =
                Vector3.Transform(AvatarHeadOffset, rotationMatrix);
            Vector3 transformedReference =
                Vector3.Transform(TargetOffset, rotationMatrix);

            Vector3 cameraPosition = position + transformedheadOffset;
            Vector3 cameraTarget = position + transformedReference;

            //Calculate the camera's view and projection 
            //matrices based on current values.
            ViewMatrix =
                Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            ProjectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(GameConstants.ViewAngle), aspectRatio,
                    GameConstants.NearClip, GameConstants.FarClip);
        }
    }

    class GameConstants
    {
        //camera constants
        public const float NearClip = 1.0f;
        public const float FarClip = 1000.0f;
        public const float ViewAngle = 45.0f;
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class Game1 : Game
    {
        GameObject ground;
        Camera gameCamera;




        private readonly GraphicsDeviceManager graphics;
        // Set the 3D model to draw.
        Model terrainModel;
        Vector3 terrainPosition = Vector3.Zero; 
        //Model myModel;
        SpriteBatch spriteBatch;

        // The aspect ratio determines how to scale 3d to 2d projection.
        float aspectRatio;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //#if MACOS
            //            Content.RootDirectory = "AngryNinjas/Content";
            //#else
            Content.RootDirectory = "Content";
            //#endif
            //
            //#if XBOX || OUYA
            //            graphics.IsFullScreen = true;
            //#else
            graphics.IsFullScreen = false;
            //#endif

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333 / 2);

            // Extend battery life under lock.
            //InactiveSleepTime = TimeSpan.FromSeconds(1);

            //#if XBOX || OUYA
            //            CCDirector.SharedDirector.GamePadEnabled = true;
            //            application.GamePadButtonUpdate += new CCGamePadButtonDelegate(application_GamePadButtonUpdate);
            //#endif
            this.Run();
        }

        //#if XBOX || OUYA
        //        private void application_GamePadButtonUpdate(CCGamePadButtonStatus backButton, CCGamePadButtonStatus startButton, CCGamePadButtonStatus systemButton, CCGamePadButtonStatus aButton, CCGamePadButtonStatus bButton, CCGamePadButtonStatus xButton, CCGamePadButtonStatus yButton, CCGamePadButtonStatus leftShoulder, CCGamePadButtonStatus rightShoulder, PlayerIndex player)
        //        {
        //            if (backButton == CCGamePadButtonStatus.Pressed)
        //            {
        //                ProcessBackClick();
        //            }
        //        }
        //#endif

        protected override void Initialize()
        {
            ground = new GameObject();
            gameCamera = new Camera();

            aspectRatio = (float)graphics.PreferredBackBufferWidth /
            (float)graphics.PreferredBackBufferHeight;

            base.Initialize();
        }

        //Posición del modelo
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;

        // Posición de la cámara para la matriz de vista
        Vector3 cameraPosition = new Vector3(0.0f, 60.0f, 160.0f);// colocamos la camara
        Vector3 cameraLookAt = new Vector3(0.0f, 50.0f, 0.0f); // seleccionamos a que punto esta mirando
        Matrix cameraProjectionMatrix; // Determina el volument de proyección ( tipo, tamaño, ...)
        Matrix cameraViewMatrix; // determina el tipo de visión de la camara

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cameraViewMatrix = Matrix.CreateLookAt(
                   cameraPosition,
                   cameraLookAt,
                   Vector3.Up);

            cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                  MathHelper.ToRadians(45.0f),
                  graphics.GraphicsDevice.Viewport.AspectRatio,
                  1.0f,
                  10000.0f);


            
            
            Content.RootDirectory = "D:\\JORGE\\ARDUINO\\02 Prusa\\VS\\SLT Printer + XNA\\SLT Printer\\Content\\";
            ground.Model = Content.Load<Model>("Models/terrain");
            terrainModel = Content.Load<Model>("Models/terrain");
           
            //myModel = Content.Load<Model>("Models\\p1_wedge");
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            ;
        }



        void DrawModel(Model model, Vector3 modelPosition)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = Matrix.CreateTranslation(modelPosition);
                    effect.Projection = cameraProjectionMatrix;
                    effect.View = cameraViewMatrix;
                }
                mesh.Draw();
            }
            DrawModel(terrainModel, terrainPosition);
        } 

        /*protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Dibujamos el modelo, puede tener múltiples mallas, asi que iteramos sobre todas.
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                //Aquí es donde se fija la orientación de la malla, así como nuestra cámara y la proyección.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = Matrix.CreateTranslation(modelPosition);
                    effect.Projection = cameraProjectionMatrix;
                    effect.View = cameraViewMatrix;
                }
                //Dibuja la malla, usando los valores de arriba.
                mesh.Draw();
            }
            base.Draw(gameTime);
        }*/

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                ButtonState.Pressed)
                this.Exit();

            modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                MathHelper.ToRadians(0.1f);

            base.Update(gameTime);
        }

        
        

       
        
    }
}
