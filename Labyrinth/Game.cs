using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Game : GameWindowLayer {
        private Random Rand;
        private int TicksCounter = 0;

        public enum DifficultyLevel {
            Easy,
            Normal,
            Hard,
        };

        private enum CameraMode {
            FirstPerson,
            ThirdPerson
        };
        private CameraMode Camera = CameraMode.FirstPerson;

        private Map Map;
        private Vector3 PlayerPosition;
        private float PlayerAngle; // in degrees, 0 = Y↑, 90 = X→

        private float PlayerMovementSpeed = 0.05f; // in cell units
        private float PlayerTurnSpeed = 5; // in degrees

        private float WallsHeight = 0.7f;
        private float WallsHeightVariation = 0.1f;
        private float WallsXyVariation = 0.1f;
        private float PlayerModelSize = 0.3f;

        private Hashtable Textures = new Hashtable();

        private float TorchLight = 0;
        private float TorchLightChangeDirection = +1;

        private float IconMinSize = 0.35f, IconMaxSize = 0.40f;
        private struct IconBufferRecord {
            public Vector2 Position;
            public int Texture;
            public Color4 Color;
        }
        private List<IconBufferRecord> IconsBuffer = new List<IconBufferRecord>(10);

        private float VisibilityDistance = 6f;

        private HashSet<int> CollectedCheckpoints = new HashSet<int>();
        private Color4[] CheckpointsColors = { Color4.Red, Color4.SpringGreen, Color4.DodgerBlue, Color4.Yellow };

        public Game(DifficultyLevel Difficulty) {
            Rand = new Random();

            Textures["Wall"] = LoadTexture("../../textures/wall.png");
            Textures["Exit"] = LoadTexture("../../textures/exit.png");
            Textures["Key"] = LoadTexture("../../textures/key.png");

            if (DifficultyLevel.Easy == Difficulty) {
                Map = new Map(Rand, 20, 20, 2);
            } else if (DifficultyLevel.Normal == Difficulty) {
                Map = new Map(Rand, 35, 35, 3);
            } else if (DifficultyLevel.Hard == Difficulty) {
                Map = new Map(Rand, 50, 50, 4);
            }

            PlayerPosition = new Vector3(Map.StartPosition.X + 0.5f, Map.StartPosition.Y + 0.5f, 0); // Z-coordinate is set in Tick

            // making player to face empty cell on start
            for (PlayerAngle = 0; PlayerAngle < 360; PlayerAngle += 90) {
                var PlayerAngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-PlayerAngle));
                var PlayerLookVector = Vector3.TransformVector(Vector3.UnitY, PlayerAngleMatrix);
                var PlayerLooksAt = Vector3.Add(PlayerPosition, PlayerLookVector);
                if (Map.CellType.Empty == Map.GetCell(PlayerLooksAt.Xy)) {
                    break;
                }
            }
        }

        public override void Tick() {
            ++TicksCounter;

            if (Window.Keyboard[Key.Left]) {
                PlayerAngle -= PlayerTurnSpeed;
            }
            if (Window.Keyboard[Key.Right]) {
                PlayerAngle += PlayerTurnSpeed;
            }

            var PlayerAngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-PlayerAngle));
            var PlayerMovementVector = new Vector3(0, 0, 0);

            if (Window.Keyboard[Key.Up] || Window.Keyboard[Key.W]) {
                PlayerMovementVector.Y += PlayerMovementSpeed;
            }
            if (Window.Keyboard[Key.Down] || Window.Keyboard[Key.S]) {
                PlayerMovementVector.Y -= PlayerMovementSpeed;
            }
            if (Window.Keyboard[Key.A]) {
                PlayerMovementVector.X -= PlayerMovementSpeed;
            }
            if (Window.Keyboard[Key.D]) {
                PlayerMovementVector.X += PlayerMovementSpeed;
            }

            PlayerMovementVector = Vector3.TransformVector(PlayerMovementVector, PlayerAngleMatrix);
            var AdditionalMovementVector = PlayerMovementVector;
            AdditionalMovementVector.NormalizeFast();
            AdditionalMovementVector = Vector3.Multiply(AdditionalMovementVector, (float)Math.Max(PlayerModelSize, WallsXyVariation * Math.Sqrt(2)));

            var NewPlayerPosition = PlayerPosition;
            NewPlayerPosition.X += PlayerMovementVector.X;
            var AdditionalPosition = NewPlayerPosition;
            AdditionalPosition.X += AdditionalMovementVector.X;
            if ((Map.CellType.Empty == Map.GetCell(NewPlayerPosition.Xy)) && (Map.CellType.Empty == Map.GetCell(AdditionalPosition.Xy))) {
                PlayerPosition = NewPlayerPosition;
            }

            NewPlayerPosition = PlayerPosition;
            NewPlayerPosition.Y += PlayerMovementVector.Y;
            AdditionalPosition = NewPlayerPosition;
            AdditionalPosition.Y += AdditionalMovementVector.Y;
            if ((Map.CellType.Empty == Map.GetCell(NewPlayerPosition.Xy)) && (Map.CellType.Empty == Map.GetCell(AdditionalPosition.Xy))) {
                PlayerPosition = NewPlayerPosition;
            }

            var FloorPosition = this.VariousedPoint(PlayerPosition.X, PlayerPosition.Y, 0);
            PlayerPosition.Z = FloorPosition.Z + 0.5f;

            var PlayerPositionCell = new Vector2((int)(Math.Floor(PlayerPosition.X)), (int)(Math.Floor(PlayerPosition.Y)));

            for (var i = 0; i < Map.Checkpoints.Count; i++) {
                if (!CollectedCheckpoints.Contains(i) && (PlayerPositionCell == Map.Checkpoints[i])) {
                    CollectedCheckpoints.Add(i);
                }
            }

            if ((PlayerPositionCell == Map.FinishPosition) && (CollectedCheckpoints.Count == Map.Checkpoints.Count)) {
                Window.Exit(); // TODO
            }
        }

        public override void OnKeyPress(Key K) {
            if (K.Equals(Key.C)) {
                if (CameraMode.FirstPerson == Camera) {
                    Camera = CameraMode.ThirdPerson;

                } else {
                    Camera = CameraMode.FirstPerson;
                }
            }
        }

        public override void Render() {
            GL.Enable(EnableCap.DepthTest);

            GL.Color4(Color4.Transparent);

            var Projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Window.Width / (float)Window.Height, 1e-3f, Math.Max(Map.Width, Map.Height)); 
            // do not clip on VisibilityDistance, because it is done by RenderMap, and other objects like icons should not be clipped
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);

            var Modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Modelview);

            if (CameraMode.ThirdPerson == Camera) {
                GL.Rotate(90, Vector3.UnitX); // look down
            }

            GL.Rotate(PlayerAngle, Vector3.UnitZ);
            GL.Translate(Vector3.Multiply(PlayerPosition, -1f));
            if (CameraMode.ThirdPerson == Camera) {
                GL.Translate(0, 0, -WallsHeight * 5);
            }

            var TorchPosition = new Vector4(PlayerPosition);
            TorchPosition.W = 1;

            var TorchLightMin = 0.10f;
            var TorchLightMax = 0.20f;
            var TorchLightChangeSpeed = 0.007f;

            TorchLight += Rand.Next(-100, +100) / 100f * TorchLightChangeSpeed * TorchLightChangeDirection;
            TorchLight = Math.Max(TorchLight, TorchLightMin);
            TorchLight = Math.Min(TorchLight, TorchLightMax);
            if ((TorchLightMin == TorchLight) || (TorchLightMax == TorchLight) || (Rand.Next(100) < 30)) {
                TorchLightChangeDirection = -TorchLightChangeDirection;
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

            for (var i = 0; i < Map.Checkpoints.Count; i++) { // TODO must fix problem when an icon overlaps another icon and hides it
                if (!CollectedCheckpoints.Contains(i)) {
                    RenderCheckpoint(Map.Checkpoints[i], i);
                }
            }

            RenderExit(Map.FinishPosition);

            if (CameraMode.ThirdPerson == Camera) {
                RenderPlayer();
            }

            RenderBufferedIcons();
        }

        private int LoadTexture(string Filename) {
            var Bitmap = new Bitmap(Filename);

            var Texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            var BitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, BitmapData.Width, BitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapData.Scan0);
            Bitmap.UnlockBits(BitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return Texture;
        }

        private void RenderMap() {
            var Xmin = (int)Math.Floor(Math.Max(PlayerPosition.X - VisibilityDistance, 0));
            var Ymin = (int)Math.Floor(Math.Max(PlayerPosition.Y - VisibilityDistance, 0));
            var Xmax = (int)Math.Ceiling(Math.Min(PlayerPosition.X + VisibilityDistance, Map.Width - 1));
            var Ymax = (int)Math.Ceiling(Math.Min(PlayerPosition.Y + VisibilityDistance, Map.Height - 1));

            for (var X = Xmin; X <= Xmax; X++) {
                for (var Y = Ymin; Y <= Ymax; Y++) {
                    var Position = new Vector2(X, Y);

                    if (Map.CellType.Empty == Map.GetCell(X, Y)) {
                        if (Map.CellType.Wall == Map.GetCell(X, Y + 1)) {
                            RenderWall(new Vector2(X, Y + 1), new Vector2(X + 1, Y + 1));
                        }
                        if (Map.CellType.Wall == Map.GetCell(X, Y - 1)) {
                            RenderWall(new Vector2(X + 1, Y), new Vector2(X, Y));
                        }
                        if (Map.CellType.Wall == Map.GetCell(X - 1, Y)) {
                            RenderWall(new Vector2(X, Y), new Vector2(X, Y + 1));
                        }
                        if (Map.CellType.Wall == Map.GetCell(X + 1, Y)) {
                            RenderWall(new Vector2(X + 1, Y + 1), new Vector2(X + 1, Y));
                        }

                        RenderFloor(Position);
                        if (CameraMode.FirstPerson == Camera) {
                            RenderCeiling(Position);
                        }
                    }
                }
            }
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
            GL.PushAttrib(AttribMask.TextureBit);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, (int)Textures["Wall"]);

            GL.Begin(BeginMode.Quads);

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

            GL.End();

            GL.PopAttrib();
        }

        private void RenderFloor(Vector2 P) {
            GL.PushAttrib(AttribMask.TextureBit);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, (int)Textures["Wall"]);

            GL.Begin(BeginMode.Quads);

            for (var X = P.X; X <= P.X + 0.5; X += 0.5f) {
                for (var Y = P.Y; Y <= P.Y + 0.5; Y += 0.5f) {
                    GL.TexCoord2(X - P.X, Y - P.Y); GL.Vertex3(VariousedPoint(X, Y, 0));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y); GL.Vertex3(VariousedPoint(X + 0.5f, Y, 0));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X + 0.5f, Y + 0.5f, 0));
                    GL.TexCoord2(X - P.X, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X, Y + 0.5f, 0));
                }
            }

            GL.End();

            GL.PopAttrib();
        }

        private void RenderCeiling(Vector2 P) {
            GL.PushAttrib(AttribMask.TextureBit);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, (int)Textures["Wall"]);

            GL.Begin(BeginMode.Quads);

            for (var X = P.X; X <= P.X + 0.5; X += 0.5f) {
                for (var Y = P.Y; Y <= P.Y + 0.5; Y += 0.5f) {
                    GL.TexCoord2(X - P.X, Y - P.Y); GL.Vertex3(VariousedPoint(X, Y, WallsHeight));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y); GL.Vertex3(VariousedPoint(X + 0.5f, Y, WallsHeight));
                    GL.TexCoord2(X - P.X + 0.5, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X + 0.5f, Y + 0.5f, WallsHeight));
                    GL.TexCoord2(X - P.X, Y - P.Y + 0.5); GL.Vertex3(VariousedPoint(X, Y + 0.5f, WallsHeight));
                }
            }

            GL.End();

            GL.PopAttrib();
        }

        private void RenderIcon(Vector2 Position, int Texture, Color4 Color) {
            var Icon = new IconBufferRecord();
            Icon.Position = Position;
            Icon.Texture = Texture;
            Icon.Color = Color;
            IconsBuffer.Add(Icon);
        }

        private int CompareBufferedIcons(IconBufferRecord A, IconBufferRecord B) {
            var DistanceA = (A.Position - PlayerPosition.Xy).Length;
            var DistanceB = (B.Position - PlayerPosition.Xy).Length;
            return DistanceA < DistanceB ? -1 : +1; // using (int)(DistanceA - DistanceB) will lead to unexpected rounding problems
        }

        private void RenderBufferedIcons() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            var Size = (IconMaxSize - IconMinSize) / 2 * (Math.Sin(TicksCounter / 5f) / 2 - 1) + IconMaxSize;

            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Fog);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            IconsBuffer.Sort(CompareBufferedIcons);
            IconsBuffer.Reverse();

            foreach (var Icon in IconsBuffer) {
                GL.BindTexture(TextureTarget.Texture2D, Icon.Texture);

                GL.PushMatrix();

                GL.Translate(Icon.Position.X + 0.5, Icon.Position.Y + 0.5, WallsHeight / 2);

                GL.Rotate(-PlayerAngle, Vector3.UnitZ);

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
            RenderIcon(Position, (int)Textures["Exit"], (CollectedCheckpoints.Count == Map.Checkpoints.Count) ? Color4.Green : Color4.Red);
        }

        private void RenderCheckpoint(Vector2 Position, int Index) {
            RenderIcon(Position, (int)Textures["Key"], CheckpointsColors[Index]);
        }

        private void RenderPlayer() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Translate(PlayerPosition.X, PlayerPosition.Y, WallsHeight / 2);
            GL.Rotate(-PlayerAngle, Vector3.UnitZ);

            GL.Color4(1.0f, 0, 0, 0.7f);

            GL.Begin(BeginMode.Polygon);
            GL.Vertex2(0, PlayerModelSize / 2);
            GL.Vertex2(PlayerModelSize / 2, -PlayerModelSize / 2);
            GL.Vertex2(0, -PlayerModelSize / 4);
            GL.Vertex2(-PlayerModelSize / 2, -PlayerModelSize / 2);
            GL.End();

            GL.Rotate(PlayerAngle, Vector3.UnitZ);
            GL.Translate(-PlayerPosition.X, -PlayerPosition.Y, -WallsHeight / 2);

            GL.PopAttrib();
        }
    }
}