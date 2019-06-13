using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject // causes enemy to inherit from moving object
{
    public int playerDamage; // the number of food points subrtracted when the enemy hits the player
    public AudioClip enemyAttack1; // used to hold the enemy attack sound effects
    public AudioClip enemyAttack2; // used to hold the enemy attack sound effects

    private Animator animator; 
    private Transform target; // used to store the players position
    private bool skipMove; // used to cause the enemy to move every other turn

   protected override void Start() // overrides the base class's start function
    {
        GameManager.instance.AddEnemyToList(this); // makes the enemy script add itself to the list of enemies in the game manager. (allows the game manager to use the public function move enemy)
        animator = GetComponent<Animator>(); // gets and stores the component reference to the animator
        target = GameObject.FindGameObjectWithTag("Player").transform; // stores the transform of the player
        base.Start(); // calls the start function of the base class
    }

    protected override void AttemptMove<T>(int xDir, int yDir) // overrides the base class's attempt move function. takes a generic perameter T
    {
        if (skipMove) // checks if skip move is true (the player moved last turn)
        {
            skipMove = false; // sets skip move to false, moving nect turn
            return;
        }

        base.AttemptMove<T>(xDir, yDir); // calls the attempt move function from the base class

        skipMove = true; // sets skip move to true, skipping the next turn.
    }

    public void MoveEnemy() // called by the game manager whenever an enemy moves
    {
        int xDir = 0; 
        int yDir = 0;


        // checks the position of the target against the transform to determine the movement direction
        if (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon) // uses float.Epsilon to see if they are roughly the same, better than comparing a float to 0
        {
            yDir = target.position.y > transform.position.y ? 1 : -1; // if true, move up, if false, move down, always towards the player
        }
        else // if not moving up or down
        {
            xDir = target.position.x > transform.position.x ? 1 : -1; // if true, move right, if false, move left, always towards the player
        }
        AttemptMove<Player>(xDir, yDir); // attempts to move towards the player
    }

    protected override void onCantMove<T>(T component) // called if the enemy attempts to move into a space occupied by the player
    { // takles a generic perameter T
        Player hitPlayer = component as Player; // casts the component passed as a parameter to a player

        animator.SetTrigger("enemyAttack"); // sets the enemy attack trigger in the animator controller

        hitPlayer.LoseFood(playerDamage); // calls the lose food function, taking food points from the player based on the enemys attack

        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2); // randomly selects one of the enemy attack sounds
    }
}
