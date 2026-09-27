using UnityEngine;

namespace Tesi.Dungeon
{
    [CreateAssetMenu(
        fileName = "NewDungeonGenerationProfile",
        menuName = "Dungeon/Generation Profile"
    )]
    public sealed class DungeonGenerationProfile : ScriptableObject
    {
        [Header("Map")]

        [SerializeField, Min(1)]
        [Tooltip("Larghezza totale della mappa in celle.")]
        private int mapWidth = 80;

        [SerializeField, Min(1)]
        [Tooltip("Altezza totale della mappa in celle.")]
        private int mapHeight = 50;

        [Header("Generation")]

        [SerializeField, Min(0)]
        [Tooltip("Seed usato per ottenere una generazione riproducibile.")]
        private int seed = 12345;

        [SerializeField, Min(0)]
        [Tooltip("Profondita massima dell'albero BSP.")]
        private int maxDepth = 4;

        [SerializeField, Min(2)]
        [Tooltip("Larghezza minima consentita per una foglia BSP.")]
        private int minLeafWidth = 12;

        [SerializeField, Min(2)]
        [Tooltip("Altezza minima consentita per una foglia BSP.")]
        private int minLeafHeight = 10;

        [SerializeField, Min(1f)]
        [Tooltip("Rapporto oltre il quale viene preferito un orientamento di taglio.")]
        private float aspectRatioBias = 1.25f;

        [Header("Rooms")]

        [SerializeField, Min(1)]
        [Tooltip("Larghezza minima di ogni stanza in celle.")]
        private int minRoomWidth = 6;

        [SerializeField, Min(1)]
        [Tooltip("Altezza minima di ogni stanza in celle.")]
        private int minRoomHeight = 5;

        [SerializeField, Min(0)]
        [Tooltip("Spazio minimo mantenuto tra la stanza e i bordi della foglia BSP.")]
        private int roomMargin = 1;

        [Header("Corridors")]

        [SerializeField, Min(1)]
        [Tooltip("Larghezza dei corridoi in celle.")]
        private int corridorWidth = 1;

        /// <summary>Crea uno snapshot dei parametri per una generazione.</summary>
        public DungeonGenerationConfig ToConfig()
        {
            return new DungeonGenerationConfig(
                mapWidth,
                mapHeight,
                (uint)seed,
                maxDepth,
                minLeafWidth,
                minLeafHeight,
                aspectRatioBias,
                minRoomWidth,
                minRoomHeight,
                roomMargin,
                corridorWidth
            );
        }

        private void OnValidate()
        {
            mapWidth = Mathf.Max(1, mapWidth);
            mapHeight = Mathf.Max(1, mapHeight);
            seed = Mathf.Max(0, seed);
            maxDepth = Mathf.Max(0, maxDepth);
            minLeafWidth = Mathf.Max(2, minLeafWidth);
            minLeafHeight = Mathf.Max(2, minLeafHeight);
            aspectRatioBias = Mathf.Max(1f, aspectRatioBias);
            minRoomWidth = Mathf.Max(1, minRoomWidth);
            minRoomHeight = Mathf.Max(1, minRoomHeight);
            roomMargin = Mathf.Max(0, roomMargin);
            corridorWidth = Mathf.Max(1, corridorWidth);
        }
    }
}
