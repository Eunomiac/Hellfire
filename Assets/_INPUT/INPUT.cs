using System.Collections.Generic;
using UnityEngine;

public class INPUT : MonoBehaviour
{

    public static readonly Vector3 NULLVEC = new Vector3(-5f, -5f, -5f);

    #region Instantiation
    private static INPUT instance;
    public static INPUT Inst
    {
        get {
            instance = instance ?? FindObjectOfType<INPUT>();
            return instance;
        }
    }
    #endregion

    #region Accessors
    private Cursor cursor;

    #endregion

    #region Private Variable Initializations
    private float minXPos, minZPos, maxXPos, maxZPos;

    #endregion

    #region Public Getters/Setters

    public bool IsCursorHeating { get { return Input.GetMouseButton(0); } }
    //private List<RaycastHit> hitIDs = new List<RaycastHit>();
    public List<RaycastHit> HitIDs { get; private set; }

    public Tile TargetedTile { get; set; }

    #endregion

    private void Start ()
    {
        cursor = GetComponentInChildren<Cursor>();
        cursor.Position = NULLVEC;

        MeshRenderer tileRenderer = ARENA.Inst.GetTileAt(SETTINGS.Inst.bottomLeftTileCenter).GetComponent<MeshRenderer>();
        minXPos = SETTINGS.Inst.bottomLeftTileCenter.x - tileRenderer.bounds.extents.x;
        minZPos = SETTINGS.Inst.bottomLeftTileCenter.z - tileRenderer.bounds.extents.z;
        maxXPos = SETTINGS.Inst.bottomLeftTileCenter.x + (SETTINGS.Inst.horizSize - 1) + tileRenderer.bounds.extents.x;
        maxZPos = SETTINGS.Inst.bottomLeftTileCenter.z + (SETTINGS.Inst.vertSize - 1) + tileRenderer.bounds.extents.x;
    }

    void Update ()
    {
        cursor.Position = GetCursorPosition();
        UpdateTargetTile();
    }

    public Vector3 GetCursorPosition (bool isValidating = false)
    {
        RaycastHit hit;
        if ( Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8) )
            return new Vector3(hit.point.x, hit.point.y, hit.point.z);
        return cursor.Position;
    }

    public Vector3 GetCursorTarget ()
    {
        RaycastHit hit;
        if ( Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, ~(1 << 8)) )
            if ( hit.point.x >= minXPos && hit.point.x <= maxXPos && hit.point.z >= minZPos && hit.point.z <= maxZPos )
                return new Vector3(hit.point.x, hit.point.y, hit.point.z);
        return NULLVEC;
    }

    void UpdateTargetTile (bool isFirstFrame = false)
    {
        Tile thisTile = GetTargetTile();

        // LEFT MOUSE BUTTON CLICKED THIS FRAME:
        if ( Input.GetMouseButtonDown(0) )
            TargetedTile = thisTile;

        // LEFT MOUSE NOT CLICKED *OR* PLAYER MOVED MOUSE OFF TILE:
        else if ( !IsCursorHeating || thisTile != TargetedTile )
            TargetedTile = null;
    }

    public Tile GetTargetTile ()
    {
        Vector3 targetPos = GetCursorTarget();
        if ( targetPos == NULLVEC )
            return null;
        return ARENA.Inst.TileList[Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.z)];
    }

    void SendPush (Tile tile, RaycastHit hit)
    {

        HitIDs.Add(hit);
    }

    public bool isTileSelected (Tile tile)
    {
        return TargetedTile == tile;
    }

    public bool isAnyTileTargeted ()
    {
        return TargetedTile != null;
    }
}