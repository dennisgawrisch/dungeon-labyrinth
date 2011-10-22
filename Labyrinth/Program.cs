using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Program : GameWindow {
        [STAThread]
        static void Main() {
            using (var Program = new Program()) {
                Program.Run(30);
            }
        }

        Menu Menu;
        Game Game;
        bool MenuIsActive;

        public Program()
            : base(800, 600, GraphicsMode.Default, "Labyrinth") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs E) {
            base.OnLoad(E);

            Menu = new Menu(); Menu.Window = this;
            Game = new Game(); Game.Window = this;
            MenuIsActive = false;
        }

        protected override void OnResize(EventArgs E) {
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs E) {
            base.OnUpdateFrame(E);

            if (Keyboard[Key.Escape]) {
                if (MenuIsActive) {
                    MenuIsActive = false;
                    // TODO check if game is null
                } else {
                    MenuIsActive = true;
                }
            }

            Game.Tick();
        }

        protected override void OnRenderFrame(FrameEventArgs E) {
            base.OnRenderFrame(E);

            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (Game != null) {
                Game.Render();
            }

            if (MenuIsActive) {
                Menu.Render();
            }

            SwapBuffers();
        }
    }
}