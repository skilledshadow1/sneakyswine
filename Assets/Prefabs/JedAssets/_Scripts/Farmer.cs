using UnityEngine;

namespace _Scripts
{
	public class Farmer : MonoBehaviour
	{
		private float _inspectionRadius;
		internal void Alert(Vector3 position)
		{
			Debug.Log("Farmer: I heard an oink at " + position);
		}
	}
}