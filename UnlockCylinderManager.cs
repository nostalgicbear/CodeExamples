using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockCylinderManager : MonoBehaviour {
    private List<GameObject> cylinderList = new List<GameObject>();
    private List<GameObject> unlockedCylinders = new List<GameObject>();


    private List<string> cylinderNumbersToActivate = new List<string>();

    GameControlls gameControlls;

    // Use this for initialization
    void Start () {
        gameControlls = GameObject.Find("[DONT_DESTROY_ON_LOAD]").GetComponent<GameControlls>();
        PopulateCylinderList(); //Find all the cylinders
        UpdateUnlockedCylinders();

	}

    /// <summary>
    /// Stores a list of all cylinders
    /// </summary>
    void PopulateCylinderList()
    {
        foreach (GameObject g in FindObjectsOfType<GameObject>())
        {
            if (g.GetComponent<VRTK.PhonographCylinder>() != null)
            {
                cylinderList.Add(g);
            }
        }
    }

    /// <summary>
    /// Takes the list of unlocked cylinders from the GC , and updates their unlocked value
    /// Goes through and sets teh values for cylinders that are ALREADY unlocked
    /// Goes through and sets the values for cylinders that need to be unlocked (based on the results of the last level)
    /// </summary>
    void UpdateUnlockedCylinders()
    {
        List<string> alreadyUnlockedCylinders = gameControlls.ReturnAlreadyUnlockedCylinders(); 

        for (int i = 0; i < cylinderList.Count; i++)
        {
            for (int j = 0; j < alreadyUnlockedCylinders.Count; j++)
            {
                string title = "PhonographCylinder" + alreadyUnlockedCylinders[j];
                if (cylinderList[i].name == title)
                {
                    if (cylinderList[i].GetComponent<VRTK.PhonographCylinder>().unlocked == false) //if its not already unlocked
                    {
                        cylinderList[i].GetComponent<VRTK.PhonographCylinder>().unlocked = true;
                    }
                    
                }
            }
        }

        List<string> cylindersToBeUnlocked = gameControlls.ReturnCylindersToBeUnlocked();

        for (int k = 0; k < cylinderList.Count; k++)
        {
            for (int l = 0; l < cylindersToBeUnlocked.Count; l++)
            {
                string title = "PhonographCylinder" + cylindersToBeUnlocked[l];
                if (cylinderList[k].name == title)
                {                    
                    if (cylinderList[k].GetComponent<VRTK.PhonographCylinder>().unlocked == false)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.CYLINDERS_NAMES[l]);
                        cylinderList[k].GetComponent<VRTK.PhonographCylinder>().unlocked = true;
                        gameControlls.AddToUnlockedCylinderList(cylindersToBeUnlocked[l]);
                    }
                    

                }
            }
        }

        gameControlls.ClearCylindersToBeUpdatedList();
        CalculateUnlockedPhonographCylindersMeshes();
    }


    /// <summary>
    /// Cycles through each phonograpgh cylinder and checks its unlocked value. If true, then it enables the cylinder. This relates to the physical activation and deactivation of the cylinders
    /// eg box colliders, scripts etc
    /// </summary>
    void CalculateUnlockedPhonographCylindersMeshes()
    { 
        for (int i = 0; i < cylinderList.Count; i++)
        {
            if (cylinderList[i].GetComponent<VRTK.PhonographCylinder>().unlocked == true)
            {
                cylinderList[i].GetComponent<VRTK.PhonographCylinder>().isGrabbable = true;
                cylinderList[i].GetComponent<BoxCollider>().enabled = true;
                cylinderList[i].GetComponentInChildren<MeshRenderer>().enabled = true;
            }
            else
            {
                cylinderList[i].GetComponent<VRTK.PhonographCylinder>().isGrabbable = false;
                cylinderList[i].GetComponent<BoxCollider>().enabled = false;
                cylinderList[i].GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }
    }
}
