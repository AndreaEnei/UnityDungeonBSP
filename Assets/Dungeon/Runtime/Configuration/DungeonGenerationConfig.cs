using System.Collections.Generic;

namespace Tesi.Dungeon
{
    public sealed class DungeonGenerationConfig
    {
        /// <summary>Larghezza totale della mappa, espressa in celle.</summary>
        public int MapWidth { get; }

        /// <summary>Altezza totale della mappa, espressa in celle.</summary>
        public int MapHeight { get; }

        /// <summary>Valore iniziale del PRNG, usato per ottenere risultati riproducibili.</summary>
        public uint Seed { get; }

        /// <summary>Profondita massima raggiungibile dall'albero BSP.</summary>
        public int MaxDepth { get; }

        /// <summary>Larghezza minima consentita per una foglia BSP.</summary>
        public int MinLeafWidth { get; }

        /// <summary>Altezza minima consentita per una foglia BSP.</summary>
        public int MinLeafHeight { get; }

        /// <summary>Rapporto oltre il quale si preferisce un orientamento di taglio.</summary>
        public float AspectRatioBias { get; }

        /// <summary>Larghezza minima consentita per una stanza.</summary>
        public int MinRoomWidth { get; }

        /// <summary>Altezza minima consentita per una stanza.</summary>
        public int MinRoomHeight { get; }
        
        /// <summary>Distanza minima tra una stanza e il bordo di una foglia.</summary>
        public int RoomMargin { get; }

        /// <summary>Larghezza dei corridoi, espressa in celle.</summary>
        public int CorridorWidth { get; }

        public DungeonGenerationConfig(
            int mapWidth,
            int mapHeight,
            uint seed,
            int maxDepth,
            int minLeafWidth,
            int minLeafHeight,
            float aspectRatioBias,
            int minRoomWidth,
            int minRoomHeight,
            int roomMargin,
            int corridorWidth)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            Seed = seed;
            MaxDepth = maxDepth;
            MinLeafWidth = minLeafWidth;
            MinLeafHeight = minLeafHeight;
            AspectRatioBias = aspectRatioBias;
            MinRoomWidth = minRoomWidth;
            MinRoomHeight = minRoomHeight;
            RoomMargin = roomMargin;
            CorridorWidth = corridorWidth;
        }

        /// <summary>Controlla che i parametri permettano una generazione valida.</summary>
        public ConfigValidationResult Validate()
        {
            var errors = new List<string>();

            if (MinLeafWidth < 2)
            {
                errors.Add(
                    $"MinLeafWidth ({MinLeafWidth}) must be at least 2."
                );
            }

            if (MinLeafHeight < 2)
            {
                errors.Add(
                    $"MinLeafHeight ({MinLeafHeight}) must be at least 2."
                );
            }

            if (MapWidth < MinLeafWidth)
            {
                errors.Add(
                    $"MapWidth ({MapWidth}) must be >= MinLeafWidth ({MinLeafWidth})."
                );
            }

            if (MapHeight < MinLeafHeight)
            {
                errors.Add(
                    $"MapHeight ({MapHeight}) must be >= MinLeafHeight ({MinLeafHeight})."
                );
            }

            if (MaxDepth < 0)
            {
                errors.Add(
                    $"MaxDepth ({MaxDepth}) must be >= 0."
                );
            }

            if (AspectRatioBias < 1f)
            {
                errors.Add(
                    $"AspectRatioBias ({AspectRatioBias}) must be >= 1."
                );
            }

            if (MinRoomWidth < 1)
            {
                errors.Add(
                    $"MinRoomWidth ({MinRoomWidth}) must be >= 1."
                );
            }

            if (MinRoomHeight < 1)
            {
                errors.Add(
                    $"MinRoomHeight ({MinRoomHeight}) must be >= 1."
                );
            }

            if (RoomMargin < 0)
            {
                errors.Add(
                    $"RoomMargin ({RoomMargin}) must be >= 0."
                );
            }

            int requiredLeafWidth = MinRoomWidth + 2 * RoomMargin;
            int requiredLeafHeight = MinRoomHeight + 2 * RoomMargin;

            if (MinLeafWidth < requiredLeafWidth)
            {
                errors.Add(
                    $"MinLeafWidth ({MinLeafWidth}) must be >= " + 
                    $"MinRoomWidth + 2 * RoomMargin ({requiredLeafWidth})."
                );
            }

            if (MinLeafHeight < requiredLeafHeight)
            {
                errors.Add(
                    $"MinLeafHeight ({MinLeafHeight}) must be >= " + 
                    $"MinRoomHeight + 2 * RoomMargin ({requiredLeafHeight})."
                );
            }

            if (CorridorWidth < 1)
            {
                errors.Add(
                    $"CorridorWidth ({CorridorWidth}) must be >= 1."
                );
            }

            if (CorridorWidth > MinRoomWidth || CorridorWidth > MinRoomHeight)
            {
                errors.Add(
                    $"CorridorWidth ({CorridorWidth}) must not exceed the minimum room dimensions."
                );
            }

            return new ConfigValidationResult(errors);
        }
    }
}
