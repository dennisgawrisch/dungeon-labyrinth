using System;
using OpenTK;

namespace Labyrinth {
    class Player {
        private Map Map;
        public Vector2 Position;
        public float Angle; // in degrees, 0 = Y↑, 90 = X→
        public const float MovementSpeed = 1.5f; // cell units per second
        public const float TurnSpeed = 150; // degrees per second
        public Vector3 Size { get; protected set; }

        public Player(Map Map) {
            Size = new Vector3(0.5f, 0.3f, 0.7f);

            this.Map = Map;

            Position = Map.StartPosition;

            Position.X += 0.5f;
            Position.Y += 0.5f;

            // face empty cell on start
            for (Angle = 0; Angle < 360; Angle += 90) {
                var LooksAt = new Position(
                    (int)(Position.X + Math.Cos(MathHelper.DegreesToRadians(90 - Angle))),
                    (int)(Position.Y + Math.Sin(MathHelper.DegreesToRadians(90 - Angle)))
                );
                if (Labyrinth.Map.CellType.Empty == Map.GetCell(LooksAt)) {
                    break;
                }
            }
        }

        public void Move(Vector2 Vector) {
            var AngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-Angle));
            var TransformedVector = Vector3.TransformVector(new Vector3(Vector), AngleMatrix);

            Vector2[] Deltas = { new Vector2(TransformedVector.X, 0), new Vector2(0, TransformedVector.Y) };

            var DistanceFromWalls = Math.Max(Size.X / 2, Size.Y / 2) + 0.1337f;

            foreach (var Delta in Deltas) {
                var NewPosition = Position + Delta;

                var CanMove = true;
                for (var DX = -DistanceFromWalls; (DX <= DistanceFromWalls) && CanMove; DX += DistanceFromWalls) {
                    for (var DY = -DistanceFromWalls; (DY <= DistanceFromWalls) && CanMove; DY += DistanceFromWalls) {
                        var Point = NewPosition + new Vector2(DX, DY);
                        if (Map.CellType.Wall == Map.GetCell((Position)Point)) {
                            CanMove = false;
                        }
                    }
                }

                if (CanMove) {
                    Position = NewPosition;
                }
            }
        }
    }
}