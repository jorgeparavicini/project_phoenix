using Level;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorScripts.Editor.PackageSpawner
{
    [CustomEditor(typeof(RandomTimePackageSpawner))]
    [CanEditMultipleObjects]
    public class RandomTimePackageSpawnerEditor : PackageSpawnerEditor
    {
        private const string UxmlPath = "Assets/Scripts/EditorScripts/Editor/PackageSpawner/RandomTimePackageSpawnerTemplate.uxml";
        private VisualTreeAsset _randomVisualTree;

        protected override void OnEnable()
        {
            base.OnEnable();
            _randomVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();
            _randomVisualTree.CloneTree(root);

            UpdateMinMaxValues(root, ((RandomTimePackageSpawner)target).Range);

            root.Query<Label>(className: "range-value").ForEach(label => label.SetEnabled(false));
            root.Q<MinMaxSlider>().RegisterCallback<ChangeEvent<Vector2>>(evt => UpdateMinMaxValues(root, evt.newValue));

            return root;
        }

        private static void UpdateMinMaxValues(VisualElement container, Vector2 range)
        {
            container.Q<Label>("min").text = $"Min: {range.x}";
            container.Q<Label>("max").text = $"Max: {range.y}";
        }
    }
}
