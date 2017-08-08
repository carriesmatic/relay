using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Relay
{
    public class BoardManager : MonoBehaviour
    {
        // Using Serializable allows us to embed a class with sub properties in the inspector.
        [Serializable]
        public class MapDictionary
        {
            internal List<char> homes;
            internal char floor = '.';
            internal char outerWall = '*';
            internal char wall = '-';
            internal char player = '@';

            public List<char> animals;

            /// <summary>
            /// Empty constructor if you want to use defaults.
            /// </summary>
            public MapDictionary()
            {
            }

            // Assignment constructor.
            public MapDictionary(List<char> animals)
            {
                this.animals = new List<char>();
                this.homes = new List<char>();

                // Make sure animals are lowercase. If not letter, ignore.
                foreach (char animal in animals)
                {
                    if (Char.IsLetter(animal))
                    {
                        this.animals.Add(Char.ToLower(animal));
                        this.homes.Add(Char.ToUpper(animal));
                    }
                }
            }
        }

        private MapDictionary mapDictionary = new MapDictionary();      //The mapping dictionary for board symbols.
        private int columns = 0;                                        //Number of columns in our game board.
        private int rows = 0;                                           //Number of rows in our game board.

        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] wallTiles;                                  //Array of wall prefabs.
        public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.
        public GameObject[] animalTiles;                                //Array of animal prefabs.
        public GameObject[] homeTiles;                                  //Array of home prefabs.

        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.

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
                    else
                    {
                        //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                        if (tileArrays[y][x] == mapDictionary.)
                        {
                            
                        }
						GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
					}

                    //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    GameObject instance =
                        Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent(boardHolder);
                }
            }
        }

        void SetupBoardObjects(string objects)
        {
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
        public void SetupScene(TextAsset tiles, TextAsset objects)
        {
            var tilesString = tiles.ToString();
	        var objectsString = objects.ToString();

            mapDictionary = new MapDictionary(new List<char> { 'a', 'b' });

			// Infers board size.
			GetBoardSize(tilesString, out this.columns, out this.rows);

            // Creates the landscape.
            SetupBoardLandscape(tilesString);

            // Creates the player, animals, and homes.
            SetupBoardObjects(objectsString);
        }
    }
}
