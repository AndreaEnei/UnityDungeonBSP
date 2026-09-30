using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tesi.Dungeon.Tests
{
    public sealed class DungeonRasterizerTests
    {
        [Test]
        public void RasterizeFloor_MarksRoomsAndCorridorsAsFloor()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();

            var partitioner = new BspPartitioner();

            BspGenerationResult bspResult = partitioner.Generate(config);

            Assert.That(bspResult.IsSuccess, Is.True);

            var roomPlacer = new RoomPlacer();

            IReadOnlyList<RectInt> rooms = roomPlacer.PlaceRooms(bspResult.Leaves, config);

            var connector = new CorridorConnector();

            IReadOnlyList<DungeonCorridor> corridors = connector.Connect(bspResult.Root, config);

            var rasterizer = new DungeonRasterizer();

            DungeonGrid grid = rasterizer.RasterizeFloor(config, rooms, corridors);

            Assert.That(grid.Width, Is.EqualTo(config.MapWidth));
            Assert.That(grid.Height, Is.EqualTo(config.MapHeight));

            for (int i = 0; i < rooms.Count; i++)
            {
                AssertRectangleIsFloor(grid, rooms[i], $"Room {i}");
            }

            for (int i = 0; i < corridors.Count; i++)
            {
                DungeonCorridor corridor = corridors[i];

                AssertRectangleIsFloor(grid, corridor.FirstSegment, $"First segment of corridor {i}");

                if (corridor.SecondSegment.HasValue)
                {
                    AssertRectangleIsFloor(grid, corridor.SecondSegment.Value, $"Second segment of corridor {i}");
                }
            }

        }

        [Test]
        public void BuildWalls_SurroundsFloorWithoutReplacingIt()
        {
            var grid = new DungeonGrid(5, 5);

            grid[2, 2] = CellType.Floor;

            var rasterizer = new DungeonRasterizer();

            rasterizer.BuildWalls(grid);

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (x == 2 && y == 2)
                    {
                        Assert.That(
                            grid[x, y], 
                            Is.EqualTo(CellType.Floor), 
                            "The floor cell was overwritten.");

                        continue;
                    }
                    
                    bool isNeighbor = 
                        x >= 1 && x <= 3 &&
                        y >= 1 && y <= 3;

                    CellType expected = isNeighbor 
                        ? CellType.Wall 
                        : CellType.Empty;

                    Assert.That(
                        grid[x, y],
                        Is.EqualTo(expected),
                        $"Unexpected cell type at ({x}, {y}).");
                }
            }
        }

        [Test]
        public void FullPipeline_SameConfigAndSeed_ProducesSameGrid()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();

            DungeonGrid firstGrid = GenerateCompleteGrid(config);
            DungeonGrid secondGrid = GenerateCompleteGrid(config);   

            Assert.That(secondGrid.Width, Is.EqualTo(firstGrid.Width));
            Assert.That(secondGrid.Height, Is.EqualTo(firstGrid.Height));

            for (int x = 0; x < firstGrid.Width; x++)
            {
                for (int y = 0; y < firstGrid.Height; y++)
                {
                    Assert.That(
                        secondGrid[x,y], 
                        Is.EqualTo(firstGrid[x,y]), 
                        $"Cell ({x}, {y}) differs between identical generations.");
                }
            }
        }

        private static DungeonGenerationConfig CreateDefaultConfig()
        {
            return new DungeonGenerationConfig(
                mapWidth: 80,
                mapHeight: 50,
                seed: 12345,
                maxDepth: 4,
                minLeafWidth: 12,
                minLeafHeight: 10,
                aspectRatioBias: 1.25f,
                minRoomWidth: 6,
                minRoomHeight: 5,
                roomMargin: 1,
                corridorWidth: 1);
        }

        private static void AssertRectangleIsFloor(
            DungeonGrid grid, 
            RectInt rectangle,
            string description)
        {
            for (int x = rectangle.xMin; x < rectangle.xMax; x++)
            {
                for (int y = rectangle.yMin; y < rectangle.yMax; y++)
                {
                    Assert.That(
                        grid[x, y], 
                        Is.EqualTo(CellType.Floor), 
                        $"{description} contains a non-floor cell at ({x}, {y}).");
                }
            }
            
        }

        private static DungeonGrid GenerateCompleteGrid(DungeonGenerationConfig config)
        {
            var partitioner = new BspPartitioner();

            BspGenerationResult bspResult = partitioner.Generate(config);

            Assert.That(bspResult.IsSuccess, Is.True);

            var roomPlacer = new RoomPlacer();

            IReadOnlyList<RectInt> rooms = roomPlacer.PlaceRooms(bspResult.Leaves, config);

            var connector = new CorridorConnector();

            IReadOnlyList<DungeonCorridor> corridors = connector.Connect(bspResult.Root, config);

            var rasterizer = new DungeonRasterizer();

            DungeonGrid grid = rasterizer.RasterizeFloor(config, rooms, corridors);

            rasterizer.BuildWalls(grid);

            return grid;
        }
    }
}
