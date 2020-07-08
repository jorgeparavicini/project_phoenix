using System.Collections.Generic;
using System.Linq;
using Phoenix.Data;
using Phoenix.Extensions;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Phoenix.Level.Packages
{
    public class NumberPackage : Package
    {
        private List<TextMeshProUGUI> _displayTexts;
        [SerializeField] private NumberBase _base;
        [SerializeField] private int _value;
        [SerializeField] private int _width;
        public int Score;

        public int Value
        {
            get => _value;
            set
            {
                var old = _value;
                _value = value;
                if (old != value) OnValidate();
            }
        }

        public NumberBase Base
        {
            get => _base;
            set
            {
                var old = _base;
                _base = value;
                if (old != value) OnValidate();
            }
        }

        // Values less than 0 become null
        public int Width
        {
            get => _width;
            set
            {
                var old = _width;
                _width = value;
                if (old != _width) OnValidate();
            }
        }

        public List<TextMeshProUGUI> DisplayTexts => _displayTexts = GetComponentsInChildren<TextMeshProUGUI>().ToList();

        private void OnEnable()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            DisplayTexts.ForEach(t =>
            {
                #if UNITY_EDITOR
                Undo.RecordObject(t, "Changed Package Text");
                #endif
                t.text = Value.ToString(Base, _width < 0 ? (int?)null : _width);
            });
        }

        private void OnValidate()
        {
            _width = _width < 0 ? -1 : Mathf.Max(_width, 
                                                 Mathf.FloorToInt(Mathf.Log(_value, (int) Base)) + 1
                                                );
            _value = Mathf.Max(0, _value);
            UpdateDisplay();
        }
    }
}
