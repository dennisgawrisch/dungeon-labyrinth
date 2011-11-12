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
        public Vector2 StartPosition { get; protected set; }
        public Vector2 FinishPosition { get; protected set; }

        public Map(int Width, int Height) {
            Rand = new Random();

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
    		var PositionsStack = new Stack<Vector2>(Width * Height);
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
    
    				var Next = Vector2.Add(Position, Direction);
    				var LeftToNext = Vector2.Add(Next, Direction.PerpendicularLeft);
    				var RightToNext = Vector2.Add(Next, Direction.PerpendicularRight);
    				var NextNext = Vector2.Add(Next, Direction);
    				var LeftToNextNext = Vector2.Add(NextNext, Direction.PerpendicularLeft);
    				var RightToNextNext = Vector2.Add(NextNext, Direction.PerpendicularRight);

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

            do {
                FinishPosition = new Vector2(Rand.Next(0, Width), Rand.Next(0, Height));
            } while (GetCell(FinishPosition) != CellType.Empty);
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

        public CellType GetCell(Vector2 Position) {
            return GetCell((int)(Math.Floor(Position.X)), (int)(Math.Floor(Position.Y)));
        }

        protected void SetCell(int X, int Y, CellType cellType) {
            Cells[X, Y] = cellType;
        }

        protected void SetCell(Vector2 Position, CellType cellType) {
            SetCell((int)(Math.Floor(Position.X)), (int)(Math.Floor(Position.Y)), cellType);
        }

        protected Vector2 RandomPosition() {
            return new Vector2(Rand.Next(0, Width), Rand.Next(0, Height));
        }
    }
}