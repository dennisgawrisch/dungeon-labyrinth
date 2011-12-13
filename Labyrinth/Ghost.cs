using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Ghost {
        private static Random Rand = new Random();
        private static Texture TextureFace = new Texture(new Bitmap("textures/ghost.png"));
        private static Texture TextureTop = new Texture(new Bitmap("textures/ghost-from-top.png"));
        private Map Map;

        private const float Size = 0.7f;
        private const float Speed = 0.01f;

        public Vector2 Position;
        public Vector2 Direction;
        public int AnimationFrame;

        public Ghost(Map Map) {
            this.Map = Map;
            RandomizePositionAndDirection();            
        }

        private void RandomizePositionAndDirection() {
            Position = new Vector2(Rand.Next(Map.Width), Rand.Next(Map.Height));

            var DirectionRand = Rand.Next(4);
            if (0 == DirectionRand) {
                Direction = new Vector2(1, 0);
            } else if (1 == DirectionRand) {
                Direction = new Vector2(-1, 0);
            } else if (2 == DirectionRand) {
                Direction = new Vector2(0, 1);
            } else {
                Direction = new Vector2(0, -1);
            }
        }

        public void Move() {
            Position = Vector2.Add(Position, Vector2.Multiply(Direction, Speed));

            if (
                (Position.X < 0)
                || (Position.Y < 0)
                || (Position.X >= Map.Width)
                || (Position.Y >= Map.Height)
                || (Rand.Next(1000) < 3)
            ) {
                RandomizePositionAndDirection();
            }
        }

        public void Render(bool Face = true) {
            var FramesCount = (int)(TextureFace.Width / TextureFace.Height);
            if (Rand.Next(100) < 10) {
                AnimationFrame = Rand.Next(FramesCount);
            }

            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Fog);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (Face) {
                TextureFace.Bind();
            } else {
                TextureTop.Bind();
            }

            GL.Color4(new Color4(1f, 1f, 1f, 1f));

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2((AnimationFrame - 1) / (float)FramesCount, 1);
            GL.Vertex3(-Size / 2, 0, -Size / 2);

            GL.TexCoord2((AnimationFrame - 1) / (float)FramesCount, 0);
            GL.Vertex3(-Size / 2, 0, Size / 2);

            GL.TexCoord2(AnimationFrame / (float)FramesCount, 0);
            GL.Vertex3(Size / 2, 0, Size / 2);

            GL.TexCoord2(AnimationFrame / (float)FramesCount, 1);
            GL.Vertex3(Size / 2, 0, -Size / 2);

            GL.End();

            GL.PopAttrib();
        }
    }
}