namespace Tesi.Dungeon
{
    /// <summary>Riassume le caratteristiche quantitative di un dungeon.</summary>
    public sealed class DungeonGenerationMetrics
    {
        public int NodeCount { get; }
        public int LeafCount { get; }
        public int RoomCount { get; }
        public int CorridorCount { get; }
        public int MaxReachedDepth { get; }

        public int FloorCellCount { get; }
        public int WallCellCount { get; }
        public int EmptyCellCount { get; }

        public int TotalCellCount => FloorCellCount + WallCellCount + EmptyCellCount;

        public float FloorCoveragePercentage => 
            TotalCellCount == 0
            ? 0f
            : (float)FloorCellCount / TotalCellCount * 100f;

        public DungeonGenerationMetrics(
            int nodeCount,
            int leafCount,
            int roomCount,
            int corridorCount,
            int maxReachedDepth,
            int floorCellCount,
            int wallCellCount,
            int emptyCellCount)
        {
            NodeCount = nodeCount;
            LeafCount = leafCount;
            RoomCount = roomCount;
            CorridorCount = corridorCount;
            MaxReachedDepth = maxReachedDepth;
            FloorCellCount = floorCellCount;
            WallCellCount = wallCellCount;
            EmptyCellCount = emptyCellCount;
        }
    }
}