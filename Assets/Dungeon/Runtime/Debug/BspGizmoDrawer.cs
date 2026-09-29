using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Disegna partizioni, stanze e corridoi nella Scene view.</summary>
    [RequireComponent(typeof(DungeonGeneratorController))]
    public sealed class BspGizmoDrawer : MonoBehaviour
    {
        private static readonly Color RootColor = new Color(1f, 0.8f, 0.2f, 1f);
        private static readonly Color LeafColor = new Color(0.2f, 0.9f, 1f, 1f);
        private static readonly Color RoomColor = new Color(0.3f, 1f, 0.3f, 1f);
        private static readonly Color CorridorColor = new Color(1f, 0.2f, 0.8f, 1f);

        [Header("Visibility")]

        [SerializeField]
        private bool showRoot = true;

        [SerializeField]
        private bool showLeaves = true;

        [SerializeField]
        private bool showRooms = true;

        [SerializeField]
        private bool showCorridors = true;

        private DungeonGeneratorController controller;

        private void OnDrawGizmosSelected()
        {
            if (controller == null)
            {
                controller = GetComponent<DungeonGeneratorController>();
            }

            BspGenerationResult result = controller.LastResult;

            if (result == null || !result.IsSuccess)
            {
                return;
            }

            if (showRoot)
            {
                Gizmos.color = RootColor;
                DrawBounds(result.Root.Bounds);
            }

            if (showLeaves)
            {
                Gizmos.color = LeafColor;

                foreach (var leaf in result.Leaves)
                {
                    DrawBounds(leaf.Bounds);
                }
            }

            if (showRooms)
            {
                Gizmos.color = RoomColor;

                foreach (var leaf in result.Leaves)
                {
                    if (leaf.Room.HasValue)
                    {
                        DrawBounds(leaf.Room.Value);
                    }
                }
            }

            if (showCorridors && controller.LastCorridors != null)
            {
                Gizmos.color = CorridorColor;

                foreach (var corridor in controller.LastCorridors)
                {
                    DrawBounds(corridor.FirstSegment);

                    if (corridor.SecondSegment.HasValue)
                    {
                        DrawBounds(corridor.SecondSegment.Value);
                    }
                }
            }

        }

        private static void DrawBounds(RectInt bounds)
        {
            var center = new Vector3(
                (bounds.xMin + bounds.xMax) / 2f,
                (bounds.yMin + bounds.yMax) / 2f,
                0f);

            var size = new Vector3(
                bounds.width,
                bounds.height,
                0f);

            Gizmos.DrawWireCube(center, size);
        }
    }
}