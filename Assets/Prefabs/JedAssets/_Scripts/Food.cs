using UnityEngine;

namespace _Scripts
{
	public class Food : MonoBehaviour
	{
		[SerializeField] private Material shadedMaterial;
		private Material _defaultMaterial;

		private void Start()
		{
			_defaultMaterial = GetComponent<Renderer>().material;
		}

		internal void Highlight()
		{
			GetComponent<Renderer>().materials = new Material[] { _defaultMaterial, shadedMaterial };
		}

		internal void ResetMaterial()
		{
			GetComponent<Renderer>().materials = new Material[] { _defaultMaterial };
		}
	}
}