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
		public string Name;

		public bool IsInHome()
		{
			Home parentHome = transform.parent.GetComponent<Home> ();
			return parentHome != null && parentHome.HomeFor == Name;
		}
	}
}

