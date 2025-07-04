using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengersChecker : Singleton<PassengersChecker>
{
    [SerializeField] public int maxSpecialPassengers;
    [SerializeField] public int currentSpecialPassengers = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
