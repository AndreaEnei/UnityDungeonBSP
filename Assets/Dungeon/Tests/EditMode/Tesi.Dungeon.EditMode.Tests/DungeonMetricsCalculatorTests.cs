using UnityEngine;
using NUnit.Framework;
using System;

namespace Tesi.Dungeon.Tests
{
    public sealed class DungeonMetricsCalculatorTests
    {
        [Test]
        public void Calculate_ReturnExpectedStructuralAndGridMetrics()
        {
            var root = new BspNode(new RectInt(0, 0, 3, 2), depth: 0);
            var room = new RectInt(0, 0, 2, 1);

            root.SetRoom(room);

            BspGenerationResult bspResult = BspGenerationResult.Succeeded(
                root,
                leaves: new[] { root },
                nodeCount: 1,
                maxReachedDepth: 0);

            var rooms = new[] { room };

            DungeonCorridor[] corridors = Array.Empty<DungeonCorridor>();

            var grid = new DungeonGrid(3, 2);

            grid[0, 0] = CellType.Floor;
            grid[1, 0] = CellType.Floor;

            grid[0, 1] = CellType.Wall;
            grid[1, 1] = CellType.Wall;
            grid[2, 0] = CellType.Wall;

            var calculator = new DungeonMetricsCalculator();

            DungeonGenerationMetrics metrics = calculator.Calculate(
                bspResult,
                rooms,
                corridors,
                grid);

            Assert.That(metrics.NodeCount, Is.EqualTo(1));
            Assert.That(metrics.LeafCount, Is.EqualTo(1));
            Assert.That(metrics.RoomCount, Is.EqualTo(1));
            Assert.That(metrics.CorridorCount, Is.EqualTo(0));
            Assert.That(metrics.MaxReachedDepth, Is.EqualTo(0));

            Assert.That(metrics.FloorCellCount, Is.EqualTo(2));
            Assert.That(metrics.WallCellCount, Is.EqualTo(3));
            Assert.That(metrics.EmptyCellCount, Is.EqualTo(1));
            Assert.That(metrics.TotalCellCount, Is.EqualTo(6));

            Assert.That(metrics.FloorCoveragePercentage, Is.EqualTo(100f / 3f).Within(0.001f));
        }
    }
}