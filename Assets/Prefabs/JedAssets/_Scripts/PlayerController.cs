using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
	public class PlayerController : MonoBehaviour
	{
		private static readonly int IsEating = Animator.StringToHash("IsEating");
		private static readonly int IsWalking = Animator.StringToHash("IsWalking");
		private int _foodEaten = 0;

		[SerializeField] private float oinkDistance = 20f;
		[SerializeField] private float eatDistance = 5f;
		[SerializeField] private float stepDistance = 1f;

		private readonly float _fovAngle = 60f;

		[SerializeField] private Image loadingCircle; // Reference to the UI Image
		private readonly float _fillDuration = 2f; // Duration in seconds to complete one full circle
		private float _fillAmount;
		private bool _isEating;

		private Food _previousFood;

		[SerializeField] private float escapeUpperX;
		[SerializeField] private float escapeLowerX;
		[SerializeField] private float escapeUpperZ;
		[SerializeField] private float escapeLowerZ;

		// Oink sound effect
		[SerializeField] private AudioClip[] oinkSound;

		private AudioSource _oinkSource;

		// Eating sound effect
		[SerializeField] private AudioClip eatingSound;

		private AudioSource _eatingSource;

		// Walking sound effect
		[SerializeField] private AudioClip[] walkingSound;
		private AudioSource _walkingSource;

		private Bush _lastBush;
		[SerializeField] private AudioClip bushSound;

		[SerializeField] private Animator animator;

		//character controller
		[SerializeField] [Tooltip("Insert Character Controller")]
		private CharacterController controller;

		//camera controller
		[SerializeField] [Tooltip("Insert Camera Controller")]
		private Camera mainCamera;

		private Vector3 velocity;
		private float gravity = -9.8f;
		public float speed = 2f;
		public float rotationSpeed = 10f; // Speed of rotation lerp

		private void Start()
		{
			AudioSource[] audioSources = GetComponents<AudioSource>();
			_oinkSource = audioSources[0];
			_eatingSource = audioSources[1];
			_walkingSource = audioSources[2];
			_eatingSource.clip = eatingSound;
			_eatingSource.loop = true;
			// set tag to pig
			gameObject.tag = "Pig";

			_fillAmount = 0f;
			loadingCircle.fillAmount = 0f;
			loadingCircle.fillClockwise = true;
		}

		void Update()
		{
			CheckEscapeActivation();
			HandleEatLogic();
			if (!_isEating)
			{
				HandleMoveLogic();
				HandleOinkNoise();
			}
			else
			{
				HandleNoiseLogic(eatDistance);
			}
		}

		private void DrawCircle(float amount)
		{
			_fillAmount = amount;
			loadingCircle.fillAmount = Mathf.Clamp01(_fillAmount); // Clamp between 0 and 1
		}

		private void ResetDrawCircle()
		{
			DrawCircle(0f);
			_isEating = false;
			_eatingSource.Stop();
			animator.SetBool(IsEating, false);
		}

		private void HandleEatLogic()
		{
			Collider[] colliders = Physics.OverlapSphere(transform.position, eatDistance);
			float angleFromPlayer = _fovAngle / 2;
			Food target = null;
			foreach (Collider c in colliders)
			{
				// Check if the object has the specific component or type
				Food hit = c.GetComponent<Food>();
				var playerTransform = transform;
				float angleToTarget = Vector3.Angle(playerTransform.forward,
					(c.gameObject.transform.position - playerTransform.position).normalized);
				if (hit && angleToTarget < angleFromPlayer)
				{
					angleFromPlayer = angleToTarget;
					target = hit;
				}
			}

			if (target)
			{
				if (target != _previousFood)
				{
					if (_previousFood)
					{
						_previousFood.ResetMaterial();
					}

					target.Highlight();
					_previousFood = target;
				}

				if (!_isEating && Input.GetButton("Fire1"))
				{
					_isEating = true;
					_eatingSource.Play();
					animator.SetBool(IsEating, true);
				}

				// Highlight food
				if (Input.GetButton("Fire1")) // Check if eating and circle isn't complete
				{
					_fillAmount += Time.deltaTime / _fillDuration; // Increment fill based on time and duration
					DrawCircle(_fillAmount);

					if (_fillAmount >= 1f)
					{
						// Eat the food
						Destroy(target.gameObject);
						_foodEaten++;
						ResetDrawCircle();
					}
				}

				if (Input.GetButtonUp("Fire1")) // Reset when the button is released
				{
					ResetDrawCircle();
				}
			}
			else
			{
				ResetDrawCircle();
				if (_previousFood)
				{
					_previousFood.ResetMaterial();
					_previousFood = null;
				}
			}
		}

		private void HandleMoveLogic()
		{
			// Movement basics (without applying it yet)
			float x = Input.GetAxis("Horizontal");
			float z = Input.GetAxis("Vertical");

			// Get directions relative to the camera
			Vector3 cameraForward = mainCamera.transform.forward;
			Vector3 cameraRight = mainCamera.transform.right;

			// Ignore vertical components
			cameraForward.y = 0;
			cameraRight.y = 0;

			// Normalize directions to ensure consistent movement speed
			cameraForward.Normalize();
			cameraRight.Normalize();

			// Calculate the movement direction relative to the camera
			Vector3 movement = (cameraForward * z + cameraRight * x).normalized;

			// Apply gravity
			velocity.y += gravity * Time.deltaTime;

			// Check if the player is moving forward
			if (z > 0) // Moving forward
			{
				// Adjust rotation speed dynamically based on angle
				float dynamicRotationSpeed = CalculateDynamicRotationSpeed(cameraForward);

				// Rotate the player toward the camera's forward direction
				AlignCharacterToCamera(cameraForward, dynamicRotationSpeed);

				// Always allow movement, regardless of rotation
				velocity.x = movement.x * speed;
				velocity.z = movement.z * speed;

				animator.SetBool(IsWalking, true);
			}
			else
			{
				// Stop horizontal movement if no input
				velocity.x = 0;
				velocity.z = 0;

				animator.SetBool(IsWalking, false);
			}

			// Apply the velocity (gravity + movement)
			controller.Move(velocity * Time.deltaTime);
		}

		private void HandleNoiseLogic(float soundRadius)
		{
			Collider[] colliders = Physics.OverlapSphere(transform.position, soundRadius);
			foreach (Collider c in colliders)
			{
				// Check if the object has the specific component or type
				Farmer hit = c.GetComponent<Farmer>();
				if (hit)
				{
					hit.Alert(transform.position);
					break;
				}
			}
		}

		private void HandleOinkNoise()
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				_oinkSource.clip = oinkSound[Random.Range(0, oinkSound.Length)];
				_oinkSource.Play();
			}

			HandleNoiseLogic(oinkDistance);
		}

		// Walking animation step hits ground event
		public void PlayFootstep()
		{
			if (_lastBush)
			{
				_walkingSource.clip = bushSound;
			}
			else
			{
				_walkingSource.clip = walkingSound[Random.Range(0, walkingSound.Length)];
			}

			_walkingSource.Play();
			HandleNoiseLogic(stepDistance);
		}

		private void AlignCharacterToCamera(Vector3 cameraForward, float dynamicRotationSpeed)
		{
			Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
			transform.rotation =
				Quaternion.Lerp(transform.rotation, targetRotation, dynamicRotationSpeed * Time.deltaTime);
		}

		private float CalculateDynamicRotationSpeed(Vector3 cameraForward)
		{
			float angle = Vector3.Angle(transform.forward, cameraForward);

			// Increase speed proportionally to the angle difference
			float speedMultiplier = Mathf.Clamp(angle / 30f, 1f, 5f); // Adjust values as needed
			return rotationSpeed * speedMultiplier;
		}

		public void SetBush(Bush bush)
		{
			_lastBush = bush;
		}

		private void CheckEscapeActivation()
		{
			if (transform.position.x < escapeUpperX || transform.position.x > escapeLowerX ||
			    transform.position.z < escapeUpperZ || transform.position.z > escapeLowerZ)
			{
				// Activate area notice

				// if enough food eaten then give button prompt

				if (Input.GetButton("Fire3")) // and enough eaten
				{
					// End game
				}
			}
		}
	}
}