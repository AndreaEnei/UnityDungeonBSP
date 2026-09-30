using System;

namespace Tesi.Dungeon
{
    /// <summary>Descrive il risultato della verifica di connettività.</summary>
    public sealed class DungeonConnectivityResult
    {
        public int TotalFloorCells { get; }

        public int ReachableFloorCells { get; }

        public int UnreachableFloorCells => TotalFloorCells - ReachableFloorCells;

        public bool IsConnected => TotalFloorCells > 0 && ReachableFloorCells == TotalFloorCells;

        public DungeonConnectivityResult(
            int totalFloorCells,
            int reachableFloorCells)
        {
            if (totalFloorCells < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalFloorCells));
            }

            if (reachableFloorCells < 0 || reachableFloorCells > totalFloorCells)
            {
                throw new ArgumentOutOfRangeException(nameof(reachableFloorCells));
            }

            TotalFloorCells = totalFloorCells;
            ReachableFloorCells = reachableFloorCells;
        }
    }
}
