using Unity.VisualScripting;
using UnityEngine.SceneManagement;

namespace Prefabs.JedAssets._Scripts
{
	public static class FoodCounter
	{
		public static int FoodCount { get; private set; } = 0;
		public const int MaxFood = 20;

		public static void AddFood()
		{
			FoodCount++;
			if (FoodCount >= MaxFood)
			{
				SceneManager.LoadScene("EndWinScreen");
			}
		}

		public static void ResetFood()
		{
			FoodCount = 0;
		}
	}
}