using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
            }

            if (GUILayout.Button("Clear Dungeon"))
            {
                controller.Clear();

                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);

                SceneView.RepaintAll();
            }
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

    }
}