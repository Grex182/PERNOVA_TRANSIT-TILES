using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegativeAura : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drag"))
        {
            PassengerData passengerData = other.GetComponent<PassengerData>();
            if (passengerData != null && !passengerData.isMoodSwung)
            {
                passengerData.isMoodSwung = true;
                passengerData.ChangeMoodValue(-1);
            }
        }
    }
}
