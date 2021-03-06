﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Relay
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.

	public class GameManager : MonoBehaviour
	{
		public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
		public float turnDelay = 0.1f;							//Delay between each Player turn.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.

		private Text levelText;									//Text to display current level number.
		private GameObject nighttimeShadow;
		private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
        private IsoBoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		private int level = 0;									//Current level number, expressed in game as "Day 1".
		private int turn = 0;									//Current turn number.

		public Sprite[] timeSprites;
		public const int phaseDuration = 8;
		public int totalPhaseDuration { get { return phaseDuration * 2; } }
		public bool IsDay { get { return ((turn - 1) % totalPhaseDuration) < phaseDuration; } }
		public bool IsNight { get { return !IsDay; } }

        public IsoBoardManager boardManager
		{
			get
			{
				return boardScript;
			}
		}

		//Awake is always called before any Start functions
		void Awake()
		{
			//Check if instance already exists
			if (instance == null)

				//if not, set instance to this
				instance = this;

			//If instance already exists and it's not this:
			else if (instance != this)

				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
				Destroy(gameObject);	

			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);

			//Get a component reference to the attached BoardManager script
            boardScript = GetComponent<IsoBoardManager>();
		}

		void Update()
		{
			if (Input.GetButtonDown("Reload"))
			{
				// Reload current level.
				level--;
				AdvanceLevel();
			}
			else if (Input.GetButtonDown("Skip"))
			{
				// Skip current level.
				AdvanceLevel();
			}
			else if (Input.GetButtonDown("Back"))
			{
				if (level == 1)
				{
					return;
				}

				// Go back one level.
				level = level - 2;
				AdvanceLevel();
			}
			else if (Input.GetButtonDown("EndTurn"))
			{
				EndTurn();
			}
		}

		//this is called only once, and the paramter tell it to be called only after the scene was loaded
		//(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static public void CallbackInitialization()
		{
			//register the callback to be called everytime the scene is loaded
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		//This is called each time a scene is loaded.
		static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			Debug.Log("Scene was loaded.");
			instance.level++;
			instance.InitGame();
		}


		//Initializes the game for each level.
		void InitGame()
		{
			enabled = true;

			levelImage = GameObject.Find("LevelImage");
			levelText = GameObject.Find("LevelText").GetComponent<Text>();
			nighttimeShadow = GameObject.Find ("NighttimeShadow");
			nighttimeShadow.SetActive (false);
			levelText.text = "Level " + level;
			levelImage.SetActive(true);

			Invoke("HideLevelImage", levelStartDelay);

			boardScript.currentBoardAnimals.Clear();

			TextAsset board = Resources.Load(level.ToString()) as TextAsset;
			boardScript.SetupScene(board);
		}


		//Hides black image used between levels
		void HideLevelImage()
		{
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);
		}

		//call this to test if the game is over
		public bool TestGameWon()
		{
			if (boardScript.IsGameWon())
			{
				//Set levelText to display number of levels passed and game over message
				levelText.text = "You beat Level " + level + " in " + turn + " turns!";

				//Enable black background image gameObject.
				levelImage.SetActive(true);

				//Disable this GameManager.
				enabled = false;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void EndTurn()
		{
			if (TestGameWon())
			{
				return;
			}

			turn++;
			string turnText = " Turn: " + turn;

			var turnIndicator = GameObject.Find("TurnIndicator").GetComponent<Text>();
			turnIndicator.text = turnText;

			nighttimeShadow.SetActive(!IsDay);

			var phaseIndicator = GameObject.Find("PhaseIndicator").GetComponent<Image>();

			// Every 2 turns, increment timeSprite.
			phaseIndicator.sprite = timeSprites[(int) Mathf.Floor((turn - 1)/2) % timeSprites.Length];

			// Make animals transparent if they are not active.
			boardScript.UpdateAnimalActiveUI();
		}	

		public void AdvanceLevel()
		{
			turn = 0;
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
	}
}

