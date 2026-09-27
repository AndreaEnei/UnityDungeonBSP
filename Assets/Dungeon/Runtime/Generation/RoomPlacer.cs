using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Crea una stanza valida dentro ogni foglia BSP.</summary>
    public sealed class RoomPlacer
    {
        public IReadOnlyList<RectInt> PlaceRooms(
            IReadOnlyList<BspNode> leaves,
            DungeonGenerationConfig config
        )
        {
            if (leaves == null)
            {
                throw new ArgumentNullException(nameof(leaves));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ConfigValidationResult validation = config.Validate();

            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    "Rooms cannot be placed with an invalid configuration.",
                    nameof(config)
                );
            } 

            var random = new DeterministicRandom(config.Seed);
            var rooms = new List<RectInt>(leaves.Count);

            for (int i = 0; i < leaves.Count; i++)
            {
                BspNode leaf = leaves[i];

                if (leaf == null)
                {
                    throw new ArgumentException(
                        $"Leaf at index {i} is null.",
                        nameof(leaves)
                    );
                }

                RectInt room = CreateRoom(
                    leaf.Bounds,
                    config,
                    random);

                leaf.SetRoom(room);
                rooms.Add(room);
            }

            return rooms;
        }

        private static RectInt CreateRoom(
            RectInt leafBounds,
            DungeonGenerationConfig config,
            DeterministicRandom random)
        {
            int availableWidth = leafBounds.width - 2 * config.RoomMargin;
            int availableHeight = leafBounds.height - 2 * config.RoomMargin;

            int roomWidth = random.NextInt(config.MinRoomWidth, availableWidth + 1);
            int roomHeight = random.NextInt(config.MinRoomHeight, availableHeight + 1);

            int availableHorizontalOffset = availableWidth - roomWidth;
            int availableVerticalOffset = availableHeight - roomHeight;

            int roomX = 
                leafBounds.xMin +
                config.RoomMargin + 
                random.NextInt(0, availableHorizontalOffset + 1);
            int roomY = 
                leafBounds.yMin +
                config.RoomMargin + 
                random.NextInt(0, availableVerticalOffset + 1);

            return new RectInt(
                roomX,
                roomY,
                roomWidth,
                roomHeight);
        }
    } 
}