using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// send from mini game
/// </summary>
public enum MiniGameEvent {
	OnMiniGameSetup,
	OnMiniGamePrepare,
	OnMiniGameStart,
	OnMiniGamePause,
	OnMiniGameFailed,
	OnMiniGameSuccessful,
}

/// <summary>
/// send from game controller
/// </summary>
public enum GameEvent {
	SetupMiniGame,
	StartMiniGame,
	SetRayCast,
	SetUsingCard,
	OnLivesChange,
	OnPlayerEnterCube,
	OnPlayerExitCube,
}

public enum UIMsg {
	ShowWandInfo,
	SetGameInfo,
    SetPoints,
}


// player using card in mini game
public enum UsingCard {
	NONE,
	HELP,
	GAMBLE,
	PRACTICE,
    DOUBLEPOINTS,
}

public enum GameColors {
	RED = 0,
	BLUE,
	GREEN,
	YELLOW,
	MAGENTA,
	CYAN,
	UNKNOWN = 99,
}

public class GameController : MonoBehaviour {
	public enum MainState { //States that the game can be in
		UNKNOWN = 0, //Only in this at the start of the game before a game is played. Set in Awake function.
		MAINMENU,
		PREPARE,
		PLAYING,
		MINIGAMEFAILED,
		MINIGAMESUCCESSED,
		FAILED,
		SUCCESSED,
        ABORT,
	}

	private MainState mainState_;

	/// <summary>
	/// the game main state
	/// </summary>
	public MainState GetMainState_ { get { return mainState_; } }

	/// <summary>
	/// game datas
	/// </summary>
	public GameData GameData_; //This is the Scriptable Object that holds the game data such as max lives, number of card uses, mini games etc

	private GameObject[] MiniGames_;    
	private int currentMiniGameIndex_;
	private MiniGameBase currentMiniGame_;
    private List<GameObject> listUnusedGames_;

    private int currentLives_;

	public int GetLives { get { return currentLives_; } }

	private int currentHelpCard_;
    public int GetHelpCardLeft { get { return currentHelpCard_; } }
	private int currentGambleCard_;
    public int GetGambleCardLeft { get { return currentGambleCard_; } }
	private int currentPracticeCard_;
    public int GetPracticeCardLeft { get { return currentPracticeCard_; } }
	private UsingCard usingCard_;

	public UsingCard CurrentUsingCard { get { return usingCard_; } }

	private int playerPassedGameCount_; //How many games the player has completed. DOESNT SEEM TO BE USED FOR ANYTHING

	public Transform PlayerTrans_; 
	private Vector3 playerStartPos_;

	public bool EnableRayCast_;

	public UIMainMenu UIMainMenu_;
    public UISubMenu UISubMenu_;
    public UIInfo UIInfo_;
    public UICardSelection UICardSelection_;
    public UILeaderboards UILeaderboards_;
    public UIGameInfo UIGameInfo_;
    public UIFinalResult UIFinalResult_;
    public GameObject UIGameTitle_;
	public UIGameOver UIGameOver_;

	public GameObject FxFireworks_;
    public LightController lightController_;

    private int currentPoints_;
    public int GetPlayerPoints { get { return currentPoints_; } }
    private string[] savedLeaderboards_;

    void OnEnable () {
		Messenger.AddListener (MiniGameEvent.OnMiniGameFailed.ToString (), MiniGameFailed);
		Messenger.AddListener (MiniGameEvent.OnMiniGameSuccessful.ToString (), MiniGameSuccessful);
	}

	void OnDisable () {
		Messenger.RemoveListener (MiniGameEvent.OnMiniGameFailed.ToString (), MiniGameFailed);
		Messenger.RemoveListener (MiniGameEvent.OnMiniGameSuccessful.ToString (), MiniGameSuccessful);
	}

    void Awake () {
        mainState_ = MainState.UNKNOWN;
		usingCard_ = UsingCard.NONE;
		playerPassedGameCount_ = 0; //Not used
    }

    // Use this for initialization
    void Start () {
        UIMainMenu_.gameObject.SetActive (true);
        UICardSelection_.gameObject.SetActive(false);
        UIGameTitle_.gameObject.SetActive(true);
        UIGameInfo_.gameObject.SetActive(true);
        UIInfo_.gameObject.SetActive(false);
        UISubMenu_.gameObject.SetActive(false);
        UILeaderboards_.gameObject.SetActive(false);
        UIFinalResult_.gameObject.SetActive(false);
        UIGameOver_.gameObject.SetActive (false);
        PrepareGame (); //Called when the scene loads up. This handles creating the list of mini games amongst other things
	}

	#region Prepare Game
	// prepare the game, init datas, random some mini games...

        /// <summary>
        /// Does set up for the game (not the mini game, this session). Called immediately once the game is run. 
        /// </summary>
	void PrepareGame () {		
		currentGambleCard_ = GameData_.MaxGambleCard_; //Sets the number of cards the player has. Set from the GameData SO.
		currentHelpCard_ = GameData_.MaxHelpCard_;  
		currentPracticeCard_ = GameData_.MaxPracticeCard_;
        SetUsingCard(UsingCard.NONE);
        mainState_ = MainState.PREPARE;
        currentPoints_ = GameData_.PointsStart_; //Taken from the Game Data SO
        EnableRayCast_ = true;
        LoadleaderBoards();
        RandomMiniGames (); //
	}

	// generate a list by random mini games. Creates a new minigame object with all the mini games and then places the index at teh start
	void RandomMiniGames () {
		int game_num = GameData_.MiniGamesCount_ < GameData_.MiniGames_.Length ? GameData_.MiniGamesCount_ : GameData_.MiniGames_.Length;
		MiniGames_ = new GameObject[game_num];
		int set_mini_game_index = 0;
		List<GameObject> list_all_mini_games = new List<GameObject> (GameData_.MiniGames_);

		while (game_num > 0) { //Adds all games one by one here. Is random each time
			int rnd_index = Random.Range (0, list_all_mini_games.Count);
			GameObject get_mini_game = list_all_mini_games [rnd_index];
			MiniGames_ [set_mini_game_index] = get_mini_game;
			set_mini_game_index++;
			game_num--;
			list_all_mini_games.Remove (get_mini_game);
		}

        listUnusedGames_ = list_all_mini_games;
       
    }

    /// <summary>
    /// THIS IS CALLED RIGHT AT THE START WHEN THE PLAYER PRESSES THE START UI BUTTON
    /// </summary>
    public void StartGame () { //Called when the player selects the Start option at the start of the game
        UIMainMenu_.gameObject.SetActive(false); //turnsoff main menu
        UISubMenu_.gameObject.SetActive(true); //turns on menu with "Next / Practice / Abort"
        UICardSelection_.gameObject.SetActive(true); //turns on card selection options
        UIInfo_.gameObject.SetActive(true);  // Lives and things like that
        SetLives(GameData_.MaxLives_); //Sets the players lives to the max lives at the beginning of the game
        OnPointsChange(0);
        PrepareMiniGame();
      
    }


	void PrepareMiniGame () {
        //If the current game index is greater than the length of the MiniGame array, then the player has successfully completed the game.
		if (currentMiniGameIndex_ >= MiniGames_.Length) {
			GameSuccessful (); 
			return;
		}
        
        //Gets in here once you click start at teh beginning of the game
        GameObject create_mini_game = GameObject.Instantiate (MiniGames_[currentMiniGameIndex_]); //MiniGames_ stores an array of all the games
		if (create_mini_game != null) {
			currentMiniGame_ = create_mini_game.GetComponent<MiniGameBase> ();
			if (currentMiniGame_ == null) {
				LogSystem.LogError ("Mini game " + create_mini_game.name + " error, can't find the class <MiniGameBase>");
				return;
			}
		}

		usingCard_ = UsingCard.NONE;
		Messenger<string, string>.Broadcast (UIMsg.SetGameInfo.ToString (), currentMiniGame_.GameName_, currentMiniGame_.GameDescription_, MessengerMode.DONT_REQUIRE_LISTENER);
		Messenger.Broadcast (GameEvent.SetupMiniGame.ToString (), MessengerMode.DONT_REQUIRE_LISTENER);
		EnableRayCast_ = true;

		//test
		//MiniGameStart ();
	}

    /// <summary>
    /// 
    /// </summary>
    public void SwitchGame () {
        if (currentMiniGame_ != null) {
            currentMiniGame_.DestroyGame();
            currentMiniGame_ = null;
        } 

        int rnd_index = Random.Range(0, listUnusedGames_.Count);
        MiniGames_[currentMiniGameIndex_] = listUnusedGames_[rnd_index];
        GameObject create_mini_game = GameObject.Instantiate(listUnusedGames_[rnd_index]);
        if (create_mini_game != null) {
            currentMiniGame_ = create_mini_game.GetComponent<MiniGameBase>();
            if (currentMiniGame_ == null) {
                LogSystem.LogError("Mini game " + create_mini_game.name + " error, can't find the class <MiniGameBase>");
                return;
            }
        }

        usingCard_ = UsingCard.NONE;
        Messenger<string, string>.Broadcast(UIMsg.SetGameInfo.ToString(), currentMiniGame_.GameName_, currentMiniGame_.GameDescription_, MessengerMode.DONT_REQUIRE_LISTENER);
        Messenger.Broadcast(GameEvent.SetupMiniGame.ToString(), MessengerMode.DONT_REQUIRE_LISTENER);
    }
	#endregion

    /// <summary>
    /// Called when the player starts a mini game. Called from UISubMenu (as this is when the player chooses Practice or Start or something when outside the cube)
    /// </summary>
	public void MiniGameStart () {
        if (mainState_ != MainState.PLAYING) {
			playerStartPos_ = PlayerTrans_.position;
			SetPlayerEnterCube ();
		}
	}

	// call when a mini game is failed
    /// <summary>
    /// Called when the player fails a mini game. Player loses a life and they exit the cube.
    /// </summary>
	void MiniGameFailed () {        
		mainState_ = MainState.MINIGAMEFAILED;
        lightController_.MainLight_.FailedLight();
        //InputController.Instance.OnGridUp_ += OnWandGridButtonDown;
        int lose_life = -1;

		if (usingCard_ == UsingCard.GAMBLE) { //If the player is using the gamble card, they lose extra lives
            //currentGambleCard_--;
            SetUsingCard (UsingCard.GAMBLE);			
			lose_life = -2;
		} else if (usingCard_ == UsingCard.PRACTICE) {
            //currentPracticeCard_--;
            SetUsingCard (UsingCard.PRACTICE);			
			lose_life = 0;
		} else if (usingCard_ == UsingCard.HELP) {
            //currentHelpCard_--;
            SetUsingCard (UsingCard.HELP);			
		} else if (usingCard_ == UsingCard.DOUBLEPOINTS) {
            OnPointsChange(GameData_.EachGamePointsLose_);
        }

        SetLives (lose_life);

		SetPlayerExitCube ();
	}

	/// <summary>
    /// Called when a game is successfully completed. 
    /// </summary>
	void MiniGameSuccessful () {
		mainState_ = MainState.MINIGAMESUCCESSED; //State is set to successed. This will cause the mini game index to be increased in SetPlayerExitCubeTimeout
        lightController_.MainLight_.SuccessfuleLight(); //Turn lights green
        //InputController.Instance.OnGridUp_ += OnWandGridButtonDown;
        if (usingCard_ == UsingCard.GAMBLE) {
            //currentGambleCard_--;
            SetUsingCard (UsingCard.GAMBLE);
			SetLives (1);			
		} else if (usingCard_ == UsingCard.HELP) {
            //currentHelpCard_--;
            SetUsingCard (UsingCard.HELP);			
		} else if (usingCard_ == UsingCard.PRACTICE) {
            //currentPracticeCard_--;
            currentMiniGameIndex_--;
            SetUsingCard(UsingCard.PRACTICE);
        } else if (usingCard_ == UsingCard.DOUBLEPOINTS) {
            OnPointsChange(GameData_.EachGamePointsWin_);
        }

        SetPlayerExitCube ();
	}

    /// <summary>
    /// Called when game is completed.
    /// </summary>
	void GameSuccessful () {
		mainState_ = MainState.SUCCESSED;
        ShowResult();
		FxFireworks_.SetActive (true);
	}

    /// <summary>
    /// Called once the player has completely lost the game. 
    /// </summary>
	void GameFailed () {
		mainState_ = MainState.FAILED;
        UIGameOver_.SetInfo(mainState_);
    }

	//void OnWandGridButtonDown (WandController wand)
	//{
	//    SceneManager.LoadScene(Application.loadedLevel);
	//}

	public bool SetUsingCard (UsingCard card) {
		bool result = false;
		if (card == usingCard_) {
			usingCard_ = UsingCard.NONE;
			result = false;
		} else {
			//if (card == UsingCard.HELP && currentHelpCard_ <= 0)
			//	return false;
			//else if (card == UsingCard.GAMBLE && currentGambleCard_ <= 0)
			//	return false;
			//else if (card == UsingCard.PRACTICE && currentPracticeCard_ <= 0)
			//	return false;

			usingCard_ = card;
			result = true;
		}
		Messenger<UsingCard>.Broadcast (GameEvent.SetUsingCard.ToString (), usingCard_, MessengerMode.DONT_REQUIRE_LISTENER);
		return result;
	}

	/// <summary>
	/// Set Lives - Add lives to player
	/// </summary>
	/// <param name="value">value that either increases or decreases life. Ef -1 to lose a life</param>
	public void SetLives (int value) {
		currentLives_ += value;
		if (currentLives_ > GameData_.MaxLives_) currentLives_ = GameData_.MaxLives_;
		else if (currentLives_ <= 0) currentLives_ = 0;
		Messenger<int>.Broadcast (GameEvent.OnLivesChange.ToString (), currentLives_, MessengerMode.DONT_REQUIRE_LISTENER);
	}

    /// <summary>
    /// Called from MiniGameStart (which is called when a player is outside the cube and uses the UI to start a game)
    /// </summary>
	public void SetPlayerEnterCube () {
		GameTimer.Instance.AddTimer (0.1f, 0.0f, OnSetPlayerEnterCubeTimeout);

	}

	void OnSetPlayerEnterCubeTimeout (int timer_id, object args) {
		PlayerTrans_.position = Vector3.zero;
		//PlayerTrans_.eulerAngles = Vector3.up * 180.0f;
		currentMiniGame_.PlayerEnteredCube (); //This is called on the current mini game. Eg ACCURACY , sWAP ETC. 
		EnableRayCast_ = false;
		Messenger.Broadcast (GameEvent.OnPlayerEnterCube.ToString (), MessengerMode.DONT_REQUIRE_LISTENER);
    }

    /// <summary>
    /// Called when a player either successfully completes or fails a mini game
    /// </summary>
	public void SetPlayerExitCube () {
		GameTimer.Instance.AddTimer (2.0f, 0.0f, OnSetPlayerExitCubeTimeout);
	}

    /// <summary>
    /// Called by SetPlayerExitCube when a player completes or fails a mini game.
    /// </summary>
    /// <param name="timer_id"></param>
    /// <param name="args"></param>
	void OnSetPlayerExitCubeTimeout (int timer_id, object args) {
		PlayerTrans_.position = playerStartPos_;
		//PlayerTrans_.eulerAngles = Vector3.zero;
		EnableRayCast_ = true;
		Messenger.Broadcast (GameEvent.OnPlayerExitCube.ToString (), MessengerMode.DONT_REQUIRE_LISTENER);

        if (currentMiniGame_ != null) {
            if (mainState_ == MainState.MINIGAMESUCCESSED) { //If the player is exiting the cube and has completed the game, this will be true, and we will increase the miniGameIndex to get a new mini game
                ++currentMiniGameIndex_;
                OnPointsChange(GameData_.EachGamePointsWin_); 
            } else if (mainState_ == MainState.MINIGAMEFAILED) {
                OnPointsChange(GameData_.EachGamePointsLose_);
            }

            currentMiniGame_.DestroyGame();
            currentMiniGame_ = null;
        }

        if (GetLives <= 0) //When lives are all gone, call the GameFailed() function that displays Game Failed UI 
			GameFailed ();
		else
			PrepareMiniGame ();
	}

	void OnTriggerDown (WandController wand) {
		if (mainState_ == MainState.MINIGAMEFAILED) {
			currentMiniGame_.GameSetup ();
			InputController.Instance.OnTriggerDown_ -= OnTriggerDown;
		}
	}

    public void TurnOffAllLights () {
        if (lightController_ != null) lightController_.TurnOffAllLights();
    }

    public void TurnOnAllLights () {
        if (lightController_ != null) lightController_.TurnOnAllLights();
    }

    /// <summary>
    /// Called once the player has either won or failed the game. Calculates the players score. 
    /// </summary>
    void ShowResult () {
        if (mainState_ == MainState.SUCCESSED || mainState_ == MainState.ABORT) {
            if (currentMiniGameIndex_ == 0)
                currentPoints_ = 0;
            else
                currentPoints_ += GetLives * GameData_.EachLifePoints_;
        }

        UIMainMenu_.gameObject.SetActive(false);
        UISubMenu_.gameObject.SetActive(false);
        UICardSelection_.gameObject.SetActive(false);
        UIGameTitle_.SetActive(false);
        UIInfo_.gameObject.SetActive(false);
        UIGameInfo_.gameObject.SetActive(false);

        UIFinalResult_.gameObject.SetActive(true);
        UIFinalResult_.SetPoints(currentPoints_);

        UIGameOver_.gameObject.SetActive(true);
        UIGameOver_.SetInfo(mainState_);
    }

    public void GameAbort () {
        mainState_ = MainState.ABORT;
        ShowResult();
    }

    public void SetLeaderboards (string name, int points) {
        if (points > 0) {
            int insert_index = -1;
            for (int i = 0; i < savedLeaderboards_.Length; ++i) {
                if (savedLeaderboards_[i].Length > 0) {
                    string[] split_s = savedLeaderboards_[i].Split(',');
                    int get_p;
                    if (int.TryParse(split_s[1], out get_p) == true) {
                        if (points > get_p) {
                            insert_index = i;
                            break;
                        }
                    }
                } else {
                    insert_index = i;
                    break;
                }
            }

            if (insert_index >= 0) {
                string tmp_s = string.Empty;
                for (int i = savedLeaderboards_.Length - 1; i > insert_index; --i) {
                    savedLeaderboards_[i] = savedLeaderboards_[i - 1];
                }

                savedLeaderboards_[insert_index] = string.Format("{0},{1}", name, points.ToString());
            }
        }

        SaveLeaderboards();
        UIFinalResult_.gameObject.SetActive(false);
        UILeaderboards_.gameObject.SetActive(true);
        UILeaderboards_.ShowBtn(false);
    }

    /// <summary>
    /// Saves the leaderboards. Need to look in to it more
    /// </summary>
    public void SaveLeaderboards () {
        string key_word = ControllerMgr.Instance.GetGameController.GameData_.LeaderboardsKeyWord_;
        for (int i = 0; i < savedLeaderboards_.Length; ++i) {
            PlayerPrefs.SetString(key_word + i.ToString(), savedLeaderboards_[i]);
        }
    }

    /// <summary>
    /// Loads any existing Leaderboard data from previous sessions
    /// </summary>
    void LoadleaderBoards () {
        savedLeaderboards_ = new string[10];
        string key_word = ControllerMgr.Instance.GetGameController.GameData_.LeaderboardsKeyWord_;
        for (int i = 0; i < savedLeaderboards_.Length; ++i) {
            if (PlayerPrefs.HasKey(key_word + i.ToString()) == true) {
                savedLeaderboards_[i] = PlayerPrefs.GetString(key_word + i.ToString());
            } else {
                savedLeaderboards_[i] = string.Empty;
            }
        }
    }

    /// <summary>
    /// Restarts the game by loading the current level (only one level
    /// )
    /// </summary>
    public void RestartGame () {
        SceneManager.LoadScene(Application.loadedLevel);
    }

    /// <summary>
    /// Adds or removes points to the game for the player
    /// </summary>
    /// <param name="value"></param>
    public void OnPointsChange (int value) {
        currentPoints_ += value;
        Messenger<int>.Broadcast(UIMsg.SetPoints.ToString(), currentPoints_, MessengerMode.DONT_REQUIRE_LISTENER);
    }
}
