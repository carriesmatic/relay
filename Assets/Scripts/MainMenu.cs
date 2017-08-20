using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Relay
{
	public class MainMenu : MonoBehaviour
	{
		public void StartGame(string startScene)
		{
			SceneManager.LoadScene(startScene, LoadSceneMode.Single);
		}
	}
}
