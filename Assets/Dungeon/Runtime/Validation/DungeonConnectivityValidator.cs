using UnityEngine;
using System.Collections.Generic;
using System;

namespace Tesi.Dungeon
{
    /// <summary>Verifica che tutte le celle di pavimento siano raggiungibili.</summary>
    public sealed class DungeonConnectivityValidator
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        public DungeonConnectivityResult Validate(DungeonGrid grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            int totalFloorCells = 0;
            Vector2Int? startCell = null;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid[x, y] != CellType.Floor)
                    {
                        continue;
                    }

                    totalFloorCells++;

                    if (!startCell.HasValue)
                    {
                        startCell = new Vector2Int(x, y);
                    }
                }
            }

            if (!startCell.HasValue)
            {
                return new DungeonConnectivityResult(
                    totalFloorCells: 0,
                    reachableFloorCells: 0
                );
            }

            var visited = new bool[grid.Width, grid.Height];
            var frontier = new Queue<Vector2Int>();

            frontier.Enqueue(startCell.Value);
            visited[startCell.Value.x, startCell.Value.y] = true;

            int reachableFloorCells = 0;

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                
                reachableFloorCells++;

                for (int i = 0; i < Directions.Length; i++)
                {
                    Vector2Int neighbor = current + Directions[i];

                    if (!grid.IsInside(neighbor.x, neighbor.y))
                    {
                        continue;
                    }

                    if (visited[neighbor.x, neighbor.y])
                    {
                        continue;
                    }

                    if (grid[neighbor.x, neighbor.y] != CellType.Floor)
                    {
                        continue;
                    }

                    visited[neighbor.x, neighbor.y] = true;
                    frontier.Enqueue(neighbor);
                }
            }

            return new DungeonConnectivityResult(totalFloorCells, reachableFloorCells);
        }
    }
}