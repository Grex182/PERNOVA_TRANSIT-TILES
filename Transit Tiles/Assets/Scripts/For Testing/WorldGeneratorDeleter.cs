using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneratorDeleter : MonoBehaviour
{

    private void Update()
    {
        if (FindObjectOfType<WorldGenerator>().enabled == true)
        {
            FindObjectOfType<WorldGenerator>().enabled = false;
            Destroy(gameObject);
        }
    }
}
