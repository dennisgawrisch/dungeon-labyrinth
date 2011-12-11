using System;
using System.Drawing;
using System.Drawing.Text;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labyrinth.Gui {
    class Text : Element {
        private string LabelValue;
        private Font FontValue;
        private bool NeedToMeasureRequiredWidth, NeedToMeasureRequiredHeight, NeedToRenderTexture;
        private int CachedRequiredWidth, CachedRequiredHeight, CachedRequiredHeightForWidth;
        private Texture Texture;
        private int RenderedTextureForWidth, RenderedTextureForHeight;

        public string Label {
            get {
                return LabelValue;
            }

            set {
                LabelValue = value;
                NeedToMeasureRequiredWidth = true;
                NeedToMeasureRequiredHeight = true;
                NeedToRenderTexture = true;
            }
        }

        public Font Font {
            get {
                return FontValue;
            }

            set {
                FontValue = value;
                NeedToMeasureRequiredWidth = true;
                NeedToMeasureRequiredHeight = true;
                NeedToRenderTexture = true;
            }
        }

        public Color4 Color = Color4.Black;

        public Text(string Label) {
            this.Label = Label;
            Font = new Font(new FontFamily(GenericFontFamilies.SansSerif), 30, GraphicsUnit.Pixel);
        }

        protected override int GetRequiredWidth() {
            if (NeedToMeasureRequiredWidth) {
                using (var Bitmap = new Bitmap(1, 1)) {
                    using (Graphics Graphics = Graphics.FromImage(Bitmap)) {
                        var Measures = Graphics.MeasureString(Label, Font);
                        CachedRequiredWidth = (int)Math.Ceiling(Measures.Width);
                    }
                }
                NeedToMeasureRequiredWidth = false;
                NeedToMeasureRequiredHeight = true;
            }
            return CachedRequiredWidth;
        }

        protected override int GetRequiredHeight() {
            if (NeedToMeasureRequiredHeight || (CachedRequiredHeightForWidth != Width.Value)) {
                using (var Bitmap = new Bitmap(1, 1)) {
                    using (Graphics Graphics = Graphics.FromImage(Bitmap)) {
                        var Measures = Graphics.MeasureString(Label, Font, Width.Value);
                        CachedRequiredHeight = (int)Math.Ceiling(Measures.Height);
                    }
                }
                NeedToMeasureRequiredHeight = false;
                CachedRequiredHeightForWidth = Width.Value;
            }
            return CachedRequiredHeight;
        }

        public override void Render() {
            if ((null == Label) || ("" == Label)) {
                return;
            }

            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.Texture2D);

            if (NeedToRenderTexture || (RenderedTextureForWidth != Width.Value) || (RenderedTextureForHeight != Height.Value)) {
                using (var Bitmap = new Bitmap(Width.Value, Height.Value)) {
                    var Rectangle = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
                    using (Graphics Graphics = Graphics.FromImage(Bitmap)) {
                        Graphics.Clear(System.Drawing.Color.Transparent);
                        Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        Graphics.DrawString(Label, Font, Brushes.White, Rectangle);

                        if (null != Texture) {
                            Texture.Dispose();
                        }
                        Texture = new Texture(Bitmap);
                    }
                }
                NeedToRenderTexture = false;
                RenderedTextureForWidth = Width.Value;
                RenderedTextureForHeight = Height.Value;
            }

            Texture.Bind();

            GL.Color4(Color);

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex2(Left.Value, Top.Value);

            GL.TexCoord2((float)Texture.Width / Texture.PotWidth, 0);
            GL.Vertex2(Right.Value, Top.Value);

            GL.TexCoord2((float)Texture.Width / Texture.PotWidth, (float)Texture.Height / Texture.PotHeight);
            GL.Vertex2(Right.Value, Bottom.Value);

            GL.TexCoord2(0, (float)Texture.Height / Texture.PotHeight);
            GL.Vertex2(Left.Value, Bottom.Value);

            GL.End();

            GL.PopAttrib();
        }
    }
}