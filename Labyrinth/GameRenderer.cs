using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labyrinth.Gui;

namespace Labyrinth {
    class GameRenderer {
        private Game Game;
        private Random Rand = new Random();

        public enum CameraMode {
            FirstPerson,
            ThirdPerson
        };
        public CameraMode Camera = CameraMode.FirstPerson;

        private const float WallsHeight = 0.7f;
        private const float WallsHeightVariation = 0.1f;
        private const float WallsXyVariation = 0.1f;
        private const float PlayerModelSize = 0.3f;
        private const float IconMinSize = 0.35f, IconMaxSize = 0.40f;
        private const float VisibilityDistance = 6f;
        private const float GhostSize = 0.7f;

        private Texture TextureWall, TextureExit, TextureKey, TextureMark, TextureGhostSide, TextureGhostTop;
        private Color4[] CheckpointsColors = { Color4.OrangeRed, Color4.Aquamarine, Color4.DodgerBlue, Color4.Yellow };
        private int GhostFramesCount;
        private Text WinLabel;

        private struct IconBufferRecord {
            public Vector2 Position;
            public Texture Texture;
            public Color4 Color;
        }
        private List<IconBufferRecord> IconsBuffer = new List<IconBufferRecord>(10);

        private float TorchLight = 0;
        private float TorchLightChangeDirection = +1;

        private int? FadeOutStarted = null;
        private const int FadeOutLength = 42;

        public GameRenderer(Game Game) {
            this.Game = Game;

            TextureWall = new Texture(new Bitmap("textures/wall.png"));
            TextureExit = new Texture(new Bitmap("textures/exit.png"));
            TextureKey = new Texture(new Bitmap("textures/key.png"));
            TextureMark = new Texture(new Bitmap("textures/mark.png"));
            TextureGhostSide = new Texture(new Bitmap("textures/ghost.png"));
            TextureGhostTop = new Texture(new Bitmap("textures/ghost-from-top.png"));

            GhostFramesCount = (int)(TextureGhostSide.Width / TextureGhostSide.Height);

            WinLabel = new Text("You win!");
            WinLabel.Color = Color4.White;
            WinLabel.Font = new Font(new FontFamily(GenericFontFamilies.SansSerif), 50, GraphicsUnit.Pixel);
        }

        public void Render(GameWindow Window) {
            GL.Enable(EnableCap.DepthTest);

            var Projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Window.Width / (float)Window.Height, 1e-3f, Math.Max(Game.Map.Width, Game.Map.Height));
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);

            var Modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Modelview);

            if (CameraMode.ThirdPerson == Camera) {
                GL.Rotate(90, Vector3.UnitX); // look down
            }

            var PlayerPosition3d = new Vector3(Game.Player.Position.X, Game.Player.Position.Y, 0.5f); // TODO varioused point

            GL.Rotate(Game.Player.Angle, Vector3.UnitZ);
            GL.Translate(Vector3.Multiply(PlayerPosition3d, -1f));
            if (CameraMode.ThirdPerson == Camera) {
                GL.Translate(0, 0, -WallsHeight * 5);
            }

            var TorchPosition = new Vector4(Game.Player.Position);
            TorchPosition.W = 1;

            var TorchLightMin = 0.10f;
            var TorchLightMax = 0.20f;
            var TorchLightChangeSpeed = 0.007f;

            if (Game.StateEnum.Playing == Game.State) {
                TorchLight += Rand.Next(-100, +100) / 100f * TorchLightChangeSpeed * TorchLightChangeDirection;
                TorchLight = Math.Max(TorchLight, TorchLightMin);
                TorchLight = Math.Min(TorchLight, TorchLightMax);
                if ((TorchLightMin == TorchLight) || (TorchLightMax == TorchLight) || (Rand.Next(100) < 30)) {
                    TorchLightChangeDirection = -TorchLightChangeDirection;
                }
            }

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Position, TorchPosition);
            GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, TorchLight);
            GL.Light(LightName.Light0, LightParameter.Ambient, Color4.SaddleBrown);
            GL.Light(LightName.Light0, LightParameter.Diffuse, Color4.SaddleBrown);
            GL.Light(LightName.Light0, LightParameter.Specular, Color4.SaddleBrown);

            GL.Enable(EnableCap.Fog);
            GL.Fog(FogParameter.FogDensity, (CameraMode.ThirdPerson != Camera) ? 0.5f : 0.1f);

            IconsBuffer.Clear();

            RenderMap();

            for (var i = 0; i < Game.Map.Checkpoints.Count; i++) {
                if (!Game.CollectedCheckpoints.Contains(i)) {
                    RenderCheckpoint(Game.Map.Checkpoints[i], i);
                }
            }

            RenderExit(Game.Map.FinishPosition);

            foreach (var Mark in Game.Marks) {
                RenderMark(Mark);
            }

            if (CameraMode.ThirdPerson == Camera) {
                RenderPlayer();
            }

            foreach (var Ghost in Game.Ghosts) { // TODO make buffer (and merge with icons’ buffer) for the sake of transparency
                RenderGhost(Ghost);
            }

            RenderBufferedIcons();

            if (Game.StateEnum.Win == Game.State) {
                RenderWinScreen(Window);
            }
        }

        private void RenderMap() {
            GL.PushAttrib(AttribMask.TextureBit);

            GL.Enable(EnableCap.Texture2D);
            TextureWall.Bind();

            GL.Begin(BeginMode.Quads);

            var Xmin = (int)Math.Floor(Math.Max(Game.Player.Position.X - VisibilityDistance, 0));
            var Ymin = (int)Math.Floor(Math.Max(Game.Player.Position.Y - VisibilityDistance, 0));
            var Xmax = (int)Math.Ceiling(Math.Min(Game.Player.Position.X + VisibilityDistance, Game.Map.Width - 1));
            var Ymax = (int)Math.Ceiling(Math.Min(Game.Player.Position.Y + VisibilityDistance, Game.Map.Height - 1));

            for (var X = Xmin; X <= Xmax; X++) {
                for (var Y = Ymin; Y <= Ymax; Y++) {
                    var Position = new Vector2(X, Y);

                    if (Map.CellType.Empty == Game.Map.GetCell(X, Y)) {
                        if (Map.CellType.Wall == Game.Map.GetCell(X, Y + 1)) {
                            RenderWall(new Vector2(X, Y + 1), new Vector2(X + 1, Y + 1));
                        }
                        if (Map.CellType.Wall == Game.Map.GetCell(X, Y - 1)) {
                            RenderWall(new Vector2(X + 1, Y), new Vector2(X, Y));
                        }
                        if (Map.CellType.Wall == Game.Map.GetCell(X - 1, Y)) {
                            RenderWall(new Vector2(X, Y), new Vector2(X, Y + 1));
                        }
                        if (Map.CellType.Wall == Game.Map.GetCell(X + 1, Y)) {
                            RenderWall(new Vector2(X + 1, Y + 1), new Vector2(X + 1, Y));
                        }

                        RenderFloor(Position);
                        if (CameraMode.FirstPerson == Camera) {
                            RenderCeiling(Position);
                        }
                    }
                }
            }

            GL.End();

            GL.PopAttrib();
        }

        private Vector3 VariousedPoint(Vector3 Position) {
            var Result = Position;
            Result.X += (float)Math.Sin((Position.X + Position.Y + Position.Z + Math.E) * 1.5) * WallsXyVariation;
            Result.Y += (float)Math.Sin((Position.X + Position.Y + Position.Z + Math.E) * 2.5) * WallsXyVariation;
            Result.Z += (float)Math.Sin((Position.X + Position.Y + Position.Z + Math.E) * 1.0) * WallsHeightVariation;
            return Result;
        }

        private Vector3 VariousedPoint(float X, float Y, float Z) {
            return VariousedPoint(new Vector3(X, Y, Z));
        }

        private void RenderWall(Vector2 A, Vector2 B) {
            var C = new Vector2((A.X + B.X) / 2f, (A.Y + B.Y) / 2f); // middlepoint
            Vector2[] P1 = { A, C };
            Vector2[] P2 = { C, B };

            for (var i = 0; i < 2; i++) {
                var M = P1[i];
                var N = P2[i];
                for (float Z = 0; Z < WallsHeight; Z += WallsHeight / 2) {
                    GL.TexCoord2(M.Equals(A) ? 0 : 0.5, Z + WallsHeight / 2); GL.Vertex3(VariousedPoint(M.X, M.Y, Z + WallsHeight / 2));
                    GL.TexCoord2(M.Equals(A) ? 0.5 : 1, Z + WallsHeight / 2); GL.Vertex3(VariousedPoint(N.X, N.Y, Z + WallsHeight / 2));
                    GL.TexCoord2(M.Equals(A) ? 0.5 : 1, Z); GL.Vertex3(VariousedPoint(N.X, N.Y, Z));
                    GL.TexCoord2(M.Equals(A) ? 0 : 0.5, Z); GL.Vertex3(VariousedPoint(M.X, M.Y, Z));
                }
            }
        }

        private void RenderFloor(Vector2 P) {
            for (var X = P.X; X <= P.X + 0.5; X += 0.5f) {
                for (var Y = P.Y; Y <= P.Y + 0.5; Y += 0.5f) {
                    GL.TexCoord2(X - P.X, Y - P.Y); GL.Vertex3(VariousedPoint(X, Y, 0));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y); GL.Vertex3(VariousedPoint(X + 0.5f, Y, 0));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X + 0.5f, Y + 0.5f, 0));
                    GL.TexCoord2(X - P.X, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X, Y + 0.5f, 0));
                }
            }
        }

        private void RenderCeiling(Vector2 P) {
            for (var X = P.X; X <= P.X + 0.5; X += 0.5f) {
                for (var Y = P.Y; Y <= P.Y + 0.5; Y += 0.5f) {
                    GL.TexCoord2(X - P.X, Y - P.Y); GL.Vertex3(VariousedPoint(X, Y, WallsHeight));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y); GL.Vertex3(VariousedPoint(X + 0.5f, Y, WallsHeight));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X + 0.5f, Y + 0.5f, WallsHeight));
                    GL.TexCoord2(X - P.X, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X, Y + 0.5f, WallsHeight));
                }
            }
        }

        private void RenderIcon(Vector2 Position, Texture Texture, Color4 Color) {
            var Icon = new IconBufferRecord();
            Icon.Position = Position;
            Icon.Texture = Texture;
            Icon.Color = Color;
            IconsBuffer.Add(Icon);
        }

        private int CompareBufferedIcons(IconBufferRecord A, IconBufferRecord B) {
            var DistanceA = (A.Position - Game.Player.Position).Length;
            var DistanceB = (B.Position - Game.Player.Position).Length;
            return DistanceA < DistanceB ? -1 : +1; // using (int)(DistanceA - DistanceB) will lead to unexpected rounding problems
        }

        private void RenderBufferedIcons() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            var Size = (IconMaxSize - IconMinSize) / 2 * (Math.Sin(Game.TicksCounter / 10f) / 2 - 1) + IconMaxSize;

            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Fog);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            IconsBuffer.Sort(CompareBufferedIcons);
            IconsBuffer.Reverse();

            foreach (var Icon in IconsBuffer) {
                Icon.Texture.Bind();

                GL.PushMatrix();

                GL.Translate(Icon.Position.X + 0.5, Icon.Position.Y + 0.5, WallsHeight / 2);

                GL.Rotate(-Game.Player.Angle, Vector3.UnitZ);

                if (CameraMode.FirstPerson != Camera) {
                    GL.Rotate(-90, Vector3.UnitX);
                }

                GL.Color4(Icon.Color.R, Icon.Color.G, Icon.Color.B, 0.7f);

                GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0, 1); GL.Vertex3(-Size / 2, 0, -Size / 2);
                GL.TexCoord2(0, 0); GL.Vertex3(-Size / 2, 0, Size / 2);
                GL.TexCoord2(1, 0); GL.Vertex3(Size / 2, 0, Size / 2);
                GL.TexCoord2(1, 1); GL.Vertex3(Size / 2, 0, -Size / 2);
                GL.End();

                GL.PopMatrix();
            }

            GL.PopAttrib();
        }

        private void RenderExit(Vector2 Position) {
            RenderIcon(Position, TextureExit, (Game.CollectedCheckpoints.Count == Game.Map.Checkpoints.Count) ? Color4.ForestGreen : Color4.Red);
        }

        private void RenderCheckpoint(Vector2 Position, int Index) {
            RenderIcon(Position, TextureKey, CheckpointsColors[Index]);
        }

        private void RenderMark(Vector2 Position) {
            RenderIcon(Position, TextureMark, Color4.MediumOrchid);
        }

        private void RenderGhost(Ghost Ghost) {
            GL.PushMatrix();

            GL.Translate(Ghost.Position.X + 0.5, Ghost.Position.Y + 0.5, WallsHeight / 2);

            if (CameraMode.FirstPerson == Camera) {
                GL.Rotate(-Game.Player.Angle, Vector3.UnitZ);
            } else {
                GL.Rotate(-90, Vector3.UnitX);
            }

            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Fog);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (CameraMode.FirstPerson == Camera) {
                TextureGhostSide.Bind();
            } else {
                TextureGhostTop.Bind();
            }

            var AnimationFrame = Rand.Next(GhostFramesCount);

            GL.Color4(new Color4(1f, 1f, 1f, 1f));

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2((AnimationFrame - 1) / (float)GhostFramesCount, 1);
            GL.Vertex3(-GhostSize / 2, 0, -GhostSize / 2);

            GL.TexCoord2((AnimationFrame - 1) / (float)GhostFramesCount, 0);
            GL.Vertex3(-GhostSize / 2, 0, GhostSize / 2);

            GL.TexCoord2(AnimationFrame / (float)GhostFramesCount, 0);
            GL.Vertex3(GhostSize / 2, 0, GhostSize / 2);

            GL.TexCoord2(AnimationFrame / (float)GhostFramesCount, 1);
            GL.Vertex3(GhostSize / 2, 0, -GhostSize / 2);

            GL.End();

            GL.PopAttrib();

            GL.PopMatrix();
        }

        private void RenderPlayer() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Translate(Game.Player.Position.X, Game.Player.Position.Y, WallsHeight / 2);
            GL.Rotate(-Game.Player.Angle, Vector3.UnitZ);

            GL.Color4(1.0f, 0, 0, 0.7f);

            GL.Begin(BeginMode.Polygon);
            GL.Vertex2(0, PlayerModelSize / 2);
            GL.Vertex2(PlayerModelSize / 2, -PlayerModelSize / 2);
            GL.Vertex2(0, -PlayerModelSize / 4);
            GL.Vertex2(-PlayerModelSize / 2, -PlayerModelSize / 2);
            GL.End();

            GL.Rotate(Game.Player.Angle, Vector3.UnitZ);
            GL.Translate(-Game.Player.Position.X, -Game.Player.Position.Y, -WallsHeight / 2);

            GL.PopAttrib();
        }

        private void RenderWinScreen(GameWindow Window) {
            if (!FadeOutStarted.HasValue) {
                FadeOutStarted = Game.TicksCounter;
            }
            var FadeOutCounter = Game.TicksCounter - FadeOutStarted.Value;

            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            var Projection = Matrix4.CreateOrthographic(-(float)Window.Width, -(float)Window.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
            GL.Translate(Window.Width / 2, -Window.Height / 2, 0);

            var Modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Modelview);

            GL.Color4(new Color4(0, 0, 0, (float)Math.Min(FadeOutCounter, FadeOutLength) / FadeOutLength));

            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(Window.Width, 0);
            GL.Vertex2(Window.Width, Window.Height);
            GL.Vertex2(0, Window.Height);
            GL.End();

            if (FadeOutCounter >= FadeOutLength) {
                WinLabel.Left = (Window.Width - WinLabel.Width.Value) / 2;
                WinLabel.Top = (Window.Height - WinLabel.Height.Value) / 2;
                WinLabel.Render();
            }

            GL.PopAttrib();
        }
    }
}