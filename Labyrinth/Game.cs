using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private Triangle triangle;

        public Game()
            : base(800, 600, GraphicsMode.Default, "OpenGL Test #1") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            triangle = new Triangle();
            triangle.Position = new Vector3(0, 0, 4);
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();

            if (Keyboard[Key.Up])
                triangle.Position.Y += 0.1f;
            if (Keyboard[Key.Down])
                triangle.Position.Y -= 0.1f;
            if (Keyboard[Key.Left])
                triangle.Position.X += 0.1f;
            if (Keyboard[Key.Right])
                triangle.Position.X -= 0.1f;
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            triangle.Render();

            SwapBuffers();
        }
    }

    class Triangle {
        public Vector3 Position;

        public void Render() {
            GL.Begin(BeginMode.Triangles);

            GL.Color4(Color4.Red); GL.Vertex3(Position.X, Position.Y + 0.5, Position.Z);
            GL.Color4(Color4.Green); GL.Vertex3(Position.X - 0.5, Position.Y - 0.5, Position.Z);
            GL.Color4(Color4.Blue); GL.Vertex3(Position.X + 0.5, Position.Y - 0.5, Position.Z);

            GL.End();
        }
    }
}