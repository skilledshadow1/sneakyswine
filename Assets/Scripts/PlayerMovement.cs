using System.Collections;
using System.Collections.Generic;
using Prefabs.JedAssets._Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
	public class PlayerMovement : MonoBehaviour
	{
		[SerializeField] private FarmerPathing[] farmers;
		[SerializeField] private GameObject pausePanel;
		[SerializeField] private TMP_Text foodText;
		private static readonly int IsEating = Animator.StringToHash("IsEating");
		private static readonly int IsWalking = Animator.StringToHash("IsWalking");
		private static readonly int IsRunning = Animator.StringToHash("IsRunning");

		// assigned values are just examples, feel free to change them
		[SerializeField] private float oinkDistance = 20f;
		[SerializeField] private float eatDistance = 3f;
		[SerializeField] private float stepDistance = 1;

		private readonly float _fovAngle = 60f;

		[SerializeField] private Image loadingCircle; // Reference to the UI Image
		private readonly float _fillDuration = 2f; // Duration in seconds to complete one full circle
		private float _fillAmount;
		private bool _isEating = false;
		private bool stopMoving;

		private Food _previousFood;

		[SerializeField] private float escapeUpperX;
		[SerializeField] private float escapeLowerX;
		[SerializeField] private float escapeUpperZ;
		[SerializeField] private float escapeLowerZ;

		[SerializeField] private AudioClip[] oinkSound;
		private AudioSource _oinkSource;

		[SerializeField] private AudioClip eatingSound;
		private AudioSource _eatingSource;

		[SerializeField] private AudioClip[] walkingSound;
		private AudioSource _walkingSource;

		[SerializeField] private Animator animator;
		
		//character controller
		[SerializeField]
		[Tooltip("Insert Character Controller")]
		private CharacterController controller;
		
		//camera controller
		[SerializeField]
		[Tooltip("Insert Camera Controller")]
		private Camera mainCamera;
		
		private Vector3 velocity;
		private float gravity = -9.8f;
		private float speed;
		public float runSpeed = 2f;
		public float walkSpeed = 0.8f;
        public float rotationSpeed = 10f; // Speed of rotation lerp

		[Header("Belly Growing")]
		[SerializeField] private Transform bellyBone;
		[SerializeField] private float startScale = 0.3f;
		[SerializeField] private float endScale = 1.3f;
		[SerializeField] private int _maxFoodEaten = 10;

		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			stopMoving = false;
			FoodCounter.ResetFood();
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

			bellyBone.localScale = new Vector3(startScale, startScale, startScale);
		}
		
		void Update()
		{
			if (stopMoving) return;
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Pause(); 
			}
			
			CheckEscapeActivation();
			HandleEatLogic();
			if (!_isEating)
			{
				HandleMoveLogic();
				HandleOinkNoise();
			}
			else
			{
				HandleNoiseLogic();
			}
		}
		public void Pause()
		{
			Cursor.lockState = CursorLockMode.None;
			Time.timeScale = 0f;
			pausePanel.SetActive(true);
		}

		public void StopMoving()
		{
			stopMoving = true;
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
						FoodCounter.AddFood();
						foodText.text = FoodCounter.FoodCount + "/" + FoodCounter.MaxFood;
						ResetDrawCircle();
						float scale = Mathf.Lerp(startScale, endScale, (float) FoodCounter.FoodCount / _maxFoodEaten);
						bellyBone.localScale = new Vector3(scale, scale, scale);
						Debug.Log(scale);
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
		
		private void HandleMoveLogic() {
		// Movement basics (without applying it yet)
			float x = Input.GetAxisRaw("Horizontal");
			float z = Input.GetAxisRaw("Vertical");

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
			
			// Adjust rotation speed dynamically based on player angle to the camera
			float dynamicRotationSpeed = CalculateDynamicRotationSpeed(cameraForward);

			// Lets check if we should walk or run
			bool holdingShift = Input.GetKey(KeyCode.LeftShift);
			speed = holdingShift ? runSpeed : walkSpeed;

			void SetAnimBools() 
			{
				animator.SetBool(IsWalking, !holdingShift);
				animator.SetBool(IsRunning, holdingShift);
			}
			
			// Check if the player is moving forward
			if (z > 0 && x == 0) // Moving forward
			{
				// Rotate the player toward the camera's forward direction
				AlignCharacterToCamera(cameraForward, dynamicRotationSpeed);

				// movement
				velocity.x = movement.x * speed;
				velocity.z = movement.z * speed;

				SetAnimBools();
			}
			else
			{
				if (z != 0 || x != 0)
				{
					// Calculate movement direction based on input and camera orientation
					movement = (cameraForward * z + cameraRight * x).normalized;

					// Align the character's rotation to the movement direction
					AlignCharacterToCamera(movement, dynamicRotationSpeed);

					// Apply movement velocity
					velocity.x = movement.x * speed;
					velocity.z = movement.z * speed;

					SetAnimBools();
				}
				else
				{
					if (x > 0)
					{
						AlignCharacterToCamera(cameraRight, dynamicRotationSpeed);
						movement = (cameraRight * x).normalized;
						velocity.x = movement.x * speed;
						velocity.z = movement.z * speed;

						SetAnimBools();
					}
					else
					{
						if (x < 0)
						{
							AlignCharacterToCamera(cameraRight * -1, dynamicRotationSpeed);
							movement = (cameraRight * x + cameraForward * z).normalized;
							velocity.x = movement.x * speed;
							velocity.z = movement.z * speed;

							SetAnimBools();
						}
						else
						{
							//if the player is moving backward, do not rotate the camera with them
							if (z < 0)
							{
								AlignCharacterToCamera(cameraForward * -1, dynamicRotationSpeed);
					
								movement = (cameraForward * z + cameraRight * x * -1).normalized;
								// movement
								velocity.x = movement.x * speed;
								velocity.z = movement.z * speed;
								print(movement.normalized);

								SetAnimBools();
							}
							else
							{
								if (x == 0)
								{
									//Stop horizontal movement if no input
									velocity.x = 0;
									velocity.z = 0;

									animator.SetBool(IsWalking, false);
									animator.SetBool(IsRunning, false);
								}
							}
						}
					}
				}
			}

			// Apply the velocity (gravity + movement)
			
			controller.Move(velocity * Time.deltaTime);
		}

		private void HandleNoiseLogic()
		{
			for (int i = 0; i < farmers.Length; i++)
			{
				farmers[i].HearNoise(transform.position);
			}
		}

		private void HandleOinkNoise()
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				_oinkSource.clip = oinkSound[Random.Range(0, oinkSound.Length)];
				_oinkSource.Play();
				HandleNoiseLogic();
			}
		}

		// Walking animation step hits ground event
		public void PlayFootstep()
		{
			_walkingSource.clip = walkingSound[Random.Range(0, walkingSound.Length)];
			_walkingSource.Play();
			//HandleNoiseLogic();
		}
		private void CheckEscapeActivation()
		{
			if (transform.position.x < escapeUpperX || transform.position.x > escapeLowerX ||
			    transform.position.z < escapeUpperZ || transform.position.z > escapeLowerZ)
			{
				// Activate area notice

				// if enough food eaten then give button prompt

				if (Input.GetButton("Fire3")) // and enough eaten (button can be changed)
				{
					// End game
				}
			}
		}
		
		private void AlignCharacterToCamera(Vector3 cameraForward, float dynamicRotationSpeed)
		{
			Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, dynamicRotationSpeed * Time.deltaTime);
		}

		private float CalculateDynamicRotationSpeed(Vector3 cameraForward)
		{
			float angle = Vector3.Angle(transform.forward, cameraForward);

			// Use a more consistent scaling factor to reduce the speed disparity
			float speedMultiplier = Mathf.Clamp(1f + angle / 90f, 1f, 2f); // Adjust max multiplier as needed
			return rotationSpeed * speedMultiplier;
		}
	}
} 