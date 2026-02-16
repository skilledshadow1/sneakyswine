using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public class FarmerPathing : MonoBehaviour
{
    //Bushes need to have the tag "Bush"
    private Collider[] bushes;
    
    //Hearing
    [SerializeField] private float hearingRadius;
    
    
    //Small Class For Audio Management
    [System.Serializable]
    public class AudioClipSettings
    {
        public float pitch = 1.0f;
        public float volume = 1.0f;
        public AudioClip clip;
    }
    
    [SerializeField] AudioClip farmerHmm;
    [SerializeField] AudioClip farmerGrunt;
    [SerializeField] AudioClip farmerLaugh;
    
    //Audio
    [Header("Audio")]
    [SerializeField] List<AudioClipSettings> clipPresets = new List<AudioClipSettings>();
    [SerializeField] AudioSource farmerAudioSource; //This is to remind me to add this
    [SerializeField] AudioSource footstepAudioSource;
    
    //Farmer Vision
    private VisionCone peripheralVision;
    private VisionCone alertVision;
    
    //Components
    [SerializeField] private LineRenderer farmerPath;
    private NavMeshAgent navMesh;
    private Animator anim;
    
    //Waypoint
    private Vector3[] waypoints;
    private int targetWaypointIndex;
    private int cornerIndex;
    private NavMeshPath newPath; //Path that the farmer is walking on to the sneak destination
    
    [Header("Turning")]
    [SerializeField] private float turnSpeed;
    
    [Tooltip("Angle required to do turn around animation")]
    [SerializeField] float turnAngleRequirement;
    //IMPORTANT NOTE: If a turn requires something to the right of the farmer, it will look weird because
    //the farmer always turns toward the left. The animation does a 180, not a 215-degree turn.
    
    public bool busyTurning;
    
    [Header("Patrolling")]
    [SerializeField] private float walkSpeed;
    private bool isWalking;
    [SerializeField] bool movingForward;
    
    [Header("Sneaking")]
    [Tooltip("This controls how far the Navmesh will look for possible points from an impassable location")]
    [SerializeField] private float navMeshErrorVision;
    
    public enum FarmerState
    {
        Patrol, Suspicious, LookingAround, Alert
    }
    
    private void PlaySound(AudioClip clip)
    {
        var preset = clipPresets.Find(p => p.clip == clip);
        if (preset != null)
        {
            farmerAudioSource.clip = clip;
            farmerAudioSource.pitch = preset.pitch;
            farmerAudioSource.volume = preset.volume;
            farmerAudioSource.Play();
        }
        else Debug.Log("NOT HERE");
    }
    
    
    private void GetBushes()
    {
        //Bushes need to have the tag "Bush"
         GameObject[] bushObjects = GameObject.FindGameObjectsWithTag("Bush");
         bushes = new Collider[bushObjects.Length];
         for (int i = 0; i < bushObjects.Length; i++)
         {
             bushes[i] = bushObjects[i].GetComponent<Collider>();
         }
        
    }
    
    private void TurnOffBushes()
    {
        for (int i = 0; i < bushes.Length; i++)
        {
            bushes[i].enabled = false;
        }
    }

    private void TurnOnBushes()
    {
        for (int i = 0; i < bushes.Length; i++)
        {
            bushes[i].enabled = true;
        }
    }
    
    [SerializeField] FarmerState farmerState;

    //Only used at start because I don't want to begin with the farmer making a grunt sound
    public void StartPatrol()
    {
        farmerState = FarmerState.Patrol;
        getNextPatrolPoint(startingUp:true);
    }
    
    public void changeState(FarmerState state)
    {
        if (farmerState == FarmerState.Alert) return; //Makes sure once you enter alert, you cannot leave
        farmerState = state;
        switch (state)
        {
            case FarmerState.Patrol:
                anim.SetBool("LookingAround", false);
                getNextPatrolPoint(startingUp:true);
                PlaySound(farmerGrunt);
                TurnOnBushes();
                break;
            case FarmerState.Suspicious:
                anim.SetBool("Suspicious", true);
                PlaySound(farmerHmm);
                TurnOffBushes();
                break;
            case FarmerState.LookingAround:
                anim.SetBool("LookingAround", true);
                anim.SetBool("Suspicious", false);
                TurnOffBushes();
                break;
            case FarmerState.Alert:
                StartCoroutine(WaitForGameOver());
                PlaySound(farmerLaugh);
                anim.SetBool("PlayerIsFound", true);
                
                break;
        }
    }
    
    [SerializeField] _Scripts.PlayerMovement player;
    
    IEnumerator WaitForGameOver()
    {
        player.StopMoving();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("EndCatchScreen");
    }
    
    private void Start()
    {
        VisionCone[] visionCones = GetComponents<VisionCone>();
        if (visionCones[0].isPeripheral)
        {
            peripheralVision = visionCones[0];
            alertVision = visionCones[1];
        }
        else
        {
            peripheralVision = visionCones[1];
            alertVision = visionCones[0];
        }
        
        GetBushes();
        TurnOnBushes();
        
        movingForward = true;
        newPath = new NavMeshPath();
        navMesh = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        navMesh.speed = walkSpeed;
        getWaypoints();
        farmerState = FarmerState.Patrol;
    }

    private void getWaypoints()
    {
        waypoints = new Vector3[farmerPath.positionCount];
        for (int i = 0; i < waypoints.Length; ++i)
        {
            waypoints[i] = farmerPath.GetPosition(i);
        }
    }
    
    public void HearNoise(Vector3 noisePos)
    {
        float noiseDist = Vector3.Distance(transform.position, noisePos);
        if (noiseDist < hearingRadius)
        {
            goTowardsPosition(noisePos);
        }
    }
    
    private void turnAndMove()
    {
        if (distIgnoringY(navMesh.destination, transform.position) > 0.01f && navMesh.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = navMesh.velocity.normalized; 
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
    
    private float distIgnoringY(Vector3 v1, Vector3 v2)
    {
        return Vector3.Distance(new Vector3(v1.x, 0, v1.z), new Vector3(v2.x, 0, v2.z));
    }


    private void turningAndMovement()
    {
        if (newPath == null) return;
        
        if (busyTurning) //Keeps track of turning in AnimationBusy.cs
        {
            navMesh.isStopped = true; //For now no extra movement if possible
        }
        else
        {
            navMesh.isStopped = false;
            
            turnAndMove(); //This is just slight turning
            getNextCorner();
        }
    }
    
    float getTurnAngle()
    {
        Vector3 turnVector = (newPath.corners[cornerIndex] - transform.position).normalized;
        return Vector3.Angle(transform.forward, turnVector);
    }
    
    void getNextCorner()
    {
        //Debug.Log(distIgnoringY(newPath.corners[cornerIndex], transform.position));
        //Just to make sure it doesn't search anything incorrect
        if (distIgnoringY(newPath.corners[cornerIndex], transform.position) < 0.01f)
        {
            cornerIndex++;
        }
        
        if (cornerIndex >= newPath.corners.Length)
        {
            cornerIndex = 1;
            navMesh.ResetPath();
            if (farmerState == FarmerState.Patrol)
            {
                getNextPatrolPoint(startingUp:false);
            }
            else if (farmerState == FarmerState.Suspicious)
            {
                changeState(FarmerState.LookingAround);
            }
        }
        else
        {
            navMesh.destination = newPath.corners[cornerIndex];
        }
    }
    
    //Farmer does sneak towards clicked area
    // private void goToClick() //For just testing purposes
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
    //         {
    //             goTowardsPosition(hit.point); //If the turn is too big, it will go to turnAround first
    //         }
    //     }
    // }

    private void goTowardsPosition(Vector3 raycastPoint)
    {
        Vector3 flatTargetPosition = new Vector3(raycastPoint.x, transform.position.y, raycastPoint.z);
        if (!NavMesh.SamplePosition(flatTargetPosition, out var hit, navMeshErrorVision, NavMesh.AllAreas))
        {
            return;
        }
        raycastPoint = hit.position;
        newPath = new NavMeshPath();
        navMesh.CalculatePath (raycastPoint, newPath);
        if(farmerState != FarmerState.Suspicious) changeState(FarmerState.Suspicious);
        cornerIndex = 1; //Sets the player to head towards the first corner
        navMesh.destination = newPath.corners[cornerIndex];
        
        //Checks if the farmer needs to turn
        if (getTurnAngle() > turnAngleRequirement)
        {
            anim.SetBool("TurnAround", true);
            //This is only called at the beginning of a new path as it is unlikely that you will turn 
            //more than 145 degrees on a path because that would make no sense. It's an open area so why turn so much
        }
    }

    private void getNextPatrolPoint(bool startingUp)
    {
        Vector3 patrolDestination = startingUp ? waypoints[getClosestWaypoint()] : waypoints[getNextWaypoint()];
        navMesh.CalculatePath (patrolDestination, newPath);
    }

    int getNextWaypoint()
    {
        if (movingForward) targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
        else targetWaypointIndex--;
        
        if(targetWaypointIndex < 0) targetWaypointIndex = waypoints.Length - 1;
        
        return targetWaypointIndex;
    }
    int getClosestWaypoint()
    {
        //Assumes there are more than 2 waypoints
        float closestDist = Mathf.Infinity;
        float secondClosestDist = Mathf.Infinity;
        int closestPointIndex = -1;
        int secondClosestIndex = -1;
        for (int i = 0; i < waypoints.Length; ++i)
        {
            float currentDist = distIgnoringY(waypoints[i], transform.position);
            if (closestDist > currentDist)
            {
                secondClosestIndex = closestPointIndex;
                closestPointIndex = i;
                
                secondClosestDist = closestDist;
                closestDist = currentDist;
                
            }
            else if (secondClosestDist > currentDist)
            {
                secondClosestIndex = i;
                
                secondClosestDist = currentDist;
            }
        }
        
        //Debug.Log("First: " + closestPointIndex + ", Second: " + secondClosestIndex);
        //Bases the direction based on the 2 closest waypoint indices 
        //This assumes the waypoints are in a grid-like fashion
        //Ex: if you are between nodes 3 and 6 and are closer to 3, you should go backwards
        movingForward = closestPointIndex - secondClosestIndex > 0;
        targetWaypointIndex = closestPointIndex;
        return closestPointIndex;
    }

    private void Update()
    {
        //Debug.Log(newPath.corners.Length);
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        GetVisionInfo();
        //goToClick();
        stateActions();
    }

    private void GetVisionInfo()
    {
        if (alertVision.playerInView)
        {
            changeState(FarmerState.Alert);
        }
        else if(peripheralVision.playerInView)
        {
            goTowardsPosition(peripheralVision.gameObjects[0].transform.position);
        }
    }

    private void stateActions()
    {
        switch (farmerState)
        {
            case FarmerState.Patrol:
                turningAndMovement();
                break;
            case FarmerState.Suspicious:
                turningAndMovement();
                break;
            case FarmerState.LookingAround:
                navMesh.isStopped = true; //Just in case
                break;
            case FarmerState.Alert:
                navMesh.isStopped = true;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        //This is to visualize the hearing
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}
