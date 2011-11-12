using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Game : GameWindowLayer {
		private Random Rand;

        public enum CameraMode {
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

        private int TextureWall;
        private Hashtable DisplayLists = new Hashtable();

        private float TorchLight = 0;
        private float TorchLightChangeDirection = +1;

        public Game() {
			Rand = new Random();

            TextureWall = LoadTexture("../../textures/wall.png");

            Map = new Map(40, 20); // TODO parametrize

            PlayerPosition = new Vector3(Map.StartPosition.X + 0.5f, Map.StartPosition.Y + 0.5f, 0.5f);

            for (PlayerAngle = 0; PlayerAngle < 360; PlayerAngle += 90) {
                var PlayerAngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-PlayerAngle));
                var PlayerMovementVector = Vector3.TransformVector(Vector3.UnitY, PlayerAngleMatrix);
                var NewPlayerPosition = Vector3.Add(PlayerPosition, PlayerMovementVector);
                if (Map.CellType.Empty == Map.GetCell(NewPlayerPosition.Xy)) {
                    break;
                }
            }
        }

        public override void Tick() {
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

            var NewPlayerPosition = PlayerPosition;
            NewPlayerPosition.X += PlayerMovementVector.X;
            if (Map.CellType.Empty == Map.GetCell(NewPlayerPosition.Xy)) {
                PlayerPosition = NewPlayerPosition;
            }

            NewPlayerPosition = PlayerPosition;
            NewPlayerPosition.Y += PlayerMovementVector.Y;
            if (Map.CellType.Empty == Map.GetCell(NewPlayerPosition.Xy)) {
                PlayerPosition = NewPlayerPosition;
            }

            if (((int)(Math.Floor(PlayerPosition.X)) == Map.FinishPosition.X) && ((int)(Math.Floor(PlayerPosition.Y)) == Map.FinishPosition.Y)) {
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

            var Projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Window.Width / (float)Window.Height, 1e-3f, Math.Max(Math.Max(Map.Width, Map.Height), WallsHeight * 10) * 2f);
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

            RenderMap();
            RenderPlayer();
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
            var DisplayListName = "map_" + Camera.GetHashCode();
            if (!DisplayLists.ContainsKey(DisplayListName)) {
                DisplayLists[DisplayListName] = GL.GenLists(1);
                GL.NewList((int)DisplayLists[DisplayListName], ListMode.Compile);
                for (var X = 0; X < Map.Width; X++) {
                    for (var Y = 0; Y < Map.Height; Y++) {
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

                            if (Map.FinishPosition.Equals(Position)) {
                                RenderExit(Position);
                            }
                        }
                    }
                }
                GL.EndList();
            }
            GL.CallList((int)DisplayLists[DisplayListName]);
        }

        private void RenderWall(Vector2 A, Vector2 B, float z) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, TextureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(A.X, A.Y, z + WallsHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(B.X, B.Y, z + WallsHeight);
            GL.TexCoord2(1, WallsHeight); GL.Vertex3(B.X, B.Y, z);
            GL.TexCoord2(0, WallsHeight); GL.Vertex3(A.X, A.Y, z);
            GL.End();

            GL.PopAttrib();
        }

        private void RenderWall(Vector2 A, Vector2 B) {
            RenderWall(A, B, 0);
        }

        private void RenderFloor(Vector2 Position) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, TextureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 1); GL.Vertex3(Position.X, Position.Y, 0);
            GL.TexCoord2(1, 1); GL.Vertex3(Position.X + 1, Position.Y, 0);
            GL.TexCoord2(1, 0); GL.Vertex3(Position.X + 1, Position.Y + 1, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(Position.X, Position.Y + 1, 0);
            GL.End();

            GL.PopAttrib();
        }

        private void RenderCeiling(Vector2 Position) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, TextureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(Position.X, Position.Y, WallsHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(Position.X + 1, Position.Y, WallsHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(Position.X + 1, Position.Y + 1, WallsHeight);
            GL.TexCoord2(0, 1); GL.Vertex3(Position.X, Position.Y + 1, WallsHeight);
            GL.End();

            GL.PopAttrib();
        }

        private void RenderExit(Vector2 Position) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            var PortalWidth = 0.7f;

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Color4(0, 1, 0, 0.3f);

            GL.Begin(BeginMode.QuadStrip);
            GL.Vertex3(Position.X + 0.5 - PortalWidth / 2, Position.Y + 0.5 - PortalWidth / 2, 0);
            GL.Vertex3(Position.X + 0.5 - PortalWidth / 2, Position.Y + 0.5 - PortalWidth / 2, WallsHeight);
            GL.Vertex3(Position.X + 0.5 + PortalWidth / 2, Position.Y + 0.5 - PortalWidth / 2, 0);
            GL.Vertex3(Position.X + 0.5 + PortalWidth / 2, Position.Y + 0.5 - PortalWidth / 2, WallsHeight);
            GL.Vertex3(Position.X + 0.5 + PortalWidth / 2, Position.Y + 0.5 + PortalWidth / 2, 0);
            GL.Vertex3(Position.X + 0.5 + PortalWidth / 2, Position.Y + 0.5 + PortalWidth / 2, WallsHeight);
            GL.End();

            GL.Begin(BeginMode.QuadStrip);
            GL.Vertex3(Position.X + 0.5 - PortalWidth / 2, Position.Y + 0.5 - PortalWidth / 2, 0);
            GL.Vertex3(Position.X + 0.5 - PortalWidth / 2, Position.Y + 0.5 - PortalWidth / 2, WallsHeight);
            GL.Vertex3(Position.X + 0.5 - PortalWidth / 2, Position.Y + 0.5 + PortalWidth / 2, 0);
            GL.Vertex3(Position.X + 0.5 - PortalWidth / 2, Position.Y + 0.5 + PortalWidth / 2, WallsHeight);
            GL.Vertex3(Position.X + 0.5 + PortalWidth / 2, Position.Y + 0.5 + PortalWidth / 2, 0);
            GL.Vertex3(Position.X + 0.5 + PortalWidth / 2, Position.Y + 0.5 + PortalWidth / 2, WallsHeight);
            GL.End();

            GL.PopAttrib();
       }

       private void RenderPlayer() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            var TriangleSize = 0.3f;

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Translate(PlayerPosition.X, PlayerPosition.Y, WallsHeight / 2);
            GL.Rotate(-PlayerAngle, Vector3.UnitZ);

            GL.Color4(1.0f, 0, 0, 0.7f);

            GL.Begin(BeginMode.Polygon);
            GL.Vertex2(0, 0);
            GL.Vertex2(TriangleSize / 2, -TriangleSize);
            GL.Vertex2(0, -TriangleSize * 0.75);
            GL.Vertex2(-TriangleSize / 2, -TriangleSize);
            GL.End();

            GL.Rotate(PlayerAngle, Vector3.UnitZ);
            GL.Translate(-PlayerPosition.X, -PlayerPosition.Y, -WallsHeight / 2);

            GL.PopAttrib();
       }
    }
}