using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBackgroundHandler : MonoBehaviour
{
    [SerializeField] private float range = 120f; // Range for visibility of child objects;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        { 
            Transform child = transform.GetChild(i);
            float childPosX = child.position.x;

            bool isInRange = childPosX >= -range && childPosX < range;

            child.gameObject.SetActive(isInRange);
        }
    }
}
