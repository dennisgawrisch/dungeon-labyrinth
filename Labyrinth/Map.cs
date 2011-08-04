using System;
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

            SetCell(startPosition, CellType.Empty);
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
            return new Vector2(rand.Next(0, Width - 1), rand.Next(0, Height - 1));
        }
    }
}