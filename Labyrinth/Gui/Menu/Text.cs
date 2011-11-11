using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth.Gui.Menu {
    class Text : Gui.Text {
        public Text(string Label)
            : base(Label) {
        }

        public override void Render() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Color4(new Color4(200, 255, 200, 70));

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
    }
}