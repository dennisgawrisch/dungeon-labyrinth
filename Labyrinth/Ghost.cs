using System;
using OpenTK;

namespace Labyrinth {
    class Ghost {
        private static Random Rand = new Random();
        private Map Map;
        public Vector2 Position;
        public float Angle; // in degrees, 0 = Y↑, 90 = X→
        public const float MovementSpeed = 0.15f; // cell units per second

        public Ghost(Map Map) {
            this.Map = Map;
            Randomize();
        }

        private void Randomize() {
            Position = Map.RandomPosition();
            Angle = 90 * Rand.Next(4);
        }

        public void Move(float Steps) {
            Position.X += Steps * MovementSpeed * (float)Math.Cos(MathHelper.DegreesToRadians(90 - Angle));
            Position.Y += Steps * MovementSpeed * (float)Math.Sin(MathHelper.DegreesToRadians(90 - Angle));

            if (
                (Position.X < 0)
                || (Position.Y < 0)
                || (Position.X >= Map.Width)
                || (Position.Y >= Map.Height)
                || (
                    (Rand.Next(1000) < 3)
                    && (Labyrinth.Map.CellType.Empty == Map.GetCell((Position)Position))
                )
            ) {
                Randomize();
            }
        }
    }
}