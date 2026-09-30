using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;

namespace Tesi.Dungeon.Editor
{
    [CustomEditor(typeof(DungeonGeneratorController))]
    public sealed class DungeonGeneratorControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var controller = (DungeonGeneratorController)target;

            EditorGUILayout.Space();

            bool canGenerate = DrawValidation(controller);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!canGenerate))
            {
                if (GUILayout.Button("Generate Dungeon"))
                {
                    controller.Generate();

                    EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);

                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("New Seed And Generate"))
                {
                    DungeonGenerationProfile profile = controller.Profile;

                    Undo.RecordObject(profile, "Change Dungeon Seed");

                    int newSeed = Guid.NewGuid().GetHashCode() & int.MaxValue;

                    profile.SetSeed(newSeed);

                    EditorUtility.SetDirty(profile);

                    controller.Generate();

                    EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);

                    SceneView.RepaintAll();
                }
            }

            if (GUILayout.Button("Clear Dungeon"))
            {
                controller.Clear();

                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);

                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();

            DrawConnectivityResult(controller);

            DrawMetrics(controller);
        }

        private static bool DrawValidation(DungeonGeneratorController controller)
        {
            bool canGenerate = true;

            if (controller.Profile == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a DungeonGenerationProfile before generating.",
                    MessageType.Error);

                canGenerate = false;
            }
            else
            {
                ConfigValidationResult validation = controller.Profile.ToConfig().Validate();

                if (!validation.IsValid)
                {
                    for (int i = 0; i < validation.Errors.Count; i++)
                    {
                        EditorGUILayout.HelpBox(
                            $"Error {i + 1}: {validation.Errors[i]}",
                            MessageType.Error);
                    }

                    canGenerate = false;
                }
            }

            if (!controller.HasTilemapRenderer)
            {
                EditorGUILayout.HelpBox(
                    "Assign a DungeonTilemapRenderer.",
                    MessageType.Error);

                canGenerate = false;
            }

            if (canGenerate)
            {
                EditorGUILayout.HelpBox(
                    "Configuration is valid.",
                    MessageType.Info);
            }

            return canGenerate;
        }

        private static void DrawConnectivityResult(DungeonGeneratorController controller)
        {
            DungeonConnectivityResult result = controller.LastConnectivityResult;

            if (result == null)
            {
                EditorGUILayout.HelpBox(
                    "No dungeon has been generated yet.",
                    MessageType.Info);
                    
                return;
            }

            if (result.IsConnected)
            {
                EditorGUILayout.HelpBox(
                    "Dungeon connected: " +
                    $"{result.ReachableFloorCells}/" +
                    $"{result.TotalFloorCells} floor cells are reachable.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Dungeon disconnected: " +
                    $"{result.UnreachableFloorCells} of " +
                    $"{result.TotalFloorCells} floor cells are unreachable.",
                    MessageType.Error);
            }
        }

        private static void DrawMetrics(DungeonGeneratorController controller)
        {
            DungeonGenerationMetrics metrics = controller.LastMetrics;

            if (metrics == null)
            {
                return;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Last Generation Metrics", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Nodes", metrics.NodeCount.ToString());
            EditorGUILayout.LabelField("Leaves", metrics.LeafCount.ToString());
            EditorGUILayout.LabelField("Rooms", metrics.RoomCount.ToString());
            EditorGUILayout.LabelField("Corridors", metrics.CorridorCount.ToString());
            EditorGUILayout.LabelField("Maximum Depth", metrics.MaxReachedDepth.ToString());

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Floor Cells", metrics.FloorCellCount.ToString());
            EditorGUILayout.LabelField("Wall Cells", metrics.WallCellCount.ToString());
            EditorGUILayout.LabelField("Empty Cells", metrics.EmptyCellCount.ToString());
            EditorGUILayout.LabelField("Total Cells", metrics.TotalCellCount.ToString());
            EditorGUILayout.LabelField("Floor Coverage", $"{metrics.FloorCoveragePercentage:F2}%");

        }
    }
}