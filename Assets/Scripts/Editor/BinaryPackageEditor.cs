using Data;
using Phoenix.Data;
using Phoenix.Level.Packages;
using UnityEditor;

namespace Phoenix.Editor
{
    [CustomEditor(typeof(NumberPackage))]
    [CanEditMultipleObjects]
    public class BinaryPackageEditor : UnityEditor.Editor
    {
        private NumberPackage _target;

        private void OnEnable()
        {
            _target = (NumberPackage)target;
        }

        public override void OnInspectorGUI()
        {
            _target.Score = EditorGUILayout.IntField("Score", _target.Score);
            EditorGUILayout.Space(20f);
            _target.Base = (NumberBase)EditorGUILayout.EnumPopup("Base", _target.Base);
            _target.Value = EditorGUILayout.IntField("Value", _target.Value);
            _target.Width = EditorGUILayout.IntField("Width", _target.Width);
        }
    }
}
