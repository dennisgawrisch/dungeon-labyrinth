using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Program : GameWindow {
        [STAThread]
        static void Main() {
            using (var program = new Program()) {
                program.Run(30);
            }
        }

        Menu menu;
        Game game;
        bool menuIsActive;

        public Program()
            : base(800, 600, GraphicsMode.Default, "Labyrinth") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            menu = new Menu();
            game = new Game();
            menuIsActive = false;
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape]) {
                if (menuIsActive) {
                    menuIsActive = false;
                    // TODO check if game is null
                } else {
                    menuIsActive = true;
                }
            }

            game.OnUpdateFrame(this);
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (game != null) {
                game.OnRenderFrame(this);
            }

            if (menuIsActive) {
                menu.OnRenderFrame(this);
            }

            SwapBuffers();
        }
    }
}