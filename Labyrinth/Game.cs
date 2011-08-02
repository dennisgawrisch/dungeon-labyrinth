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

        private Map map;
        private Vector3 playerPosition;
        private float playerAngle; // in degrees, 0 = Y↑, 90 = X→

        public Game()
            : base(800, 600, GraphicsMode.Default, "OpenGL Test #1") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            map = new Map(30, 30);

            playerPosition = new Vector3(0.5f, 0.5f, 0.5f);
            playerAngle = 0;
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

            var newPlayerPosition = Vector3.Add(playerPosition, Vector3.TransformVector(playerMovementVector, playerAngleMatrix));
            if (Map.CellType.Empty == map.GetCell((int)Math.Floor(newPlayerPosition.X), (int)Math.Floor(newPlayerPosition.Y))) {
                playerPosition = newPlayerPosition;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.Rotate(playerAngle, Vector3.UnitZ);
            GL.Translate(Vector3.Multiply(playerPosition, -1f));

            var wallsHeight = 2;

            GL.Color4(Color4.SaddleBrown);
            for (var x = 0; x < map.Width; x++) {
                for (var y = 0; y < map.Height; y++) {
                    if (Map.CellType.Empty == map.GetCell(x, y)) {
                        if (Map.CellType.Wall == map.GetCell(x, y - 1)) {
                            GL.Begin(BeginMode.Quads);
                            GL.Vertex3(x, y, 0);
                            GL.Vertex3(x, y, wallsHeight);
                            GL.Vertex3(x + 1, y, wallsHeight);
                            GL.Vertex3(x + 1, y, 0);
                            GL.End();
                        }

                        if (Map.CellType.Wall == map.GetCell(x, y + 1)) {
                            GL.Begin(BeginMode.Quads);
                            GL.Vertex3(x, y + 1, 0);
                            GL.Vertex3(x, y + 1, wallsHeight);
                            GL.Vertex3(x + 1, y + 1, wallsHeight);
                            GL.Vertex3(x + 1, y + 1, 0);
                            GL.End();
                        }

                        if (Map.CellType.Wall == map.GetCell(x - 1, y)) {
                            GL.Begin(BeginMode.Quads);
                            GL.Vertex3(x, y, 0);
                            GL.Vertex3(x, y, wallsHeight);
                            GL.Vertex3(x, y + 1, wallsHeight);
                            GL.Vertex3(x, y + 1, 0);
                            GL.End();
                        }

                        if (Map.CellType.Wall == map.GetCell(x + 1, y)) {
                            GL.Begin(BeginMode.Quads);
                            GL.Vertex3(x + 1, y, 0);
                            GL.Vertex3(x + 1, y, wallsHeight);
                            GL.Vertex3(x + 1, y + 1, wallsHeight);
                            GL.Vertex3(x + 1, y + 1, 0);
                            GL.End();
                        }
                    }
                }
            }

            GL.Color4(Color4.AntiqueWhite);
            GL.Begin(BeginMode.Quads);
            
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(map.Width, 0, 0);
            GL.Vertex3(map.Width, map.Height, 0);
            GL.Vertex3(0, map.Height, 0);

            GL.Vertex3(0, 0, wallsHeight);
            GL.Vertex3(map.Width, 0, wallsHeight);
            GL.Vertex3(map.Width, map.Height, wallsHeight);
            GL.Vertex3(0, map.Height, wallsHeight);

            GL.End();

            SwapBuffers();
        }
    }
}