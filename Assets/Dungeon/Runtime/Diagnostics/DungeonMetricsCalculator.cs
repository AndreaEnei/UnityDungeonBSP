using UnityEngine;
using System.Collections.Generic;
using System;

namespace Tesi.Dungeon
{
    /// <summary>Calcola le metriche descrittive di un dungeon generato.</summary>
    public sealed class DungeonMetricsCalculator
    {
        public DungeonGenerationMetrics Calculate(
            BspGenerationResult bspResult,
            IReadOnlyList<RectInt> rooms,
            IReadOnlyList<DungeonCorridor> corridors,
            DungeonGrid grid)
        {
            if (bspResult == null)
            {
                throw new ArgumentNullException(nameof(bspResult));
            }

            if (!bspResult.IsSuccess)
            {
                throw new ArgumentException(
                    "Metrics require a successful BSP result.", 
                    nameof(bspResult));
            }

            if (rooms == null)
            {
                throw new ArgumentNullException(nameof(rooms));
            }

            if (corridors == null)
            {
                throw new ArgumentNullException(nameof(corridors));
            }

            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            int floorCellCount = 0;
            int wallCellCount = 0;
            int emptyCellCount = 0;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    switch (grid[x, y])
                    {
                        case CellType.Floor:
                            floorCellCount++;
                            break;

                        case CellType.Wall:
                            wallCellCount++;
                            break;

                        case CellType.Empty:
                            emptyCellCount++;
                            break;

                        default:
                            throw new InvalidOperationException(
                                $"Unsupported cell type at ({x}, {y})");
                    }
                }
            }

            return new DungeonGenerationMetrics(
                nodeCount: bspResult.NodeCount,
                leafCount: bspResult.Leaves.Count,
                roomCount: rooms.Count,
                corridorCount: corridors.Count,
                maxReachedDepth: bspResult.MaxReachedDepth,
                floorCellCount: floorCellCount,
                wallCellCount: wallCellCount,
                emptyCellCount: emptyCellCount);
        }
    }
}