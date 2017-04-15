using UnityEngine;

public class SETTINGS : MonoBehaviour
{
    #region Instantiation
    private static SETTINGS instance;
    public static SETTINGS Inst
    {
        get {
            instance = instance ?? FindObjectOfType<SETTINGS>();
            return instance;
        }
    }
    #endregion

    [Header("Main Settings")]
    public int horizSize = 8;
    public int vertSize = 8;
    public Vector3 bottomLeftTileCenter;
    public float tickFrequency = 0.5f;

    [Header("Charging Tiles")]
    public float timeToRaiseTile = 2f;
    public float chargeGainedPerSec = 10f;
    public float chargeNeededForPull = 400f;
    public float chargeLostPerSec = 25f;
    public float tempGainedAtFullRise = 5f;
    public float tempGainedByCursor = 5f;
    public float minTempForFailure = 1000f;
    public float maxTempForFailure = 1200f;

    [Header("Tile Thermal Properties")]
    public float thermalConductivity = 500f;
    public float thermalContactResistance = 0.000005f;
    public float tileEdgeThickness = 0.01f;
    public float specHeatCapacity = 902f;

    [Header("Tile Shake")]
    public float kickPulseSpeed = 0.2f;
    public float kickMultHealMax = -0.5f;
    public float kickMultMax = 2f;
    public float kickMultMinPercent = 0.1f;
    public float kickDampingCoefficient = 1f;
    public float percentHeatAtDischarge = 0.25f;
    public float kickDischargeChainMult = 1.5f;
    public float maxChanceOfDischarge = 0.5f;

    [Header("Heat-Based Effect Settings")]
    public float maxShakeForSparks = 1.5f;
    public float minShakeAsPercentOfMax = 0.333f;
    public float maxSparkRate = 30f;
    public float[] emissionMapTempBreaks;
    public float[] emissionMapMins;
    public float[] emissionMapMaxs;

    [Header("Shake-Based Effect Settings")]
    public float[] shakeBreaks;
    public float[] shakeMins;
    public float[] shakeMaxs;





}
