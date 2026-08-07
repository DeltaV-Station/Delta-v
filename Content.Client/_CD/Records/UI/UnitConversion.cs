namespace Content.Client._CD.Records.UI;

public static class UnitConversion
{
    /// <summary>
    /// DeltaV - The average height of a human in centimeters. According to the US CDC, its
    /// 171 for men and 160 for women. So average of that is ~165cm.
    /// 
    /// Just kidding, we're going with EE's arbitrary standard of 175cm.
    /// </summary>
    private const int AVERAGE_HEIGHT_CM = 175;

    /// <summary>
    /// DeltaV - 1.0 scale is considered average for humans, so a scale of 1 will be 175cm.
    /// Ensure that scale also includes the base species height AND the user-defined height.
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    private static int GetMetricHeightFromScale(float scale = 1)
    {
        // cast as int because we don't care about decimal
        return (int)Math.Max(scale * AVERAGE_HEIGHT_CM, 1); // can't be shorter than 1cm I guess
    }

    /// <summary>
    /// DeltaV - Gets nicely formatted string that contains both metric and imperial measurements.
    /// With a scale of 1, it should look like... 175cm (5' 9")
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static string GetMetricAndImperialDisplayFromScale(float scale = 1)
    {
        var metricHeight = GetMetricHeightFromScale(scale);
        return $"{metricHeight}cm ({GetImperialDisplayLength(metricHeight)})";
    }

    public static string GetImperialDisplayLength(int lengthCm)
    {
        var heightIn = (int)Math.Round(lengthCm * 0.3937007874 /* cm to in*/);
        return $"{heightIn / 12}'{heightIn % 12}\"";
    }

    public static string GetImperialDisplayMass(int massKg)
    {
        var weightLbs = (int)Math.Round(massKg * 2.2046226218 /* kg to lbs */);
        return $"{weightLbs} lbs";
    }
}
