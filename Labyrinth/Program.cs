using System;
using System.Collections.Generic;
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

        private Array Keys;
        private HashSet<Key> PressedKeys = new HashSet<Key>();

        public Program()
            : base(800, 600, GraphicsMode.Default, "Labyrinth") {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs E) {
            base.OnLoad(E);
            Layer = new MainCompositeLayer(this);
            Keys = Enum.GetValues(typeof(OpenTK.Input.Key));
        }

        protected override void OnResize(EventArgs E) {
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs E) {
            base.OnUpdateFrame(E);
            Layer.Tick();

            foreach (OpenTK.Input.Key Key in Keys) {
                if (Keyboard[Key]) {
                    PressedKeys.Add(Key);
                } else if (PressedKeys.Contains(Key)) {
                    PressedKeys.Remove(Key);
                    Layer.OnKeyPress(Key);
                }
            }
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