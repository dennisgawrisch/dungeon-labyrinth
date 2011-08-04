using System;
using System.Collections.Generic;
using OpenTK;

namespace Labyrinth {
    class Map {
        public enum CellType {
            Empty,
            Wall
        };

        protected Random rand;
        protected CellType[,] cells;
        protected Vector2 startPosition;
        protected Vector2 finishPosition;

        public Map(int Width, int Height) {
            rand = new Random();

            cells = new CellType[Width, Height];

            for (var x = 0; x < Width; x++) {
                for (var y = 0; y < Height; y++) {
                      cells[x, y] = CellType.Wall;
                }
            }

            startPosition = RandomPosition();
            
			var maxMovementTries = 3;
			var directionChangePossibility = 10;
			
			var position = startPosition;
			var positionsStack = new Stack<Vector2>(Width * Height);
			var direction = Vector2.UnitY;
			do {
				SetCell(position, CellType.Empty);
				
				var success = false;
				for (var tries = 0; !success && tries < maxMovementTries; tries++) {
					var directionChange = rand.Next(-100, +100);
					if (directionChange < -maxMovementTries) {
						direction = direction.PerpendicularLeft;
					} else if (directionChange > +maxMovementTries) {
						direction = direction.PerpendicularRight;
					}
	
					var nextPosition = Vector2.Add(position, direction);
					var leftToNextPosition = Vector2.Add(nextPosition, direction.PerpendicularLeft);
					var rightToNextPosition = Vector2.Add(nextPosition, direction.PerpendicularRight);
					
					if (
					    (0 <= nextPosition.X) && (nextPosition.X < Width)
					    && (0 <= nextPosition.Y) && (nextPosition.Y < Height)
					    && (CellType.Wall == GetCell(nextPosition))
					    && (CellType.Wall == GetCell(leftToNextPosition))
					    && (CellType.Wall == GetCell(rightToNextPosition))
					) {
						position = nextPosition;
						success = true;
					}
				}
				
				if (success) {
					positionsStack.Push(position);
				} else {
					position = positionsStack.Pop();
				}
			} while (positionsStack.Count > 0);
        }

        public int Width {
            get { return cells.GetLength(0); }
        }
        
        public int Height {
            get { return cells.GetLength(1); }
        }

        public Vector2 StartPosition {
            get { return startPosition; }
        }

        public Vector2 FinishPosition {
            get { return finishPosition; }
        }

        public CellType GetCell(int x, int y) {
            if ((x < 0) || (x >= Width) || (y < 0) || (y >= Height)) {
                return CellType.Wall;
            } else {
                return cells[x, y];
            }
        }

        public CellType GetCell(Vector2 position) {
            return GetCell((int)(Math.Floor(position.X)), (int)(Math.Floor(position.Y)));
        }

        protected void SetCell(int x, int y, CellType cellType) {
            cells[x, y] = cellType;
        }

        protected void SetCell(Vector2 position, CellType cellType) {
            SetCell((int)(Math.Floor(position.X)), (int)(Math.Floor(position.Y)), cellType);
        }

        protected Vector2 RandomPosition() {
            return new Vector2(rand.Next(0, Width), rand.Next(0, Height));
        }
    }
}