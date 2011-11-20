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
        private bool NeedToMeasureRequiredWidth, NeedToMeasureRequiredHeight, NeedToRenderTexture;
        private int CachedRequiredWidth, CachedRequiredHeight, CachedRequiredHeightForWidth;
        private int Texture;
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

        public Text(string Label) {
            this.Label = Label;
            Font = new Font(new FontFamily(GenericFontFamilies.SansSerif), 30, GraphicsUnit.Pixel);
            Texture = GL.GenTexture();
        }

        protected void RenderTexture() {
            GL.BindTexture(TextureTarget.Texture2D, Texture);
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
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            if (NeedToRenderTexture || (RenderedTextureForWidth != Width.Value) || (RenderedTextureForHeight != Height.Value)) {
                using (var Bitmap = new Bitmap(Width.Value, Height.Value)) {
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
                NeedToRenderTexture = false;
                RenderedTextureForWidth = Width.Value;
                RenderedTextureForHeight = Height.Value;
            }

            GL.Color4(Color4.Black);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(Left.Value, Top.Value);
            GL.TexCoord2(1, 0); GL.Vertex2(Right.Value, Top.Value);
            GL.TexCoord2(1, 1); GL.Vertex2(Right.Value, Bottom.Value);
            GL.TexCoord2(0, 1); GL.Vertex2(Left.Value, Bottom.Value);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.PopAttrib();
        }
    }
}