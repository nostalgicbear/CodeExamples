using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to the podium and its values. Sends the difficulty, gameMode, mission, and gameStart values to the gameControlls script. 
/// </summary>
public class PodiumManager : MonoBehaviour
{

    /* References to the different podiums so we can use their lever values*/
    public VRTK.RotationPrinter difficultyPodium;
    public VRTK.RotationPrinter gameModePodium;
    public VRTK.RotationPrinter missionPodium;
    public VRTK.RotationPrinter gameStartPodium;

    List<Mission> missionList = new List<Mission>();
    //  Mission london = new Mission("StreetsOfLondon", false);


    void Start()
    {
        missionList = GameControlls.Instance.ReturnMissionList(); //Gets the up to date mission list each time the scene loads
        StartCoroutine(WaitUntilMissionListNotEmpty());
    }

    IEnumerator WaitUntilMissionListNotEmpty()
    {
        yield return new WaitUntil(MissionListNotEmpty);
       // Debug.Log("Mision List Not Empty: " + missionList.Count);
        CalculateUnlockedMissions(); //After getting the list, it then calculates to see what levels are now unlocked
    }

    public bool MissionListNotEmpty()
    {
        return missionList.Count > 0;
    }

    /// <summary>
    /// Test methos for printing values from levers
    /// </summary>
    public void PrintLeverValues()
    {
        Debug.Log("diff : " + difficultyPodium.ReturnDifficulty() + "  ::: gm : " + gameModePodium.ReturnGameMode() + "   :::   miss : " + missionPodium.ReturnMission());
    }

    /// <summary>
    /// Returns a mission based on the string passed in
    /// </summary>
    public Mission ReturnMission(string missionToReturnName)
    {

        Mission missToReturn = new Mission();

        for (int i = 0; i < missionList.Count; i++)
        {
            if (missionList[i].missionName == missionToReturnName)
            {
                missToReturn = missionList[i];
            }
        }
        return missToReturn;
    }

    /// <summary>
    /// Method that returns whether or not the currently selected mission is unlocked
    /// </summary>
    /// <returns></returns>
    public bool CheckIsCurrentMissionUnlocked()
    {
      
        bool isUnlocked = missionPodium.ReturnCurrentMissionObject().unlocked;
       // Debug.Log("Checking that current mission is unlocked: "+isUnlocked);
        return isUnlocked;
    }

    /// <summary>
    /// Called by the gameStart podium when the level is set on teh GO section and the level is unlocked. Passes in the mission to load
    /// </summary>
    /// <param name="levelName"></param>
    public void LoadLevelBasedOnLeverValues()
    {
        SendPodiumValuesToGameController();
        GameControlls.Instance.LoadLevelBasedOnLeverValues(missionPodium.ReturnMission());
    }

    /// <summary>
    /// Cycles through missions to see which are unlocked. If a mission is unlocked, its lock symbol is turned off.
    /// </summary>
    void CalculateUnlockedMissions()
    {

        for (int i = 0; i < missionList.Count; i++)
        {
           // print(missionList[i].missionName + " " + missionList[i].unlocked);
            if (missionList[i].unlocked == true)
            {
                foreach (Transform t in missionPodium.transform.parent.GetComponentInChildren<Transform>())
                {
                    if (t.name == missionList[i].missionName + "Lock")
                    {
                        //t.GetComponent<Renderer>().enabled = false;
                        t.gameObject.SetActive(false);
                    }
                }
            }
        }

        // gameControlls.UpdateMissionList(missionList); //After updating the list. Send the updated list back to the gameControlls so it has an up to date reference.
    }

    /// <summary>
    /// Sends the podium values to the game controller so it knows what difficulty, game mode, and mission have been selected
    /// </summary>
    public void SendPodiumValuesToGameController()
    {
        GameControlls.Instance.SetCurrentGameMode(gameModePodium.ReturnGameMode());
        GameControlls.Instance.SetCurrentDifficulty(difficultyPodium.ReturnDifficulty());
      //  print("Mission podium: "+missionPodium.ReturnMission());
        GameControlls.Instance.SetCurrentMission(missionPodium.ReturnMission());
    }

}
