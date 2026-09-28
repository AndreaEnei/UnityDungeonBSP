using System.Collections.Generic;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Collega il generatore BSP alla scena Unity.</summary>
    public sealed class DungeonGeneratorController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Profilo contenente i parametri della generazione BSP.")]
        private DungeonGenerationProfile profile;

        [SerializeField]
        [Tooltip("Renderer che visualizza la griglia sulle Tilemap.")]
        private DungeonTilemapRenderer tilemapRenderer;

        private BspGenerationResult lastResult;

        private IReadOnlyList<DungeonCorridor> lastCorridors;

        /// <summary>Ultimo risultato generato, disponibile soltanto in memoria.</summary>
        public BspGenerationResult LastResult => lastResult;

        /// <summary>Corridoi prodotti dall'ultima generazione valida.</summary>
        public IReadOnlyList<DungeonCorridor> LastCorridors => lastCorridors;

        private DungeonGrid lastGrid;

        /// <summary>Griglia prodotta dall'ultima generazione valida.</summary>
        public DungeonGrid LastGrid => lastGrid;

        /// <summary>Genera un nuovo albero usando il profilo assegnato.</summary>
        [ContextMenu("Generate Dungeon")]
        public void Generate()
        {
            lastCorridors = null;
            lastGrid = null;

            if (tilemapRenderer != null)
            {
                tilemapRenderer.Clear();
            }

            if (profile == null)
            {
                lastResult = null;

                Debug.LogError(
                    "Assign a DungeonGenerationProfile before generating.",
                    this);

                return;
            }

            if (tilemapRenderer == null)
            {
                lastResult = null;

                Debug.LogError(
                    "Assign a DungeonTilemapRenderer before generating.",
                    this);

                return;
            }

            DungeonGenerationConfig config = profile.ToConfig();
            var partitioner = new BspPartitioner();

            lastResult = partitioner.Generate(config);

            if (!lastResult.IsSuccess)
            {
                foreach (string error in lastResult.Validation.Errors)
                {
                    Debug.LogError(error, this);
                }

                return;
            }

            var roomPlacer = new RoomPlacer();

            IReadOnlyList<RectInt> rooms = roomPlacer.PlaceRooms(
                lastResult.Leaves,
                config);

            var corridorConnector = new CorridorConnector();

            lastCorridors = corridorConnector.Connect(
                lastResult.Root,
                config);

            var rasterizer = new DungeonRasterizer();

            lastGrid = rasterizer.RasterizeFloor(
                config,
                rooms,
                lastCorridors);

            rasterizer.BuildWalls(lastGrid);

            tilemapRenderer.Render(lastGrid);

            Debug.Log(
                $"Dungeon generated: {lastResult.NodeCount} nodes, " + 
                $"{lastResult.Leaves.Count} leaves, " + 
                $"{rooms.Count} rooms, " +
                $"{lastCorridors.Count} corridors, " +
                $"maximum depth: {lastResult.MaxReachedDepth}.",
                this);
        }
    }
}