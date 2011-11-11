using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth.Gui.Menu {
    class Button : Gui.Text, Control {
        public bool Enabled { get; set; }
        public bool Focused { get; set; }

        public Button(string Label) 
            : base(Label) {
            Enabled = true;
            Focused = false;
        }

        public override void Render() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (!Enabled) {
                GL.Color4(new Color4(255, 255, 255, 70));
            } else if (Focused) {
                GL.Color4(new Color4(100, 150, 200, 150));
            } else {
                GL.Color4(new Color4(100, 150, 200, 70));
            }

            GL.Begin(BeginMode.Quads);
            GL.Vertex2(Left.Value, Top.Value);
            GL.Vertex2(Right.Value, Top.Value);
            GL.Vertex2(Right.Value, Bottom.Value);
            GL.Vertex2(Left.Value, Bottom.Value);
            GL.End();

            GL.Disable(EnableCap.Blend);

            GL.PopAttrib();

            base.Render();
        }

        public event EventHandler<EventArgs> Enter;

        protected virtual void OnEnter(EventArgs E) {
            if (Enter != null) {
                Enter(this, E);
            }
        }

        public void OnKeyPress(Key K) {
            if (Enabled && (K.Equals(Key.Enter) || K.Equals(Key.KeypadEnter))) {
                OnEnter(EventArgs.Empty);
            }
        }
    }
}