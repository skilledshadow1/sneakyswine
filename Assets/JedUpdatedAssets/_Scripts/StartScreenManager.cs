using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Jed._Scripts
{
	public class StartScreenManager : MonoBehaviour
	{
		[SerializeField] private GameObject creditsPanel;
		[SerializeField] private GameObject controlsPanel;
		[SerializeField] private GameObject mainPanel;

		public void StartGameClicked()
		{
			SceneManager.LoadScene("Scenes/GameScene");
		}

		public void CreditsClicked()
		{
			creditsPanel.SetActive(true);
			mainPanel.SetActive(false);
		}

		public void CloseCreditsClicked()
		{
			creditsPanel.SetActive(false);
			mainPanel.SetActive(true);
		}

		public void ControlsClicked()
		{
			controlsPanel.SetActive(true);
			mainPanel.SetActive(false);
		}

		public void CloseControlsClicked()
		{
			controlsPanel.SetActive(false);
			mainPanel.SetActive(true);
		}

		public void ExitClicked()
		{
			Application.Quit();
		}

		private void Start()
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}
}