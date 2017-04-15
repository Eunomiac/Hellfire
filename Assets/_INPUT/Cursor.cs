using UnityEngine;

public class Cursor : MonoBehaviour
{
    #region Public Getters/Setters
    public Vector3 Position { get { return transform.position; } set { transform.position = value; } }

    #endregion
}
