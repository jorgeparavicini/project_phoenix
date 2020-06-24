using Data;
using EventArgs;
using Extensions;
using Level.Packages;
using Score;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Level.Containers
{
    /// <summary>
    /// A container for handling Number Packages.
    /// </summary>
    public class NumberContainer : Container
    {
        private TextMeshProUGUI _displayText;
        [SerializeField] private NumberBase _base = NumberBase.Binary;
        [SerializeField] private int _value;
        [SerializeField] private int _width;

        /// <summary>
        /// The Text Object above the container.
        /// <remarks>The value gets cached after the first time getting it.</remarks>
        /// </summary>
        public TextMeshProUGUI DisplayText =>
            _displayText ? _displayText : _displayText = GetComponentInChildren<TextMeshProUGUI>();
        
        /// <summary>
        /// The value to be displayed in decimal format.
        /// Only positive values are allowed
        /// </summary>
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

        /// <summary>
        /// The Base in which the <see cref="Value"/> should be displayed at
        /// </summary>
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

        /// <summary>
        /// The amount of characters the final string should be displayed with.
        /// Useful for creating a padded string.
        /// <example>
        /// A binary value of 11100 will be converted to 0001 1100
        /// </example>
        /// <remarks>If the width is set to a value that could not be used to represent <see cref="Value"/>
        /// in <see cref="Base"/>, then the Width will be adjusted to the minimal width needed to represent the
        /// <see cref="Value"/></remarks>
        /// </summary>
        public int Width
        {
            get => _width;
            set
            {
                var old = _width;
                _width = value;
                if (old != _width)
                    OnValidate();
            }
        }

        protected override void OnPackageReleased(object sender, PackageEventArgs e)
        {
            if (!(e.Package is NumberPackage package))
            {
                Debug.Log($"Got unrecognized package in color container: {e.Package}");
                return;
            }

            if (package.Value == Value) ScoreManager.AddScore(package.Score);

            package.Destroy();
        }

        /// <summary>
        /// Updates the <see cref="DisplayText"/> to the <see cref="Value"/> in the correct <see cref="Base"/>
        /// respecting the <see cref="Width"/>
        /// </summary>
        private void UpdateDisplay()
        {
            #if UNITY_EDITOR
            Undo.RecordObject(DisplayText, "Changed Container Text");
            #endif
            DisplayText.text = Value.ToString(Base, _width < 0 ? (int?) null : _width);
        }

        /// <summary>
        /// Called automatically whenever a SerializedField value has changed.
        /// It validates the <see cref="Width"/> and the <see cref="Value"/>
        /// </summary>
        public void OnValidate()
        {
            _width = _width < 0 ? -1 : Mathf.Max(_width, 
                                                 Mathf.FloorToInt(Mathf.Log(_value, (int) Base)) + 1
                                                 );
            _value = Mathf.Max(0, _value);
            UpdateDisplay();
        }
    }
}
