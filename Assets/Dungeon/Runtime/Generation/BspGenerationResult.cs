using System;
using System.Collections.Generic;

namespace Tesi.Dungeon
{
    /// <summary>Contiene l'esito completo di una generazione BSP.</summary>
    public sealed class BspGenerationResult
    {
        public BspNode Root { get; }
        public IReadOnlyList<BspNode> Leaves { get; }
        public int NodeCount { get; }
        public int MaxReachedDepth { get; }
        public ConfigValidationResult Validation { get; }
        public bool IsSuccess => Validation.IsValid && Root != null;

        private BspGenerationResult(
            BspNode root,
            IReadOnlyList<BspNode> leaves,
            int nodeCount,
            int maxReachedDepth,
            ConfigValidationResult validation)
        {
            Root = root;
            Leaves = new List<BspNode>(leaves).AsReadOnly();
            NodeCount = nodeCount;
            MaxReachedDepth = maxReachedDepth;
            Validation = validation;
        }

        public static BspGenerationResult Failed(
            ConfigValidationResult validation)
        {
            return new BspGenerationResult(
                null, 
                Array.Empty<BspNode>(),
                0,
                0,
                validation);
        }

        public static BspGenerationResult Succeeded(
            BspNode root,
            IReadOnlyList<BspNode> leaves,
            int nodeCount,
            int maxReachedDepth)
        {
            return new BspGenerationResult(
                root,
                leaves,
                nodeCount,
                maxReachedDepth,
                new ConfigValidationResult(Array.Empty<string>()));
        }

        
    }
}