using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ENUMS 
{
    public enum VirusName { Red, Yellow, Blue};
    public enum GameOverReasons { PlayersWon, TooManyOutbreaks, NoMoreCubesOfAColor, NoMorePlayerCards, None};
    public enum AoIType { CityCard, City, Action,Board,EventCard, Cube, InfectionCard, Avatar};
    public enum ActionType {PMoveEvent, PCharterEvent, PTreatDisease,PShareKnowledge, PPilotFlyToCity, PVirologist, PCureDisease,PContainSpecialistRemoveWhenEntering, PFlyToCity, PQuarantineSpecialist, PRoleCard, PEndTurn}
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : value.ToString();
    }

    public static T GetEnumValueFromDescription<T>(string description) where T : Enum
    {
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0 && attributes[0].Description == description)
            {
                return value;
            }
        }
        throw new ArgumentException("Enum value not found for description: " + description);
    }
}
