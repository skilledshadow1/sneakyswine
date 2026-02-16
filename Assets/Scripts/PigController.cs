// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace _Scripts
// {
// 	public class PlayerController : MonoBehaviour
// 	{
// 		private int _foodEaten = 0;
//
// 		[SerializeField] private float eatDistance = 3f;
// 		private readonly float _fovAngle = 60f;
//
// 		//[SerializeField] private Image loadingCircle; // Reference to the UI Image
// 		private readonly float _fillDuration = 2f; // Duration in seconds to complete one full circle
// 		private float _fillAmount;
// 		private bool _isEating = false;
//
// 		//private Food _previousFood;
//
// 		// Oink sound effect
// 		[SerializeField] private AudioClip[] oinkSound;
// 		private AudioSource _audioSource;
// 		
// 		//character controller
// 		[SerializeField]
// 		[Tooltip("Insert Character Controller")]
// 		private CharacterController controller;
// 		
// 		//camera controller
// 		[SerializeField]
// 		[Tooltip("Insert Camera Controller")]
// 		private Camera mainCamera;
// 		
// 		private Vector3 velocity;
// 		private float gravity = -9.8f;
// 		public float speed = 2f;
//         public float rotationSpeed = 10f; // Speed of rotation lerp
//         private BellyCounter bellyCounter;
//
// 		private void Start()
// 		{
// 			_audioSource = GetComponent<AudioSource>();
// 			bellyCounter = FindObjectOfType<BellyCounter>();
// 		}
//
// 		void Update()
// 		{
// 			// Movement basics (without applying it yet)
// 			float x = Input.GetAxis("Horizontal");
// 			float z = Input.GetAxis("Vertical");
//
// 			// Get directions relative to the camera
// 			Vector3 cameraForward = mainCamera.transform.forward;
// 			Vector3 cameraRight = mainCamera.transform.right;
//
// 			// Ignore vertical components
// 			cameraForward.y = 0;
// 			cameraRight.y = 0;
//
// 			// Normalize directions to ensure consistent movement speed
// 			cameraForward.Normalize();
// 			cameraRight.Normalize();
//
// 			// Calculate the movement direction relative to the camera
// 			Vector3 movement = (cameraForward * z + cameraRight * x).normalized;
//
// 			// Apply gravity
// 			velocity.y += gravity * Time.deltaTime;
//
// 			// Check if the player is moving forward
// 			if (z > 0) // Moving forward
// 			{
// 				// Adjust rotation speed dynamically based on angle
// 				float dynamicRotationSpeed = CalculateDynamicRotationSpeed(cameraForward);
// 			
// 				// Rotate the player toward the camera's forward direction
// 				AlignCharacterToCamera(cameraForward, dynamicRotationSpeed);
// 			
// 				// Always allow movement, regardless of rotation
// 				velocity.x = movement.x * speed;
// 				velocity.z = movement.z * speed;
// 			}
// 			else
// 			{
// 				// Stop horizontal movement if no input
// 				velocity.x = 0;
// 				velocity.z = 0;
// 			}
//
// 			// Apply the velocity (gravity + movement)
// 			controller.Move(velocity * Time.deltaTime);
// 			
// 			HandleEatLogic();
// 			if (!_isEating)
// 			{
// 				// Apply movement
// 				controller.Move(velocity * Time.deltaTime);
// 				HandleNoiseLogic();
// 			}
// 		}
//
// 		private void ResetDrawCircle()
// 		{
// 			_fillAmount = 0f;
// 			//loadingCircle.fillAmount = 0f;
// 			_isEating = false;
// 		}
// 		//For Jed: implement this when the food is actually eaten. 
// 		//bellyCounter.IncreaseCount();
//
// 		private void HandleEatLogic()
// 		{
// 			Collider[] colliders = Physics.OverlapSphere(transform.position, eatDistance);
// 			float angleFromPlayer = _fovAngle / 2;
// 			//Food target = null;
// 			foreach (Collider c in colliders)
// 			{
// 				// Check if the object has the specific component or type
// 				//Food hit = c.GetComponent<Food>();
// 				var playerTransform = transform;
// 				float angleToTarget = Vector3.Angle(playerTransform.forward,
// 					(c.gameObject.transform.position - playerTransform.position).normalized);
// 				// if (hit && angleToTarget < angleFromPlayer)
// 				// {
// 				// 	angleFromPlayer = angleToTarget;
// 				// 	target = hit;
// 				// }
// 			}
//
// 			// if (target)
// 			// {
// 			// 	if (target != _previousFood)
// 			// 	{
// 			// 		if (_previousFood)
// 			// 		{
// 			// 			_previousFood.ResetMaterial();
// 			// 		}
// 			//
// 			// 		Debug.Log("Highlighting");
// 			// 		target.Highlight();
// 			// 		_previousFood = target;
// 			// 	}
// 			//
// 			// 	if (Input.GetButtonDown("Fire1"))
// 			// 	{
// 			// 		_isEating = true;
// 			// 	}
// 			//
// 			// 	// Highlight food
// 			// 	if (Input.GetButton("Fire1")) // Check if eating and circle isn't complete
// 			// 	{
// 			// 		_fillAmount += Time.deltaTime / _fillDuration; // Increment fill based on time and duration
// 			// 		//loadingCircle.fillAmount = Mathf.Clamp01(_fillAmount); // Clamp between 0 and 1
// 			//
// 			// 		if (_fillAmount >= 1f)
// 			// 		{
// 			// 			// Eat the food
// 			// 			Destroy(target.gameObject);
// 			// 			_foodEaten++;
// 			// 			ResetDrawCircle();
// 			// 		}
// 			// 	}
// 			//
// 			// 	if (Input.GetButtonUp("Fire1")) // Reset when the button is released
// 			// 	{
// 			// 		ResetDrawCircle();
// 			// 	}
// 			// }
// 			// else
// 			// {
// 			// 	ResetDrawCircle();
// 			// 	if (_previousFood)
// 			// 	{
// 			// 		_previousFood.ResetMaterial();
// 			// 		_previousFood = null;
// 			// 	}
// 			// }
// 		}
// 		
//
// 		private void HandleNoiseLogic()
// 		{
// 			if (Input.GetKeyDown(KeyCode.E))
// 			{
// 				_audioSource.clip = oinkSound[Random.Range(0, oinkSound.Length)];
// 				_audioSource.Play();
// 			}
//
// 			Collider[] colliders = Physics.OverlapSphere(transform.position, eatDistance);
// 			foreach (Collider c in colliders)
// 			{
// 				// Check if the object has the specific component or type
// 				//Farmer hit = c.GetComponent<Farmer>();
// 				// if (hit)
// 				// {
// 				// 	hit.Alert(transform.position);
// 				// 	break;
// 				// }
// 			}
// 		}
// 		
// 		private void AlignCharacterToCamera(Vector3 cameraForward, float dynamicRotationSpeed)
// 		{
// 			Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
// 			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, dynamicRotationSpeed * Time.deltaTime);
// 		}
//
// 		private float CalculateDynamicRotationSpeed(Vector3 cameraForward)
// 		{
// 			float angle = Vector3.Angle(transform.forward, cameraForward);
//
// 			// Increase speed proportionally to the angle difference
// 			float speedMultiplier = Mathf.Clamp(angle / 30f, 1f, 5f); // Adjust values as needed
// 			return rotationSpeed * speedMultiplier;
// 		}
// 	}
// } 