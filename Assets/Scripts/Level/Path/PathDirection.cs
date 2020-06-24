using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Valve.VR.InteractionSystem;

namespace Level.Path
{
    [Flags]
    public enum PathDirection : short
    {
        None,
        Top,
        Right,
        Bottom,
        Left
    }

    public static class PathDirectionExtension
    {
        public static IEnumerable<PathDirection> GetFlags(this PathDirection input)
        {
            return Enum.GetValues(input.GetType()).Cast<PathDirection>().Where(value => input.HasFlag(value));
        }

        public static PathDirection ConnectionDirection(this PathDirection input, PathDirection other)
        {
            var dir = PathDirection.None;
            input.GetFlags().ForEach(direction =>
            {
                switch (direction)
                {
                    case PathDirection.None:
                        dir = PathDirection.None;
                        break;
                    case PathDirection.Bottom:
                        if (other.HasFlag(PathDirection.Top)) dir = PathDirection.Bottom;
                        break;
                    case PathDirection.Left:
                        if (other.HasFlag(PathDirection.Right)) dir = PathDirection.Left;
                        break;
                    case PathDirection.Right:
                        if (other.HasFlag(PathDirection.Left)) dir = PathDirection.Right;
                        break;
                    case PathDirection.Top:
                        if (other.HasFlag(PathDirection.Bottom)) dir = PathDirection.Top;
                        break;
                    default:
                        dir = PathDirection.None;
                        break;
                }
            });
            return dir;
        }
    }
}
