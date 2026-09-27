using System;

namespace Tesi.Dungeon
{
    /// <summary>
    /// Generatore pseudo casuale locale e riproducibile basato su xorshift32.
    /// </summary>
    public sealed class DeterministicRandom
    {
        private const uint NonZeroFallbackSeed = 0xA341316C;
        private uint state;

        public DeterministicRandom(uint seed)
        {
            state = seed == 0
                ? NonZeroFallbackSeed
                : seed;
        }

        /// <summary>Restituisce il prossimo intero senza segno della sequenza.</summary>
        public uint NextUInt()
        {
            uint value = state;

            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;

            state = value;
            return value;
        }

        /// <summary>Restituisce un intero compreso tra min incluso e max escluso.</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive),
                    "maxExclusive must be greater than minInclusive."
                );
            }

            uint range = (uint)(maxExclusive - minInclusive);
            uint offset = NextUInt() % range;

            return minInclusive + (int)offset;
        }

        /// <summary>Restituisce vero o falso usando il bit più significativo.</summary>
        public bool NextBool()
        {
            return (NextUInt() & 0x80000000u) != 0u;
        }
    }
}