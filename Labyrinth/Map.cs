using System;
using System.Collections.Generic;
using OpenTK;

namespace Labyrinth {
    class Map {
        public enum CellType {
            Empty,
            Wall
        };

        protected Random Rand;
        protected CellType[,] Cells;
        public Position StartPosition { get; protected set; }
        public Position FinishPosition { get; protected set; }
        public List<Position> Checkpoints { get; protected set; }

        public Map(Random Randomizer, int Width, int Height, int CheckpointsCount) {
            Rand = Randomizer;

            Cells = new CellType[Width, Height];

			var MaxMovementTries = 15;
			var DirectionChangePossibility = 50;
            var LoopChancePerMille = 10;

            for (var X = 0; X < Width; X++) {
                for (var Y = 0; Y < Height; Y++) {
                        Cells[X, Y] = CellType.Wall;
                }
            }

            StartPosition = RandomPosition();

    		var Position = StartPosition;
    		var PositionsStack = new Stack<Position>(Width * Height);
    		PositionsStack.Push(Position);
    		var Direction = Vector2.UnitY;

    		do {
    			SetCell(Position, CellType.Empty);
    
    			var Success = false;
    			for (var Tries = 0; !Success && (Tries < MaxMovementTries); Tries++) {
    				var DirectionChange = Rand.Next(-200, +200);
    				if (DirectionChange < DirectionChangePossibility - 200) {
    					Direction = Direction.PerpendicularLeft;
    				} else if (DirectionChange > 200 - DirectionChangePossibility) {
    					Direction = Direction.PerpendicularRight;
    				}
    
    				var Next = (Position)(Vector2.Add(Position, Direction));
    				var LeftToNext = (Position)(Vector2.Add(Next, Direction.PerpendicularLeft));
    				var RightToNext = (Position)(Vector2.Add(Next, Direction.PerpendicularRight));
    				var NextNext = (Position)(Vector2.Add(Next, Direction));
    				var LeftToNextNext = (Position)(Vector2.Add(NextNext, Direction.PerpendicularLeft));
    				var RightToNextNext = (Position)(Vector2.Add(NextNext, Direction.PerpendicularRight));

                    var WithMapBounds = (
                        (0 <= Next.X) && (Next.X < Width)
                        && (0 <= Next.Y) && (Next.Y < Height)
                    );
                    var NextIsWall = (
                        (CellType.Wall == GetCell(Next))
                        && (CellType.Wall == GetCell(LeftToNext))
                        && (CellType.Wall == GetCell(RightToNext))
                    );
                    var NextNextIsWall = (
                        (CellType.Wall == GetCell(NextNext))
                        && (CellType.Wall == GetCell(LeftToNextNext))
                        && (CellType.Wall == GetCell(RightToNextNext))
                    );

    				if (
    					WithMapBounds
                        && NextIsWall
                        && ((Rand.Next(0, 1000) < LoopChancePerMille) || NextNextIsWall)
    				) {
    					Position = Next;
    					Success = true;
    				}
    			}
    
    			if (Success) {
    				PositionsStack.Push(Position);
    			} else {
    				Position = PositionsStack.Pop();
    			}
    		} while (PositionsStack.Count > 0);

            Checkpoints = new List<Position>(CheckpointsCount);
            for (var i = 0; i < CheckpointsCount; i++) {
                do {
                    Position = RandomPosition();
                } while ((GetCell(Position) != CellType.Empty) || (StartPosition == Position) || Checkpoints.Contains(Position));
                Checkpoints.Add(Position);
            }

            do {
                FinishPosition = RandomPosition();
            } while ((GetCell(FinishPosition) != CellType.Empty) || (StartPosition == FinishPosition) || Checkpoints.Contains(FinishPosition));
        }

        public int Width {
            get { return Cells.GetLength(0); }
        }

        public int Height {
            get { return Cells.GetLength(1); }
        }

        public CellType GetCell(int X, int Y) {
            if ((X < 0) || (X >= Width) || (Y < 0) || (Y >= Height)) {
                return CellType.Wall;
            } else {
                return Cells[X, Y];
            }
        }

        public CellType GetCell(Position Position) {
            return GetCell(Position.X, Position.Y);
        }

        protected void SetCell(int X, int Y, CellType CellType) {
            Cells[X, Y] = CellType;
        }

        protected void SetCell(Position Position, CellType CellType) {
            SetCell(Position.X, Position.Y, CellType);
        }

        public Position RandomPosition() {
            return new Position(Rand.Next(0, Width), Rand.Next(0, Height));
        }
    }
}