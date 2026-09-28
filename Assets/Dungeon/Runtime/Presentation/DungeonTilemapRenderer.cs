using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tesi.Dungeon
{
    /// <summary></summary>
    public sealed class DungeonTilemapRenderer : MonoBehaviour
    {
        [Header("Tilemaps")]
        
        [SerializeField]
        private Tilemap floorTilemap;

        [SerializeField]
        private Tilemap wallTilemap;

        [Header("Tiles")]

        [SerializeField]
        private TileBase floorTile;

        [SerializeField]
        private TileBase wallTile;

        public void Render(DungeonGrid grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            ValidateReferences();

            Clear();

            var bounds = new BoundsInt(0, 0, 0, grid.Width, grid.Height, 1);
            int cellCount = grid.Width * grid.Height;
            var floorTiles = new TileBase[cellCount];
            var wallTiles = new TileBase[cellCount];

            int index = 0;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    CellType cell = grid[x, y];

                    switch (cell)
                    {
                        case CellType.Empty:
                            break;

                        case CellType.Floor:
                            floorTiles[index] = floorTile;
                            break;

                        case CellType.Wall:
                            wallTiles[index] = wallTile;
                            break;

                        default:
                            throw new InvalidOperationException(
                                $"Unexpected cell type: {cell}");
                    }

                    index++;
                }
            }

            floorTilemap.SetTilesBlock(bounds, floorTiles);
            wallTilemap.SetTilesBlock(bounds, wallTiles);
        }

        public void Clear()
        {
            if (floorTilemap != null)
            {
                floorTilemap.ClearAllTiles();
            }
            
            if (wallTilemap != null)
            {
                wallTilemap.ClearAllTiles();
            }
        }

        private void ValidateReferences()
        {
            if (floorTilemap == null)
            {
                throw new InvalidOperationException(
                    "Floor Tilemap is not assigned.");
            }

            if (wallTilemap == null)
            {
                throw new InvalidOperationException(
                    "Wall Tilemap is not assigned.");
            }

            if (floorTile == null)
            {
                throw new InvalidOperationException(
                    "Floor Tile is not assigned.");
            }

            if (wallTile == null)
            {
                throw new InvalidOperationException(
                    "Wall Tile is not assigned.");
            }
        }

    }
}