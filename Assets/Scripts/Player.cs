﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Relay
{
	// A hand might have a held animal, and has a UI indicator for it.
	[System.Serializable]
	public class Hand
	{
		private Animal heldAnimal = null;
		readonly GameObject handUIRoot = null;

		private GameObject heldAnimalImage
		{
			get
			{
				return handUIRoot.transform.Find ("HeldAnimalImage").gameObject;
			}
		}


		public Hand(GameObject handUIRoot)
		{
			this.handUIRoot = handUIRoot;
		}

		public bool TryHold(Animal a)
		{
			if (heldAnimal == null)
			{
				heldAnimal = a;
				a.transform.gameObject.SetActive(false);
				UpdateUI();
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool IsHolding<T>() where T : Animal
		{
			if (heldAnimal != null)
			{
				var animal = heldAnimal.gameObject.GetComponent<T>();

				if (animal != null)
				{
					return animal.IsCurrentlyActive();
				}
			}

			return false;
		}

		public bool TryPlaceIntoHome(Home home)
		{
			if (heldAnimal != null && heldAnimal.Name == home.HomeFor)
			{
				heldAnimal.transform.gameObject.SetActive(false);
				home.gameObject.SetActive(false);
				heldAnimal.transform.SetParent(home.gameObject.transform);

				heldAnimal.transform.position = home.transform.position;
				heldAnimal = null;
				UpdateUI();
				return true;
			}
			return false;
		}

		public bool TryDrop(int x, int y)
		{
			if (heldAnimal != null)
			{
				heldAnimal.transform.gameObject.SetActive(true);
				heldAnimal.transform.position = new Vector2(x, y);
				heldAnimal = null;
				UpdateUI();
				return true;
			}
			return false;
		}

		public bool IsEmpty()
		{
			return this.heldAnimal == null;
		}

		// heldAnimal existing => the UIRoot's HeldAnimalImage's
		// Image source is the heldAnimal's image
		// heldAnimal null => HeldAnimalImage is inactive
		private void UpdateUI()
		{
			if (heldAnimal != null && !heldAnimalImage.activeSelf)
			{
				// activate heldAnimalImage
				// TODO make this work in 3D
//				heldAnimalImage.SetActive(true);
//				Image image = heldAnimalImage.GetComponent<Image>();
//				SpriteRenderer animalSpriteRenderer = heldAnimal.GetComponent<SpriteRenderer>();
//				// animalSpriteRenderer.color
//				image.color = animalSpriteRenderer.color;
//				image.sprite = animalSpriteRenderer.sprite;
			}
			else if (heldAnimal == null && heldAnimalImage.activeSelf)
			{
				// deactivate heldAnimalImage
				heldAnimalImage.SetActive(false);
			}
		}

		public void UpdateAnimalActiveUI()
		{
			if (heldAnimal != null)
			{
				BoardManager.FlipYScaleIfNeeded(heldAnimalImage.transform, heldAnimal.IsCurrentlyActive());
			}
		}
	}
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		private Hand leftHand;
		private Hand rightHand;
		private LayerMask swimmingLayer;

		public AudioClip moveSound1;
		//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;
		//2 of 2 Audio clips to play when player moves.

		public AudioClip hitWallSound;

		private Animator animator;
		//Used to store a reference to the Player's animator component.

		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
		#endif

		//Start overrides the Start function of MovingObject
		protected override void Start()
		{
			leftHand = new Hand(GameObject.Find("LeftHand"));
			rightHand = new Hand(GameObject.Find("RightHand"));
			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();

			//Call the Start function of the MovingObject base class.
			base.Start();
		}

		private void Update()
		{
			leftHand.UpdateAnimalActiveUI ();
			rightHand.UpdateAnimalActiveUI ();
			//If it's not the player's turn, exit the function.
			if (this.isCurrentlyMoving())
			{
				return;
			}

			if (GameManager.instance.enabled == false && GameManager.instance.boardManager.IsGameWon())
			{
				if (Input.anyKeyDown)
				{
					GameManager.instance.AdvanceLevel();
				}
			}

			// For debug purposes.
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.

			//Check if we are running either in the Unity editor or in a standalone build.
			#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR

			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int)(Input.GetAxisRaw("Horizontal"));

			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int)(Input.GetAxisRaw("Vertical"));

			//Check if moving horizontally, if so set vertical to zero.
			if (horizontal != 0)
			{
				vertical = 0;
			}
			//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
			#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
			//Store the first touch detected.
			Touch myTouch = Input.touches[0];

			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began)
			{
			//If so, set touchOrigin to the position of that touch
			touchOrigin = myTouch.position;
			}

			//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
			{
			//Set touchEnd to equal the position of this touch
			Vector2 touchEnd = myTouch.position;

			//Calculate the difference between the beginning and end of the touch on the x axis.
			float x = touchEnd.x - touchOrigin.x;

			//Calculate the difference between the beginning and end of the touch on the y axis.
			float y = touchEnd.y - touchOrigin.y;

			//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
			touchOrigin.x = -1;

			//Check if the difference along the x axis is greater than the difference along the y axis.
			if (Mathf.Abs(x) > Mathf.Abs(y))
			//If x is greater than zero, set horizontal to 1, otherwise set it to -1
			horizontal = x > 0 ? 1 : -1;
			else
			//If y is greater than zero, set horizontal to 1, otherwise set it to -1
			vertical = y > 0 ? 1 : -1;
			}
			}

			#endif //End of mobile platform dependendent compilation section started above with #elif
			//Check if we have a non-zero value for horizontal or vertical
			if (horizontal != 0 || vertical != 0)
			{
				AttemptMove(horizontal, vertical);
			}
			else
			{
				if (Input.GetButtonDown("LeftHand"))
				{
					leftHand.TryDrop((int)transform.position.x, (int)transform.position.y);
				}
				else if (Input.GetButtonDown("RightHand"))
				{
					rightHand.TryDrop((int)transform.position.x, (int)transform.position.y);
				}
			}
		}

		protected override void OnMoveBegan()
		{
			GameManager.instance.EndTurn();

			//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
			SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
		}

		public bool IsHolding<T>() where T : Animal
		{
			return leftHand.IsHolding<T> () || rightHand.IsHolding<T> ();
		}

		protected override Transform CheckCollision(Vector3 start, Vector3 end)
		{
			var hitTransform =  base.CheckCollision(start, end);

			if (hitTransform != null && hitTransform.tag == "Swimmable" && (IsHolding<Dolphin>() || IsHolding<Orca>()))
			{
				return null;
			}

			return hitTransform;
		}

		protected override void OnMoveBlocked(Transform component)
		{
			Animal animal = component.GetComponent<Animal>();

			if (animal != null)
			{
				// this is an animal; pick it up if we can
				bool pickedUp = TryPickUp(animal);
				if (pickedUp)
				{
					// pickup successful - go to next turn
					GameManager.instance.EndTurn();
					return;
				}
			}

			Home home = component.GetComponent<Home>();

			if (home != null)
			{
				// this is a home; drop an animal into it if we can
				bool animalPlaced = TryPutAnimalHome(home);
				if (animalPlaced)
				{
					// animal successfully placed - go to next turn
					GameManager.instance.EndTurn();
					return;
				}
			}

			if (component.tag == "Jumpable" || component.tag == "Home" || component.tag == "Animal")
			{
				// if you can't pickup or dropoff, try jumping over it if you have the rabbit
				float distanceToObstruction =
					Mathf.Abs (component.transform.position.x - transform.position.x) + Mathf.Abs (component.transform.position.y - transform.position.y);
				if (IsHolding<Rabbit> () && distanceToObstruction < 2)
				{
					// use rabbit to jump!
					int offsetX = (int)(component.transform.position.x - transform.position.x);
					int offsetY = (int)(component.transform.position.y - transform.position.y);
					AttemptMove(offsetX * 2, offsetY * 2);
					return;
				}
			}

			// if none of the above, you just hit an obstruction and couldn't move.
			SoundManager.instance.RandomizeSfx(hitWallSound);

		}

		protected override void AnimateDirection(Direction d)
		{
			switch (d)
			{
			case Direction.UpLeft:
				animator.SetTrigger("walkBack");
				break;
			case Direction.DownRight:
				animator.SetTrigger("walkFront");
				break;
			case Direction.DownLeft:
				animator.SetTrigger("walkLeft");
				break;
			case Direction.UpRight:
				animator.SetTrigger("walkRight");
				break;
			}
		}

		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D(Collider2D other)
		{
			// We can use this by setting collision triggers.
			if (other.tag == "Swimmable")
			{
				Debug.Log("Swimming");
			}
		}

		public bool TryPutAnimalHome(Home home)
		{
			bool placed = leftHand.TryPlaceIntoHome(home);
			if (placed)
			{
				return true;
			}
			else
				return rightHand.TryPlaceIntoHome(home);
		}

		public bool TryPickUp(Animal a)
		{
			float distanceToAnimal = Mathf.Abs(this.transform.position.x - a.transform.position.x) + Mathf.Abs(this.transform.position.y - a.transform.position.y);
			if (distanceToAnimal > 1)
			{
				// animal is too far away to grab
				return false;
			}

			if (leftHand.IsEmpty())
			{
				return leftHand.TryHold(a);
			}
			else if (rightHand.IsEmpty())
			{
				return rightHand.TryHold(a);
			}

			// hands are full
			return false;
		}
	}
}

