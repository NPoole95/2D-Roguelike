using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{

    [Serializable]
    public class Count
    {
        public int minimum; // integer used to represent the maximum of a value used in the count function
        public int maximum; // integer used to represent the maximum of a value used in the count function

        public Count(int min, int max)
        {
            minimum = min; // Assigns the min value passed to the funciton as a perameter to minimum 
            maximum = max; // Assigns the max value passed to the funciton as a perameter to maximum
        }
    }
        public int columns = 8; // Defines the number of columns in a Level
        public int rows = 8;    // Defines the number of rows in a level
        public Count wallCount = new Count(5, 9); // Sets the minimum and maximum number of Walls
        public Count foodCount = new Count(1, 5); // Sets the minimum and maximum number of Rows
        public GameObject exit; // created a game object for the exit
        public GameObject[] floorTiles; // Creates an array of game objects for the floor tiles
        public GameObject[] wallTiles;  // Creates an array of game objects for the wall tiles
        public GameObject[] foodTiles;  // Creates an array of game objects for the food tiles
        public GameObject[] enemyTiles; // Creates an array of game objects for the enemy tiles
        public GameObject[] outerWallTiles; // Creates an array of game objects for the outer wall tiles

        private Transform boardHolder; // used to keep the hierarchy clean, game objects will be parented to board holder to prevent the hierarchy being filled.
        private List<Vector3> gridPositions = new List<Vector3>(); // used to track all possible posiitons on the game board and if an object has been spawned there.

        // this function initializes the grid ready for it to be populated
        void InitialiseList()
        {
            gridPositions.Clear(); // clears the positions on the grid

            for (int x = 1; x < columns - 1; x++) // This nested for loop populates the grid with the positions of each tile in the form of coordinates
            {                                     // The loop runs from 1 -> -1 in order to leave a border around the edge of the grid to prefent impassable levels being created
                for (int y = 1; y < rows - 1; y++)
                {
                    gridPositions.Add(new Vector3(x, y, 0.0f));
                }
            }
        }

        // used to set up the outer wall and floor of the game board
        void BoardSetup()
        {
            boardHolder = new GameObject("Board").transform; // creates the Board game object

            for (int x = -1; x < columns + 1; x++) // this loop runs through the -1 in rows and columns to form the outer perimeter of the map
            {
                for (int y = -1; y < rows + 1; y++)
                {
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)]; // chooses a floor tile at random, ready for instantiation

                    if (x == -1 || x == columns || y == -1 || y == rows) // checks if the position is in the outer wall
                    {
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)]; // sets to a randomly selected wall from the outer wall array
                    }
                    GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0), Quaternion.identity) as GameObject; // creates a game objkect instance, passes through the selected prefab and positions defined by the loop
                    instance.transform.SetParent(boardHolder); // sets the parent of the new object to the board holder.
                }
            }
        }

        Vector3 RandomPosition() // Generates a random position on the board, that hasnt already been selected
        {
            int randomIndex = Random.Range(0, gridPositions.Count); // Generates a random number in the range of the number of positions in the grid position list
            Vector3 randomPosition = gridPositions[randomIndex];    // sets the random position to the vector3 stored in the gridPositions list
            gridPositions.RemoveAt(randomIndex); // removes the selected position from the gridPosition list
            return randomPosition; // returns the position
        }

        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum) // Spawns a tile at the random position
        {
            int objectCount = Random.Range(minimum, maximum + 1);  // controls how many of a given object will be spawned 

            for (int i = 0; i < objectCount; i++) // spawns the number of objects specifdied by object count
            {
                Vector3 randomPosition = RandomPosition(); // selects a random position
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)]; // selects a random object to spawn
                Instantiate(tileChoice, randomPosition, Quaternion.identity); // instantiates the random object
            }
        }

        public void SetupScene(int level) // The scene setup function is the only public function in the script, meaning it can be called externally and acts as a manager, calling all other functions
        {
            BoardSetup(); // Calls the boardSetup function
            InitialiseList(); // calls the initialiseList function
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);  // Lays out a random number of walls based on the minimum and maximum values sent as perameters
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);  // Lays out a random number of food tiles based on the minimum and maximum values sent as perameters
            int enemyCount = (int)Mathf.Log(level, 2.0f); // Sets the number of enemies to increase based on the level.  (e.g. lvl 2 = 1. lvl 4 = 2, lvl 8 = 3.) note: mathf.log returns a float so this is cast to an int
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount); // Lays out the enemies. (min and max are the same as the number of enemies is defined in the previous line, there is no range)
            Instantiate(exit, new Vector3(columns - 1, rows - 1, 0.0f), Quaternion.identity); // instantiates the exit, it is always in the upper right corner of the level.
        }
    
}
