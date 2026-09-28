using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Converte stanze e corridoi in celle della griglia</summary>
    public sealed class DungeonRasterizer
    {
        public DungeonGrid RasterizeFloor(
            DungeonGenerationConfig config,
            IReadOnlyList<RectInt> rooms,
            IReadOnlyList<DungeonCorridor> corridors)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (rooms == null)
            {
                throw new ArgumentNullException(nameof(rooms));
            }

            if (corridors == null)
            {
                throw new ArgumentNullException(nameof(corridors));
            }

            ConfigValidationResult validation = config.Validate();

            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    "The dungeon cannot be rasterized with an invalid configuration.", 
                    nameof(config));
            }

            var grid = new DungeonGrid(
                config.MapWidth, 
                config.MapHeight);

            for (int i = 0; i < rooms.Count; i++)
            {
                FillRectangle(grid, rooms[i], CellType.Floor);
            }

            for (int i = 0; i < corridors.Count; i++)
            {
                DungeonCorridor corridor = corridors[i];

                if (corridor == null)
                {
                    throw new ArgumentException(
                        $"Corridor at index {i} is null.",
                        nameof(corridors));
                }

                FillRectangle(grid, corridor.FirstSegment, CellType.Floor);

                if (corridor.SecondSegment.HasValue)
                {
                    FillRectangle(grid, corridor.SecondSegment.Value, CellType.Floor);
                }
            }

            return grid;
        }

        private static void FillRectangle(
            DungeonGrid grid,
            RectInt rectangle,
            CellType cellType)
        {
            if (!Contains(grid.Bounds, rectangle))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rectangle),
                    "The rectangle must be contained inside the grid.");     
            }

            for (int x = rectangle.xMin; x < rectangle.xMax; x++)
            {
                for (int y = rectangle.yMin; y < rectangle.yMax; y++)
                {
                    grid[x,y] = cellType;
                }
            }
        }

        private static bool Contains(RectInt outer, RectInt inner)
        {
            return inner.xMin >= outer.xMin &&
                   inner.xMax <= outer.xMax &&
                   inner.yMin >= outer.yMin &&
                   inner.yMax <= outer.yMax;
        }
    }
}