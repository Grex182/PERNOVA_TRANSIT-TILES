using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PassengerTypes : MonoBehaviour
{
    public enum PassengerSize
    {
        Single, // one (1) tile
        Bulky // two (2) tiles
    }

    public enum PassengerTrait
    {
        Standard,
        Priority,
        Noisy,
        Stinky,
        Sleepy
    }

    public class Passenger
    {
        public string Name { get; private set; }
        // Enums
        public PassengerSize SizeType {  get; private set; }
        public PassengerTrait TraitType { get; private set; } // Change to list if passenger can have numerous traits

        // Ints
        public int StationNumber { get; private set; }
        public int MoodValue { get; private set; } = 3; // 3 = Happy, 2 = Neutral, 1 = Angry [Default is happy]
        public int TileSize { get; private set; }

        // Bools
        public bool NeedSeat { get; private set; }
        public bool HasNegativeAura { get; private set; }
        public bool CanRotate { get; private set; }

        // Strings
        public string StationColor { get; private set; }
        public string StationShape { get; private set; }

        // Lookup table to set StationColor & StationShape
        private static readonly (string Color, string Shape)[] StationData =
        {
            ("Red", "Heart"),
            ("Pink", "Flower"),
            ("Orange", "Circle"),
            ("Yellow", "Star"),
            ("Green", "Square"),
            ("Blue", "Diamond"),
            ("Violet", "Triangle")
        };

        // Lookup table based on passenger trait
        private static readonly (string name, PassengerSize size, PassengerTrait trait)[] PassengerConfigurations =
        {
            ("Standard", PassengerSize.Single, PassengerTrait.Standard),
            ("BulkyElder", PassengerSize.Bulky, PassengerTrait.Priority),
            ("Elder", PassengerSize.Single, PassengerTrait.Priority),
            ("Parent", PassengerSize.Bulky, PassengerTrait.Priority),
            ("Pregnant", PassengerSize.Single, PassengerTrait.Priority),
            ("Injured", PassengerSize.Bulky, PassengerTrait.Priority),
            ("Noisy", PassengerSize.Single, PassengerTrait.Noisy),
            ("Stinky", PassengerSize.Single, PassengerTrait.Stinky),
            ("Sleepy", PassengerSize.Single, PassengerTrait.Sleepy)
        };

        #region SET BASE PASSENGER
        public Passenger(string name, PassengerSize size, PassengerTrait trait, int stationNumber)
        {
            this.Name = name;
            this.SizeType = size;
            this.TraitType = trait;
            this.StationNumber = Mathf.Clamp(stationNumber, 1, 7);

            ApplyConstantValues();

            Debug.Log("Passenger Values Set: \n" +
                      "Tile Size = " + SizeType + "\n" +
                      "Can Rotate = " + CanRotate + "\n" +
                      "Trait = " + TraitType + "\n" +
                      "Mood Value = " + MoodValue + "\n" +
                      "Need Seat = " + NeedSeat + "\n" +
                      "Has Negative Aura = " + HasNegativeAura + "\n" +
                      "Station Number = " + StationNumber + "\n"
            );
        }

        private void ApplyConstantValues()
        {
            // Set TileSize & CanRotate
            TileSize = SizeType == PassengerSize.Single ? 1 : 2;
            CanRotate = SizeType == PassengerSize.Bulky;

            switch (TraitType) // Set Booleans
            {
                case PassengerTrait.Standard:
                    NeedSeat = false;
                    HasNegativeAura = false;
                    break;

                case PassengerTrait.Priority:
                    NeedSeat = true;
                    HasNegativeAura = false;
                    break;

                case PassengerTrait.Noisy:
                    NeedSeat = false;
                    HasNegativeAura = true;
                    break;

                case PassengerTrait.Stinky:
                    NeedSeat = false;
                    HasNegativeAura = true;
                    break;

                case PassengerTrait.Sleepy:
                    NeedSeat = true;
                    HasNegativeAura = false;
                    break;
            }

            if (StationNumber >= 1 && StationNumber <= 7)
            {
                StationColor = StationData[StationNumber - 1].Color;
                StationShape = StationData[StationNumber - 1].Shape;
            }
        }
        #endregion

        #region PASSENGER LIST
        public static Passenger CreatePassenger(string passengerName, int stationNumber)
        {
            foreach (var config in PassengerConfigurations)
            {
                if (config.name == passengerName)
                {
                    return new Passenger(passengerName, config.size, config.trait, stationNumber);
                }
            }

            Debug.LogWarning($"Unknown passenger type: {passengerName}. Creating Standard passenger.");
            return new Passenger(passengerName, PassengerSize.Single, PassengerTrait.Standard, stationNumber);
        }
        #endregion
    }
}
