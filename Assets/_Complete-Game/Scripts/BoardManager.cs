﻿using UnityEngine;
using System;
using System.Collections.Generic;

 		//Allows us to use Lists.
using Random = UnityEngine.Random;

 		//Tells Random to use the Unity Engine random number generator.

namespace Relay
{
	public class BoardManager : MonoBehaviour
	{
		public enum Animal
		{
			Charlie = 'a',
			Sheila = 'b',
			Spring = 'c',
			Summer = 'd'
		}

		public class MapDictionary
		{
			internal HashSet<char> animals = new HashSet<char>();
			internal HashSet<char> homes = new HashSet<char>();
			internal char floor = '.';
			internal char outerWall = '*';
			internal char wall = '-';
			internal char player = '@';

			// Assignment constructor.
			public MapDictionary()
			{
				// Make sure animals are lowercase. If not letter, ignore.
				foreach (Animal enumAnimal in Enum.GetValues(typeof(Animal)))
				{
					var animal = (char)enumAnimal;
					if (Char.IsLetter(animal))
					{
						this.animals.Add(Char.ToLower(animal));
						this.homes.Add(Char.ToUpper(animal));
					}
				}
			}
		}

		//The mapping dictionary for board symbols.
		private MapDictionary mapDictionary = new MapDictionary();

		//Number of columns in our game board.
		private int columns = 0;

		//Number of rows in our game board.
		private int rows = 0;

		//Array of floor prefabs.
		public GameObject[] floorTiles;

		//Array of wall prefabs.
		public GameObject[] wallTiles;

		//Array of outer tile prefabs.
		public GameObject[] outerWallTiles;

		//Array of animal prefabs.
		public GameObject[] animalTiles;

		//Array of home prefabs.
		public GameObject[] homeTiles;

		//A variable to store a reference to the transform of our Board object.
		private Transform boardHolder;

		//Sets up the outer walls and floor (background) of the game board.
		void SetupBoardLandscape(string tiles)
		{
			var tileArrays = tiles.Split('\n');

			//Instantiate Board and set boardHolder to its transform.
			boardHolder = new GameObject("Board").transform;

			//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
			for (int x = -1; x < columns + 1; x++)
			{
				//Loop along y axis, starting from -1 to place floor or outerwall tiles.
				for (int y = -1; y < rows + 1; y++)
				{
					GameObject toInstantiate;

					//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
					if (x == -1 || x == columns || y == -1 || y == rows)
					{
						toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
					}
					else if (tileArrays[y][x] == mapDictionary.wall)
					{
						toInstantiate = wallTiles[Random.Range(0, wallTiles.Length)];
					}
					else
					{
						// Instantiate a floor tile by default.
						toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
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
			foreach (var boardObject in boardObjects.Split('\n'))
			{
				// The boardObject looks like "@ 1,2"
				var tileAndPosition = boardObject.Split(' ');
				var tileArray = tileAndPosition[0].ToCharArray();

				if (tileArray.Length != 1)
				{
					continue;
				}

				var tile = tileArray[0];
				var position = tileAndPosition[1].Split(',');
				int y = Int32.Parse(position[0]);
				int x = Int32.Parse(position[1]);

				GameObject toInstantiate;

				if (mapDictionary.animals.Contains(tile))
				{
					toInstantiate = animalTiles[Random.Range(0, animalTiles.Length)];
				}
				else if (mapDictionary.homes.Contains(tile))
				{
					toInstantiate = homeTiles[Random.Range(0, homeTiles.Length)];
				}
				else
				{
					continue;
				}

				GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
				instance.transform.SetParent(boardHolder);
			}
		}

		// Instantiate the given object at the given vector.
		void LayoutObject(char tile, int column, int row)
		{
			var tileArray = animalTiles;

			//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
			Vector3 position = new Vector3(column, row, 0f);

			//Choose a random tile from tileArray and assign it to tileChoice
			GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

			//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
			Instantiate(tileChoice, position, Quaternion.identity);
		}


		void GetBoardSize(string level, out int columns, out int rows)
		{
			columns = 0;
			rows = 0;

			string[] levelRows = level.Split('\n');

			if (!CheckRectangularBoard(levelRows))
			{
				return;
			}

			rows = levelRows.Length;
			columns = levelRows.Length == 0 ? 0 : levelRows[0].Length;
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

			mapDictionary = new MapDictionary();

			// Infers board size.
			GetBoardSize(tilesString, out this.columns, out this.rows);

			// Creates the landscape.
			SetupBoardLandscape(tilesString);

			// Creates the player, animals, and homes.
			SetupBoardObjects(objectsString);
		}
	}
}
