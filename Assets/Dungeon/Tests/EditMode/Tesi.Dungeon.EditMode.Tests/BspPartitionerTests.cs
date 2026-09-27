using System.Text;
using NUnit.Framework;

namespace Tesi.Dungeon.Tests
{
    public sealed class BspPartitionerTests
    {
        [Test]
        public void Generate_SameConfigAndSeed_ProducesSameTree()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();
            var partitioner = new BspPartitioner();
            
            BspGenerationResult first = partitioner.Generate(config);
            BspGenerationResult second = partitioner.Generate(config);

            // Assert.That(valoreEffettivo, condizioneAttesa);
            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);

            string firstSignature = BuildTreeSignature(first.Root);
            string secondSignature = BuildTreeSignature(second.Root);

            Assert.That(secondSignature, Is.EqualTo(firstSignature));

        }

        [Test]
        public void Generate_AllLeavesRespectMinimumDimensions()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();
            var partitioner = new BspPartitioner();

            BspGenerationResult result = partitioner.Generate(config);

            Assert.That(result.IsSuccess, Is.True);

            for (int i = 0; i < result.Leaves.Count; i++)
            {
                BspNode leaf = result.Leaves[i];
                
                Assert.That(leaf.IsLeaf, Is.True);
            
                Assert.That(
                    leaf.Bounds.width,
                    Is.GreaterThanOrEqualTo(config.MinLeafWidth),
                    $"Leaf {i} is too narrow.");

                Assert.That(
                    leaf.Bounds.height,
                    Is.GreaterThanOrEqualTo(config.MinLeafHeight),
                    $"Leaf {i} is too short.");
            }
        }

        [Test]
        public void Generate_MaxDepthZero_ReturnsOnlyRoot()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 80,
                mapHeight: 50,
                seed: 12345,
                maxDepth: 0,
                minLeafWidth: 12,
                minLeafHeight: 10,
                aspectRatioBias: 1.25f,
                minRoomWidth: 6,
                minRoomHeight: 5,
                roomMargin: 1,
                corridorWidth: 1);

            var partitioner = new BspPartitioner();

            BspGenerationResult result = partitioner.Generate(config);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.NodeCount, Is.EqualTo(1));
            Assert.That(result.Leaves.Count, Is.EqualTo(1));
            Assert.That(result.MaxReachedDepth, Is.EqualTo(0));
            Assert.That(result.Root.IsLeaf, Is.True);
            Assert.That(result.Leaves[0], Is.SameAs(result.Root));
        }

        [Test]
        public void Generate_EveryInternalNodeHasValidGeometry()
        {
            DungeonGenerationConfig config = CreateDefaultConfig();
            var partitioner = new BspPartitioner();

            BspGenerationResult result = partitioner.Generate(config);

            Assert.That(result.IsSuccess, Is.True);

            AssertValidHierarchy(result.Root);
        }

        [Test]
        public void Generate_InvalidConfiguration_ReturnFailure()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 8,
                mapHeight: 8,
                seed: 12345,
                maxDepth: 4,
                minLeafWidth: 12,
                minLeafHeight: 10,
                aspectRatioBias: 1.25f,
                minRoomWidth: 6,
                minRoomHeight: 5,
                roomMargin: 1,
                corridorWidth: 1);

            var partitioner = new BspPartitioner();

            BspGenerationResult result = partitioner.Generate(config);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Root, Is.Null);
            Assert.That(result.Leaves, Is.Empty);
            Assert.That(result.NodeCount, Is.EqualTo(0));
            Assert.That(result.Validation.IsValid, Is.False);
            Assert.That(result.Validation.Errors, Is.Not.Null);

            string errorText = string.Join("\n", result.Validation.Errors);

            Assert.That(errorText, Does.Contain("MapWidth"));
            Assert.That(errorText, Does.Contain("MapHeight"));
        }

        [Test]
        public void Generate_OnlyVerticalSplitIsPossible_SplitsVertically()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 30,
                mapHeight: 10,
                seed: 12345,
                maxDepth: 4,
                minLeafWidth: 12,
                minLeafHeight: 10,
                aspectRatioBias: 1.25f,
                minRoomWidth: 6,
                minRoomHeight: 5,
                roomMargin: 1,
                corridorWidth: 1);

            var partitioner = new BspPartitioner();

            BspGenerationResult result = partitioner.Generate(config);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.NodeCount, Is.EqualTo(3));
            Assert.That(result.Leaves.Count, Is.EqualTo(2));
            Assert.That(result.Root.Split.HasValue, Is.True);
            Assert.That(result.Root.Split.Value, Is.EqualTo(SplitOrientation.Vertical));
        }

        [Test]
        public void Generate_OnlyHorizontalSplitIsPossible_SplitsHorizontally()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 12,
                mapHeight: 25,
                seed: 12345,
                maxDepth: 4,
                minLeafWidth: 12,
                minLeafHeight: 10,
                aspectRatioBias: 1.25f,
                minRoomWidth: 6,
                minRoomHeight: 5,
                roomMargin: 1,
                corridorWidth: 1);

            var partitioner = new BspPartitioner();

            BspGenerationResult result = partitioner.Generate(config);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.NodeCount, Is.EqualTo(3));
            Assert.That(result.Leaves.Count, Is.EqualTo(2));
            Assert.That(result.Root.Split.HasValue, Is.True);
            Assert.That(result.Root.Split.Value, Is.EqualTo(SplitOrientation.Horizontal));
        }

        [Test]
        public void Generate_RegionTooSmallForAnySplit_ReturnSingleLeaf()
        {
            var config = new DungeonGenerationConfig(
                mapWidth: 20,
                mapHeight: 15,
                seed: 12345,
                maxDepth: 4,
                minLeafWidth: 12,
                minLeafHeight: 10,
                aspectRatioBias: 1.25f,
                minRoomWidth: 6,
                minRoomHeight: 5,
                roomMargin: 1,
                corridorWidth: 1);

            var partitioner = new BspPartitioner();

            BspGenerationResult result = partitioner.Generate(config);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.NodeCount, Is.EqualTo(1));
            Assert.That(result.Leaves.Count, Is.EqualTo(1));
            Assert.That(result.MaxReachedDepth, Is.EqualTo(0));
            Assert.That(result.Root.IsLeaf, Is.True);
            Assert.That(result.Root.Split.HasValue, Is.False);
            Assert.That(result.Leaves[0], Is.SameAs(result.Root));
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

        private static string BuildTreeSignature(BspNode root)
        {
            // StringBuilder() mantiene un buffer modificabile e non crea nuove stringhe temporanee ogni volta.
            var builder = new StringBuilder();
            AppendNodeSignature(root, builder);
            return builder.ToString();
        }

        private static void AppendNodeSignature(
            BspNode node,
            StringBuilder builder)
        {
            if (node == null)
            {
                builder.Append("#;");
                return;
            }

            builder
                .Append(node.Depth).Append(',')
                .Append(node.Bounds.x).Append(',')
                .Append(node.Bounds.y).Append(',')
                .Append(node.Bounds.width).Append(',')
                .Append(node.Bounds.height).Append(',')
                .Append(node.Split.HasValue
                    ? node.Split.Value.ToString()
                    : "Leaf")
                .Append(';');

            AppendNodeSignature(node.Left, builder);
            AppendNodeSignature(node.Right, builder);
        }      

        private static void AssertValidHierarchy(BspNode node)
        {
            if (node.IsLeaf)
            {
                Assert.That(node.Left, Is.Null);
                Assert.That(node.Right, Is.Null);
                Assert.That(node.Split.HasValue, Is.False);
                return;
            }

            Assert.That(node.Left, Is.Not.Null);
            Assert.That(node.Right, Is.Not.Null);
            Assert.That(node.Split.HasValue, Is.True);

            BspNode firstChild = node.Left;
            BspNode secondChild = node.Right;

            AssertChildIsInsideParent(node, firstChild);
            AssertChildIsInsideParent(node, secondChild);

            Assert.That(
                firstChild.Bounds.Overlaps(secondChild.Bounds), 
                Is.False, 
                "Siblings regions overlap.");

            long parentArea = (long)node.Bounds.width * node.Bounds.height;
            long childrenArea = 
                (long)firstChild.Bounds.width * firstChild.Bounds.height +
                (long)secondChild.Bounds.width * secondChild.Bounds.height;

            Assert.That(childrenArea, Is.EqualTo(parentArea));
            Assert.That(firstChild.Depth, Is.EqualTo(node.Depth + 1));
            Assert.That(secondChild.Depth, Is.EqualTo(node.Depth + 1));

            AssertValidHierarchy(firstChild);
            AssertValidHierarchy(secondChild);
        }  

        private static void AssertChildIsInsideParent(BspNode parent, BspNode child)
        {
            Assert.That(child.Bounds.xMin, Is.GreaterThanOrEqualTo(parent.Bounds.xMin));
            Assert.That(child.Bounds.yMin, Is.GreaterThanOrEqualTo(parent.Bounds.yMin));
            Assert.That(child.Bounds.xMax, Is.LessThanOrEqualTo(parent.Bounds.xMax));
            Assert.That(child.Bounds.yMax, Is.LessThanOrEqualTo(parent.Bounds.yMax));
        }
    }
}