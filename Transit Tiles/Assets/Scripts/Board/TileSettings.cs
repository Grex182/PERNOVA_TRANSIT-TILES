using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSettings : MonoBehaviour
{
    [SerializeField] public float tileSize = 1f;
    [SerializeField] public float gapSize = 0f;
    [SerializeField] public float yOffset = 0.52f;
    [SerializeField] public float yPositionOffset;

    public Vector3 bounds;

    public Vector3 GetTileCenter(int x, int y)
    {
        float xPos = x * (GetComponent<TileSettings>().tileSize + GetComponent<TileSettings>().gapSize);
        float zPos = y * (GetComponent<TileSettings>().tileSize + GetComponent<TileSettings>().gapSize);

        return new Vector3(xPos, GetComponent<TileSettings>().yOffset + GetComponent<TileSettings>().yPositionOffset, zPos) - bounds + new Vector3(GetComponent<TileSettings>().tileSize / 2, 0, GetComponent<TileSettings>().tileSize / 2);
    }
}
