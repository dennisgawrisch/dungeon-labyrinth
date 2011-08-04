using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labyrinth {
    class Program : GameWindow {
        [STAThread]
        static void Main() {
            using (var program = new Program()) {
                program.Run(30);
            }
        }

        Game game;

        public Program()
            : base(800, 600, GraphicsMode.Default, "Labyrinth") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            game = new Game();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            game.OnUpdateFrame(this);
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            game.OnRenderFrame(this);

            SwapBuffers();
        }
    }
}