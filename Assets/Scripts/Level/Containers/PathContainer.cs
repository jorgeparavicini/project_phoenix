using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EventArgs;
using JetBrains.Annotations;
using Level.Packages;
using Level.Path;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Level.Containers
{
    public class PathContainer : Container
    {
        #region Fields

        private readonly List<PathElement> _userElements = new List<PathElement>();
        private PathPackage _controlledPackage;
        private PathElement _controlledElement;

#pragma warning disable 649

        // Actual size will be doubled
        [SerializeField] private Vector2 _gridSize = new Vector2(2, 2);
        [SerializeField] private int _gridColumns = 2;
        [SerializeField] private int _gridRows = 2;
        [SerializeField] private PathElement _startElement;
        [SerializeField] private PathElement _endElement;
        [SerializeField] private List<PathElement> _obstacles = new List<PathElement>();

        [SerializeField] private bool _displayGrid;
        [SerializeField] private float _pathElementZOffset = 0.5f;
        [SerializeField] private float _placeDuration = 1f;

#pragma warning restore

        #endregion

        #region Properties

        public float CellWidth => _gridSize.x / _gridColumns;
        public float CellHeight => _gridSize.y / _gridRows;

        public IEnumerable<PathElement> AllPackages =>
            new List<PathElement>()
               .Concat(_obstacles)
               .Concat(_userElements)
               .Append(_startElement)
               .Append(_endElement)
               .ToList()
               .AsReadOnly();

        #endregion

        #region Unity Events

        private void Start()
        {
            SnapToGrid(_startElement);
            SnapToGrid(_endElement);
        }

        private void Update()
        {
            //_userPackages.ForEach(p => SnapToGrid(p.gameObject));

            if (_controlledPackage is null) return;


            SnapToGrid(_controlledElement, _controlledPackage.transform, _pathElementZOffset);
            _controlledElement.Valid = IsOccupied(_controlledElement.GridPoint);
        }

        private void OnDrawGizmos()
        {
            if (!_displayGrid) return;

            // Render Grid

            var position = transform.position;
            Gizmos.color = new Color(255, 204, 0);

            // Vertical Lines
            Gizmos.DrawLine(new Vector3(position.x, position.y - _gridSize.y, position.z),
                            new Vector3(position.x, position.y + _gridSize.y, position.z));
            for (var i = 0; i < _gridColumns; i++)
            {
                Gizmos.DrawLine(new Vector3(position.x - CellWidth * (i + 1), position.y - _gridSize.y, position.z),
                                new Vector3(position.x - CellWidth * (i + 1), position.y + _gridSize.y, position.z));

                Gizmos.DrawLine(new Vector3(position.x + CellWidth * (i + 1), position.y - _gridSize.y, position.z),
                                new Vector3(position.x + CellWidth * (i + 1), position.y + _gridSize.y, position.z));
            }

            // Horizontal Lines
            Gizmos.DrawLine(new Vector3(position.x - _gridSize.x, position.y, position.z),
                            new Vector3(position.x + _gridSize.x, position.y, position.z));

            for (var i = 0; i < _gridRows; i++)
            {
                Gizmos.DrawLine(new Vector3(position.x - _gridSize.x, position.y - CellHeight * (i + 1), position.z),
                                new Vector3(position.x + _gridSize.x, position.y - CellHeight * (i + 1), position.z));

                Gizmos.DrawLine(new Vector3(position.x - _gridSize.x, position.y + CellHeight * (i + 1), position.z),
                                new Vector3(position.x + _gridSize.x, position.y + CellHeight * (i + 1), position.z));
            }
        }

        #endregion

        private bool IsOccupied(GridPoint point)
        {
            return !AllPackages.Select(package => package.GridPoint)
                               .Any(p => p.Equals(point));
        }

        [CanBeNull]
        private PathElement GetElementAt(GridPoint point)
        {
            return AllPackages.FirstOrDefault(p => p.GridPoint.Equals(point));
        }

        private IEnumerable<PathElement> GetAdjacentElements(PathElement element)
        {
            if (GetElementAt(element.GridPoint.TranslateY(1)) is PathElement top) yield return top;
            if (GetElementAt(element.GridPoint.TranslateX(1)) is PathElement right) yield return right;
            if (GetElementAt(element.GridPoint.TranslateY(-1)) is PathElement bottom) yield return bottom;
            if (GetElementAt(element.GridPoint.TranslateX(-1)) is PathElement left) yield return left;
        }

        private IEnumerable<(PathElement bot, PathDirection Bottom)> GetConnectedElements(PathElement element)
        {
            foreach (var direction in element.Direction.GetFlags())
            {
                switch (direction)
                {
                    case PathDirection.Bottom:
                        if (GetElementAt(element.GridPoint.TranslateY(-1)) is PathElement bot)
                        {
                            if (bot.Direction.HasFlag(PathDirection.Top))
                                yield return (bot, PathDirection.Bottom);
                        }

                        continue;
                    case PathDirection.Left:
                        if (GetElementAt(element.GridPoint.TranslateX(-1)) is PathElement left)
                        {
                            if (left.Direction.HasFlag(PathDirection.Right))
                                yield return (left, PathDirection.Left);
                        }

                        continue;
                    case PathDirection.Right:
                        if (GetElementAt(element.GridPoint.TranslateX(1)) is PathElement right)
                        {
                            if (right.Direction.HasFlag(PathDirection.Left))
                                yield return (right, PathDirection.Right);
                        }

                        continue;
                    case PathDirection.Top:
                        if (GetElementAt(element.GridPoint.TranslateY(1)) is PathElement top)
                        {
                            if (top.Direction.HasFlag(PathDirection.Bottom))
                                yield return (top, PathDirection.Top);
                        }

                        continue;
                    case PathDirection.None:
                        continue;
                    default:
                        continue;
                }
            }
        }

        private GridPoint SnapToGrid(PathElement element, Transform origin = null, float zOffset = 0)
        {
            var point = GetGridPointForObject(origin is null ? element.transform : origin);
            var localPos = new Vector3(point.X * CellWidth, point.Y * CellHeight, zOffset);
            element.transform.position = localPos + transform.position;
            element.GridPoint = point;
            return point;
        }

        private GridPoint GetGridPointForObject(Transform element)
        {
            var pos = element.position;
            var normalized = pos - transform.position;
            var x = Mathf.RoundToInt(normalized.x / CellWidth);
            var y = Mathf.RoundToInt(normalized.y / CellHeight);
            x = Mathf.Clamp(x, -_gridColumns, _gridColumns);
            y = Mathf.Clamp(y, -_gridRows, _gridRows);
            return new GridPoint(x, y);
        }

        private void DropPackage(Package p)
        {
            if (_controlledElement is null) return;

            p.GetComponent<Renderer>().enabled = true;

            Destroy(_controlledElement.gameObject);
            _controlledPackage = null;
            _controlledElement = null;
        }

        private IEnumerator PlaceElement(PathElement element)
        {
            var passedTime = 0f;
            while (passedTime < _placeDuration)
            {
                passedTime += Time.deltaTime;
                var zPos = Mathf.Lerp(_pathElementZOffset, 0, passedTime / _placeDuration);
                var t = element.transform;
                var position = t.localPosition;
                position = new Vector3(position.x, position.y, zPos);
                t.localPosition = position;
                yield return new WaitForEndOfFrame();
            }

            element.OnPlace();
            StartCoroutine(UpdateConnection(element));
        }

        private IEnumerator UpdateConnection(PathElement newElement)
        {
            if (!newElement.Connected)
            {
                // Get Connected Elements with their direction and select only the ones that are connected to the start element;
                var connections = GetConnectedElements(newElement)
                                 .Where(element => element.bot.Connected)
                                 .Select(element => newElement.Connect(element.Bottom)).ToList();
                if (connections.Count == 0) yield break;

                while (connections.Any())
                {
                    for (var i = connections.Count - 1; i >= 0; i--)
                    {
                        if (!connections[i].MoveNext()) connections.RemoveAt(i);
                    }

                    // Wait a frame
                    yield return null;
                }
            }

            newElement.Connected = true;
            GetConnectedElements(newElement).Where(e => !e.bot.Connected)
                                            .ForEach(e => StartCoroutine(UpdateConnection(e.bot)));
        }

        #region Events

        protected override void OnPackageEnter(object sender, PackageEventArgs e)
        {
            if (_controlledPackage != null) throw new InvalidOperationException("Already controlling a package");

            // Create the path element from the package
            _controlledPackage = (PathPackage) e.Package;
            _controlledElement = Instantiate(_controlledPackage.PathElement.gameObject,
                                             _controlledPackage.transform.position,
                                             Quaternion.identity,
                                             transform).GetComponent<PathElement>();

            _controlledPackage.GetComponent<Renderer>().enabled = false;
        }

        protected override void OnPackageExit(object sender, PackageEventArgs e)
        {
            DropPackage(e.Package);
        }

        protected override void OnPackageReleased(object sender, PackageEventArgs e)
        {
            if (!_controlledElement.Valid)
            {
                DropPackage(_controlledPackage);
                return;
            }

            _userElements.Add(_controlledElement);
            StartCoroutine(PlaceElement(_controlledElement));

            Destroy(_controlledPackage.gameObject);
            _controlledElement = null;
            _controlledPackage = null;
        }

        #endregion
    }
}
