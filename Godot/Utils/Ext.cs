using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Futzim
{
    public static class Ext
    {
        public static string RdNo(this float value, int digits = 2)
        {
            return Math.Round(value, digits).ToString(CultureInfo.InvariantCulture);
        }

        public static string RdNo(this double value, int digits = 2)
        {
            return Math.Round(value, digits).ToString(CultureInfo.InvariantCulture);
        }

        public static string RdNo(this Vector3 value, int digits = 2)
        {
            var x = Math.Round(value.X, digits).ToString(CultureInfo.InvariantCulture);
            var y = Math.Round(value.Y, digits).ToString(CultureInfo.InvariantCulture);
            var z = Math.Round(value.Z, digits).ToString(CultureInfo.InvariantCulture);

            return $"({x}, {y}, {z})";
        }

        public static string RdPerc(this double value, int digits = 2)
        {
            return $"{Math.Round(value * 100, digits).ToString(CultureInfo.InvariantCulture)}%";
        }
        public static string RdPerc(this float value, int digits = 2)
        {
            return $"{Math.Round(value * 100, digits).ToString(CultureInfo.InvariantCulture)}%";
        }

    }
}
