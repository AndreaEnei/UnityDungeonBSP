using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Disegna radice e foglie BSP nella Scene view.</summary>
    [RequireComponent(typeof(DungeonGeneratorController))]
    public sealed class BspGizmoDrawer : MonoBehaviour
    {
        private static readonly Color RootColor = new Color(1f, 0.8f, 0.2f, 1f);
        private static readonly Color LeafColor = new Color(0.2f, 0.9f, 1f, 1f);
        private static readonly Color RoomColor = new Color(0.3f, 1f, 0.3f, 1f);

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

            Gizmos.color = LeafColor;
            for (int i = 0; i < result.Leaves.Count; i++)
            {
                DrawBounds(result.Leaves[i].Bounds);
            }

            Gizmos.color = RoomColor;
            for (int i = 0; i < result.Leaves.Count; i++)
            {
                BspNode leaf = result.Leaves[i];

                if (leaf.Room.HasValue)
                {
                    DrawBounds(leaf.Room.Value);
                }
            }

            Gizmos.color = RootColor;
            DrawBounds(result.Root.Bounds);
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