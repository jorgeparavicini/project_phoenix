﻿using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Phoenix.Editor.PackageSpawner
{
    [CustomEditor(typeof(Phoenix.Level.PackageSpawner), true)]
    [CanEditMultipleObjects]
    public class PackageSpawnerEditor : UnityEditor.Editor
    {
        private const string UxmlPath = "Assets/Scripts/Editor/PackageSpawner/PackageSpawnerTemplate.uxml";
        private const string StylesheetPath = "Assets/Scripts/Editor/PackageSpawner/PackageSpawnerStyles.uss";
        private VisualElement _rootElement;
        private VisualTreeAsset _visualTree;

        protected virtual void OnEnable()
        {
            _rootElement = new VisualElement();
            _visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);

            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylesheetPath);
            _rootElement.styleSheets.Add(stylesheet);
        }

        public override VisualElement CreateInspectorGUI()
        {
            _rootElement.Clear();
            _visualTree.CloneTree(_rootElement);

            _rootElement.Q<ObjectField>().objectType = typeof(Transform);
            return _rootElement;
        }
    }
}
