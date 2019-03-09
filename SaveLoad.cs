using UnityEngine;
using System.Collections;
using System;
using System.IO;
//using System.IO.Stream;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoad
{

    private static string pathToFile = Directory.GetParent(Application.dataPath).ToString(); //Directory where save game exists

    // Use this for initialization
    public static void SaveFile(String filename, System.Object obj)
    {
        try
        {
            Debug.Log("Writing Stream to Disk.");
            Stream fileStream = File.Open(filename, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, obj);
            fileStream.Close();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Save.SaveFile(): Failed to serialize object to a file " + filename + " (Reason: " + e.ToString() + ")");
        }
    }

    /// <summary>
    /// Saves a Serialized version of the GAME CONTROLLER. Saves it as a GameController.SerializedGameController class, which is a custom class.
    /// </summary>
    /// <param name="fileName"></param>
    public static void SaveSerializedGameControlls(String fileName)
    {

        Stream fileStream = File.Open(fileName, FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();

        GameControlls.SerializedGameControlls dataToSerialize = new GameControlls.SerializedGameControlls(); //Create a new object that will hold our data.
        //GameControlls.SerializedGameControlls dataToSerialize = GameControlls.SerializedGameControlls.CreateInstance<GameControlls.SerializedGameControlls>(); //Create a new object that will hold our data.



        dataToSerialize._currentDifficulty = GameControlls.Instance.GetCurrentDifficulty();
        dataToSerialize._introVideoPlayed = GameControlls.Instance.ReturnIntroVideoPlayedStatus();

        //save mission list
        dataToSerialize._missionList = GameControlls.Instance.ReturnMissionList();

        //Debug.Log("Save: " + dataToSerialize._missionList.Count);

        //Save unlocked cylinders
        dataToSerialize._unlockedCylinders = GameControlls.Instance.ReturnAlreadyUnlockedCylinders();

        //Save MK values
        dataToSerialize._steamHammerMK = GameControlls.Instance.ReturnAWeaponsMKValue("STEAM_HAMMER");
        dataToSerialize._steamCannonMK = GameControlls.Instance.ReturnAWeaponsMKValue("STEAM_CANNON");
        dataToSerialize._steamMagnetMK = GameControlls.Instance.ReturnAWeaponsMKValue("STEAM_MAGNET");
        dataToSerialize._steamRailGunMK = GameControlls.Instance.ReturnAWeaponsMKValue("STEAM_RAILGUN");
        dataToSerialize._teslaRayMK = GameControlls.Instance.ReturnAWeaponsMKValue("TESLA_RAY");
        dataToSerialize._rotarSawMK = GameControlls.Instance.ReturnAWeaponsMKValue("STEAM_SAW");

        formatter.Serialize(fileStream, dataToSerialize);
        fileStream.Close();
    }

    /// <summary>
    /// Loads the Serialized object that holds the saved GAME CONTROLLER data. Deserialzied it as a GameController.SerializedGameController object, which is a custom made object. It then
    /// calls a method that assigns the deserialized objects values to the GameController so it has the saved values.
    /// </summary>
    /// <param name="filename"></param>
    public static void LoadSerialziedGameControlls(String filename)
    {
        Stream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read);
        BinaryFormatter formatter = new BinaryFormatter();

        GameControlls.SerializedGameControlls data = (GameControlls.SerializedGameControlls)formatter.Deserialize(fileStream);
        fileStream.Close();
        // Debug.Log("Load " + data._missionList.Count);
        data.AssignSavedValues(data); //Calls a method that assigns the saved data to the game controller so we have the saved values.
    }



    public static System.Object LoadFile(String filename)
    {
        try
        {
            Debug.Log("Reading Stream from Disk.");
            Stream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read);
            BinaryFormatter formatter = new BinaryFormatter();
            System.Object obj = formatter.Deserialize(fileStream);
            fileStream.Close();
            return obj;
        }
        catch (Exception e)
        {
            Debug.LogWarning("SaveLoad.LoadFile(): Failed to deserialize a file " + filename + " (Reason: " + e.ToString() + ")");
            return null;
        }
    }

    /// <summary>
    /// Removes a save file. Called when a save file exists but the player chooses new game
    /// </summary>
    /// <param name="fileName"></param>
    public static void RemoveSaveFile(string fileName)
    {
        string fileToDelete = pathToFile + "/" + fileName;
        File.Delete(fileToDelete);
    }


}
