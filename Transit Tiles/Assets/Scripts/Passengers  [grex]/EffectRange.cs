using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectRange : MonoBehaviour
{
    private List<GameObject> collidedTiles = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TrainTile") || other.gameObject.CompareTag("PlatformTile"))
        {
            other.gameObject.layer = LayerMask.NameToLayer("EffectRange");
            collidedTiles.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("TrainTile") || other.gameObject.CompareTag("PlatformTile"))
        {
            ResetTileLayer(other.gameObject);
            collidedTiles.Remove(other.gameObject);
        }
    }

    private void OnDisable()
    {
        foreach (var obj in collidedTiles)
        {
            ResetTileLayer(obj);
        }
        collidedTiles.Clear();
    }

    private void ResetTileLayer(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("Tile");
    }
}
