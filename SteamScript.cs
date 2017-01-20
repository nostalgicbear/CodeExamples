using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the unlocking of achievements via Steam
/// </summary>
public class SteamScript : MonoBehaviour
{
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;
    public bool Check_It;
    // Use this for initialization
    void Start()
    {
        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }
    }

    public void UnlockSteamAchievement(string name)
    {
        if(SteamManager.Initialized)
        {
            SteamUserStats.GetAchievement(name, out Check_It); //Checks to see whether the achievement has been unlocked or not
            if (!Check_It)
            {
                SteamUserStats.SetAchievement(name); //Sets teh achievement
                SteamUserStats.StoreStats(); //Notifies the Stema server that it has been updated. Otherwise the achievement wont be updated until the player exits the game.
            }
        }
    }

}

