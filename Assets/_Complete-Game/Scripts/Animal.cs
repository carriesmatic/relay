using System;
using UnityEngine;
using System.Collections;

namespace Relay
{
	/// <summary>
	/// An animal is an Entity that wants to go to a specific home.
	/// </summary>
	public class Animal : MonoBehaviour
	{
		public Home wantedHome;
	}

	/// <summary>
	/// Special type of animal - a bear.
	/// </summary>
	public class Bear : Animal
	{
	}
}

