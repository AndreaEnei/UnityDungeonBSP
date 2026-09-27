using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Costruisce un albero BSP da una configurazione valida.</summary>
    public sealed class BspPartitioner
    {
        public BspGenerationResult Generate(
            DungeonGenerationConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ConfigValidationResult validation = config.Validate();

            if (!validation.IsValid)
            {
                return BspGenerationResult.Failed(validation);
            }

            var random = new DeterministicRandom(config.Seed);

            var root = new BspNode(
                new RectInt(0, 0, config.MapWidth, config.MapHeight),
                0);

            SplitRecursive(root, config, random);

            var leaves = new List<BspNode>();
            int nodeCount = 0;
            int maxReachedDepth = 0;

            CollectTreeData(
                root,
                leaves,
                ref nodeCount,
                ref maxReachedDepth); 

            return BspGenerationResult.Succeeded(
                root,
                leaves,
                nodeCount,
                maxReachedDepth);
        }

        private static void SplitRecursive(
            BspNode node,
            DungeonGenerationConfig config,
            DeterministicRandom random)
        {
            if (node.Depth >= config.MaxDepth)
            {
                return;
            }

            RectInt bounds = node.Bounds;

            bool canSplitVertically = bounds.width >= config.MinLeafWidth * 2;
            bool canSplitHorizontally = bounds.height >= config.MinLeafHeight * 2;

            if (!canSplitVertically && !canSplitHorizontally)
            {    
                return;
            }

            SplitOrientation orientation = ChooseSplitOrientation(
                bounds,
                canSplitVertically,
                canSplitHorizontally,
                config.AspectRatioBias,
                random);

            RectInt firstChildBounds;
            RectInt secondChildBounds;

            if (orientation == SplitOrientation.Vertical)
            {
                int minCut = config.MinLeafWidth;
                int maxCutInclusive = bounds.width - minCut;

                int cut = random.NextInt(minCut, maxCutInclusive + 1);

                firstChildBounds = new RectInt(
                    bounds.xMin,
                    bounds.yMin,
                    cut,
                    bounds.height);

                secondChildBounds = new RectInt(
                    bounds.xMin + cut,
                    bounds.yMin,
                    bounds.width - cut,
                    bounds.height);
            }
            else 
            {
                int minCut = config.MinLeafHeight;
                int maxCutInclusive = bounds.height - minCut;

                int cut = random.NextInt(minCut, maxCutInclusive + 1);

                firstChildBounds = new RectInt(
                    bounds.xMin,
                    bounds.yMin,
                    bounds.width,
                    cut);

                secondChildBounds = new RectInt(
                    bounds.xMin,
                    bounds.yMin + cut,
                    bounds.width,
                    bounds.height - cut);
            }

            var firstChild = new BspNode(
                firstChildBounds,
                node.Depth + 1);
            var secondChild = new BspNode(
                secondChildBounds,
                node.Depth + 1);

            AssertValidSplit(node, firstChild, secondChild);

            node.SetChildren(firstChild, secondChild, orientation);

            SplitRecursive(firstChild, config, random);
            SplitRecursive(secondChild, config, random);
        }

        private static SplitOrientation ChooseSplitOrientation(
            RectInt bounds,
            bool canSplitVertically,
            bool canSplitHorizontally,
            float aspectRatioBias,
            DeterministicRandom random)
        {
            if (canSplitVertically && !canSplitHorizontally)
            {
                return SplitOrientation.Vertical;
            }

            if (!canSplitVertically && canSplitHorizontally)
            {
                return SplitOrientation.Horizontal;
            }

            if (!canSplitVertically && !canSplitHorizontally)
            {
                throw new InvalidOperationException(
                    "Cannot choose an orientation when no split is valid.");
            }

            float widthToHeight = (float)bounds.width / bounds.height;
            float heightToWidth = (float)bounds.height / bounds.width;

            if (bounds.width > bounds.height && widthToHeight >= aspectRatioBias)
            {
                return SplitOrientation.Vertical;
            }

            if (bounds.height > bounds.width && heightToWidth >= aspectRatioBias)
            {
                return SplitOrientation.Horizontal;
            }

            return random.NextBool() 
                ? SplitOrientation.Vertical
                : SplitOrientation.Horizontal;
        }

        private static void AssertValidSplit(
            BspNode parent,
            BspNode firstChild,
            BspNode secondChild)
        {
            RectInt parentBounds = parent.Bounds;
            RectInt firstChildBounds = firstChild.Bounds;
            RectInt secondChildBounds = secondChild.Bounds;

            Debug.Assert(
                firstChildBounds.width > 0 && firstChildBounds.height > 0,
                "The first child must have positive dimensions.");
            
            Debug.Assert(
                secondChildBounds.width > 0 && secondChildBounds.height > 0,
                "The second child must have positive dimensions.");
            
            Debug.Assert(
                Contains(parentBounds, firstChildBounds),
                "The first child must be contained in its parent.");

            Debug.Assert(
                Contains(parentBounds, secondChildBounds),
                "The second child must be contained in its parent.");

            Debug.Assert(
                !firstChildBounds.Overlaps(secondChildBounds),
                "Siblings regions must not overlap.");
            
            long parentArea = (long)parentBounds.width * parentBounds.height;
            long childrenArea = 
                (long)firstChildBounds.width * firstChildBounds.height + 
                (long)secondChildBounds.width * secondChildBounds.height;

            Debug.Assert(
                childrenArea == parentArea,
                "The children must cover the complete parent area.");

            Debug.Assert(
                firstChild.Depth == parent.Depth + 1,
                "The first child has an invalid depth.");

            Debug.Assert(
                secondChild.Depth == parent.Depth + 1,
                "The second child has an invalid depth.");
        }

        private static bool Contains(
            RectInt outer,
            RectInt inner)
        {
            return inner.xMin >= outer.xMin &&
                inner.yMin >= outer.yMin &&
                inner.xMax <= outer.xMax &&
                inner.yMax <= outer.yMax;
        }

        private static void CollectTreeData(
            BspNode node,
            List<BspNode> leaves,
            ref int nodeCount,
            ref int maxReachedDepth)
        {
            nodeCount++;

            if (node.Depth > maxReachedDepth)
            {
                maxReachedDepth = node.Depth;
            }

            if (node.IsLeaf)
            {
                leaves.Add(node);
                return;
            }

            CollectTreeData(
                node.Left,
                leaves,
                ref nodeCount,
                ref maxReachedDepth);

            CollectTreeData(
                node.Right,
                leaves,
                ref nodeCount,
                ref maxReachedDepth);
        }
    }
}