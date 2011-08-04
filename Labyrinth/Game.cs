using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Game : GameWindow {
        [STAThread]
        static void Main() {
            using (var game = new Game()) {
                game.Run(30);
            }
        }

		private Random rand;

        private Map map;
        private Vector3 playerPosition;
        private float playerAngle; // in degrees, 0 = Y↑, 90 = X→

        private float playerMovementSpeed = 0.05f; // in cell units
        private float playerTurnSpeed = 5; // in degrees

        private float wallsHeight = 0.7f;

        private int textureWall;

		private float torchLightMin = 0;
		private float torchLightMax = 10;
        private float torchLight = 5;
        private float torchLightMaxChange = 1f;

        public Game()
            : base(800, 600, GraphicsMode.Default, "Labyrinth") {
            VSync = VSyncMode.On;
			rand = new Random();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            textureWall = LoadTexture("../../textures/wall.png");

            map = new Map(10, 10); // TODO parametrize

            playerPosition = new Vector3(map.StartPosition.X + 0.5f, map.StartPosition.Y + 0.5f, 0.5f);
            playerAngle = 0;
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape]) {
                Exit();
            }

            if (Keyboard[Key.Left]) {
                playerAngle -= playerTurnSpeed;
            }
            if (Keyboard[Key.Right]) {
                playerAngle += playerTurnSpeed;
            }

            var playerAngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-playerAngle));
            var playerMovementVector = new Vector3(0, 0, 0);

            if (Keyboard[Key.Up] || Keyboard[Key.W]) {
                playerMovementVector.Y += playerMovementSpeed;
            }
            if (Keyboard[Key.Down] || Keyboard[Key.S]) {
                playerMovementVector.Y -= playerMovementSpeed;
            }
            if (Keyboard[Key.A]) {
                playerMovementVector.X -= playerMovementSpeed;
            }
            if (Keyboard[Key.D]) {
                playerMovementVector.X += playerMovementSpeed;
            }

            playerMovementVector = Vector3.TransformVector(playerMovementVector, playerAngleMatrix);

            var newPlayerPosition = playerPosition;
            newPlayerPosition.X += playerMovementVector.X;
            if (Map.CellType.Empty == map.GetCell(newPlayerPosition.Xy)) {
                playerPosition = newPlayerPosition;
            }

            newPlayerPosition = playerPosition;
            newPlayerPosition.Y += playerMovementVector.Y;
            if (Map.CellType.Empty == map.GetCell(newPlayerPosition.Xy)) {
                playerPosition = newPlayerPosition;
            }

            if (((int)(Math.Floor(playerPosition.X)) == map.FinishPosition.X) && ((int)(Math.Floor(playerPosition.Y)) == map.FinishPosition.Y)) {
                Exit(); // TODO
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            var projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 0.00001f, Math.Max(map.Width, map.Height) * 2f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            
            GL.ClearColor(Color4.Black);
            GL.Color4(Color4.Transparent);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Light1);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
            var modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.Rotate(playerAngle, Vector3.UnitZ);
            GL.Translate(Vector3.Multiply(playerPosition, -1f));

            var torchPosition = new Vector4(playerPosition);
            torchPosition.W = 1;

			torchLight += (rand.Next(-100, +100) / 100f) * torchLightMaxChange;
			torchLight = Math.Max(torchLight, torchLightMin);
			torchLight = Math.Min(torchLight, torchLightMax);

            GL.Light(LightName.Light0, LightParameter.Position, torchPosition);
            GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, (-torchLight / 3f + 10.3333f) / 100f);
            GL.Light(LightName.Light0, LightParameter.Diffuse, Color4.SaddleBrown);

            GL.Light(LightName.Light1, LightParameter.Position, new Vector4(map.FinishPosition.X + 0.5f, map.FinishPosition.Y + 0.5f, wallsHeight * 100, 1));
            GL.Light(LightName.Light1, LightParameter.Diffuse, Color4.White);
            GL.Light(LightName.Light1, LightParameter.Specular, Color4.White);
            GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 0f);
            GL.Light(LightName.Light1, LightParameter.SpotDirection, new Vector4(0, 0, -1, 0));
            GL.Light(LightName.Light1, LightParameter.SpotCutoff, 1);

            for (var x = 0; x < map.Width; x++) {
                for (var y = 0; y < map.Height; y++) {
                    var position = new Vector2(x, y);

                    if (Map.CellType.Empty == map.GetCell(x, y)) {
                        if (Map.CellType.Wall == map.GetCell(x, y + 1)) {
                            RenderWall(new Vector2(x, y + 1), new Vector2(x + 1, y + 1));
                        }
                        if (Map.CellType.Wall == map.GetCell(x, y - 1)) {
                            RenderWall(new Vector2(x + 1, y), new Vector2(x, y));
                        }
                        if (Map.CellType.Wall == map.GetCell(x - 1, y)) {
                            RenderWall(new Vector2(x, y), new Vector2(x, y + 1));
                        }
                        if (Map.CellType.Wall == map.GetCell(x + 1, y)) {
                            RenderWall(new Vector2(x + 1, y + 1), new Vector2(x + 1, y));
                        }

                        RenderFloor(position);
                        if (map.FinishPosition.Equals(position)) {
                            RenderExit(position);
                        } else {
                            RenderCeiling(position);
                        }
                    }
                }
            }

            SwapBuffers();
        }

        private int LoadTexture(string filename) {
            var bitmap = new Bitmap(filename);

            var texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return texture;
        }

        private void RenderWall(Vector2 A, Vector2 B, float z) {
            GL.BindTexture(TextureTarget.Texture2D, textureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(A.X, A.Y, z + wallsHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(B.X, B.Y, z + wallsHeight);
            GL.TexCoord2(1, wallsHeight); GL.Vertex3(B.X, B.Y, z);
            GL.TexCoord2(0, wallsHeight); GL.Vertex3(A.X, A.Y, z);
            GL.End();
        }

        private void RenderWall(Vector2 A, Vector2 B) {
            RenderWall(A, B, 0);
        }

        private void RenderFloor(Vector2 position) {
            GL.BindTexture(TextureTarget.Texture2D, textureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 1); GL.Vertex3(position.X, position.Y, 0);
            GL.TexCoord2(1, 1); GL.Vertex3(position.X + 1, position.Y, 0);
            GL.TexCoord2(1, 0); GL.Vertex3(position.X + 1, position.Y + 1, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(position.X, position.Y + 1, 0);
            GL.End();
        }

        private void RenderCeiling(Vector2 position) {
            GL.BindTexture(TextureTarget.Texture2D, textureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(position.X, position.Y, wallsHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(position.X + 1, position.Y, wallsHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(position.X + 1, position.Y + 1, wallsHeight);
            GL.TexCoord2(0, 1); GL.Vertex3(position.X, position.Y + 1, wallsHeight);
            GL.End();
        }

        private void RenderExit(Vector2 position) {
            RenderWall(new Vector2(position.X, position.Y + 1), new Vector2(position.X + 1, position.Y + 1), wallsHeight);
            RenderWall(new Vector2(position.X + 1, position.Y), new Vector2(position.X, position.Y), wallsHeight);
            RenderWall(new Vector2(position.X, position.Y), new Vector2(position.X, position.Y + 1), wallsHeight);
            RenderWall(new Vector2(position.X + 1, position.Y + 1), new Vector2(position.X + 1, position.Y), wallsHeight);

            var ropeWidth = 0.01;

            GL.Begin(BeginMode.QuadStrip);
            GL.Vertex3(position.X + 0.5 - ropeWidth, position.Y + 0.5 - ropeWidth, 0.3);
            GL.Vertex3(position.X + 0.5 - ropeWidth, position.Y + 0.5 - ropeWidth, wallsHeight * 2);
            GL.Vertex3(position.X + 0.5 + ropeWidth, position.Y + 0.5 - ropeWidth, 0.3);
            GL.Vertex3(position.X + 0.5 + ropeWidth, position.Y + 0.5 - ropeWidth, wallsHeight * 2);
            GL.Vertex3(position.X + 0.5 + ropeWidth, position.Y + 0.5 + ropeWidth, 0.3);
            GL.Vertex3(position.X + 0.5 + ropeWidth, position.Y + 0.5 + ropeWidth, wallsHeight * 2);
            GL.Vertex3(position.X + 0.5 - ropeWidth, position.Y + 0.5 + ropeWidth, 0.3);
            GL.Vertex3(position.X + 0.5 - ropeWidth, position.Y + 0.5 + ropeWidth, wallsHeight * 2);
            GL.Vertex3(position.X + 0.5 - ropeWidth, position.Y + 0.5 - ropeWidth, 0.3);
            GL.Vertex3(position.X + 0.5 - ropeWidth, position.Y + 0.5 - ropeWidth, wallsHeight * 2);
            GL.End();
        }
    }
}