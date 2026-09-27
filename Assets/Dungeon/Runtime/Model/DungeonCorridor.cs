using System;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Rappresenta un corridoio rettilineo oppure composto da due segmenti a L.</summary>
    public sealed class DungeonCorridor
    {
        /// <summary>Cella centrale della stanza di partenza.</summary>
        public Vector2Int Start { get; }
        /// <summary>Cella centrale della stanza di arrivo.</summary>
        public Vector2Int End { get; }
        /// <summary>Primo segmento del corridoio.</summary>
        public RectInt FirstSegment { get; }
        /// <summary>Secondo segmento del corridoio, se presente.</summary>
        public RectInt? SecondSegment { get; }

        public DungeonCorridor(
            Vector2Int start,
            Vector2Int end,
            RectInt firstSegment,
            RectInt? secondSegment
        )
        {
            if (firstSegment.width <= 0 || firstSegment.height <= 0)
            {
                throw new ArgumentException(
                    "The first corridor segment must have positive dimensions.",
                    nameof(firstSegment));
            }

            if (secondSegment.HasValue && (secondSegment.Value.width <= 0 || secondSegment.Value.height <= 0))
            {
                throw new ArgumentException(
                    "The second corridor segment must have positive dimensions.",
                    nameof(secondSegment));
            }

            Start = start;
            End = end;
            FirstSegment = firstSegment;
            SecondSegment = secondSegment;
        }
    }
}