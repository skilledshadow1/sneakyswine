using UnityEngine;

namespace _Scripts
{
	public class Bush : MonoBehaviour
	{
		private AudioSource audioSource;
		[SerializeField] AudioClip audioClip;
		bool _hidingPig = false;

		void Start()
		{
			audioSource = GetComponent<AudioSource>();
		}
		
		private void OnTriggerEnter(Collider other)
		{
			Debug.Log("Enter");
			if (other.CompareTag("Pig"))
			{
				audioSource.Play();
				_hidingPig = true;
				other.GetComponent<PlayerController>().SetBush(this);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			Debug.Log("Exit");
			if (other.CompareTag("Pig"))
			{
				_hidingPig = false;
				other.GetComponent<PlayerController>().SetBush(null);
			}
		}

		public bool HasPig()
		{
			return _hidingPig;
		}
	}
}