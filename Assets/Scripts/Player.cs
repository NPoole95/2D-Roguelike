using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject
{
    public int wallDamage = 1; // the damage the player applies to wall objects with each hit
    public int pointsPerFood = 10; // the number of food points the player gains from picking up a food item
    public int pointsPerSoda = 20; // the number of food points the player gains from picking up a soda
    public float restartLevelDelay = 1.0f;
    public Text foodText;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator; // used to store reference to the animator component
    private int food; // stores the players score in the level before passing it back to the game manager as the level changes
    private Vector2 touchOrigin = -Vector2.one; // used to record the place where the placers finger started touhcing the touch screen. it starts as a position off the screen.

    // Start is called before the first frame update
    protected override void Start() // protected & ovveride  because different implementation for start in the player class than the moving pbject class
    {

        animator = GetComponent<Animator>(); // gets and stores a reference to the animator component

        food = GameManager.instance.playerFoodPoints; // allows player to manage the food score during the level and then store in the game manager as we change levels

        foodText.text = "Food: " + food; //sets the output message to show the updated food score

        base.Start(); // calls the start function of the base class moving object
    }

    private void OnDisable() // part of the unity api and will be called when the player game object is disabled
    {
        GameManager.instance.playerFoodPoints = food; // used to store food in the game manager as we change levels
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn)
        {
            return;
        }

        int horizontal = 0; // used to store the direction we are moving as either 1 or -1 in each axis 
        int vertical = 0;   // used to store the direction we are moving as either 1 or -1 in each axis 

#if UNITY_STANDALONE || UNITY_WEBPLAYER // Checks to see if we are usind the standard unity client (keyboard and mouse input)

        horizontal = (int)Input.GetAxisRaw("Horizontal"); // gets the inpout from the input manager and casts it from a float to an int
        vertical = (int)Input.GetAxisRaw("Vertical"); // gets the inpout from the input manager and casts it from a float to an int

        if (horizontal != 0) // checks to see if the player is moving horizontally
        {
            vertical = 0; // if so, sets the players vertical movement to 0, preventing diagonal movement (this will prioritize horizontal moveement over vertical)
        }

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE // using touch screen input
        if (Input.touchCount > 0) // checks to see if more than one touch has been registered
        {
            Touch myTouch = Input.touches[0]; // holds the first touch detected. ignoring all others (only supporting singlke touches)

            if (myTouch.phase == TouchPhase.Began) // determines if this is the beginning of a touch on the screen
            {
                touchOrigin = myTouch.position; // sets the touch origin to the poisition on the touch        
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) // checks if the touch phase is ended and if the touch origin is greater than 0 (meaning the finger has left the screen and the touch is inside the screen boundaries)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x; // gives us the difference between the two touches on the x axis and therefore a direction to move in
                float y = touchEnd.y - touchOrigin.y; // gives us the difference between the two touches on the y axis and therefore a direction to move in
                touchOrigin.x = -1; // used so the conditional does not repeatidly evaluate to true

                if(Mathf.Abs(x) > Mathf.Abs(y)) // checks if the click had a bigger change in the X or Y direction         
                {
                    horizontal = x > 0 ? 1 : -1; // if x is greater than 0, move rioght, if not, move left
                }
                else
                {
                    vertical = y > 0 ? 1 : -1; // if y is greater than 0 move up, else move down
                }
            }
        }
#endif
        if (horizontal != 0 || vertical != 0) // check if the player is moving
        {
            AttemptMove<Wall>(horizontal, vertical); // passes through the generic component wall to the attempt move funtion, meaning we expect to encounter a wall, which is interactable
        }
    }

    protected override void AttemptMove<T>(int xDir, int yDir) // T specifies the type of component we expect the mover to encounter
    {
        food--; // subtracts 1 food each time the player moves
        foodText.text = "Food: " + food; //sets the output message to show the updated food score
        base.AttemptMove<T>(xDir, yDir); // calls the attempt move function of the base class
        RaycastHit2D hit;
        if(Move(xDir, yDir, out hit)) // check if move returns true (they could move)
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2); // chooses a random one of the move sounds
        }
        CheckIfGameOver();
        GameManager.instance.playersTurn = false; // sets the players turn to be over
    }

    protected override void onCantMove<T>(T component) // implementation for the player class
    {
        Wall hitWall = component as Wall; // sets the wall to the component that was passed as a parameter, whilst casting it to a wall
        hitWall.DamageWall(wallDamage); //calls the damage wall function
        animator.SetTrigger("playerChop"); // set the player chop trigger of the animator
    }
    private void OnTriggerEnter2D(Collider2D other) // used to allow the player to interract with other things on the board. ontrigger 2d is part of the unity API
    {
        // checks the tag of the object we collided with

        if (other.tag == "Exit") // checks if the item is the exit
        {
            Invoke("Restart", restartLevelDelay); // calls the restart level function after the delay passed as a parameter.
            enabled = false; //disables the player
        }
        else if (other.tag == "Food") // checks if the item is food
        {
            food += pointsPerFood; // increases the players food score by the value of a food item
            foodText.text = "+" + pointsPerFood + " Food: " + food; //sets the output message to show the updated food score along with the value of the item they picked up
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2); // randomly selects one of the eating sounds
            other.gameObject.SetActive(false); // sets the object to inactive (destroyed)
        }
        else if (other.tag == "Soda") // checks if the item is Soda
        {
            food += pointsPerSoda; // increases the players food score by the value of a soda item
            foodText.text = "+" + pointsPerSoda + " Food: " + food; //sets the output message to show the updated food score along with the value of the item they picked up
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2); // randomly selects one of the drinking sounds
            other.gameObject.SetActive(false); // sets the object to inactive (destroyed)
        }
    }


    private void Restart() //reload the level, called if the player collides with the exit, meaniung we are advancing
    {
        
       // Application.LoadLevel(Application.loadedLevel); // loads the last scene that was loaded, in this case main, which is the only scene. 
        SceneManager.LoadScene(0);
        //(in other games we would load specific scnenes, but this game procediorally generates them.
    }

    public void LoseFood (int loss) // called when an enemy attacks the player
    {
        animator.SetTrigger("playerHit"); // sets the animator trigger to play player Hit
        food -= loss; // removes the value passed as  a parameter from the players food total
        foodText.text = "Ouch! -" + loss + " Food: " + food; //sets the output message to show the updated food score along with the amount of food lost
        CheckIfGameOver(); // as the player has lost food, we check if the game is over
    }

    private void CheckIfGameOver()
    {
        if (food <= 0) // checks if the players food has reached 0
        {
            SoundManager.instance.PlaySingle(gameOverSound); // plays the game over sound
            SoundManager.instance.musicSource.Stop(); // stops the background music source playing
            GameManager.instance.GameOver(); // calls the game over function
        }
    }
}
