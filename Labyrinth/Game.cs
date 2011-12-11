using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Labyrinth.Gui;

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

        private Texture TextureWall, TextureExit, TextureKey, TextureMark;

        private float TorchLight = 0;
        private float TorchLightChangeDirection = +1;

        private float IconMinSize = 0.35f, IconMaxSize = 0.40f;
        private struct IconBufferRecord {
            public Vector2 Position;
            public Texture Texture;
            public Color4 Color;
        }
        private List<IconBufferRecord> IconsBuffer = new List<IconBufferRecord>(10);

        private float VisibilityDistance = 6f;

        private HashSet<int> CollectedCheckpoints = new HashSet<int>();
        private Color4[] CheckpointsColors = { Color4.OrangeRed, Color4.Aquamarine, Color4.DodgerBlue, Color4.Yellow };

        private int MarksLeft = 10;
        private List<Vector2> Marks = new List<Vector2>();

        private enum GameStateEnum {
            Playing,
            Win
        }
        private GameStateEnum GameState;
        private int GameStateLastChangeTicksCounter = 0;
        private Text WinLabel;

        public Game(DifficultyLevel Difficulty) {
            Rand = new Random();

            TextureWall = new Texture(new Bitmap("textures/wall.png"));
            TextureExit = new Texture(new Bitmap("textures/exit.png"));
            TextureKey = new Texture(new Bitmap("textures/key.png"));
            TextureMark = new Texture(new Bitmap("textures/mark.png"));

            WinLabel = new Text("You win!");
            WinLabel.Color = Color4.White;
            WinLabel.Font = new Font(new FontFamily(GenericFontFamilies.SansSerif), 50, GraphicsUnit.Pixel);

            if (DifficultyLevel.Easy == Difficulty) {
                Map = new Map(Rand, 10, 10, 2);
            } else if (DifficultyLevel.Normal == Difficulty) {
                Map = new Map(Rand, 20, 20, 3);
            } else if (DifficultyLevel.Hard == Difficulty) {
                Map = new Map(Rand, 30, 30, 4);
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

            GameState = GameStateEnum.Playing;
        }

        public override void Tick() {
            ++TicksCounter;

            if (GameStateEnum.Playing == GameState) {
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
                    GameState = GameStateEnum.Win;
                    GameStateLastChangeTicksCounter = TicksCounter;
                }
            }
        }

        public override void OnKeyPress(Key K) {
            if (GameStateEnum.Playing == GameState) {
                if (K.Equals(Key.C)) {
                    if (CameraMode.FirstPerson == Camera) {
                        Camera = CameraMode.ThirdPerson;
    
                    } else {
                        Camera = CameraMode.FirstPerson;
                    }
                } else if (K.Equals(Key.F)) {
                    if (MarksLeft > 0) {
                        Marks.Add(new Vector2((float)Math.Floor(PlayerPosition.X), (float)Math.Floor(PlayerPosition.Y)));
                        --MarksLeft;
                    }
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

            if (GameStateEnum.Playing == GameState) {
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

            for (var i = 0; i < Map.Checkpoints.Count; i++) {
                if (!CollectedCheckpoints.Contains(i)) {
                    RenderCheckpoint(Map.Checkpoints[i], i);
                }
            }

            RenderExit(Map.FinishPosition);

            foreach (var Mark in Marks) {
                RenderMark(Mark);
            }

            if (CameraMode.ThirdPerson == Camera) {
                RenderPlayer();
            }

            RenderBufferedIcons();

            if (GameStateEnum.Win == GameState) {
                RenderWinScreen();
            }
        }

        private void RenderMap() {
            GL.PushAttrib(AttribMask.TextureBit);

            GL.Enable(EnableCap.Texture2D);
            TextureWall.Bind();

            GL.Begin(BeginMode.Quads);

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
                Icon.Texture.Bind();

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
            RenderIcon(Position, TextureExit, (CollectedCheckpoints.Count == Map.Checkpoints.Count) ? Color4.ForestGreen : Color4.Red);
        }

        private void RenderCheckpoint(Vector2 Position, int Index) {
            RenderIcon(Position, TextureKey, CheckpointsColors[Index]);
        }

        private void RenderMark(Vector2 Position) {
            RenderIcon(Position, TextureMark, Color4.MediumOrchid);
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

        private void RenderWinScreen() {
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

            var Ticks = TicksCounter - GameStateLastChangeTicksCounter;
            var MaxTicks = 42;
            GL.Color4(new Color4(0, 0, 0, (float)Math.Min(Ticks, MaxTicks) / MaxTicks));

            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(Window.Width, 0);
            GL.Vertex2(Window.Width, Window.Height);
            GL.Vertex2(0, Window.Height);
            GL.End();

            if (Ticks >= MaxTicks) {
                WinLabel.Left = (Window.Width - WinLabel.Width.Value) / 2;
                WinLabel.Top = (Window.Height - WinLabel.Height.Value) / 2;
                WinLabel.Render();
            }

            GL.PopAttrib();
        }
    }
}