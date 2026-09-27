using System;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Griglia rettangolare contenente le celle del dungeon.</summary>
    public sealed class DungeonGrid
    {
        private readonly CellType[,] cells;

        public int Width { get; }
        public int Height { get; }

        public RectInt Bounds => new RectInt(0, 0, Width, Height);

        public DungeonGrid(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    width,
                    "Grid width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    height,
                    "Grid height must be positive.");
            }
            
            Width = width;
            Height = height;
            cells = new CellType[width, height];
        }

        public CellType this[int x, int y]
        {
            get
            {
                ValidateCoordinates(x, y);
                return cells[x, y];
            }
            set
            {
                ValidateCoordinates(x, y);
                cells[x, y] = value;
            }
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private void ValidateCoordinates(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(x),
                    x,
                    $"X must be between 0 and {Width - 1}.");
            }

            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(y),
                    y,
                    $"Y must be between 0 and {Height - 1}.");
            }
        }
    }
}
