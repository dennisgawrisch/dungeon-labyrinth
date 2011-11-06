using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Labyrinth.Gui;

namespace Labyrinth.Gui.Menu {
    class Button : Element {
        public string Label;
        public bool Enabled = true;

        public Button(string Label) {
            this.Label = Label;
        }

        protected override int GetRequiredWidth() {
            return Label.Length * 50; // TODO
        }

        protected override int GetRequiredHeight() {
            return 50; // TODO
        }

        public override void Render() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Color4(Color4.ForestGreen);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(Left.Value, Top.Value);
            GL.Vertex2(Right.Value, Top.Value);
            GL.Vertex2(Right.Value, Bottom.Value);
            GL.Vertex2(Left.Value, Bottom.Value);
            GL.End();

            GL.PopAttrib();

            // TODO

            // TODO respect Enabled
        }

        public event EventHandler<EventArgs> Enter;

        protected virtual void OnEnter(EventArgs E) {
            if (Enter != null) {
                Enter(this, E);
            }
        }

        public override void OnKeyPress(Key K) {
            if (Enabled && K.Equals(Key.Enter)) {
                OnEnter(EventArgs.Empty);
            }
        }
    }
}