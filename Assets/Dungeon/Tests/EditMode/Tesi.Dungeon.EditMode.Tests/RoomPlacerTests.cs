using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tesi.Dungeon.RoomPlacerTests
{
    public sealed class RoomPlacerTests
    {
        [Test]
        public void PlaceRooms_AssignsValidRoomToEveryLeaf()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();

            var partitioner = new BspPartitioner();
            BspGenerationResult bspResult = partitioner.Generate(config);

            Assert.That(bspResult.IsSuccess, Is.True);

            var roomPlacer = new RoomPlacer();

            IReadOnlyList<RectInt> rooms = roomPlacer.PlaceRooms(bspResult.Leaves, config);

            Assert.That(rooms.Count, Is.EqualTo(bspResult.Leaves.Count));

            for (int i = 0; i < bspResult.Leaves.Count; i++)
            {
                BspNode leaf = bspResult.Leaves[i];

                Assert.That(leaf.Room.HasValue, Is.True, $"Leaf {i} has no room.");

                RectInt room = leaf.Room.Value;

                Assert.That(room, Is.EqualTo(rooms[i]));
                Assert.That(room.width, Is.GreaterThanOrEqualTo(config.MinRoomWidth));
                Assert.That(room.height, Is.GreaterThanOrEqualTo(config.MinRoomHeight));
                Assert.That(room.xMin, Is.GreaterThanOrEqualTo(leaf.Bounds.xMin + config.RoomMargin));
                Assert.That(room.yMin, Is.GreaterThanOrEqualTo(leaf.Bounds.yMin + config.RoomMargin));
                Assert.That(room.xMax, Is.LessThanOrEqualTo(leaf.Bounds.xMax - config.RoomMargin));
                Assert.That(room.yMax, Is.LessThanOrEqualTo(leaf.Bounds.yMax - config.RoomMargin));
            }
        }

        [Test]
        public void PlaceRooms_SameConfigAndSeed_ProducesSameRooms()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();

            var partitioner = new BspPartitioner();

            BspGenerationResult firstBsp = partitioner.Generate(config);
            BspGenerationResult secondBsp = partitioner.Generate(config);

            Assert.That(firstBsp.IsSuccess, Is.True);
            Assert.That(secondBsp.IsSuccess, Is.True);

            var roomPlacer = new RoomPlacer();

            IReadOnlyList<RectInt> firstRooms = roomPlacer.PlaceRooms(firstBsp.Leaves, config);
            IReadOnlyList<RectInt> secondRooms = roomPlacer.PlaceRooms(secondBsp.Leaves, config);

            Assert.That(secondRooms.Count, Is.EqualTo(firstRooms.Count));

            for (int i = 0; i < firstRooms.Count; i++)
            {
                Assert.That(secondRooms[i], Is.EqualTo(firstRooms[i]), $"Room {i} differs between identical generations.");
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
    }
}