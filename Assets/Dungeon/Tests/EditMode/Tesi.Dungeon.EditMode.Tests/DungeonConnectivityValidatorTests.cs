using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tesi.Dungeon.Tests
{
    public sealed class DungeonConnectivityValidatorTests
    {
        [Test]
        public void Validate_ConnectedFloor_ReturnsConnected()
        {
            var grid = new DungeonGrid(5, 4);

            grid[1, 1] = CellType.Floor;
            grid[2, 1] = CellType.Floor;
            grid[3, 1] = CellType.Floor;
            grid[3, 2] = CellType.Floor;

            var validator = new DungeonConnectivityValidator();

            DungeonConnectivityResult result = validator.Validate(grid);

            Assert.That(result.IsConnected, Is.True);
            Assert.That(result.TotalFloorCells, Is.EqualTo(4));
            Assert.That(result.ReachableFloorCells, Is.EqualTo(4));
            Assert.That(result.UnreachableFloorCells, Is.EqualTo(0));
        }

        [Test]
        public void Validate_DisconnectedFloor_ReturnDisconnected()
        {
            var grid = new DungeonGrid(5, 3);

            grid[0, 0] = CellType.Floor;
            grid[1, 0] = CellType.Floor;

            grid[3, 2] = CellType.Floor;
            grid[4, 2] = CellType.Floor;

            var validator = new DungeonConnectivityValidator();

            DungeonConnectivityResult result = validator.Validate(grid);

            Assert.That(result.IsConnected, Is.False);
            Assert.That(result.TotalFloorCells, Is.EqualTo(4));
            Assert.That(result.ReachableFloorCells, Is.EqualTo(2));
            Assert.That(result.UnreachableFloorCells, Is.EqualTo(2));
        }

        [Test]
        public void Validate_EmptyGrid_ReturnDisconnected()
        {
            var grid = new DungeonGrid(5, 3);
            var validator = new DungeonConnectivityValidator();

            DungeonConnectivityResult result = validator.Validate(grid);

            Assert.That(result.IsConnected, Is.False);
            Assert.That(result.TotalFloorCells, Is.EqualTo(0));
            Assert.That(result.ReachableFloorCells, Is.EqualTo(0));
            Assert.That(result.UnreachableFloorCells, Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(42)]
        [TestCase(12345)]
        [TestCase(987654)]
        public void FullPipeline_MultipleSeeds_ProducesConnectedDungeon(int seed)
        {
            DungeonGenerationConfig config = CreateConfig((uint)seed);

            DungeonConnectivityResult result = GenerateAndValidate(config);

            Assert.That(
                result.IsConnected, 
                Is.True, 
                $"Dungeon generated with seed {seed} has " +
                $"{result.UnreachableFloorCells} unreachable floor cells.");
        }

        [Test]
        public void FullPipeline_MinimumUnsplitMap_ProducesConnectedDungeon()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 6,
                mapHeight: 5,
                seed: 12345,
                maxDepth: 0,
                minLeafWidth: 6,
                minLeafHeight: 5,
                aspectRatioBias: 1.25f,
                minRoomWidth: 4,
                minRoomHeight: 3,
                roomMargin: 1,
                corridorWidth: 1);

            DungeonConnectivityResult result = GenerateAndValidate(config);

            Assert.That(result.IsConnected, Is.True);
        }

        [Test]
        public void FullPipeline_DeepPartitioning_ProducesConnectedDungeon()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 96,
                mapHeight: 64,
                seed: 12345,
                maxDepth: 8,
                minLeafWidth: 6,
                minLeafHeight: 5,
                aspectRatioBias: 1.25f,
                minRoomWidth: 4,
                minRoomHeight: 3,
                roomMargin: 1,
                corridorWidth: 1);

            DungeonConnectivityResult result = GenerateAndValidate(config);

            Assert.That(result.IsConnected, Is.True);
        }

        [Test]
        public void FullPipeline_NarrowMap_ProducesConnectedDungeon()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 12,
                mapHeight: 60,
                seed: 12345,
                maxDepth: 6,
                minLeafWidth: 6,
                minLeafHeight: 8,
                aspectRatioBias: 1.25f,
                minRoomWidth: 4,
                minRoomHeight: 6,
                roomMargin: 1,
                corridorWidth: 1);

            DungeonConnectivityResult result = GenerateAndValidate(config);

            Assert.That(result.IsConnected, Is.True);
        }

        [Test]
        public void FullPipeline_WideCorridors_ProducesConnectedDungeon()
        {
            var config = new DungeonGenerationConfig(
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
                corridorWidth: 5);

            DungeonConnectivityResult result =
                GenerateAndValidate(config);

            Assert.That(result.IsConnected, Is.True);
        }

        [Test]
        [Category("Stress")]
        public void FullPipeline_FirstThousandSeeds_ProduceConnectedDungeons()
        {
            const int seedCount = 1000;

            for (int seed = 0; seed < seedCount; seed++)
            {
                DungeonGenerationConfig config = CreateConfig((uint)seed);

                DungeonConnectivityResult result = GenerateAndValidate(config);

                Assert.That(
                    result.IsConnected,
                    Is.True,
                    $"Dungeon generated with seed {seed} has " +
                    $"{result.UnreachableFloorCells} unreachable floor cells.");
            }
        }

        private static DungeonGenerationConfig CreateConfig(uint seed)
        {
            return new DungeonGenerationConfig(
                mapWidth: 80,
                mapHeight: 50,
                seed: seed,
                maxDepth: 4,
                minLeafWidth: 12,
                minLeafHeight: 10,
                aspectRatioBias: 1.25f,
                minRoomWidth: 6,
                minRoomHeight: 5,
                roomMargin: 1,
                corridorWidth: 1);
        }

        private static DungeonConnectivityResult GenerateAndValidate(DungeonGenerationConfig config)
        {
            var partitioner = new BspPartitioner();
            BspGenerationResult bspResult = partitioner.Generate(config);

            Assert.That(bspResult.IsSuccess, Is.True);

            var roomPlacer = new RoomPlacer();

            IReadOnlyList<RectInt> rooms = roomPlacer.PlaceRooms(bspResult.Leaves, config);

            var corridorConnector = new CorridorConnector();

            IReadOnlyList<DungeonCorridor> corridors = corridorConnector.Connect(bspResult.Root, config);

            var rasterizer = new DungeonRasterizer();

            DungeonGrid grid = rasterizer.RasterizeFloor(config, rooms, corridors);

            rasterizer.BuildWalls(grid);

            var validator = new DungeonConnectivityValidator();

            return validator.Validate(grid);
        }
    }
}