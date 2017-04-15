using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Tile : MonoBehaviour
{
    public bool IsHeating { get { return INPUT.Inst.IsCursorHeating && INPUT.Inst.GetTargetTile() == this; } }
    public bool IsTargeted { get { return IsHeating && INPUT.Inst.TargetedTile == this; } }
    public bool IsCharging { get { return IsTargeted && transform.position.y == INPUT.Inst.transform.position.y; } }

    public enum Axes { X, Z }

    private Vector3 startPos, endPos; // the start and end positions of the tile when charged by a player
    private Vector3 cursorPos; // the location of the cursor when it began targeting the tile
    private LTDescr tileTween = null; // the current tween-movement of the tile as it is being charged by player input

    private float maxTemp, maxTempInverse, prevTemp;
    private float contactArea;
    private float contactConductance, contactFlowInv;
    private float mass, energyToTemp, tempToEnergy;
    private float baseOscillationFunction, dampingFunction, normalizedOscillationFunction;

    private float xKickMultiplier, zKickMultiplier;

    public float HeatEnergy { get; set; }
    public float Temp { get { return HeatEnergy * energyToTemp; } }
    public float TempPercentage { get { return Temp * maxTempInverse; } }
    public float Shake { get { return Temp + AddedShake; } }
    public float Charge { get; set; }
    public float AddedShake { get; set; }

    private GameObject tileExploder;
    private Material material;
    private int emissionColorPropertyID;
    private int currentEmissionMapNum;


    //private GameObject tempIndicator;
    private TextMesh tempAmount, lightAmount, shakeAmount, addedShakeAmount, shakeTier, breakPercent;
    private float lightAtPushStart;
    //private List<Tile> neighbours = new List<Tile>();
    //private Dictionary<Tile, float> neighbourDict = new Dictionary<Tile, float>();
    private Vector3[] neighborDirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    private Dictionary<Vector3, Tile> neighbourDict = new Dictionary<Vector3, Tile>();
    private List<Tile> neighbours = new List<Tile>();
    public List<int> kickIDs = new List<int>();

    private Light spotlight;
    private ParticleSystem ambientSparks;
    private ParticleSystem.EmissionModule sparksEmissionModule;

    private Dictionary<Vector3, ParticleSystem> kickFlares = new Dictionary<Vector3, ParticleSystem>();
    private ParticleSystem shakeFlipBoom;

    private int currentShakeCurveNum = 0;
    private AnimationCurve currentShakeCurve;
    private float shakeOffset;
    private enum Dir { X, Z }
    private Dir shakeDir;

    private Vector3 debugPos;


    //private int debugCounter = 0;

    private void Awake ()
    {
        startPos = transform.position;
        endPos = transform.position;

        name = "Tile[" + transform.position.z.ToString() + ", " + transform.position.x.ToString() + "]";

        contactArea = SETTINGS.Inst.tileEdgeThickness * transform.localScale.x;
        maxTemp = Random.Range(SETTINGS.Inst.minTempForFailure, SETTINGS.Inst.maxTempForFailure);
        maxTempInverse = 1 / maxTemp;
        mass = GetComponent<Rigidbody>().mass;
        energyToTemp = 1 / (mass * SETTINGS.Inst.specHeatCapacity);
        tempToEnergy = 1 / energyToTemp;
        contactConductance = 1f / (SETTINGS.Inst.thermalContactResistance * contactArea);
        contactFlowInv = 1f / (SETTINGS.Inst.thermalConductivity * contactArea);
        prevTemp = 0f;

        tempAmount = transform.Find("TempReadout").GetComponent<TextMesh>();
        lightAmount = transform.Find("LightReadout").GetComponent<TextMesh>();
        shakeAmount = transform.Find("ShakeReadout").GetComponent<TextMesh>();
        addedShakeAmount = transform.Find("AddedShakeReadout").GetComponent<TextMesh>();
        shakeTier = transform.Find("ShakeTierReadout").GetComponent<TextMesh>();
        breakPercent = transform.Find("BreakReadout").GetComponent<TextMesh>();

        tileExploder = transform.Find("TileExplosion").gameObject;
        material = GetComponent<MeshRenderer>().material;
        emissionColorPropertyID = Shader.PropertyToID("_EmissionColor");
        currentEmissionMapNum = 0;

        ambientSparks = transform.Find("Ambient: Embers").GetComponent<ParticleSystem>();
        sparksEmissionModule = ambientSparks.emission;

        kickFlares.Add(Vector3.forward, transform.Find("Effect: Kick Fire F").GetComponent<ParticleSystem>());
        kickFlares.Add(Vector3.back, transform.Find("Effect: Kick Fire B").GetComponent<ParticleSystem>());
        kickFlares.Add(Vector3.left, transform.Find("Effect: Kick Fire L").GetComponent<ParticleSystem>());
        kickFlares.Add(Vector3.right, transform.Find("Effect: Kick Fire R").GetComponent<ParticleSystem>());

        shakeFlipBoom = transform.Find("Effect: Shake Flip").GetComponent<ParticleSystem>();

        currentShakeCurve = ARENA.Inst.shakeCurves[currentShakeCurveNum];
        shakeOffset = Random.Range(-5f, 5f);

        HeatEnergy = 0f;
        Charge = 0f;
    }

    private void Start ()
    {
        foreach ( Vector3 dir in neighborDirs )
        {
            Tile thisTile = ARENA.Inst.GetTileAt(transform.position + dir);
            if ( thisTile )
                neighbourDict.Add(dir, thisTile);
        }
    }

    void Update ()
    {
        UpdatePosition();
        UpdateCharge();
        UpdateHeat();
        StartCoroutine(AbsorbHeatStep());
        UpdateEffects();
        UpdateIndicators();
    }

    #region Update Position, Charge & Heat

    void UpdatePosition ()
    {
        if ( IsTargeted && tileTween == null )
        {
            cursorPos = INPUT.Inst.GetCursorTarget();
            endPos = INPUT.Inst.GetCursorPosition();
            tileTween = LeanTween.move(gameObject, endPos, SETTINGS.Inst.timeToRaiseTile).setEase(LeanTweenType.easeOutCubic);
            StartCoroutine(WaitTillCharged());
        }
        else if ( !IsTargeted && tileTween != null )
            AbandonTile();
    }

    void UpdateCharge ()
    {
        if ( IsCharging )
            Charge += SETTINGS.Inst.chargeGainedPerSec;
        else if ( Charge > 0 )
            Charge = Mathf.Max(Charge - (SETTINGS.Inst.chargeLostPerSec * Time.deltaTime), 0f);
    }

    void UpdateHeat ()
    {
        if ( IsHeating )
        {
            if ( IsTargeted )
                HeatEnergy += Vector3.Distance(transform.position, endPos) / Vector3.Distance(startPos, endPos) * SETTINGS.Inst.tempGainedAtFullRise * tempToEnergy;
            HeatEnergy += SETTINGS.Inst.tempGainedByCursor * tempToEnergy;
        }
    }

    IEnumerator AbsorbHeatStep ()
    {
        if ( INPUT.Inst.IsCursorHeating && INPUT.Inst.GetTargetTile() != null )
        {
            foreach ( Tile neighbour in neighbourDict.Values.ToList() )
            {
                double theirTemp = neighbour.Temp;
                if ( Temp > neighbour.Temp )
                {
                    double energyFlowPerSecond = (Temp - neighbour.Temp) / (transform.localScale.x * contactFlowInv + contactConductance) * SETTINGS.Inst.tickFrequency;
                    HeatEnergy -= Mathf.RoundToInt((float) energyFlowPerSecond / neighbourDict.Count);
                    neighbour.HeatEnergy += Mathf.RoundToInt((float) energyFlowPerSecond);
                }
            }
            ARENA.Inst.UpdateHeatAverage(prevTemp, Temp);
            prevTemp = Temp;
        }
        yield return new WaitForSeconds(SETTINGS.Inst.tickFrequency);
    }

    void UpdateIndicators ()
    {
        tempAmount.text = Mathf.RoundToInt(Temp).ToString();
        lightAmount.text = Mathf.RoundToInt(Charge).ToString();
        shakeAmount.text = Mathf.RoundToInt(Shake * maxTempInverse * 100f).ToString() + "%";
        addedShakeAmount.text = Mathf.RoundToInt(AddedShake * maxTempInverse * 100f).ToString() + "%";

        breakPercent.text = Mathf.RoundToInt(Temp / maxTemp * 100f).ToString() + "%";
    }

    #endregion

    #region Tile Effects
    void UpdateEffects ()
    {
        Effect_EmissionMap(Temp);
        Effect_AmbientSparks(Shake);
        //Effect_Shaking(Shake);
    }

    void Effect_EmissionMap (float intensity)
    {
        intensity *= maxTempInverse;
        if ( currentEmissionMapNum < ARENA.Inst.emissionMaps.Length - 1 && intensity > SETTINGS.Inst.emissionMapTempBreaks[currentEmissionMapNum + 1] )
            currentEmissionMapNum++;
        else if ( currentEmissionMapNum > 0 && intensity < SETTINGS.Inst.emissionMapTempBreaks[currentEmissionMapNum] )
            currentEmissionMapNum--;
        GetComponent<MeshRenderer>().material = ARENA.Inst.emissionMaps[currentEmissionMapNum];
        material = GetComponent<MeshRenderer>().material;
        float convertIntensity = intensity.GetDial(0f, 1f, SETTINGS.Inst.emissionMapTempBreaks[currentEmissionMapNum], SETTINGS.Inst.emissionMapTempBreaks[currentEmissionMapNum + 1]);
        float newIntensity = SETTINGS.Inst.emissionMapMins[currentEmissionMapNum] + convertIntensity * (SETTINGS.Inst.emissionMapMaxs[currentEmissionMapNum] - SETTINGS.Inst.emissionMapMins[currentEmissionMapNum]);
        material.SetColor(emissionColorPropertyID, Color.white * newIntensity);
    }

    void Effect_AmbientSparks (float intensity)
    {
        //intensity = Mathf.Max((intensity - 0.5f) * 2, 0);
        intensity *= maxTempInverse;
        intensity = intensity.GetDial(0f, SETTINGS.Inst.maxShakeForSparks, SETTINGS.Inst.minShakeAsPercentOfMax, 1f);
        float amount = Mathf.RoundToInt(Mathf.Pow(intensity, 2) * SETTINGS.Inst.maxSparkRate);
        sparksEmissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(amount);
    }

    void Effect_Shaking (float intensity)
    {
        float shakeBase = ARENA.Inst.shakeBase.Evaluate(Mathf.Repeat(Time.time + shakeOffset, 1f));

        if ( currentShakeCurveNum < ARENA.Inst.shakeCurves.Length - 1 && intensity > SETTINGS.Inst.shakeBreaks[currentShakeCurveNum + 1] )
        {
            currentShakeCurveNum++;
            shakeDir = shakeDir == Dir.X ? Dir.Z : Dir.X;
        }
        else if ( currentShakeCurveNum > 0 && intensity < SETTINGS.Inst.shakeBreaks[currentShakeCurveNum] )
        {
            currentShakeCurveNum--;
            shakeDir = shakeDir == Dir.X ? Dir.Z : Dir.X;
        }
        currentShakeCurve = ARENA.Inst.shakeCurves[currentShakeCurveNum];
        shakeTier.text = currentShakeCurveNum.ToString();
        //float convertIntensity = intensity.GetDial(0f, 1f, SETTINGS.Inst.shakeBreaks[currentShakeCurveNum], SETTINGS.Inst.shakeBreaks[currentShakeCurveNum + 1]);
        float thisShake = shakeBase * currentShakeCurve.Evaluate(Mathf.Repeat(Time.time + shakeOffset, 10f)) * SETTINGS.Inst.shakeMaxs[currentShakeCurveNum];

        Vector3 currentPosition = transform.position;
        transform.eulerAngles = shakeDir == Dir.X ? new Vector3(thisShake, 0, 0) : new Vector3(0, 0, thisShake);
        transform.position = currentPosition;

    }

    void Effect_KickFlare (Vector3 dir, float intensity)
    {
        ParticleSystem.MainModule thisMain = kickFlares[dir].main;
        if ( intensity > 0 )
        {
            thisMain.startSize = intensity.GetDial(0f, SETTINGS.Inst.kickMultMax) * 1.5f;
            thisMain.startColor = Color.yellow;
        }
        else
        {
            intensity *= -1;
            thisMain.startSize = intensity.GetDial(0f, -1 * SETTINGS.Inst.kickMultHealMax) * 0.8f;
            thisMain.startColor = Color.green;
        }
        kickFlares[dir].Play();
    }

    void Effect_ShakeFlip ()
    {
        shakeFlipBoom.Play();
    }
    #endregion

    #region Pulling & Charging Tiles

    IEnumerator WaitTillCharged ()
    {
        //Debug.Log("Coroutine Started!");
        yield return new WaitWhile(() => Charge < SETTINGS.Inst.chargeNeededForPull);
        //if ( INPUT.Inst.ChargingTile != null )
        if ( IsTargeted )
        {
            Charge = 0;
            ARENA.Inst.PullSuccess();
            PullTile();
        }

    }

    void PullTile ()
    {
        ClearPull();
        LeanTween.move(gameObject, startPos, 0.5f).setEase(LeanTweenType.easeOutElastic);
        ReleaseTile(SETTINGS.Inst.kickMultMax - 1f);
    }

    void AbandonTile ()
    {
        ClearPull();
        LeanTween.move(gameObject, startPos, 0.5f).setEase(LeanTweenType.easeOutBounce);
        ReleaseTile(SETTINGS.Inst.kickMultHealMax + Vector3.Distance(transform.position, startPos) / Vector3.Distance(startPos, endPos) * 2 * Mathf.Abs(SETTINGS.Inst.kickMultHealMax));
    }

    void ClearPull ()
    {
        LeanTween.cancel(tileTween.id, false);
        StopCoroutine("HoldTileDown");
        INPUT.Inst.TargetedTile = null;
        tileTween = null;
    }

    #endregion

    #region Releasing Tiles & Kicks
    void ReleaseTile (float kickMult)
    {
        ARENA.Inst.debugKickStartPos = startPos;
        float distance, kickMultiplier;
        if ( cursorPos == INPUT.NULLVEC )
            cursorPos = new Vector3(startPos.x + 0.5f, startPos.y, startPos.z + 0.5f);
        int kickID = Random.Range(0, 100000);
        kickIDs.Add(kickID);
        distance = Vector3.Distance(cursorPos, startPos);
        cursorPos = INPUT.NULLVEC;
        //distance = Mathf.Min(2 * Mathf.Abs(targetPos.x - transform.position.x) / transform.localScale.x, 1);
        kickMultiplier = kickMult + distance;
        ARENA.Inst.kickStartStrength = kickMultiplier;
        //Debug.Log("Kick Mult = " + kickMultiplier);
        foreach ( Vector3 dir in neighborDirs )
            SendKicks(kickMultiplier, dir, kickID);
    }

    void SendKicks (float mult, Vector3 dir, int ID)
    {
        if ( neighbourDict.ContainsKey(dir) && !neighbourDict[dir].kickIDs.Contains(ID) )
        {
            neighbourDict[dir].TakeKick(Shake, mult, dir, ID);
            Effect_KickFlare(dir, mult);
        }
    }

    public void TakeKick (float shake, float mult, Vector3 dir, int ID)
    {
        if ( kickIDs.Contains(ID) )
            return;
        else
        {
            kickIDs.Add(ID);
            CheckShakeFlip(mult, ID);
            //float thisMult = (SETTINGS.Inst.kickDampingCoefficient * mult) / 8 * (temp - Temp) / (Temp + SETTINGS.Inst.maxTempForFailure / 4) + SETTINGS.Inst.kickShiftCoefficient * mult;
            float thisMult = SETTINGS.Inst.kickDampingCoefficient * mult;
            float shakeChange = shake * thisMult;
            AddedShake = Mathf.Max(AddedShake + shakeChange, 0);
            if ( ARENA.Inst.CheckDistance(Mathf.RoundToInt(Vector3.Distance(startPos, ARENA.Inst.debugKickStartPos))) )
                //Debug.Log(Mathf.RoundToInt(Vector3.Distance(startPos, ARENA.Inst.debugKickStartPos)) + "m: " + thisMult.ToString() + "x = " + shakeChange + " -> " + AddedShake);
                if ( Mathf.Abs(SETTINGS.Inst.kickDampingCoefficient * thisMult) > SETTINGS.Inst.kickMultMinPercent * (thisMult > 0 ? SETTINGS.Inst.kickMultMax : (-1 * SETTINGS.Inst.kickMultHealMax)) )
                    StartCoroutine(KickShake(thisMult, ID));
        }

    }

    void CheckShakeFlip (float mult, int ID)
    {
        float random = Random.Range(0f, 1f);
        if ( random < Mathf.Min(Mathf.Pow(Shake / SETTINGS.Inst.maxTempForFailure, 2), SETTINGS.Inst.maxChanceOfDischarge) )
        {
            //Debug.Log("DISCHARGE: " + random.ToString() + " < " + (Mathf.Pow(Shake / SETTINGS.Inst.maxTempForFailure, 2).ToString()));
            Effect_ShakeFlip();
            //Debug.Log("DISCHARGE AT " + name + ": Temp = " + Temp + ", A.Shake = " + AddedShake);
            float dischargeAmount = AddedShake * SETTINGS.Inst.percentHeatAtDischarge;
            AddedShake -= dischargeAmount;
            HeatEnergy += dischargeAmount * tempToEnergy;
            //Debug.Log("... New Temp: " + Temp + ", New A.Shake: " + AddedShake);
            if ( Temp > maxTemp )
            {
                GetComponent<MeshRenderer>().enabled = false;
                tileExploder.SetActive(true);
            }
            StartCoroutine(KickShake(mult * SETTINGS.Inst.kickDischargeChainMult, ID));
        }
    }

    IEnumerator KickShake (float mult, int ID)
    {
        //while ( true )
        //{
        yield return new WaitForSeconds(SETTINGS.Inst.kickPulseSpeed);
        foreach ( Vector3 dir in neighborDirs )
            SendKicks(mult, dir, ID);
        //StopCoroutine("KickShake");
        //}
    }

    #endregion

}
