using System;
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

        private Vector3 playerPosition;
        private float playerAngle; // in degrees

        public Game()
            : base(800, 600, GraphicsMode.Default, "OpenGL Test #1") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            playerPosition = new Vector3(0.5f, 0.5f, 0.5f);
            playerAngle = 90;
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            var projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 0.00001f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape]) {
                Exit();
            }

            if (Keyboard[Key.Left]) {
                playerAngle -= 5f;
            }
            if (Keyboard[Key.Right]) {
                playerAngle += 5f;
            }

            var playerAngleMatrix = Matrix4.CreateRotationZ((float)(-playerAngle * Math.PI / 180));
            var playerMovementVector = new Vector3(0, 0, 0);

            if (Keyboard[Key.Up] || Keyboard[Key.W]) {
                playerMovementVector.Y += 0.1f;
            }
            if (Keyboard[Key.Down] || Keyboard[Key.S]) {
                playerMovementVector.Y -= 0.1f;
            }
            if (Keyboard[Key.A]) {
                playerMovementVector.X -= 0.1f;
            }
            if (Keyboard[Key.D]) {
                playerMovementVector.X += 0.1f;
            }

            playerPosition = Vector3.Add(playerPosition, Vector3.TransformVector(playerMovementVector, playerAngleMatrix));
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.Rotate(playerAngle, Vector3.UnitZ);
            GL.Translate(Vector3.Multiply(playerPosition, -1f));

            GL.Begin(BeginMode.Quads);
            
            GL.Color4(Color4.SaddleBrown);

            GL.Vertex3(0, 0, 0);
            GL.Vertex3(7, 0, 0);
            GL.Vertex3(7, 0, 2);
            GL.Vertex3(0, 0, 2);

            GL.Vertex3(0, 4, 0);
            GL.Vertex3(7, 4, 0);
            GL.Vertex3(7, 4, 2);
            GL.Vertex3(0, 4, 2);

            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 4, 0);
            GL.Vertex3(0, 4, 2);
            GL.Vertex3(0, 0, 2);

            GL.Vertex3(7, 0, 0);
            GL.Vertex3(7, 4, 0);
            GL.Vertex3(7, 4, 2);
            GL.Vertex3(7, 0, 2);

            GL.Vertex3(1, 1, 0);
            GL.Vertex3(4, 1, 0);
            GL.Vertex3(4, 1, 2);
            GL.Vertex3(1, 1, 2);

            GL.Vertex3(1, 2, 0);
            GL.Vertex3(4, 2, 0);
            GL.Vertex3(4, 2, 2);
            GL.Vertex3(1, 2, 2);

            GL.Vertex3(1, 1, 0);
            GL.Vertex3(1, 2, 0);
            GL.Vertex3(1, 2, 2);
            GL.Vertex3(1, 1, 2);

            GL.Vertex3(4, 1, 0);
            GL.Vertex3(4, 2, 0);
            GL.Vertex3(4, 2, 2);
            GL.Vertex3(4, 1, 2);

            GL.Color4(Color4.AntiqueWhite);
            
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(7, 0, 0);
            GL.Vertex3(7, 4, 0);
            GL.Vertex3(0, 4, 0);

            GL.Vertex3(0, 0, 2);
            GL.Vertex3(7, 0, 2);
            GL.Vertex3(7, 4, 2);
            GL.Vertex3(0, 4, 2);

            GL.End();

            SwapBuffers();
        }
    }
}