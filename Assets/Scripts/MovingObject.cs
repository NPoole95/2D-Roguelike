using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour // set to abstract so they must be implemented in derived class
{
    public float moveTime = 0.1f; // the time it takes an object to move in seconds
    public LayerMask blockingLayer; // the layer on which we check collisions (players, enemies and walls set to this layer in prefebs)

    private BoxCollider2D boxCollider; // created a 2d box collider
    private Rigidbody2D rb2D; // stores component reference to rigid body 2d
    private float inverseMoveTime; // used to make movement calculations more efficient

    // Start is called before the first frame update
    protected virtual void Start() // virtual can be overriden by inheriting classes and so can have different implementations
    {
        boxCollider = GetComponent<BoxCollider2D>(); // gets component reference to the box collider
        rb2D = GetComponent<Rigidbody2D>(); // gets component reference to the rigid body 2D
        inverseMoveTime = 1.0f / moveTime; // stores the reciprical and can be used to multiply instead of dividing (more efficient)

    }

    protected bool Move ( int xDir, int yDir, out RaycastHit2D hit) // 'out' causes arguments to be passed by reference
    {
        Vector2 start = transform.position; // stores the current transform position (transform.position is a vector 3, but is cast to a vector 2 to discard the Z component)
        Vector2 end = start + new Vector2(xDir, yDir); // calculates the end pos based on the parameters passed to the move function

        boxCollider.enabled = false; // disables the box collider to make sure the ray cast does not hit the objects own collider
        hit = Physics2D.Linecast(start, end, blockingLayer); // casts a line from start to end, checking collisions on the blocking layer, storing the result in hit
        boxCollider.enabled = true; // re enables the box collider

        if(hit.transform == null) // checks if anything was hit by the line cast (if null, space was open)
        {
            StartCoroutine(SmoothMovement(end)); // starts movement
            return true; // returns true (we can move)
        }

        return false; // else returns false (cannot move)
    }

    protected IEnumerator SmoothMovement (Vector3 end) // end is the tile we will move to
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude; // calculate the remainind distance to move

        // while (sqrRemainingDistance > float.Epsilon) // checks that the remaining distance is greater than "almost zero" - this is just flat out stupid and nowhere near large enough to avoid floating point comparison errors
        while (sqrRemainingDistance > 0.05f)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime); // new pos is proportionally lcoser to the end based on move time
            rb2D.MovePosition(newPosition); // moves to the new position
            sqrRemainingDistance = (transform.position - end).sqrMagnitude; // recalculates the remaining distance
            yield return null; // waits for a frame to execute before reevaluating the loop
        }
         float test = (transform.position - end).sqrMagnitude;
    }

    protected virtual void AttemptMove<T> (int xDir, int yDir) 
        where T : Component // takes a generic perameter, changes based on the unit we will interact with (eg, player or walls)
    {
        RaycastHit2D hit; // declares a ray cast
        bool canMove = Move(xDir, yDir, out hit); // calls the move function to see if it is possible

        if (hit.transform == null) // because hit is an out perameter for move, we can check if the transform we hit in move is null
        {
            return; //returns so following code is not executed
        }

        T hitComponent = hit.transform.GetComponent<T>(); //Get reference to component t, attatched to component that was hit

        if (!canMove && hitComponent != null) // means the moving object is blocked, but can interact with the object
        {
            onCantMove(hitComponent); // call the on cant move function, passing the hit component as a parameter
        }
    }

    protected abstract void onCantMove<T>(T Component) // takes a generic perameter. will be overriden by functions in inheriting classes
        where T : Component;
   
}
