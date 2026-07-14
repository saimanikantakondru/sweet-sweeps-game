using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SweetSweeps.Gameplay.Level
{
    public class LevelEditorHelper : MonoBehaviour
    {
        [Header("Scaffold Target")]
        [SerializeField] private string scaffoldName = "LevelContent_New";
        [SerializeField] private int defaultPlatformSlots = 2;

#if UNITY_EDITOR
        [Button("Scaffold LevelContentRoot Under This Object")]
        private void ScaffoldRoot()
        {
            var rootGo = new GameObject(scaffoldName);
            rootGo.transform.SetParent(transform, false);

            var content = rootGo.AddComponent<LevelContentRoot>();

            CreateChild(rootGo.transform, "PlayerSpawn");
            CreateChild(rootGo.transform, "Tilemap_Ground");
            CreateChild(rootGo.transform, "Tilemap_Platforms");
            CreateChild(rootGo.transform, "Tilemap_Hazards");
            CreateChild(rootGo.transform, "Tilemap_Decor");

            var platformsRoot = CreateChild(rootGo.transform, "Platforms");
            for (int i = 0; i < defaultPlatformSlots; i++)
                CreateChild(platformsRoot, $"PlatformSlot_{i} (drop MovingPlatform prefab here)");

            Selection.activeObject = content;
            Debug.Log($"[LevelEditorHelper] Scaffolded '{scaffoldName}'. Drop tilemaps into the layer GameObjects, " +
                      "then drag the root into Project as a prefab and assign it to a LevelPresetSO.");
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
#endif
    }
}