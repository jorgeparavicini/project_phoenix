using System;
using Data;
using UnityEngine.Assertions;

namespace Extensions
{
    public static class NumberConverter
    {
        public static string ToBinary(this int value, int? width = null)
        {
            var val = Convert.ToString(value, 2);
            if (width is int w)
                val = val.PadLeft(w, '0');
            for (var i = val.Length - 4; i > 0; i-=4) 
            {
                val = val.Insert(i, " ");
            }
            return val;
        }

        public static string ToString(this int value, NumberBase numberBase, int? width = null)
        {
            switch (numberBase)
            {
                case NumberBase.Binary:
                    return value.ToBinary(width);

                case NumberBase.Decimal:
                    return value.ToString();

                default:
                    return value.ToString();
            }
        }
    }
}
