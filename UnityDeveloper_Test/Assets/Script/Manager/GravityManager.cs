using UnityEngine;

public class GravityManager : MonoBehaviour
{
    [SerializeField] Transform CharacterPivot;
    [SerializeField] GameObject HoloCharacter;

    // A cool curve we can use to change the rotation speed based on how close to the new surface it is (0 - basically touching, 1 - super far away)
    [Space]
    [SerializeField] AnimationCurve GravCorrectionSpeed;
    private Vector3 GravityOrientationNormal, NewGravityOrientationNormal, playerForward, NewplayerForward;
    private float floorDistance;

    [Space]
    // This is the multiplying speed we use to rotate faster
    [SerializeField] float MaxCorrectionSpeed = 20f;
    // This value is the distance our speed calc's cap at, any further than this value from the surface and it's the same speed
    [SerializeField] float gravCorrectionDist = 10f;

    // So we know if the rotations need to be redone
    private bool floorChanged = false;
    private bool isGrounded = false;
    public bool IsGrounded
    {
        get
        {
            return isGrounded;
        }
        set
        {
            isGrounded = value;
        }
    }

    Camera Cam;

    void Start()
    {
        Cam = Camera.main;

        // Get a initial reference to the scene gravity settings
        GravityOrientationNormal = Physics.gravity.normalized;

        HoloCharacter.SetActive(false);
    }

    void ChangeGravity(Vector3 ChangeDirection)
    {
        // Change global gravity
        Physics.gravity = 9.81f * ChangeDirection;

        // Tell the component it's no longer grounded and needs to do rotations
        floorChanged = true;
    }

    void FixedUpdate()
    {
        floorChanged = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CorrectRotation(playerForward, CharacterPivot.transform.up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CorrectRotation(playerForward, -CharacterPivot.transform.up);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //ChangeGravity(-CharacterPivot.transform.right);
            CorrectRotation(playerForward, -CharacterPivot.transform.right);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //ChangeGravity(CharacterPivot.transform.right);
            CorrectRotation(playerForward, CharacterPivot.transform.right);
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            HoloCharacter.SetActive(false);

            playerForward = NewplayerForward;
            GravityOrientationNormal = NewGravityOrientationNormal;
            ChangeGravity(GravityOrientationNormal);
        }

        // If we were rotating but now we've hit the floor, stop rotating to ground and just set it
        if (floorChanged)
        {
            floorChanged = !isGrounded;
        }

        // If the gravity changed, do rotations to correct for orientation
        if (!floorChanged) floorDistance = 0;
    }

    public Quaternion LookRotation(Vector3 forward, Vector3 up)
    {
        // We want to rotate the obj to look toward the camera, and to rotate to face the floor at the same time
        Quaternion zUp = Quaternion.LookRotation(forward, up);
        return zUp;
    }

    public void CorrectRotation(Vector3 forward)
    {
        playerForward = forward;
        GravityOrientationNormal = Physics.gravity.normalized;

        // Get new floor and front rotation
        Quaternion desiredRot = LookRotation(forward, -GravityOrientationNormal);

        // Calculate the speed we want to rotate by how much time we have to do it
        float strength = (MaxCorrectionSpeed * GravCorrectionSpeed.Evaluate(Mathf.Clamp(floorDistance, 0, gravCorrectionDist) / gravCorrectionDist));

        // Apply the new Slerp'd rotation
        CharacterPivot.rotation = Quaternion.Slerp(CharacterPivot.rotation, desiredRot, strength);
    }

    public void CorrectRotation(Vector3 forward, Vector3 NewGravityOrientationNormal)
    {
        NewplayerForward = forward;
        this.NewGravityOrientationNormal = NewGravityOrientationNormal;

        // Get new floor and front rotation
        Quaternion desiredRot = LookRotation(forward, -NewGravityOrientationNormal);

        HoloCharacter.transform.localPosition = NewGravityOrientationNormal;
        HoloCharacter.transform.rotation = desiredRot;
        HoloCharacter.SetActive(true);
    }
}