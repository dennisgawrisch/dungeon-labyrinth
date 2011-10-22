using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth {
    class Game : GameWindowLayer {
		private Random rand;

        public enum CameraMode {
            FirstPerson,
            ThirdPerson,
            BirdEye
        };
        private CameraMode cameraMode = CameraMode.BirdEye;

        private Map map;
        private Vector3 playerPosition;
        private float playerAngle; // in degrees, 0 = Y↑, 90 = X→

        private float playerMovementSpeed = 0.05f; // in cell units
        private float playerTurnSpeed = 5; // in degrees

        private float wallsHeight = 0.7f;

        private int textureWall;

        private float torchLight = 0;
        private float torchLightChangeDirection = +1;

        public Game() {
			rand = new Random();

            textureWall = LoadTexture("../../textures/wall.png");

            map = new Map(30, 30); // TODO parametrize

            playerPosition = new Vector3(map.StartPosition.X + 0.5f, map.StartPosition.Y + 0.5f, 0.5f);

            for (playerAngle = 0; playerAngle < 360; playerAngle += 90) {
                var playerAngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-playerAngle));
                var playerMovementVector = Vector3.TransformVector(Vector3.UnitY, playerAngleMatrix);
                var newPlayerPosition = Vector3.Add(playerPosition, playerMovementVector);
                if (Map.CellType.Empty == map.GetCell(newPlayerPosition.Xy)) {
                    break;
                }
            }
        }

        public override void OnUpdateFrame(GameWindow window) {
            if (window.Keyboard[Key.C]) {
                if (CameraMode.FirstPerson == cameraMode) {
                    cameraMode = CameraMode.ThirdPerson;
                } else if (CameraMode.ThirdPerson == cameraMode) {
                    cameraMode = CameraMode.BirdEye;
                } else {
                    cameraMode = CameraMode.FirstPerson;
                }
            }

            if (window.Keyboard[Key.Left]) {
                playerAngle -= playerTurnSpeed;
            }
            if (window.Keyboard[Key.Right]) {
                playerAngle += playerTurnSpeed;
            }

            var playerAngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-playerAngle));
            var playerMovementVector = new Vector3(0, 0, 0);

            if (window.Keyboard[Key.Up] || window.Keyboard[Key.W]) {
                playerMovementVector.Y += playerMovementSpeed;
            }
            if (window.Keyboard[Key.Down] || window.Keyboard[Key.S]) {
                playerMovementVector.Y -= playerMovementSpeed;
            }
            if (window.Keyboard[Key.A]) {
                playerMovementVector.X -= playerMovementSpeed;
            }
            if (window.Keyboard[Key.D]) {
                playerMovementVector.X += playerMovementSpeed;
            }

            playerMovementVector = Vector3.TransformVector(playerMovementVector, playerAngleMatrix);

            var newPlayerPosition = playerPosition;
            newPlayerPosition.X += playerMovementVector.X;
            if (Map.CellType.Empty == map.GetCell(newPlayerPosition.Xy)) {
                playerPosition = newPlayerPosition;
            }

            newPlayerPosition = playerPosition;
            newPlayerPosition.Y += playerMovementVector.Y;
            if (Map.CellType.Empty == map.GetCell(newPlayerPosition.Xy)) {
                playerPosition = newPlayerPosition;
            }

            if (((int)(Math.Floor(playerPosition.X)) == map.FinishPosition.X) && ((int)(Math.Floor(playerPosition.Y)) == map.FinishPosition.Y)) {
                window.Exit(); // TODO
            }
        }

        public override void OnRenderFrame(GameWindow window) {
            GL.Enable(EnableCap.DepthTest);

            var projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, window.Width / (float)window.Height, 1e-3f, Math.Max(Math.Max(map.Width, map.Height), wallsHeight * 10) * 2f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            var modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            if ((CameraMode.ThirdPerson == cameraMode) || (CameraMode.BirdEye == cameraMode)) {
                GL.Rotate(90, Vector3.UnitX); // look down
            }

            if (CameraMode.BirdEye != cameraMode) {
                GL.Rotate(playerAngle, Vector3.UnitZ);
                GL.Translate(Vector3.Multiply(playerPosition, -1f));
                if (CameraMode.ThirdPerson == cameraMode) {
                    GL.Translate(0, 0, -wallsHeight * 5);
                }
            } else {
                GL.Translate(-map.Width / 2, -map.Height / 2, -wallsHeight * Math.Max(map.Width, map.Height) * 2f);
            }

            if (CameraMode.BirdEye != cameraMode) {
                var torchPosition = new Vector4(playerPosition);
                torchPosition.W = 1;
    
                var torchLightMin = 0.10f;
                var torchLightMax = 0.20f;
                var torchLightChangeSpeed = 0.007f;
    
                torchLight += rand.Next(-100, +100) / 100f * torchLightChangeSpeed * torchLightChangeDirection;
                torchLight = Math.Max(torchLight, torchLightMin);
                torchLight = Math.Min(torchLight, torchLightMax);
                if ((torchLightMin == torchLight) || (torchLightMax == torchLight) || (rand.Next(100) < 30)) {
                    torchLightChangeDirection = -torchLightChangeDirection;
                }
    
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
                GL.Light(LightName.Light0, LightParameter.Position, torchPosition);
                GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, torchLight);
                GL.Light(LightName.Light0, LightParameter.Ambient, Color4.SaddleBrown);
                GL.Light(LightName.Light0, LightParameter.Diffuse, Color4.SaddleBrown);
                GL.Light(LightName.Light0, LightParameter.Specular, Color4.SaddleBrown);

                GL.Enable(EnableCap.Fog);
                GL.Fog(FogParameter.FogDensity, (CameraMode.ThirdPerson != cameraMode) ? 0.5f : 0.1f);
            } else {
                GL.Disable(EnableCap.Lighting);
                GL.Disable(EnableCap.Fog);
            }

            for (var x = 0; x < map.Width; x++) {
                for (var y = 0; y < map.Height; y++) {
                    var position = new Vector2(x, y);

                    if (Map.CellType.Empty == map.GetCell(x, y)) {
                        if (Map.CellType.Wall == map.GetCell(x, y + 1)) {
                            RenderWall(new Vector2(x, y + 1), new Vector2(x + 1, y + 1));
                        }
                        if (Map.CellType.Wall == map.GetCell(x, y - 1)) {
                            RenderWall(new Vector2(x + 1, y), new Vector2(x, y));
                        }
                        if (Map.CellType.Wall == map.GetCell(x - 1, y)) {
                            RenderWall(new Vector2(x, y), new Vector2(x, y + 1));
                        }
                        if (Map.CellType.Wall == map.GetCell(x + 1, y)) {
                            RenderWall(new Vector2(x + 1, y + 1), new Vector2(x + 1, y));
                        }

                        RenderFloor(position);
                        if ((CameraMode.ThirdPerson != cameraMode) && (CameraMode.BirdEye != cameraMode)) {
                            RenderCeiling(position);
                        }

                        if (map.FinishPosition.Equals(position)) {
                            RenderExit(position);
                        }
                    }
                }
            }

            if ((CameraMode.ThirdPerson == cameraMode) || (CameraMode.BirdEye == cameraMode)) {
                RenderPlayer();
            }
        }

        private int LoadTexture(string filename) {
            var bitmap = new Bitmap(filename);

            var texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return texture;
        }

        private void RenderWall(Vector2 A, Vector2 B, float z) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(A.X, A.Y, z + wallsHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(B.X, B.Y, z + wallsHeight);
            GL.TexCoord2(1, wallsHeight); GL.Vertex3(B.X, B.Y, z);
            GL.TexCoord2(0, wallsHeight); GL.Vertex3(A.X, A.Y, z);
            GL.End();

            GL.PopAttrib();
        }

        private void RenderWall(Vector2 A, Vector2 B) {
            RenderWall(A, B, 0);
        }

        private void RenderFloor(Vector2 position) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 1); GL.Vertex3(position.X, position.Y, 0);
            GL.TexCoord2(1, 1); GL.Vertex3(position.X + 1, position.Y, 0);
            GL.TexCoord2(1, 0); GL.Vertex3(position.X + 1, position.Y + 1, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(position.X, position.Y + 1, 0);
            GL.End();

            GL.PopAttrib();
        }

        private void RenderCeiling(Vector2 position) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureWall);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(position.X, position.Y, wallsHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(position.X + 1, position.Y, wallsHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(position.X + 1, position.Y + 1, wallsHeight);
            GL.TexCoord2(0, 1); GL.Vertex3(position.X, position.Y + 1, wallsHeight);
            GL.End();

            GL.PopAttrib();
        }

        private void RenderExit(Vector2 position) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            var portalWidth = 0.7f;

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Color4(0, 1, 0, 0.3f);

            GL.Begin(BeginMode.QuadStrip);
            GL.Vertex3(position.X + 0.5 - portalWidth / 2, position.Y + 0.5 - portalWidth / 2, 0);
            GL.Vertex3(position.X + 0.5 - portalWidth / 2, position.Y + 0.5 - portalWidth / 2, wallsHeight);
            GL.Vertex3(position.X + 0.5 + portalWidth / 2, position.Y + 0.5 - portalWidth / 2, 0);
            GL.Vertex3(position.X + 0.5 + portalWidth / 2, position.Y + 0.5 - portalWidth / 2, wallsHeight);
            GL.Vertex3(position.X + 0.5 + portalWidth / 2, position.Y + 0.5 + portalWidth / 2, 0);
            GL.Vertex3(position.X + 0.5 + portalWidth / 2, position.Y + 0.5 + portalWidth / 2, wallsHeight);
            GL.End();

            GL.Begin(BeginMode.QuadStrip);
            GL.Vertex3(position.X + 0.5 - portalWidth / 2, position.Y + 0.5 - portalWidth / 2, 0);
            GL.Vertex3(position.X + 0.5 - portalWidth / 2, position.Y + 0.5 - portalWidth / 2, wallsHeight);
            GL.Vertex3(position.X + 0.5 - portalWidth / 2, position.Y + 0.5 + portalWidth / 2, 0);
            GL.Vertex3(position.X + 0.5 - portalWidth / 2, position.Y + 0.5 + portalWidth / 2, wallsHeight);
            GL.Vertex3(position.X + 0.5 + portalWidth / 2, position.Y + 0.5 + portalWidth / 2, 0);
            GL.Vertex3(position.X + 0.5 + portalWidth / 2, position.Y + 0.5 + portalWidth / 2, wallsHeight);
            GL.End();

            GL.PopAttrib();
       }

       private void RenderPlayer() {
            GL.PushAttrib(AttribMask.AllAttribBits);

            var triangleSize = 0.3f;

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Translate(playerPosition.X, playerPosition.Y, wallsHeight * 2);
            GL.Rotate(-playerAngle, Vector3.UnitZ);

            GL.Color4(1.0f, 0, 0, 0.7f);

            GL.Begin(BeginMode.Triangles);
            GL.Vertex2(0, 0);
            GL.Vertex2(triangleSize / 2, -triangleSize);
            GL.Vertex2(-triangleSize / 2, -triangleSize);
            GL.End();

            GL.Rotate(playerAngle, Vector3.UnitZ);
            GL.Translate(-playerPosition.X, -playerPosition.Y, -wallsHeight / 2);

            GL.PopAttrib();
       }
    }
}