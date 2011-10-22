using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Menu : GameWindowLayer {
        public Menu() {
        }

        public override void Tick() {
        }

        public override void Render() {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);

            var Projection = Matrix4.CreateOrthographic(-(float)Window.Width, -(float)Window.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
            GL.Translate(Window.Width / 2, -Window.Height / 2, 0);

            var Modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Modelview);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Color4(new Color4(0, 0, 0, 200));
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(Window.Width, 0);
            GL.Vertex2(Window.Width, Window.Height);
            GL.Vertex2(0, Window.Height);
            GL.End();

            GL.Disable(EnableCap.Blend);

            GL.Color4(Color4.Red);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(10, 0);
            GL.Vertex2(10, 10);
            GL.Vertex2(0, 10);
            GL.End();

            GL.Color4(Color4.Green);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(Window.Width - 10, 0);
            GL.Vertex2(Window.Width, 0);
            GL.Vertex2(Window.Width, 10);
            GL.Vertex2(Window.Width - 10, 10);
            GL.End();

            GL.Color4(Color4.Blue);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(Window.Width - 10, Window.Height - 10);
            GL.Vertex2(Window.Width, Window.Height - 10);
            GL.Vertex2(Window.Width, Window.Height);
            GL.Vertex2(Window.Width - 10, Window.Height);
            GL.End();
        }
    }
}