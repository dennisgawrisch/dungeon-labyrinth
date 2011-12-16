using System;
using OpenTK;

namespace Labyrinth {
    class Player {
        private Map Map;
        public Vector2 Position;
        public float Angle; // in degrees, 0 = Y↑, 90 = X→
        public const float MovementSpeed = 1.5f; // cell units per second
        public const float TurnSpeed = 150; // degrees per second

        public Player(Map Map) {
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
                if (Map.CellType.Empty == Map.GetCell(LooksAt)) {
                    break;
                }
            }
        }

        public void Move(Vector2 Vector) {
            var PlayerAngleMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-Angle));
            var TransformedVector = Vector3.TransformVector(new Vector3(Vector), PlayerAngleMatrix);

            // TODO don’t allow to look through walls

            var NewPosition = Position;
            NewPosition.X += TransformedVector.X;
            if (Map.CellType.Empty == Map.GetCell((Position)NewPosition)) {
                Position = NewPosition;
            }

            NewPosition = Position;
            NewPosition.Y += TransformedVector.Y;
            if (Map.CellType.Empty == Map.GetCell((Position)NewPosition)) {
                Position = NewPosition;
            }
        }
    }
}