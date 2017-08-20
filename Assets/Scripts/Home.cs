using UnityEngine;

namespace Relay
{
	/// <summary>
	///  an Entity that an Animal wants to live in. A home can either
	/// be empty or filled. The Level ends when all homes have been filled.
	/// 
	/// When an Animal enters the home, the home becomes filled. Homes block
	/// movement until they are filled.
	/// 
	/// Open questions:
	/// 
	/// Can you take an animal out of the home after you fill it?
	/// Can you put the wrong animal into the home?
	/// </summary>
	public class Home : MonoBehaviour
	{
		public string HomeFor;
	}
}

