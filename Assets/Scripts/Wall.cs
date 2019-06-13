using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Sprite dmgSprite; // the damaged wall sprite to give feedback to the player that they have hit the wall
    public int hp = 4; // the walls HP
    public AudioClip chopSound1; // used to hold the audio clips for attacking walls
    public AudioClip chopSound2; // used to hold the audio clips for attacking walls

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // gets and stores the sprite renderer component
    }

    public void DamageWall(int loss)
    {
        SoundManager.instance.RandomizeSfx(chopSound1, chopSound2); // randomly selects one of the chopping sounds;
        spriteRenderer.sprite = dmgSprite; // changes the wall sprite to show it is damaged
        hp -= loss; // removes the valuse of loss passed to the function from the walls hp

        if(hp <= 0) // checks if the walls hp has reached 0
        {
            gameObject.SetActive(false); //sets the game object to false meaning it has been destroyed
        }
    }
}
