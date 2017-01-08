//One with all enums

namespace VRTK
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Placed on each individual lever. This script has two use cases. It allows the player to grab the levers (because it inherits from VRTK_InteractableObject) and it also manages everything to 
    /// do with calculating the value the lever has selected
    /// </summary>
     [ExecuteInEditMode]
    public class RotationPrinter : VRTK_InteractableObject
    {
        Vector3 angle; //angle the lever is at. 0-360 degrees.

        string missionValue = ""; //Mission is a string rather than enum because its easier and quicker to just compare the name to the name in build settings

        [SerializeField]
        GameControlls gameControlls; //Reference to the GameController script. Only used by the GAME START podium (the one with "GO").

        public enum LEVERTYPE //Different podiumns will control different things. Eg one for the level select, one for difficulty select etc
        {
            DIFFICULTY,
            GAME_MODE,
            MISSION,
            GAME_START

        };

        [HideInInspector]
        public enum DIFFICULTY
        {
            LIGHTWEIGHT,
            EASY,
            MEDIUM,
            HARD,
            DEATH_LIKELY
        };

        [HideInInspector]
        public DIFFICULTY difficulty;

        [HideInInspector]
        public enum GAME_MODE
        {
            STORY,
            DLC,
            BRING_A_FRIEND,
            SMASHING_FUN,
            SEEK_AND_DESTROY
        };

        [HideInInspector]
        public GAME_MODE gameMode;

        /** All the default values below relate to the position to set the levers when they are let go by a player */
        #region default values for the DIFFICULTY PODIUM
        Vector3 defaultLightweight = new Vector3(290.5f, 180, 180);
        Vector3 defaultEasy = new Vector3(282.6f, 0, 0);
        Vector3 defaultMedium = new Vector3(0, 0, 0);
        Vector3 defaultHard = new Vector3(77, 0, 0);
        Vector3 defaultDeathLikely = new Vector3(65, 180, 180);
        #endregion

        #region default values for the GAME_MODE podium
        Vector3 defaultStoryMode = new Vector3(0, 0, 0);
        Vector3 defaultDLC = new Vector3(76.2f, 0, 0);
        Vector3 defaultBringAFriend = new Vector3(67.5f, 180, 180);
        Vector3 defaultSmashingFun = new Vector3(293.5f, 180, 180);
        Vector3 defaultSeekAndDestroy = new Vector3(280.3f, 0, 0);
        #endregion

        #region default values for the MISSION podium
        Vector3 defaultTrafalgarSquare = new Vector3(15.5f, 0, 0);
        Vector3 defaultEastenders = new Vector3(35.3f, 0, 0);
        Vector3 defaultNightOnTheTown = new Vector3(59.1f, 0, 0);
        Vector3 defaultThePalace = new Vector3(89.3f, 0, 0);
        Vector3 defaultRattwurmsMansion = new Vector3(53.7f, 180, 180);
        Vector3 defaultStreetsOfLondon = new Vector3(303.9f, 180, 180);
        Vector3 defaultSkillingtonArms = new Vector3(272.2f, 180, 180);
        Vector3 defaultTheTower = new Vector3(294.2f, 0, 0);
        Vector3 defaultTheSewers = new Vector3(321.2f, 0, 0);
        Vector3 defaultLimehouseMansion = new Vector3(347.8f, 0, 0);
        #endregion

        #region default values for the GAMESTART podium
        Vector3 defaultStop = new Vector3(272.7f, 180, 180);
        Vector3 defaultInToBattle = new Vector3(66.9f, 180, 180);
        Vector3 defaultSabatical = new Vector3(294.4f, 180, 180);
        Vector3 defaultNone = new Vector3(0.8f, 180, 180);
        #endregion

        #region levelNames string values
        string trafalgarSquare = "TrafalgarSquare";
        string eastenders = "Eastenders";
        string nightOnTheTown = "NightOnTheTown";
        string thePalace = "ThePalace";
        string rattwurmMansion = "RattwurmsMansion";
        string streetsOfLondon = "StreetsOfLondon";
        string skillingtonArms = "SkillingtonArms";
        string theTower = "TheTower";
        string theSewers = "TheSewers";
        string limeHouseWarehouse = "LimehouseWarehouse";
        string london = "StreetsOfLondon"; //This is the section that says "Bellingers Telegraph". I just set this default to StreetsOfLondon.
        #endregion

        public LEVERTYPE leverType;

        private enum GAME_START_MODE
        {
            GO,
            STOP,
            INTO_BATTLE,
            SABATICAL_MODE,
            NONE
        };

        private GAME_START_MODE gameStartMode;

        bool levelLoadAttempted = false; //Once the player puts the final lever in the "GO" position, that counts as an attempt to load the level


        // Use this for initialization
        void Start()
        {
            if (leverType != LEVERTYPE.GAME_START)
            {
                SetLeverValue(); //Get initial values from the levers at the start of teh scene in case the player doesnt move any before starting the game
            }
           
        }

        /// <summary>
        /// Each time a lever is moved we will get the value it lands on. This is only done at the start of the scene to get default values, and when a player leaves go of a lever. Depending on 
        /// whether the lever is for the DIFFICULTY, GAME MODE, MISSION, or GAME START podium, it will call a method to store the relevant value (eg calls SelectDifficulty if its the DIFFICULTY podium)
        /// </summary>
        private void SetLeverValue()
        {
            switch (leverType)
            {
                case LEVERTYPE.DIFFICULTY:
                    SelectDifficulty();
                    break;

                case LEVERTYPE.GAME_MODE:
                    SelectGameMode();
                    break;

                case LEVERTYPE.MISSION:
                    SelectMission();
                    break;

                case LEVERTYPE.GAME_START:
                    SelectGameStart();
                    break;
            }
        }

        /// <summary>
        /// Overrides the VRTK Ungrabbed function. We also stop the levers velocity (to prevent drift), and call the SETLEVERVALUE function.
        /// </summary>
        /// <param name="previousGrabbingObject"></param>
        public override void Ungrabbed(GameObject previousGrabbingObject)
        {
            base.Ungrabbed(previousGrabbingObject);
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            SetLeverValue();

        }

        /// <summary>
        /// Sets the difficulty value based on rotation of the lever of the podium that has DIFFICULTY selected from the LEVER TYPE enum dropdown. 
        /// The x valeus refer to the rotation of the lever
        /// Teh Y values ideally should only ever be 0 or 180, but the reason I give a bit of leeway is because its often not exactly 0 or exactly 180
        /// </summary>
        void SelectDifficulty()
        {
            if (leverType == LEVERTYPE.DIFFICULTY)
            {
                angle = gameObject.transform.localRotation.eulerAngles;
                if (angle.x >= 52.1 && angle.x <= 89.9)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        difficulty = DIFFICULTY.HARD;
                    }
                }

                if (angle.x <= 90.0 && angle.x >= 75.6)
                {
                    if (angle.y >= 260.0 && angle.y <= 300.0)
                    {
                        difficulty = DIFFICULTY.HARD;
                    }
                }

                if (angle.x <= 75.5 && angle.x >= 59.1)
                {
                    if (angle.y >= 160.0 && angle.y <= 200.0)
                    {
                        difficulty = DIFFICULTY.DEATH_LIKELY;
                    }
                }

                if (angle.x <= 59.0 && angle.x >= 0.1)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        difficulty = DIFFICULTY.DEATH_LIKELY; //This is when the lever is on the "Bellinger Telegraph" part.  Its closer to DEATH LIKELY than LIGHTWEIGHT so I just set it to DEATH LIKELY
                    }
                }

                if (angle.x <= 359.9 && angle.x >= 301.7)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        difficulty = DIFFICULTY.LIGHTWEIGHT; //This is when the lever is on the "Bellinger Telegraph" part. Its closer to LIGHTWEIGHT than DEATH LIKELY so I just set it to LIGHTWEIGHT
                    }
                }

                if (angle.x <= 301.6 && angle.x >= 285.0)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        difficulty = DIFFICULTY.LIGHTWEIGHT;
                    }
                }

                if (angle.x <= 284.9 && angle.x >= 270.1)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        difficulty = DIFFICULTY.EASY;
                    }
                }

                if (angle.x >= 269.9 && angle.x <= 307.0)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        difficulty = DIFFICULTY.EASY;
                    }
                }

                if (angle.x >= 307.1 && angle.x <= 359.9)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        difficulty = DIFFICULTY.MEDIUM;
                    }
                }

                if (angle.x >= 0.1 && angle.x <= 52.1)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        difficulty = DIFFICULTY.MEDIUM;
                    }
                }
                DifficultySnapToCentre(difficulty);
               // Debug.Log("diff val  " + transform.localRotation.eulerAngles);
            }
        }

        /// <summary>
        /// Selects the game mode based on the lever rotation of the podium that has GAME_MODE selected from the LEVER TYPE enum dropdown
        /// </summary>
        private void SelectGameMode()
        {
            if (leverType == LEVERTYPE.GAME_MODE)
            {
                angle = gameObject.transform.localRotation.eulerAngles;
                #region GameMode DL Soon
                if (angle.x >= 54.8 && angle.x <= 90.0)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        gameMode = GAME_MODE.DLC;
                    }
                }

                if (angle.x <= 90.0 && angle.x >= 75.6)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameMode = GAME_MODE.DLC;
                    }
                }
                #endregion

                if (angle.x <= 75.5 && angle.x >= 58.8)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameMode = GAME_MODE.BRING_A_FRIEND;
                    }
                }

                if (angle.x <= 58.7 && angle.x >= 0.0)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameMode = GAME_MODE.BRING_A_FRIEND; //This is the section that says "Bellingers Telegraph". Its closer to BRINGAFRIEND than to SMASHINGFUN so I set it to BRINGAFRIEND
                    }
                }

                if (angle.x <= 359.9 && angle.x >= 300.2)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameMode = GAME_MODE.SMASHING_FUN; //This is the section that says "Bellingers Telegraph". Its closer to SMASHING_FUN than BRINGAFRIEND so I set it to SMASHINGFUN
                    }
                }

                if (angle.x <= 300.1 && angle.x >= 285.4)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameMode = GAME_MODE.SMASHING_FUN;
                    }
                }

                if (angle.x <= 285.3 && angle.x >= 270.1)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameMode = GAME_MODE.SEEK_AND_DESTROY;
                    }
                }

                if (angle.x >= 270.0 && angle.x <= 306.3)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        gameMode = GAME_MODE.SEEK_AND_DESTROY;
                    }
                }

                if (angle.x >= 306.4 && angle.x <= 359.9)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        gameMode = GAME_MODE.STORY;
                    }
                }

                if (angle.x >= 0.0 && angle.x <= 54.7)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        gameMode = GAME_MODE.STORY;
                    }
                }
                GameModeSnapToCentre(gameMode);
                //   Debug.Log("Game mode value is " + gameModeValue);
            }
        }

        /// <summary>
        /// Selects the mission based on the lever rotation of the podium with MISSION selected from the enum dropdown
        /// </summary>
        private void SelectMission()
        {
            if (leverType == LEVERTYPE.MISSION)
            {
                angle = gameObject.transform.localRotation.eulerAngles;
                if (angle.x >= 0.0 && angle.x <= 28.7)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = trafalgarSquare;
                    }
                }

                if (angle.x >= 28.8 && angle.x <= 47.7)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = eastenders;
                    }
                }

                if (angle.x >= 47.8 && angle.x <= 73.0)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = nightOnTheTown;
                    }
                }

                if (angle.x >= 73.1 && angle.x <= 89.9)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = thePalace;
                    }
                }

                if (angle.x <= 90.0 && angle.x >= 73.5)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        missionValue = thePalace;
                    }
                }

                if (angle.x <= 73.4 && angle.x >= 33.6)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        missionValue = rattwurmMansion;
                    }
                }

                if (angle.x <= 33.5 && angle.x >= 0.0)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        missionValue = london;
                    }
                }

                if (angle.x <= 359.9 && angle.x >= 320.6)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        missionValue = london;
                    }
                }

                if (angle.x <= 320.5 && angle.x >= 287.4)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        missionValue = streetsOfLondon;
                    }
                }

                if (angle.x <= 287.3 && angle.x >= 270.1)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        missionValue = skillingtonArms;
                    }
                }

                if (angle.x >= 270.1 && angle.x <= 278.6)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = skillingtonArms;
                    }
                }

                if (angle.x >= 278.7 && angle.x <= 306.0)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = theTower;
                    }
                }

                if (angle.x >= 306.1 && angle.x <= 336.8)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = theSewers;
                    }
                }

                if (angle.x >= 336.9 && angle.x <= 359.9)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        missionValue = limeHouseWarehouse;
                    }
                }

                MissionSnapToCentre(missionValue);

                // Debug.Log("miss val  " + transform.localRotation.eulerAngles);

                //  Debug.Log("Mission is " + missionValue);
            }
        }

        /// <summary>
        /// Checks the value that the GAME START PODIUM lever points to. If it is in the GO section, then it use the values of the other levers to load a game
        /// </summary>
        private void SelectGameStart()
        {
            if (leverType == LEVERTYPE.GAME_START)
            {
                angle = gameObject.transform.localRotation.eulerAngles;
                if (angle.x <= 74.6 && angle.x >= 57.2)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameStartMode = GAME_START_MODE.INTO_BATTLE;
                    }
                }

                if (angle.x <= 57.1 && angle.x >= 0.0)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameStartMode = GAME_START_MODE.NONE; //This is the section that says "Bellingers Telegraph". 
                    }
                }

                if (angle.x <= 359.9 && angle.x >= 302.1)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameStartMode = GAME_START_MODE.NONE;//This is the section that says "Bellingers Telegraph". 
                    }
                }

                if (angle.x <= 302.0 && angle.x >= 287.0)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameStartMode = GAME_START_MODE.SABATICAL_MODE;
                    }
                }

                if (angle.x <= 286.9 && angle.x >= 270.2)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameStartMode = GAME_START_MODE.STOP;
                    }
                }

                if (angle.x >= 270.2 && angle.x <= 279.1)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        gameStartMode = GAME_START_MODE.STOP;
                    }
                }

                if (angle.x >= 279.3 && angle.x <= 359.9)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        gameStartMode = GAME_START_MODE.GO;
                          AttemptToLoadLevel();
                    }
                }

                if (angle.x >= 0.0 && angle.x <= 89.9)
                {
                    if (angle.y >= -15.0 && angle.y <= 15.0)
                    {
                        gameStartMode = GAME_START_MODE.GO;
                         AttemptToLoadLevel();
                    }
                }

                if (angle.x <= 90.0 && angle.x >= 74.7)
                {
                    if (angle.y >= 160.0 && angle.y <= 195.0)
                    {
                        gameStartMode = GAME_START_MODE.GO;
                         AttemptToLoadLevel();
                    }
                }

                GameStartSnapToCentre(gameStartMode);

            }
        }

        /// <summary>
        /// Called when the player lets go of the lever in the GREEN AREA on the final podium. Calls a function that loads the level 
        /// </summary>
        void AttemptToLoadLevel()
        {
            if (!levelLoadAttempted)
            {
                levelLoadAttempted = true;
                gameControlls.LoadLevelBasedOnLeverValues();
            }
        }

        /// <summary>
        /// Called when the player leaves go of the DIFFICULTY podium lever. Will set the lever to the centre of whichever section it is in. Eg, if in MEDIUM, the lever will be set to the 
        /// centre of the MEDIUM section.
        /// </summary>
        /// <param name="diff">The difficulty</param>
        void DifficultySnapToCentre(DIFFICULTY diff)
        {
            Quaternion q = transform.localRotation;
            switch (diff)
            {
                case DIFFICULTY.LIGHTWEIGHT:
                    q.eulerAngles = defaultLightweight;
                    break;

                case DIFFICULTY.EASY:
                    q.eulerAngles = defaultEasy;
                    break;

                case DIFFICULTY.MEDIUM:
                    q.eulerAngles = defaultMedium;
                    break;

                case DIFFICULTY.HARD:
                    q.eulerAngles = defaultHard;
                    break;

                case DIFFICULTY.DEATH_LIKELY:
                    q.eulerAngles = defaultDeathLikely;
                    break;
            }
            transform.localRotation = q;
        }

        /// <summary>
        /// Sets the lever of the GAME MODE podium to the centre of whichever section it is in when the player leaves go of the handle
        /// </summary>
        /// <param name="gm"></param>
        void GameModeSnapToCentre(GAME_MODE gm)
        {
            Quaternion q = transform.localRotation;
            switch (gm)
            {
                case GAME_MODE.STORY:
                    q.eulerAngles = defaultStoryMode;
                    break;

                case GAME_MODE.DLC:
                    q.eulerAngles = defaultDLC;
                    break;

                case GAME_MODE.SMASHING_FUN:
                    q.eulerAngles = defaultSmashingFun;
                    break;

                case GAME_MODE.SEEK_AND_DESTROY:
                    q.eulerAngles = defaultSeekAndDestroy;
                    break;
            }
            transform.localRotation = q;
        }

        void GameStartSnapToCentre(GAME_START_MODE gameStartMode)
        {
            Quaternion q = transform.localRotation;
            switch (gameStartMode)
            {
                case GAME_START_MODE.STOP:
                    q.eulerAngles = defaultStop;
                    break;

                case GAME_START_MODE.INTO_BATTLE:
                    q.eulerAngles = defaultInToBattle;
                    break;

                case GAME_START_MODE.SABATICAL_MODE:
                    q.eulerAngles = defaultSabatical;
                    break;

                case GAME_START_MODE.NONE:
                    q.eulerAngles = defaultNone;
                    break;
            }

            transform.localRotation = q;
        }

        /// <summary>
        /// Sets the lever of the MISSION podium to be in the centre of whicever section it is in when the player lets go of the lever
        /// </summary>
        /// <param name="mission"></param>
        void MissionSnapToCentre(string mission)
        {
            Quaternion q = transform.localRotation;
            switch (mission)
            {
                case "TrafalgarSquare":
                    q.eulerAngles = defaultTrafalgarSquare;
                    break;

                case "Eastenders":
                    q.eulerAngles = defaultEastenders;
                    break;

                case "NightOnTheTown":
                    q.eulerAngles = defaultNightOnTheTown;
                    break;

                case "ThePalace":
                    q.eulerAngles = defaultThePalace;
                    break;

                case "RattwurmsMansion":
                    q.eulerAngles = defaultRattwurmsMansion;
                    break;

                case "StreetsOfLondon":
                    q.eulerAngles = defaultStreetsOfLondon;
                    break;

                case "SkillingtonArms":
                    q.eulerAngles = defaultSkillingtonArms;
                    break;

                case "TheTower":
                    q.eulerAngles = defaultTheTower;
                    break;

                case "TheSewers":
                    q.eulerAngles = defaultTheSewers;
                    break;

                case "LimehouseWarehouse":
                    q.eulerAngles = defaultLimehouseMansion;
                    break;
            }

            transform.localRotation = q;
        }


        /// <summary>
        /// Retunrs the difficulty
        /// </summary>
        /// <returns></returns>
        public DIFFICULTY ReturnDifficulty()
        {
            return difficulty;

        }
        public GAME_MODE ReturnGameMode()
        {
            return gameMode;

        }

        public string ReturnMission()
        {
            return missionValue;

        }
    }
}