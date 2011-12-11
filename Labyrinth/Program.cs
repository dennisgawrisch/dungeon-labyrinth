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

        private GameWindowLayer Layer;

        public Program()
            : base(800, 600, GraphicsMode.Default, "Labyrinth") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs E) {
            base.OnLoad(E);
            WindowState = WindowState.Fullscreen;
            CursorVisible = false;
            Layer = new MainCompositeLayer(this);
            Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(OnKeyDown);
        }

        protected override void OnResize(EventArgs E) {
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs E) {
            base.OnUpdateFrame(E);
            Layer.Tick();
        }

        protected void OnKeyDown(object Sender, KeyboardKeyEventArgs E) {
            Layer.OnKeyPress(E.Key);
        }

        protected override void OnRenderFrame(FrameEventArgs E) {
            base.OnRenderFrame(E);

            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Layer.Render();

            SwapBuffers();
        }
    }
}