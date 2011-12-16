using System;
using OpenTK;

namespace Labyrinth {
    public class Position {
        public int X;
        public int Y;

        public Position(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }

        public static implicit operator Vector2(Position Position) {
            return new Vector2(Position.X, Position.Y);
        }

        public static explicit operator Position(Vector2 RealPosition) {
            return new Position((int)(Math.Floor(RealPosition.X)), (int)(Math.Floor(RealPosition.Y)));
        }

        public bool Equals(Position Another) {
            return (null == (object)Another) ? false : ((X == Another.X) && (Y == Another.Y));
        }

        public override bool Equals(object Another) {
            return (null == Another) ? false : Equals(Another as Position);
        }

        public static bool operator ==(Position A, Position B) {
            return (null == (object)A) ? (null == (object)B) : A.Equals(B);
        }

        public static bool operator !=(Position A, Position B) {
            return (null == (object)A) ? (null != (object)B) : !A.Equals(B);
        }

        public override int GetHashCode() {
            return X ^ Y;
        }
    }
}