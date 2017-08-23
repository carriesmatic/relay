using UnityEngine;
using System;
using System.Collections.Generic;

 		//Allows us to use Lists.
using Random = UnityEngine.Random;

 		//Tells Random to use the Unity Engine random number generator.
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace Relay
{
	public class BoardManager : MonoBehaviour
	{
		public enum AnimalSymbols
		{
			Charlie = 'a',
			Sheila = 'b',
			Spring = 'c',
			Summer = 'd',
			Chorlie = 'e',
			Chirlie = 'f',
		}

		public static class Symbols
		{
			public const char Floor = '.';
			public const char OuterWall = '*';
			public const char Wall = '-';
			public const char Chasm = '#';
			public const char Ocean = '~';
			public const char Player = '@';
		}

		// A hashset of chars that represent animals.
		private HashSet<char> animals = new HashSet<char>();

		// A hashset of chars that represent homes.
		private HashSet<char> homes = new HashSet<char>();

		// A dictionary that represents a mapping of a char to a GameObject.
		private Dictionary<char, GameObject> prefabMap = new Dictionary<char, GameObject>();

		//Width of our game board (usually 10 tiles).
		private int boardWidth = 0;

		//Height of our game board (usually 7 tiles).
		private int boardHeight = 0;

		public GameObject OceanTile;
		public GameObject FloorTile;
		public GameObject OuterWallTile;
		public GameObject WallTile;
		public GameObject ChasmTile;
		public GameObject Player;
		public GameObject Chorlie;
		public GameObject Charlie;
		public GameObject Chirlie;
		public GameObject Sheila;
		public GameObject Spring;
		public GameObject Summer;
		public GameObject ChorlieHome;
		public GameObject CharlieHome;
		public GameObject ChirlieHome;
		public GameObject SheilaHome;
		public GameObject SpringHome;
		public GameObject SummerHome;

		//A variable to store a reference to the transform of our Board object.
		public Transform boardHolder;

		public List<Animal> currentBoardAnimals;

		// The game is won when all animals in the board have gone into their homes.
		public bool IsGameWon()
		{
			return currentBoardAnimals.TrueForAll(animal => animal.IsInHome());
		}

		//Sets up the outer walls and floor (background) of the game board.
		void SetupBoardTiles(string tiles)
		{
			var tileArrays = tiles.Split('\n');
			// the tiles array is read from ASCII top-to-bottom, but Unity's y axis is bottom-to-top, so reverse order
			Array.Reverse (tileArrays);

			//Instantiate Board and set boardHolder to its transform.
			var board = new GameObject("Board");
			board.AddComponent<AspectRatioFitter>();

			var aspectRatioFitter = board.GetComponent<AspectRatioFitter>();
			aspectRatioFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
			aspectRatioFitter.aspectRatio = boardWidth / boardHeight;

			boardHolder = board.transform;

			//Loop along y axis (+ goes up), starting from -1 to place floor or outerwall tiles.
			for (int y = -1; y < boardHeight + 1; y++)
			{
				//Loop along x axis (+ goes right), starting from -1 (to fill corner) with floor or outerwall edge tiles.
				for (int x = -1; x < boardWidth + 1; x++)
				{
					GameObject toInstantiate;

					//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
					if (x == -1 || x == boardWidth || y == -1 || y == boardHeight)
					{
						toInstantiate = OuterWallTile;
					}
					else
					{
						// Instantiate the appropriate tile.
						toInstantiate = prefabMap[tileArrays[y][x]];
					}

					//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
					GameObject instance =
						Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

					//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
					instance.transform.SetParent(boardHolder);
				}
			}
		}

		void SetupBoardObjects(string boardObjects)
		{
			currentBoardAnimals = new List<Animal>();
			foreach (var boardObject in boardObjects.Split('\n'))
			{
				var regex = new Regex(@"([a-zA-Z@]) (\d),(\d)");
				var matches = regex.Matches(boardObject);

				foreach (Match match in matches)
				{
					var tile = match.Groups[1].Value.ToCharArray()[0];
					var x = Int32.Parse(match.Groups[2].Value);
					var y = Int32.Parse(match.Groups[3].Value);

					if (tile == '@')
					{
						GameObject player = GameObject.FindWithTag ("Player");
						player.transform.position = new Vector2 (x, y);
					}
					else
					{
						LayoutObject (tile, x, y);
					}
				}
			}
		}

		// Instantiate the given object at the given vector.
		void LayoutObject(char boardObject, int x, int y)
		{
			if (animals.Contains(boardObject) || homes.Contains(boardObject))
			{
				var toInstantiate = prefabMap[boardObject];
				GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
				instance.transform.SetParent(boardHolder);
				Animal animalComponent = instance.GetComponent<Animal> ();
				if (animalComponent != null)
				{
					currentBoardAnimals.Add(animalComponent);
				}
			}
		}


		void GetBoardSize(string level, out int boardWidth, out int boardHeight)
		{
			boardWidth = 0;
			boardHeight = 0;

			string[] levelRows = level.Split('\n');

			if (!CheckRectangularBoard(levelRows))
			{
				return;
			}

			boardHeight = levelRows.Length;
			boardWidth = levelRows.Length == 0 ? 0 : levelRows[0].Length;
		}

		bool CheckRectangularBoard(string[] levelRows)
		{
			if (levelRows.Length == 0)
			{
				return true;
			}

			var rowLength = levelRows[0].Length;

			foreach (string row in levelRows)
			{
				if (row.Length != rowLength)
				{
					return false;
				}
			}

			return true;
		}

		//SetupScene initializes our level and calls the previous functions to lay out the game board
		public void SetupScene(TextAsset board)
		{
			// For some reason, there's an invisible whitespace character at the beginning of the TextAsset.
			var splitBoard = board.text.Split('&');
			var tilesString = splitBoard[0].Trim();
			var objectsString = splitBoard[1].Trim();

			SetupMapSymbols();

			// Infers board size.
			GetBoardSize(tilesString, out this.boardWidth, out this.boardHeight);

			// Creates the landscape.
			SetupBoardTiles(tilesString);

			// Creates the player, animals, and homes.
			SetupBoardObjects(objectsString);
		}

		/// <summary>
		/// Sets up map symbol translation dictionary.
		/// </summary>
		public void SetupMapSymbols()
		{
			// Make sure animals are lowercase. If not letter, ignore.
			foreach (AnimalSymbols enumAnimal in Enum.GetValues(typeof(AnimalSymbols)))
			{
				var animal = (char) enumAnimal;
				if (Char.IsLetter(animal))
				{
					this.animals.Add(Char.ToLower(animal));
					this.homes.Add(Char.ToUpper(animal));
				}
			}

			prefabMap[Symbols.Floor] = FloorTile;
			prefabMap[Symbols.Ocean] = OceanTile;
			prefabMap[Symbols.OuterWall] = OuterWallTile;
			prefabMap[Symbols.Wall] = WallTile;
			prefabMap[Symbols.Chasm] = ChasmTile;
			prefabMap[Symbols.Player] = Player;

			prefabMap[(char) AnimalSymbols.Charlie] = Charlie;
			prefabMap[(char) AnimalSymbols.Sheila] = Sheila;
			prefabMap[(char) AnimalSymbols.Spring] = Spring;
			prefabMap[(char) AnimalSymbols.Summer] = Summer;
			prefabMap[(char)AnimalSymbols.Chorlie] = Chorlie;
			prefabMap[(char)AnimalSymbols.Chirlie] = Chirlie;

			prefabMap[Char.ToUpper((char) AnimalSymbols.Charlie)] = CharlieHome;
			prefabMap[Char.ToUpper((char) AnimalSymbols.Chorlie)] = ChorlieHome;
			prefabMap[Char.ToUpper((char) AnimalSymbols.Chirlie)] = ChirlieHome;
			prefabMap[Char.ToUpper((char) AnimalSymbols.Sheila)] = SheilaHome;
			prefabMap[Char.ToUpper((char) AnimalSymbols.Spring)] = SpringHome;
			prefabMap[Char.ToUpper((char) AnimalSymbols.Summer)] = SummerHome;
		}
	}
}
