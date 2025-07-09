using UnityEngine;

[System.Serializable]
public class PassengerTypes
{
    public PassengerType type;
    public PassengerEffect effect;
    public string assignedColor; // store as string
    public Vector2Int position;

    public PassengerTypes(PassengerType type, PassengerEffect effect, string assignedColor, Vector2Int position)
    {
        this.type = type;
        this.effect = effect;
        this.assignedColor = assignedColor;
        this.position = position;
    }
}