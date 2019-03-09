using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;

public class GameControlls : MonoBehaviour
{

    [SerializeField]
    private GameObject loadingScreenPrefab;
    public static GameControlls Instance { get; private set; }
    public bool canPullTriggerForWeapons = true;
    public bool gameIsLoading { get { return _gameIsLoading; } }
    private bool _gameIsLoading = false;
    public int gameLevel { get { return _gameLevel; } }
    [SerializeField]
    private int _gameLevel = 1;
    AsyncOperation loadingScene;
    SteamVR_TrackedObject[] trackedObjs;
    private const string fileName = "data.gl";
    private string filePath;

    [HideInInspector]
    bool introVideoPlayed = false; //Set to true once the intro video has been played

    [HideInInspector]
    List<Mission> missionList = new List<Mission>();

    //Mission list is here because this one centralized object holds the list of missions in the game
    Mission trafalgarSquare = new Mission("TrafalgarSquare", false);
    Mission eastenders = new Mission("Eastenders", false);
    Mission nightOnTheTown = new Mission("NightOnTheTown", false);
    Mission thePalace = new Mission("ThePalace", false);
    Mission rattwurmMansion = new Mission("RattwurmsMansion", false);
    Mission streetsOfLondon = new Mission("StreetsOfLondon", true);
    Mission skillingtonArms = new Mission("SkillingtonArms", false);
    Mission theTower = new Mission("Tower_of_London", false);
    Mission theSewers = new Mission("TheSewers", false);
    Mission limeHouseWarehouse = new Mission("LimehouseWarehouse", false);


    //Lists for storing which cylinders are unlocked and which need to be unlocked
    [HideInInspector]
    private List<string> cylinderNumbersToActivate = new List<string>(); //These are cylinders that need to be unlocked
    [HideInInspector]
    private List<string> unlockedCylinders = new List<string>();

    //Weapon related variables
    [HideInInspector]
    public int steamHammerWeaponMK = 0; //Initial MK values for all the weapons. Think of these as indexes, 0,1,2 .... not 1,2,3
    [HideInInspector]
    public int steamCannonWeaponMK = 0;
    [HideInInspector]
    public int steamMagnetWeaponMK = 0;
    [HideInInspector]
    public int steamRailGunWeaponMK = 0;
    [HideInInspector]
    public int teslaRayWeaponMK = 0;
    [HideInInspector]
    public int steamSawWaponMK = 0;

    Dictionary<string, int> weaponsAndMKValues = new Dictionary<string, int>();

    [HideInInspector]
    private List<EndOfLevelUnlockManager.WeaponUpgrade> weaponsToUpgrade = new List<EndOfLevelUnlockManager.WeaponUpgrade>(); //weapons that are not yet upgraded

    /// <summary>
    /// Initially creates a dictionary for eaach weapon and its MK Value. Eg, STEAM_HAMMER : 0, STEAM_CANNON : 2. This is called once we instantiate the GameController so we always have a dict
    /// of weapons and their values.
    /// </summary>
    void InitializeWeaponValues()
    {
        weaponsAndMKValues.Add("STEAM_HAMMER", steamHammerWeaponMK);
        weaponsAndMKValues.Add("STEAM_CANNON", steamCannonWeaponMK);
        weaponsAndMKValues.Add("STEAM_MAGNET", steamMagnetWeaponMK);
        weaponsAndMKValues.Add("STEAM_RAILGUN", steamRailGunWeaponMK);
        weaponsAndMKValues.Add("TESLA_RAY", teslaRayWeaponMK);
        weaponsAndMKValues.Add("STEAM_SAW", steamSawWaponMK);
    }

    public Dictionary<string, int> ReturnWeaponMKValuesDictionary()
    {
        return weaponsAndMKValues;
    }

    /// <summary>
    /// Reutns the MK value for the weapon you pass in. 
    /// </summary>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public int ReturnAWeaponsMKValue(string weaponName)
    {
        return weaponsAndMKValues[weaponName];
    }

    /// <summary>
    /// Called by the WeaponMKManager. Updates the weaponsAndMKValues dictionary after the WeaponMKManager has made the changes. Eg we can change STEAM_HAMMER : 0, to STEAM_HAMMER : 1 if an MK
    /// upgrade has been unlocked
    /// </summary>
    /// <param name="weaponToUpgrade"></param>
    /// <param name="newMKValue"></param>
    public void UpgradeWeaponValue(string weaponToUpgrade, int newMKValue)
    {
        if (weaponsAndMKValues.ContainsKey(weaponToUpgrade))
        {
            weaponsAndMKValues[weaponToUpgrade] = newMKValue;

            Debug.Log("Upgraded weapon called " + weaponToUpgrade + " :: and the value it was given is " + newMKValue + ":: and steamHammerMK is now " + steamHammerWeaponMK);
        }
    }
    [HideInInspector]
    private int currentDifficulty = 2; //Difficulty enum that stores the difficulty value//default value for medium
    [HideInInspector]
    private int currentGameMode; //Stores teh current game mode 
    [HideInInspector]
    private string currentMission;

    void Awake()
    {
        if (!Instance)
        {
            // Instance = this;
            string pathToDirectoryWithSaveFile = Directory.GetParent(Application.dataPath).ToString(); //Gets the path to the where the save file will be if it exists

            if (System.IO.File.Exists(pathToDirectoryWithSaveFile + "/" + fileName) == false) //no save file exists
            {
                Instance = this; //NO SAVE FILE EXISTS, SO CREATE A NEW GAME CONTROLLER                
            }
            else
            { //A save file exists so load the saved data
                Instance = this;
                Load();
            }



            //LoadGameLevelFromFile();
            InitializeWeaponValues(); //Creates a dictionary for eaach weapon and its MK value. That way the GC ALWAYS has a dictionary of weapons and their value. 
            PopulateMissionList(); //Populates the list of missions once the scene is initially opened. That way it ALWAYS creates a list of missions and whether they are unlocked or not
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }




    /// <summary>
    /// Delegate called each time a level is loaded. If the scene is the blimp scene, the missionList is recalculated to update the unlocked levels 
    /// </summary>
    /// 
    
    public void TryToSave()
    {
        try
        {
            SaveLoad.SaveSerializedGameControlls(fileName);
        }
        catch (Exception e)
        {
            Debug.Log("too soon to save");
        }
    }

   

    /// <summary>
    /// Lets you set whether the video has been played or not
    /// </summary>
    /// <param name="status"></param>
    public void SetIntroVideoPlayedStatus(bool status)
    {
        introVideoPlayed = status;
    }

    /// <summary>
    /// Returns whether or not the intro video has been played
    /// </summary>
    public bool ReturnIntroVideoPlayedStatus()
    {
        return introVideoPlayed;
    }

    /// <summary>
    /// Calls a method in the SaveLoad class to load existing game controller data.
    /// </summary>
    void Load()
    {
        SaveLoad.LoadSerialziedGameControlls(fileName);
    }

    
    public void RestartLevel()
    {
        Color color = new Color(0, 0, 0, 0);
        SteamVR_Fade.View(color, 1);

        ToggleGame(false);
        loadingScene = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        loadingScene.allowSceneActivation = false;
        StartCoroutine(AllowToLoadScene());
    }

    public void LoadGameLevel()
    {
        ToggleGame(false);
        //loadingScene = SceneManager.LoadSceneAsync("Main");
        loadingScene = SceneManager.LoadSceneAsync("Sandbox");
        loadingScene.allowSceneActivation = false;
        StartCoroutine(AllowToLoadScene());
    }

    public void LoadSpecificLevel(string levelToLoad)
    {
        ToggleGame(false);
        //loadingScene = SceneManager.LoadSceneAsync("Main");
        loadingScene = SceneManager.LoadSceneAsync(levelToLoad);
        loadingScene.allowSceneActivation = false;
        StartCoroutine(AllowToLoadScene());
    }

    public void LoadBlimp()
    {
        ToggleGame(false);

        loadingScene = SceneManager.LoadSceneAsync("BlimpScene");
        loadingScene.allowSceneActivation = false;
        StartCoroutine(AllowToLoadScene());
        // RestartLevel();
    }

    IEnumerator AllowToLoadScene()
    {
        yield return new WaitUntil(CheckIfSceneLoaded);
        loadingScene.allowSceneActivation = true;
    }

    bool CheckIfSceneLoaded()
    {
        if (loadingScene != null)
        {
            if (loadingScene.progress >= .9f)
            {
                return true;
            }
        }
        return false;
    }

    public void MakeGameActiveAfterLoad()
    {
        Color color = new Color(0, 0, 0, 0);
        SteamVR_Fade.View(color, .5f);
        ToggleGame(true);
    }

    private void ToggleGame(bool enabled)
    {
        _gameIsLoading = !enabled;
        if (enabled)
        {
            AudioListener.pause = false;
            Time.timeScale = 1f;
        }
        else
        {
            ShowLoadingScreen();
            turnOffTrackedObjs();
            AudioListener.pause = true;
            Time.timeScale = 0f;
        }
    }

    private void turnOffTrackedObjs()
    {
        trackedObjs = UnityEngine.Object.FindObjectsOfType<SteamVR_TrackedObject>();
        foreach (SteamVR_TrackedObject to in trackedObjs)
        {
            to.gameObject.SetActive(false);
        }
    }

    private void ShowLoadingScreen()
    {
        Camera.main.cullingMask = 0;
        Camera.main.cullingMask = 1 << 19;
        Transform parentTransform = (Camera.main.gameObject as GameObject).transform;
        Instantiate(loadingScreenPrefab, parentTransform.position, parentTransform.rotation, parentTransform);

    }

    /// <summary>
    /// Called by the podium manager when the player loads a level that is unlocked
    /// </summary>
    /// <param name="difficulty">Difficulty based on the Difficulty Lever</param>
    /// <param name="gameMode">Game Mode based on the Game Mode lever</param>
    /// <param name="mission"> Mission based on the mission lever</param>
    public void LoadLevelBasedOnLeverValues(string levelName)
    {
        //Can put code here to check set some things based on the diffulty value
        ToggleGame(false);
        //loadingScene = SceneManager.LoadSceneAsync("Main");
        loadingScene = SceneManager.LoadSceneAsync(levelName); //Load the level passed in to the function from the mission lever
        loadingScene.allowSceneActivation = false;
        StartCoroutine(AllowToLoadScene());
    }

    public void ExitGame()
    {
        Application.Quit();
    }


    public int GetCurrentDifficulty()
    {
        return currentDifficulty;
    }

    /// <summary>
    /// Called by the PodiumManager. Tells the GC what difficulty has been selected.
    /// </summary>
    /// <param name="newDifficulty"></param>
    public void SetCurrentDifficulty(VRTK.RotationPrinter.DIFFICULTY newDifficulty)
    {
        currentDifficulty = (int)newDifficulty;
    }

    public int GetCurrentGameMode()
    {
        return currentGameMode;
    }

    /// <summary>
    /// Called by the PodiumManager. Tells the GC what game mode has been selected.
    /// </summary>
    /// <param name="newGameMode"></param>
    public void SetCurrentGameMode(VRTK.RotationPrinter.GAME_MODE newGameMode)
    {
        currentGameMode = (int)newGameMode;
    }

    public void SetCurrentMission(string newMission)
    {
        currentMission = newMission;
    }

    public string GetCurrentMission()
    {
        return currentMission;
    }


    /// <summary>
    /// Adds all missions to mission array. The game manager keeps a list of missions. Called ONCE when we create an instance of the GameController.
    /// </summary>
    void PopulateMissionList() //This is called from the awake function. At the time of calling, only the first level will be unlocked
    {
        if (missionList.Count == 0) //IF NOT LOADED!!!!!!!!
        {
            missionList.Add(trafalgarSquare);
            missionList.Add(eastenders);
            missionList.Add(nightOnTheTown);
            missionList.Add(thePalace);
            missionList.Add(rattwurmMansion);
            missionList.Add(streetsOfLondon);
            missionList.Add(skillingtonArms);
            missionList.Add(theTower);
            missionList.Add(theSewers);
            missionList.Add(limeHouseWarehouse);
        }
    }

    /// <summary>
    /// Returns the list of missions. 
    /// </summary>
    /// <returns></returns>
    public List<Mission> ReturnMissionList()
    {
        return missionList;
    }

    /// <summary>
    /// Allows the cylinderManager to add an unlocked cylinderto the GC's list of unlocked cylinders
    /// </summary>
    /// <param name="cylinderNumber"></param>
    public void AddToUnlockedCylinderList(string cylinderNumber)
    {
        unlockedCylinders.Add(cylinderNumber);
    }

    ///Returns the list of cylinders to be unlocked
    public List<string> ReturnCylindersToBeUnlocked()
    {
        return cylinderNumbersToActivate;
    }

    /// <summary>
    /// Returns the cylinders that are already unlocked. NOT the ones that need to be unlocked after a level is complete.
    /// </summary>
    /// <returns></returns>
    public List<string> ReturnAlreadyUnlockedCylinders()
    {
        return unlockedCylinders;
    }

    /// <summary>
    ///
    /// </summary>
    public void ClearCylindersToBeUpdatedList()
    {
        cylinderNumbersToActivate.Clear();
    }

    /// <summary>
    /// Called by the EndOfLevelManager on each level. Passes in a list of strings (phono cylinder numbers)
    /// </summary>
    /// <param name="cylinderNumbersToUnlock"></param>
    public void AddCylinderNumbersToTempList(List<string> cylinderNumbersToUnlock)
    {
        for (int i = 0; i < cylinderNumbersToUnlock.Count; i++)
        {
            cylinderNumbersToActivate.Add(cylinderNumbersToUnlock[i]);
        }

    }
    /// <summary>
    /// Called at the end of a level when there is a weapon to upgrade. Adds the weaponToUpgrade to a list of weapons that will be upgraded by the WeaponsMKManager
    /// </summary>
    /// <param name="weaponToUpgrade"></param>
    public void AddWeaponToTempList(EndOfLevelUnlockManager.WeaponUpgrade weaponToUpgrade)
    {
        weaponsToUpgrade.Add(weaponToUpgrade);
    }

    /// <summary>
    /// Returns the list of weapons to be upgraded so the WeaponMKManager class can make the required changes
    /// </summary>
    /// <returns></returns>
    public List<EndOfLevelUnlockManager.WeaponUpgrade> ReturnWeaponsToUpgradeList()
    {
        return weaponsToUpgrade;
    }

    /***************************************************** */

    /*REASON THIS IS HERE : The GameController holds a list of every mission in the game. From here you can easily set levels to unlocked or locked by just changing the value up at the top
     * of this script. The reason it is not in the PodiumManager.cs script, is because that handles mission stuff related to the podiums. Eg, turning off the locks, on teh podium if a level
     * is unlocked. That PodiumManager gets the list from this GameController. So the reason this function below is NOT in that script is because it is in no way related to the PODIUMS. This
     * holds the state of each level.
     */
    public void UnlockNextMission(string currentlLevel)
    {
        
        switch (currentlLevel)
        {
            case "TrafalgarSquare":

                break;

            case "Eastenders":

                break;

            case "NightOnTheTown":

                break;

            case "ThePalace":

                break;

            case "RattwurmsMansion":

                break;

            case "StreetsOfLondon":
                missionList[5].levelCompleted = true;
                streetsOfLondon.levelCompleted = true;
                skillingtonArms.unlocked = true;
                missionList[6].unlocked = true;
                theTower.unlocked = true;
                missionList[7].unlocked = true;
                break;

            case "SkillingtonArms": //Nothing is unlocked when we complete this level. 
                skillingtonArms.levelCompleted = true;
                missionList[6].levelCompleted = true;
                break;

            case "Tower_of_London": //When this is completed no other level is unlocked in the early access version
                theTower.levelCompleted = true;
                missionList[7].levelCompleted = true;
                break;

            case "TheSewers":

                break;

            case "LimehouseWarehouse":

                break;
        }
    }

    /**Below is a Serializable Class that is used for saving the GAME CONTROLLER data. When save is called, we create an instance of this class, and then assign the game controller variables to
     * the variables in this class. Then when we deserialize it, we assign the values from this serialized object to the game controller via the AssignSavedValues() method. */

    [System.Serializable]
    public class SerializedGameControlls
    {
        public bool _introVideoPlayed;
        public int _currentDifficulty;
        public int _currentGameMode;
        public string _currentMission;

        public Mission _trafalgarSquare = new Mission();
        public Mission _skillArms = new Mission();
        public Mission _theTower = new Mission();
        public Mission _eastenders = new Mission();
        public Mission _nightOnTheTown = new Mission();
        public Mission _rattwurmsMansion = new Mission();
        public Mission _streetsOfLondon = new Mission();
        public Mission _limehouseMansion = new Mission();
        public Mission _thePalace = new Mission();
        public Mission _theSewers = new Mission();

        //mk values
        public int _steamHammerMK;
        public int _steamCannonMK;
        public int _steamMagnetMK;
        public int _steamRailGunMK;
        public int _teslaRayMK;
        public int _rotarSawMK;

        public List<Mission> _missionList = new List<Mission>();
        public List<string> _unlockedCylinders = new List<string>();

        /// <summary>
        /// This method id called on the SerializedGameControlls object in the SaveLoad class. It takes the saved values from the serialized object and assigns them to the Game Controlls object.
        /// </summary>
        /// <param name="obj"></param>
        public void AssignSavedValues(SerializedGameControlls obj)
        {
            Instance.introVideoPlayed = obj._introVideoPlayed;
            Instance.currentDifficulty = obj._currentDifficulty;
            Instance.currentGameMode = obj._currentGameMode;

            //print("Before: "+Instance.missionList.Count);
            Instance.missionList = obj._missionList;
            //print("After: " + Instance.missionList.Count);

            Instance.unlockedCylinders = obj._unlockedCylinders;
            Instance.trafalgarSquare = obj._trafalgarSquare;
            Instance.skillingtonArms = obj._skillArms;
            Instance.theTower = obj._theTower;
            Instance.eastenders = obj._eastenders;
            Instance.nightOnTheTown = obj._nightOnTheTown;
            Instance.rattwurmMansion = obj._rattwurmsMansion;
            Instance.streetsOfLondon = obj._streetsOfLondon; 
            Instance.theSewers = obj._theSewers;
            Instance.thePalace = obj._thePalace;
            Instance.limeHouseWarehouse = obj._limehouseMansion;

            //Assign saved mk values
            Instance.steamHammerWeaponMK = obj._steamHammerMK;
            Instance.steamCannonWeaponMK = obj._steamCannonMK;
            Instance.steamMagnetWeaponMK = obj._steamMagnetMK;
            Instance.steamRailGunWeaponMK = obj._steamRailGunMK;
            Instance.teslaRayWeaponMK = obj._teslaRayMK;
            Instance.steamSawWaponMK = obj._rotarSawMK;
        }
    }


}