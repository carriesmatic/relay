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
			Debug.Log("Starting game.");
			SceneManager.LoadScene(startScene, LoadSceneMode.Single);
		}

		public void ExitGame()
		{
			Debug.Log("Quitting game.");
			Application.Quit();
		}
	}
}
