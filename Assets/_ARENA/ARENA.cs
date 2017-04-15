using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARENA : MonoBehaviour
{
    #region Instantiation
    private static ARENA instance;
    public static ARENA Inst
    {
        get {
            instance = instance ?? FindObjectOfType<ARENA>();
            return instance;
        }
    }
    #endregion

    #region Configurable in Editor Hierarchy
    public Text pullCounter;
    public Text avgTempIndicator;
    public Material[] emissionMaps;
    public AnimationCurve shakeIn;
    public AnimationCurve shakeOut;
    public AnimationCurve shakeBase;
    public AnimationCurve[] shakeCurves;

    #endregion

    #region Accessors
    private Light sceneLight;

    #endregion

    #region Private Variable Initializations

    private int pullCount = 0;
    private int numTiles;
    private double tempAvg;

    #endregion

    #region Public Getters/Setters

    private Tile[,] tileRegistry;
    public Tile[,] TileList { get { return tileRegistry; } private set { tileRegistry = value; } }
    public float kickStartStrength;

    public Vector3 debugKickStartPos = INPUT.NULLVEC;
    public List<int> debugDistanceRegistry = new List<int>();

    #endregion

    void Awake ()
    {
        numTiles = SETTINGS.Inst.horizSize * SETTINGS.Inst.vertSize;
        tileRegistry = new Tile[SETTINGS.Inst.horizSize, SETTINGS.Inst.vertSize];
        foreach ( Transform child in transform )
            foreach ( Transform tile in child.transform )
                TileList[(int) tile.position.x, (int) tile.position.z] = tile.GetComponent<Tile>();
    }

    void Start ()
    {
        sceneLight = GetComponentInChildren<Light>();
        pullCounter.text = pullCount + " PULL" + (pullCount == 1 ? "" : "S");
        //StartCoroutine(UpdateSceneLighting());
    }

    public void UpdateHeatAverage (float oldTemp, float newTemp)
    {
        tempAvg += (newTemp - oldTemp) / numTiles;
        sceneLight.intensity = (float) tempAvg.GetDial(0, SETTINGS.Inst.maxTempForFailure, 0.25f, 0.75f);
        avgTempIndicator.text = Mathf.RoundToInt((float) tempAvg).ToString() + " Deg = " + sceneLight.intensity.ToString() + "\n(" + SETTINGS.Inst.maxTempForFailure.ToString() + ")";
    }

    IEnumerator UpdateSceneLighting ()
    {
        while ( true )
        {
            float testTempAvg = 0f;
            foreach ( Tile tile in TileList )
                testTempAvg += tile.Temp / numTiles;
            sceneLight.intensity = (float) tempAvg.GetDial(0, SETTINGS.Inst.maxTempForFailure, 0.25f, 0.75f);
            avgTempIndicator.text = Mathf.RoundToInt((float) tempAvg).ToString() + " Deg = " + sceneLight.intensity.ToString() + "\nvs. " + Mathf.RoundToInt(testTempAvg).ToString() + "\n(" + SETTINGS.Inst.maxTempForFailure.ToString() + ")";
            yield return new WaitForSeconds(SETTINGS.Inst.tickFrequency);
        }
    }

    public void PullSuccess ()
    {
        pullCount++;
        pullCounter.text = pullCount + " PULL" + (pullCount == 1 ? "" : "S");
    }

    public Tile GetTileAt (Vector3 position)
    {
        int xPos = Mathf.RoundToInt(position.x);
        int zPos = Mathf.RoundToInt(position.z);
        if ( xPos >= 0 && xPos < SETTINGS.Inst.horizSize && zPos >= 0 && zPos < SETTINGS.Inst.vertSize )
            return TileList[xPos, zPos].GetComponent<Tile>();
        return null;
    }

    public bool CheckDistance (int dist)
    {
        if ( debugDistanceRegistry.Contains(dist) )
            return false;
        else
        {
            debugDistanceRegistry.Add(dist);
            return true;
        }
    }
}
