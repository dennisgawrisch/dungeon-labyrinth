using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labyrinth.Gui {
    class Text : Element {
        private string LabelValue;
        private Font FontValue;
        private int Texture;
        private int RequiredWidth, RequiredHeight;

        public string Label {
            get {
                return LabelValue;
            }

            set {
                LabelValue = value;
                RenderTexture();
            }
        }

        public Font Font {
            get {
                return FontValue;
            }

            set {
                FontValue = value;
                RenderTexture();
            }
        }

        public Text(string Label) {
            LabelValue = Label;
            FontValue = new Font(new FontFamily(GenericFontFamilies.SansSerif), 50, GraphicsUnit.Pixel);
            Texture = GL.GenTexture();
            RenderTexture();
        }

        protected void RenderTexture() {
            using (var Bitmap = new Bitmap(1, 1)) {
                using (Graphics Graphics = Graphics.FromImage(Bitmap)) {
                    var Measures = Graphics.MeasureString(Label, Font);
                    RequiredWidth = (int)Math.Ceiling(Measures.Width);
                    RequiredHeight = (int)Math.Ceiling(Measures.Height);
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, Texture);
            using (var Bitmap = new Bitmap(GetRequiredWidth(), GetRequiredHeight())) {
                var Rectangle = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
                using (Graphics Graphics = Graphics.FromImage(Bitmap)) {
                    Graphics.Clear(Color.Transparent);
                    Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    Graphics.DrawString(Label, Font, Brushes.White, Rectangle);

                    var BitmapData = Bitmap.LockBits(Rectangle, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, BitmapData.Width, BitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapData.Scan0);
                    Bitmap.UnlockBits(BitmapData);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
            }
        }

        protected override int GetRequiredWidth() {
            return RequiredWidth;
        }

        protected override int GetRequiredHeight() {
            return RequiredHeight;
        }

        public override void Render() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            var TextWidth = GetRequiredWidth(); // TODO this is hack, it ignores the bounds (Left, Right, Width etc.)

            GL.Color4(Color4.Black);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(Left.Value, Top.Value);
            GL.TexCoord2(1, 0); GL.Vertex2(Right.Value - Width.Value + TextWidth, Top.Value);
            GL.TexCoord2(1, 1); GL.Vertex2(Right.Value - Width.Value + TextWidth, Bottom.Value);
            GL.TexCoord2(0, 1); GL.Vertex2(Left.Value, Bottom.Value);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.PopAttrib();
        }
    }
}