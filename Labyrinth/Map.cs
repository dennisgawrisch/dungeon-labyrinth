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
            
			var maxMovementTries = 15;
			var directionChangePossibility = 50;
			
			var position = startPosition;
			var positionsStack = new Stack<Vector2>(Width * Height);
			positionsStack.Push(position);
			var direction = Vector2.UnitY;
			
			var possibleFinishPositions = new List<Vector2>(Width * Height);
			var minPathToFinish = Math.Sqrt(Width * Height);
				
			do {
				SetCell(position, CellType.Empty);
				
				var success = false;
				for (var tries = 0; !success && (tries < maxMovementTries); tries++) {
					var directionChange = rand.Next(-200, +200);
					if (directionChange < directionChangePossibility - 200) {
						direction = direction.PerpendicularLeft;
					} else if (directionChange > 200 - directionChangePossibility) {
						direction = direction.PerpendicularRight;
					}
	
					var next = Vector2.Add(position, direction);
					var leftToNext = Vector2.Add(next, direction.PerpendicularLeft);
					var rightToNext = Vector2.Add(next, direction.PerpendicularRight);
					var nextNext = Vector2.Add(next, direction);
					var leftToNextNext = Vector2.Add(nextNext, direction.PerpendicularLeft);
					var rightToNextNext = Vector2.Add(nextNext, direction.PerpendicularRight);
					
					if (
					    (0 <= next.X) && (next.X < Width)
					    && (0 <= next.Y) && (next.Y < Height)
					    && (CellType.Wall == GetCell(next))
					    && (CellType.Wall == GetCell(leftToNext))
					    && (CellType.Wall == GetCell(rightToNext))
					    && (CellType.Wall == GetCell(nextNext))
					    && (CellType.Wall == GetCell(leftToNextNext))
					    && (CellType.Wall == GetCell(rightToNextNext))
					) {
						position = next;
						success = true;
					}
				}
				
				if (success) {
					positionsStack.Push(position);
				} else {
					if (positionsStack.Count > minPathToFinish) {
						possibleFinishPositions.Add(position);
					}

					position = positionsStack.Pop();
				}
			} while (positionsStack.Count > 0);
			
			if (0 == possibleFinishPositions.Count) {
	            for (var x = 0; x < Width; x++) {
	                for (var y = 0; y < Height; y++) {
						finishPosition = new Vector2(x, y);
	                	if ((CellType.Empty == GetCell(finishPosition)) && !startPosition.Equals(finishPosition)) {
							possibleFinishPositions.Add(finishPosition);
						}
	                }
	            }
			}
			finishPosition = possibleFinishPositions[rand.Next(0, possibleFinishPositions.Count)];
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