using System.Linq;

namespace Extensions
{
    public static class MathExtensions
    {
        public static int Pow(this int bas, int exp)
        {
            return Enumerable.Repeat(bas, exp).Aggregate(1, (a, b) => a * b);
        }
    }
}