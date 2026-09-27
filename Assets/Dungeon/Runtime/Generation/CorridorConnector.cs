using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Collega ricorsivamente i sottoalberi BSP tramite corridoi.</summary>
    public sealed class CorridorConnector
    {
        public IReadOnlyList<DungeonCorridor> Connect(
            BspNode root,
            DungeonGenerationConfig config)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ConfigValidationResult validation = config.Validate();

            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    "Corridors cannot be created with an invalid configuration.",
                    nameof(config)
                );
            }

            var random = new DeterministicRandom(config.Seed);
            var corridors = new List<DungeonCorridor>();

            ConnectRecursive(
                root,
                corridors,
                config.CorridorWidth,
                random);

            return corridors;
        }

        private static RectInt ConnectRecursive(
            BspNode node,
            List<DungeonCorridor> corridors,
            int corridorWidth,
            DeterministicRandom random)
        {
            if (node.IsLeaf)
            {
                if (!node.Room.HasValue)
                {
                    throw new InvalidOperationException(
                        "Every BSP leaf must have a room before corridors are created.");
                }

                return node.Room.Value;
            }

            RectInt leftRoom = ConnectRecursive(
                node.Left,
                corridors,
                corridorWidth,
                random);
            RectInt rightRoom = ConnectRecursive(
                node.Right,
                corridors,
                corridorWidth,
                random);

            DungeonCorridor corridor = CreateCorridor(
                leftRoom,
                rightRoom,
                corridorWidth,
                random);

            corridors.Add(corridor);

            return random.NextBool()
                ? leftRoom
                : rightRoom;
        }

        private static DungeonCorridor CreateCorridor(
            RectInt leftRoom,
            RectInt rightRoom,
            int corridorWidth,
            DeterministicRandom random)
        {
            Vector2Int start = GetRoomCenter(leftRoom);
            Vector2Int end = GetRoomCenter(rightRoom);

            if (start.x == end.x)
            {
                RectInt verticalSegment = CreateVerticalSegment(start, end, corridorWidth);

                return new DungeonCorridor(
                    start,
                    end,
                    verticalSegment,
                    null);
            }

            if (start.y == end.y)
            {
                RectInt horizontalSegment = CreateHorizontalSegment(start, end, corridorWidth);

                return new DungeonCorridor(
                    start,
                    end,
                    horizontalSegment,
                    null);
            }

            if (random.NextBool())
            {
                var bend = new Vector2Int(end.x, start.y);

                RectInt horizontalSegment = CreateHorizontalSegment(start, bend, corridorWidth);
                RectInt verticalSegment = CreateVerticalSegment(bend, end, corridorWidth);

                return new DungeonCorridor(
                    start,
                    end,
                    horizontalSegment,
                    verticalSegment);
            }
            else
            {
                var bend = new Vector2Int(start.x, end.y);

                RectInt verticalSegment = CreateVerticalSegment(start, bend, corridorWidth);
                RectInt horizontalSegment = CreateHorizontalSegment(bend, end, corridorWidth);

                return new DungeonCorridor(
                    start,
                    end,
                    verticalSegment,
                    horizontalSegment);
            }
        }

        private static Vector2Int GetRoomCenter(RectInt room)
        {
            return new Vector2Int(
                (room.xMin + room.xMax) / 2,
                (room.yMin + room.yMax) / 2);
        }

        private static RectInt CreateHorizontalSegment(
            Vector2Int start,
            Vector2Int end,
            int corridorWidth)
        {
            int xMin = Math.Min(start.x, end.x);
            int segmentLength = Math.Abs(end.x - start.x) + 1;

            int yMin = start.y - corridorWidth / 2;

            return new RectInt(
                xMin,
                yMin,
                segmentLength,
                corridorWidth);
        }

        private static RectInt CreateVerticalSegment(
            Vector2Int start,
            Vector2Int end,
            int corridorWidth)
        {
            int yMin = Math.Min(start.y, end.y);
            int segmentLength = Math.Abs(end.y - start.y) + 1;

            int xMin = start.x - corridorWidth / 2;

            return new RectInt(
                xMin,
                yMin,
                corridorWidth,
                segmentLength);
        }
    }
}
