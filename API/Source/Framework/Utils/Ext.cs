using System.Globalization;
namespace SRkMatchSimAPI.Framework;

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

    public static string RdPerc(this double value, int digits = 2)
    {
        return $"{Math.Round(value * 100, digits).ToString(CultureInfo.InvariantCulture)}%";
    }
    public static string RdPerc(this float value, int digits = 2)
    {
        return $"{Math.Round(value * 100, digits).ToString(CultureInfo.InvariantCulture)}%";
    }

    public static float InverseLerp(float a, float b, float v)
    {
        return (v - a) / (b - a);
    }
    public static double InverseLerp(double a, double b, double v)
    {
        return (v - a) / (b - a);
    }
}
