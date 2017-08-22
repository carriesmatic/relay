using UnityEngine;
using System.Collections;

namespace Relay
{
	//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
	public abstract class MovingObject : MonoBehaviour
	{
		public enum Direction
		{
			Up, Down, Left, Right
		}

		public LayerMask blockingLayer;			//Layer on which collision will be checked.
		private bool isMoving = false;          //Is this object currently moving?
		public float moveTime = 0.1f;			//Time it will take object to move, in seconds.
		
		private BoxCollider2D boxCollider; 		//The BoxCollider2D component attached to this object.
		private Rigidbody2D rb2D;				//The Rigidbody2D component attached to this object.
		private float inverseMoveTime;			//Used to make movement more efficient.

		public Direction moveDirection = Direction.Down;

		//Protected, virtual functions can be overridden by inheriting classes.
		protected virtual void Start ()
		{
			//Get a component reference to this object's BoxCollider2D
			boxCollider = GetComponent <BoxCollider2D> ();
			
			//Get a component reference to this object's Rigidbody2D
			rb2D = GetComponent <Rigidbody2D> ();
			
			//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
			inverseMoveTime = 1f / moveTime;
		}

		public bool isCurrentlyMoving() {
			return isMoving;
		}

		//Move returns true if it is able to move and false if not.
		//hit != null <=> Move returns false
		private bool Move (int xDir, int yDir, out RaycastHit2D hit)
		{
			//Store start position to move from, based on objects current transform position.
			Vector2 start = transform.position;
			
			// Calculate end position based on the direction parameters passed in when calling Move.
			Vector2 end = start + new Vector2 (xDir, yDir);
			
			//Disable the boxCollider so that linecast doesn't hit this object's own collider.
			boxCollider.enabled = false;
			
			//Cast a line from start point to end point checking collision on blockingLayer.
			hit = Physics2D.Linecast (end, end, blockingLayer);
			
			//Re-enable boxCollider after linecast
			boxCollider.enabled = true;
			
			//Check if anything was hit
			if(hit.transform == null)
			{
				//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
				StartCoroutine (SmoothMovement (end));
				
				//Return true to say that Move was successful
				return true;
			}
			
			//If something was hit, return false, Move was unsuccesful.
			return false;
		}
		
		
		//Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
		protected IEnumerator SmoothMovement (Vector3 end)
		{
			isMoving = true;
			//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
			//Square magnitude is used instead of magnitude because it's computationally cheaper.
			float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			
			//While that distance is greater than a very small amount (Epsilon, almost zero):
			while(sqrRemainingDistance > float.Epsilon)
			{
				//Find a new position proportionally closer to the end, based on the moveTime
				Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
				
				//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
				rb2D.MovePosition (newPostion);
				
				//Recalculate the remaining distance after moving.
				sqrRemainingDistance = (transform.position - end).sqrMagnitude;
				
				//Return and loop until sqrRemainingDistance is close enough to zero to end the function
				yield return null;
			}
			rb2D.MovePosition (end);
			isMoving = false;
		}

		public void AttemptMove (int xDir, int yDir)
		{
			RaycastHit2D hit;
			Direction newMoveDirection = moveDirection;

			if (xDir > 0)
			{
				newMoveDirection = Direction.Right;
			}
			else if (xDir < 0)
			{
				newMoveDirection = Direction.Left;
			}
			else if (yDir > 0)
			{
				newMoveDirection = Direction.Up;
			}
			else if (yDir < 0)
			{
				newMoveDirection = Direction.Down;
			}

			if (moveDirection != newMoveDirection)
			{
				moveDirection = newMoveDirection;
				AnimateDirection(moveDirection);
			}

			//Set canMove to true if Move was successful, false if failed.
			bool canMove = Move (xDir, yDir, out hit);

			if (canMove)
			{
				OnMoveBegan ();
			} else 
			{
				OnMoveBlocked (hit.transform);
			}
		}

		/// <summary>
		/// Called when this object starts an unobstructed movement. Subclasses
		/// should implement e.g. movement sfx here
		/// </summary>
		protected abstract void OnMoveBegan ();

		/// <summary>
		/// Called when this object bumps into another Component. Subclasses
		/// should handle logic for bumping into components (e.g. picking up
		/// an animal).
		/// </summary>
		protected abstract void OnMoveBlocked (Transform component);

		protected abstract void AnimateDirection(Direction d);
	}
}
