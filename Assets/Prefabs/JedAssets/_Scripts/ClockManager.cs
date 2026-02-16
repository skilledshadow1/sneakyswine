using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
	public class ClockManager : MonoBehaviour
	{
		private const float HOURS_PER_GAME_DAY = 6;
		private const float SECONDS_PER_GAME_HOUR = 75;
		private const float DAY_LENGTH = HOURS_PER_GAME_DAY * SECONDS_PER_GAME_HOUR;

		private float _elapsedTime;

		[SerializeField] private TMP_Text clockText;
		[SerializeField] private EnvironmentalSounds environmentalSounds;
		private bool _franticMusicPlayed;

		void Start()
		{
			_elapsedTime = 0;
		}

		void Update()
		{
			_elapsedTime += Time.deltaTime;
			string time = GetClockTime();
			clockText.text = time;
		}

		private string GetClockTime()
		{
			float currentHour = _elapsedTime / SECONDS_PER_GAME_HOUR;
			float currentMinute = (_elapsedTime * (60 / SECONDS_PER_GAME_HOUR)) % 60;

			int hours = (int)currentHour % 12;
			if (hours == 0)
			{
				hours = 12;
			}
			else if (hours == 6)
			{
				SceneManager.LoadScene("EndTimeOutScreen");
			}

			int minutes = (int)currentMinute;
			if (hours != 12 && hours >= 5)
			{
				if (!_franticMusicPlayed)
				{
					environmentalSounds.SetLastHourMusic();
					clockText.color = Color.red;
					_franticMusicPlayed = true;
				}
			}
			else
			{
				clockText.color = Color.black;
			}

			return $"{hours}:{minutes:00} PM";
		}
	}
}