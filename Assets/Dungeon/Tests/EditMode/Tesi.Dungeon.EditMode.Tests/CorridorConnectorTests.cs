using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tesi.Dungeon.Tests
{
    public sealed class CorridorConnectorTests
    {
        [Test]
        public void Connect_CreatesValidCorridorTree()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();

            var partitioner = new BspPartitioner();
            BspGenerationResult bspResult = partitioner.Generate(config);

            Assert.That(bspResult.IsSuccess, Is.True);

            var roomPlacer = new RoomPlacer();
            roomPlacer.PlaceRooms(bspResult.Leaves, config);

            var connector = new CorridorConnector();
            IReadOnlyList<DungeonCorridor> corridors = connector.Connect(bspResult.Root, config);

            Assert.That(corridors.Count, Is.EqualTo(bspResult.Leaves.Count - 1));

            RectInt mapBounds = bspResult.Root.Bounds;

            for (int i = 0; i < corridors.Count; i++)
            {
                DungeonCorridor corridor = corridors[i];

                Assert.That(
                    Contains(mapBounds, corridor.FirstSegment),
                    Is.True,
                    $"First segment of corridor {i} exits the map."
                );

                Assert.That(
                    corridor.FirstSegment.Contains(corridor.Start),
                    Is.True,
                    $"Corridor {i} does not begin at Start."
                );

                if (corridor.SecondSegment.HasValue)
                {
                    RectInt secondSegment = corridor.SecondSegment.Value;

                    Assert.That(
                        Contains(mapBounds, secondSegment),
                        Is.True,
                        $"Second segment of corridor {i} exits the map."
                    );

                    Assert.That(
                        secondSegment.Contains(corridor.End),
                        Is.True,
                        $"Corridor {i} does not end at End."
                    );

                    Assert.That(
                        corridor.FirstSegment.Overlaps(secondSegment),
                        Is.True,
                        $"Segments of corridor {i} are disconnected."
                    );
                }
                else
                {
                    Assert.That(
                        corridor.FirstSegment.Contains(corridor.End),
                        Is.True,
                        $"Straight corridor {i} does not reach End."
                    );
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

        private static bool Contains(RectInt outer, RectInt inner)
        {
            return inner.xMin >= outer.xMin &&
                   inner.yMin >= outer.yMin &&
                   inner.xMax <= outer.xMax &&
                   inner.yMax <= outer.yMax;
        }
    }
}
