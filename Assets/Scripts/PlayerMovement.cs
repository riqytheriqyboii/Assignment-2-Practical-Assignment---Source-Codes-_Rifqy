using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGGE;
using System;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public CharacterController mCharacterController;
    public Animator mAnimator;


    public float mWalkSpeed = 1.5f;
    public float mRotationSpeed = 50.0f;
    public bool mFollowCameraForward = false;
    public float mTurnRate = 10.0f;

#if UNITY_ANDROID
    public FixedJoystick mJoystick; 
#endif

    private float hInput;
    private float vInput;
    private float speed;
    private bool jump = false;
    private bool crouch = false;
    public float mGravity = -30.0f;
    public float mJumpHeight = 1.0f;

    // Variables to detect movement using mobile
    private float xMobileMovement = 0f;
    private float zMobileMovement = 0f;

    public string floorType = "Default";

    public AudioSource footstepAudioSource; // AudioSource for footstep sounds
    public AudioClip[] footstepSoundsConcrete; // Stores footstep sounds for concrete/deafult surfaces
    public AudioClip[] footstepSoundsDirt; // Stores footstep sounds for dirt
    public AudioClip[] footstepSoundsMetal; // Stores footstep sounds for metal
    public AudioClip[] footstepSoundsSand; // Stores footstep sounds for sand
    public AudioClip[] footstepSoundsWood; // Stores footstep sounds for wood

    public float stepRate = 0.69f; // Time between each step (step/second)
    private float nextStepTime = 0.0f; // Determines when the next step sound effect is played
    private bool isMoving = false; // Flag to check if the player is moving

    private Vector3 mVelocity = new Vector3(0.0f, 0.0f, 0.0f);

    void Start()
    {
        mCharacterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleInputs();
        Move();
        DetectSurface();

    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    public void HandleInputs()
    {
        // We shall handle our inputs here.
#if UNITY_STANDALONE
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
#endif

#if UNITY_ANDROID
        hInput = 2.0f * mJoystick.Horizontal;
        vInput = 2.0f * mJoystick.Vertical;
#endif

        speed = mWalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = mWalkSpeed * 2.0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jump = false;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            crouch = !crouch;
            Crouch();
        }


        // Determine if the player is moving
        isMoving = (hInput != 0 || vInput != 0);

        //Play footstep sounds when moving
        if (isMoving && Time.time >= nextStepTime)
        {
            PlayFootstepSound();
            nextStepTime = Time.time + stepRate;
        }
    }

    public void Move()
    {
        if (crouch) return;

        // We shall apply movement to the game object here.
        if (mAnimator == null) return;
        if (mFollowCameraForward)
        {
            // rotate Player towards the camera forward.
            Vector3 eu = Camera.main.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.Euler(0.0f, eu.y, 0.0f),
                mTurnRate * Time.deltaTime);
        }
        else
        {
            transform.Rotate(0.0f, hInput * mRotationSpeed * Time.deltaTime, 0.0f);
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward).normalized;
        forward.y = 0.0f;

        mCharacterController.Move(forward * vInput * speed * Time.deltaTime);
        mAnimator.SetFloat("PosX", 0);
        mAnimator.SetFloat("PosZ", vInput * speed / (2.0f * mWalkSpeed));

        if (jump)
        {
            Jump();
            jump = false;
        }
    }

    void Jump()
    {
        mAnimator.SetTrigger("Jump");
        mVelocity.y += Mathf.Sqrt(mJumpHeight * -2f * mGravity);
    }

    private Vector3 HalfHeight;
    private Vector3 tempHeight;
    void Crouch()
    {
        mAnimator.SetBool("Crouch", crouch);
        if (crouch)
        {
            tempHeight = CameraConstants.CameraPositionOffset;
            HalfHeight = tempHeight;
            HalfHeight.y *= 0.5f;
            CameraConstants.CameraPositionOffset = HalfHeight;
        }
        else
        {
            CameraConstants.CameraPositionOffset = tempHeight;
        }
    }

    void ApplyGravity()
    {
        // apply gravity.
        mVelocity.y += mGravity * Time.deltaTime;
        if (mCharacterController.isGrounded && mVelocity.y < 0)
            mVelocity.y = 0f;
    }
    // Plays a random footstep sound based on the current surface type
    private void PlayFootstepSound()
    {
        AudioClip clip = null; // Variable to hold the selected footstep sound

        // Check the current surface type and select a corresponding footstep sound
        if (floorType == "Concrete")
        {
            // Randomly pick a footstep sound for concrete surfaces
            clip = footstepSoundsConcrete[UnityEngine.Random.Range(0, footstepSoundsConcrete.Length)];
        }
        else if (floorType == "Dirt")
        {
            // Randomly pick a footstep sound for dirt surfaces
            clip = footstepSoundsDirt[UnityEngine.Random.Range(0, footstepSoundsDirt.Length)];
        }
        else if (floorType == "Wood")
        {
            // Randomly pick a footstep sound for wooden surfaces
            clip = footstepSoundsWood[UnityEngine.Random.Range(0, footstepSoundsWood.Length)];
        }
        else if (floorType == "Sand")
        {
            // Randomly pick a footstep sound for sandy surfaces
            clip = footstepSoundsSand[UnityEngine.Random.Range(0, footstepSoundsSand.Length)];
        }
        else if (floorType == "Metal")
        {
            // Randomly pick a footstep sound for metal surfaces
            clip = footstepSoundsMetal[UnityEngine.Random.Range(0, footstepSoundsMetal.Length)];
        }
        else
        {
            // Default to a concrete footstep sound if no matching surface is found
            clip = footstepSoundsConcrete[UnityEngine.Random.Range(0, footstepSoundsConcrete.Length)];
        }

        // If a sound clip was successfully selected, play it
        if (clip != null)
        {
            // Randomize the volume and pitch to add variation to the footstep sounds
            footstepAudioSource.volume = UnityEngine.Random.Range(0.5f, 1.4f);
            footstepAudioSource.pitch = UnityEngine.Random.Range(0.4f, 1.3f);
            footstepAudioSource.PlayOneShot(clip); // Play the selected sound clip
        }
    }

    
    private void DetectSurface()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down); // Create a ray starting slightly above the object's position and pointing downward.
        RaycastHit hit; // Variable to store information about the object hit by the ray.

        if (Physics.Raycast(ray, out hit, 1.0f))  // Check if the ray hits a collider within a distance of 1.0f.
        {
            if (hit.collider.CompareTag("Concrete"))  // If the hit collider's tag is "Concrete", set the floorType to "Concrete".
            {
                floorType = "Concrete";
            }
            else if (hit.collider.CompareTag("Sand"))  // If the hit collider's tag is "Sand", set the floorType to "Sand".
            {
                floorType = "Sand";
            }
            else if (hit.collider.CompareTag("Metal")) // If the hit collider's tag is "Metal", set the floorType to "Metal".
            {
                floorType = "Metal";
            }
            else if (hit.collider.CompareTag("Wood")) // If the hit collider's tag is "Wood", set the floorType to "Wood".
            {
                floorType = "Wood";
            }
            else if (hit.collider.CompareTag("Dirt")) // If the hit collider's tag is "Dirt", set the floorType to "Dirt".
            {
                floorType = "Dirt";
            }
            else
            {
                floorType = "Default";  // If the tag does not match any known type, set the floorType to "Default".
            }
        }
        else
        {
            floorType = "Default"; // If the ray does not hit anything, set the floorType to "Default".
        }
    }
}
