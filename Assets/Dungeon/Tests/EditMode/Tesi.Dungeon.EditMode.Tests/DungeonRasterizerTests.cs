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
    }
}