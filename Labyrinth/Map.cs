using System;

namespace Labyrinth {
    class Map {
        public enum CellType {
            Empty,
            Wall
        };

        protected CellType[,] cells;

        public Map(int Width, int Height) {
            cells = new CellType[Width, Height];

            var rand = new Random();

            for (var x = 0; x < Width; x++) {
                for (var y = 0; y < Height; y++) {
                    cells[x, y] = (rand.Next(100) < 90) ? CellType.Empty : CellType.Wall;
                }
            }
        }

        public int Width {
            get { return cells.GetLength(0); }
        }
        
        public int Height {
            get { return cells.GetLength(1); }
        }

        public CellType GetCell(int x, int y) {
            if ((x < 0) || (x >= Width) || (y < 0) || (y >= Height)) {
                return CellType.Wall;
            } else {
                return cells[x, y];
            }
        }
    }
}