using Level;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorScripts.Editor.PackageSpawner
{
    [CustomPropertyDrawer(typeof(SpawnItem))]
    public class PackageSpawnItemEditor : PropertyDrawer
    {
        private const string UxmlPath = "Assets/Scripts/EditorScripts/Editor/PackageSpawner/PackageSpawnItemTemplate.uxml";
        private const string StylesheetPath = "Assets/Scripts/EditorScripts/Editor/PackageSpawner/PackageSpawnItemStyles.uss";
        private readonly VisualTreeAsset _visualTree;
        private readonly StyleSheet _stylesheet;

        public PackageSpawnItemEditor()
        {
            _visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            _stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylesheetPath);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var tree = _visualTree.CloneTree(property.propertyPath);
            tree.styleSheets.Add(_stylesheet);

            UpdateMaxSpawns(property.FindPropertyRelative("HasMaxSpawns").boolValue, tree);
            tree.Q<ObjectField>().objectType = typeof(GameObject);
            tree.Q<Toggle>("hasMaxSpawns")
                .RegisterCallback<ChangeEvent<bool>>(evt => UpdateMaxSpawns(evt.newValue, tree));
            tree.Q<IntegerField>("maxSpawns").RegisterCallback<ChangeEvent<int>>(evt =>
            {
                if (evt.newValue < 0)
                {
                    tree.Q<IntegerField>("maxSpawns").value = 0;
                }
            });
            tree.Q<IntegerField>("priority").RegisterCallback<ChangeEvent<int>>(evt =>
            {
                if (evt.newValue < 0)
                {
                    tree.Q<IntegerField>("priority").value = 0;
                }
            });

            return tree;
        }

        private static void UpdateMaxSpawns(bool enabled, VisualElement tree)
        {
            tree.Q<IntegerField>("maxSpawns").SetEnabled(enabled);
        }
    }
}
