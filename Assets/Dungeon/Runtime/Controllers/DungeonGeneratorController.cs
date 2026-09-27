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

        private BspGenerationResult lastResult;

        /// <summary>Ultimo risultato generato, disponibile soltanto in memoria.</summary>
        public BspGenerationResult LastResult => lastResult;

        /// <summary>Genera un nuovo albero usando il profilo assegnato.</summary>
        [ContextMenu("Generate BSP And Rooms")]
        public void Generate()
        {
            if (profile == null)
            {
                lastResult = null;

                Debug.LogError(
                    "Assign a DungeonGenerationProfile before generating.",
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

            Debug.Log(
                $"BSP and rooms generated: {lastResult.NodeCount} nodes, " + 
                $"{lastResult.Leaves.Count} leaves, " + 
                $"{rooms.Count} rooms, " +
                $"maximum depth: {lastResult.MaxReachedDepth}.",
                this);
        }
    }
}