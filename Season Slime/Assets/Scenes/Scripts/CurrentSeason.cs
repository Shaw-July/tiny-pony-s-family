using System.Collections.Generic;
using UnityEngine;

public class CurrentSeason : MonoBehaviour
{
    public enum SeasonIdentifier
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public SeasonIdentifier currentSeason;
}
