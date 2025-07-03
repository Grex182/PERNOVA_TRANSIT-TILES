using UnityEngine;

[System.Serializable]
public class PassengerData
{
    public PassengerType type;
    public string assignedColor; // store as string
    public Vector2Int position;

    public PassengerData(PassengerType type, string assignedColor, Vector2Int position)
    {
        this.type = type;
        this.assignedColor = assignedColor;
        this.position = position;
    }
}