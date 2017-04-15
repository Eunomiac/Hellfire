using UnityEngine;

public static class ExtensionMethods
{
    public static double GetDial (this double value, double minVal, double maxVal, float minThreshold = 0f, float maxThreshold = 1f, bool isClamping = true, bool isDebugging = false)
    {
        double minThreshVal = minVal + minThreshold * (maxVal - minVal);
        double maxThreshVal = minVal + maxThreshold * (maxVal - minVal);
        double result = (value - minThreshVal) / (maxThreshVal - minThreshVal);
        //if ( isDebugging )
        //Debug.Log("mTV = " + minThreshVal + ", xTV = " + maxThreshVal + ". TOP: " + (value - minThreshVal) + " / BOTTOM: " + (maxThreshVal - minThreshVal));
        if ( isClamping )
            return Mathf.Max(Mathf.Min((float) result, 1f), 0f);
        else
            return result;
    }

    public static float GetDial (this float value, float minVal, float maxVal, float minThreshold = 0f, float maxThreshold = 1f, bool isClamping = true, bool isDebugging = false)
    {
        return (float) ((double) value).GetDial(minVal, maxVal, minThreshold, maxThreshold, isClamping, isDebugging);
    }

    public static float GetDial (this int value, int minVal, int maxVal, float minThreshold = 0f, float maxThreshold = 1f, bool isClamping = true, bool isDebugging = false)
    {
        return (float) ((double) value).GetDial(minVal, maxVal, minThreshold, maxThreshold, isClamping, isDebugging);
    }

}