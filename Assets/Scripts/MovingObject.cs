using UnityEngine;
using System.Collections;

namespace Relay
{
	//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
	public abstract class MovingObject : MonoBehaviour
	{
		public enum Direction
		{
			DownLeft,
			DownRight,
			UpLeft,
			UpRight
		}

		//Layer on which collision will be checked.
		public LayerMask blockingLayer;

		// Contact filter to filter movement collisions.
		private ContactFilter2D contactFilter;

		//Is this object currently moving?
		private bool isMoving = false;

		//Time it will take object to move, in seconds.
		public float moveTime = 0.1f;

		private BoxCollider boxCollider;
		//The BoxCollider2D component attached to this object.
		private Rigidbody rigidBody;
		//The Rigidbody2D component attached to this object.
		private float inverseMoveTime;
		//Used to make movement more efficient.

		public Direction moveDirection = Direction.DownRight;

		//Protected, virtual functions can be overridden by inheriting classes.
		protected virtual void Start()
		{
			//Get a component reference to this object's BoxCollider
			boxCollider = GetComponent <BoxCollider>();
			
			//Get a component reference to this object's Rigidbody
			rigidBody = GetComponent <Rigidbody>();
			
			//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
			inverseMoveTime = 1f / moveTime;
		}

		public bool isCurrentlyMoving()
		{
			return isMoving;
		}

		protected virtual Transform CheckCollision(Vector3 start, Vector3 end)
		{
			//Disable the boxCollider so that linecast doesn't hit this object's own collider.
			boxCollider.enabled = false;

			//Check if there's something at the end location.
			RaycastHit hitInfo;
			Physics.Linecast(start, end, out hitInfo, blockingLayer);

			//Re-enable boxCollider after linecast
			boxCollider.enabled = true;

			return hitInfo.transform;
		}

		//Move returns true if it is able to move and false if not.
		//hit != null <=> Move returns false
		private bool Move(int xDir, int zDir, out Transform hitTransform)
		{
			//Store start position to move from, based on objects current transform position.
			Vector3 start = transform.position;

			// Calculate end position based on the direction parameters passed in when calling Move.
			Vector3 end = start + new Vector3(xDir, 0, zDir);

			hitTransform = CheckCollision(start, end);
			
			//Check if anything was hit
			if (hitTransform == null)
			{
				//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
				StartCoroutine(SmoothMovement(end));
				
				//Return true to say that Move was successful
				return true;
			}
			
			//If something was hit, return false, Move was unsuccesful.
			return false;
		}
		
		
		//Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
		protected IEnumerator SmoothMovement(Vector3 end)
		{
			isMoving = true;
			//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
			//Square magnitude is used instead of magnitude because it's computationally cheaper.
			float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

			int iterations = 0;
			
			//While that distance is greater than a very small amount
			while (sqrRemainingDistance > 1e-5 && iterations++ < 100)
			{
				//Find a new position proportionally closer to the end, based on the moveTime
				Vector3 newPosition = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
				
				//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
				transform.position = newPosition;
				
				//Recalculate the remaining distance after moving.
				sqrRemainingDistance = (transform.position - end).sqrMagnitude;
				
				//Return and loop until sqrRemainingDistance is close enough to zero to end the function
				yield return null;
			}
			transform.position = end;
			isMoving = false;
		}

		public void AttemptMove(int xDir, int zDir)
		{
			Transform hitTransform;
			Direction newMoveDirection = moveDirection;

			if (xDir > 0)
			{
				newMoveDirection = Direction.UpRight;
			}
			else if (xDir < 0)
			{
				newMoveDirection = Direction.DownLeft;
			}
			else if (zDir > 0)
			{
				newMoveDirection = Direction.UpLeft;
			}
			else if (zDir < 0)
			{
				newMoveDirection = Direction.DownRight;
			}

			if (moveDirection != newMoveDirection)
			{
				moveDirection = newMoveDirection;
				AnimateDirection(moveDirection);
			}

			//Set canMove to true if Move was successful, false if failed.
			bool canMove = Move(xDir, zDir, out hitTransform);

			if (canMove)
			{
				OnMoveBegan();
			}
			else
			{
				OnMoveBlocked(hitTransform);
			}
		}

		/// <summary>
		/// Called when this object starts an unobstructed movement. Subclasses
		/// should implement e.g. movement sfx here
		/// </summary>
		protected abstract void OnMoveBegan();

		/// <summary>
		/// Called when this object bumps into another Component. Subclasses
		/// should handle logic for bumping into components (e.g. picking up
		/// an animal).
		/// </summary>
		protected abstract void OnMoveBlocked(Transform component);

		protected abstract void AnimateDirection(Direction d);
	}
}
