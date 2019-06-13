using System.Collections;
using System.Collections.Generic; // allows the use of lists
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2.0f; // the amount of time to wait before starting a new level 
    public float turnDelay = 0.1f; // the time the game will wait between turns
    public static GameManager instance = null; // declared as static so that the variable will belong to the class and there is only 1.
    public int playerFoodPoints = 100; // the players starting food level
    [HideInInspector] public bool playersTurn = true; // hide inspector means that even though the variable is public, it wont be displayed in the editor

    private BoardManager boardScript; // creates an instance of the board manager
    private Text levelText; // the text do display the level number
    private GameObject levelImage; // used to store a reference to the level image so it can be activated/deactivated
    private int level = 0; // sets the level top 3
    private List<Enemy> enemies;  // used to keep track of the enemies and to send them orders to move
    private bool enemiesMoving;
    private bool doingSetup = true; // used to check we are setting up the board and prevent the player moving while this is happening

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) // checks if the game manager has already been created
        {
            instance = this; // if not, it is assigned to this instance of the game manager
        }
        else if (instance != this) //if the instamce opf the game object is not this one
        {
            Destroy(gameObject); // we destroy this the ensure we dont have 2 instances of the game manager
        }
        DontDestroyOnLoad(gameObject); // when we load a new scene, all objects in the hierarchy are normally destroyed.
                                       // because we want to keep track of the score between scenes, we will do this later instead
        enemies = new List<Enemy>();   // declares a new list of enemies
        boardScript = GetComponent<BoardManager>(); // gets the gameManager component from GameManager.cs
    }

    // this function was deprecated and replaced by the following 3 functions
    //private void OnLevelWasLoaded(int index)// part of the unity API and is called every time a scene is loaded
    //{
    //    level++; // advances the level by 1
    //    InitGame(); // used to manage the UI elements and setup each level

    //}

   //This is called each time a scene is loaded
   void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        level++; //add one to our level count
        InitGame(); // Call InitGame to initialise our level
    }

    void OnEnable()
    {
        // Tell out "OnLevelFinishedLoading" function to start listening for a scene change
        // event as soon as the script is enabled
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        // Tell out "OnLevelFinishedLoading" function to stop listening for a scene change
        // event as soon as the script is disabled
        // Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    void InitGame()
    {
        doingSetup = true; // prevents the player moving whilst the level is being set up
        levelImage = GameObject.Find("LevelImage"); // get a reference to the level image object
        levelText = GameObject.Find("LevelText").GetComponent<Text>(); // get a reference to the level text object
        levelText.text = "Day " + level; // updates the level text to show the new level
        levelImage.SetActive(true); // makes the image visable
        Invoke("HideLevelImage", levelStartDelay); //once the title card is displayed, wait for the specified delay before turning it off

        enemies.Clear(); // clears the list in the game manager as it is not reset between levels
        boardScript.SetupScene(level); // calls the scnene setup for the level determined by the perameter
    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false); // hides the level image
        doingSetup = false; // allows the player to move again
    }

    public void GameOver()
    {
        levelText.text = "After " + level + " days, you starved."; // outputs a message showing the player how long they survived
        levelImage.SetActive(true); // sets the level image to visable
        enabled = false; // disables the game manager
    }

    // Update is called once per frame
    void Update()
    {
        if (playersTurn || enemiesMoving || doingSetup) // checks if it is the players turn, if the enemies are already moving, or if the level is in setup
        {
            return;
        }
        StartCoroutine(MoveEnemies()); // starts the enemy movement coroutine
    }

    public void AddEnemyToList(Enemy script) // used to add a new enemy so that the game manager can issue orders to them
    {
        enemies.Add(script);
    }

    IEnumerator MoveEnemies() // coroutine used to move the enemies one at a time in sequence
    {
        enemiesMoving = true; // sets the enemies to moving
        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count == 0) // check if no enemies have been spawned yet
        {
            yield return new WaitForSeconds(turnDelay); // add an additional yield, causing the player to wait, even though there is no enemy to wait for
        }

        for (int i = 0; i < enemies.Count; i++) // loops through the enemies list
        {
            enemies[i].MoveEnemy(); // tells each enemy to move
            yield return new WaitForSeconds(enemies[i].moveTime); // wait for the next enemy to move so they do not all move at once
        }

        playersTurn = true; // players turn is now true
        enemiesMoving = false; // enemies turn is now false
    }
}
