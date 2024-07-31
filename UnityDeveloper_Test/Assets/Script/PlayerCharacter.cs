using UnityEngine;
using UnityEngine.Events;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(GravityManager))]
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] Transform CameraPivot;
    [SerializeField] Transform GroundCheckPos;

    [SerializeField] Animator PlayerAnimator;

    [SerializeField] float movespeed = 15f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float MouseSensitivity = 1f;

    [Space]
    [SerializeField] bool isGrounded;
    [SerializeField] float groundCheckDistance = 0.1f;
    [SerializeField] LayerMask groundLayer;

    public UnityEvent OnPlayerLeavePlayArea;

    public UnityEvent OnPlayerCollectedCube;

    GravityManager GravityManager;

    float xRot;

    Vector3 movedirection,rotDir;

    Rigidbody rb;

    bool GameOver = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        GravityManager = GetComponent<GravityManager>();

        InvokeRepeating("CheckGrounded", 0.1f, 0.1f);
    }

    private void Update()
    {
        if (GameOver) return;

        movedirection = new Vector3(Input.GetAxisRaw("Horizontal"),0, Input.GetAxisRaw("Vertical")).normalized;

        rotDir = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
    private void FixedUpdate()
    {
        if (GameOver) return;

        movedirection = transform.TransformDirection(movedirection);
        rb.MovePosition(rb.position + movespeed * Time.deltaTime * movedirection);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        xRot -= rotDir.y * MouseSensitivity;
        xRot = Mathf.Clamp(xRot, -89, 89);

        transform.Rotate(0,rotDir.x * MouseSensitivity,0);

        CameraPivot.localRotation = Quaternion.Euler(xRot,0,0);

        PlayerAnimator.SetBool("isRunning", movedirection.sqrMagnitude > 0);
        PlayerAnimator.SetBool("isFalling", !isGrounded);

        PlayerAnimator.transform.LookAt(PlayerAnimator.transform.position + movedirection,transform.up);
    }

    private void LateUpdate()
    {
        if (GameOver) return;

        GravityManager.CorrectRotation(transform.forward);
    }

    void CheckGrounded()
    {
        if (GameOver) return;

        // Cast a ray downwards from the center of the Rigidbody
        RaycastHit hit;

        isGrounded = 
        Physics.Raycast(GroundCheckPos.position, -GroundCheckPos.up, out hit, groundCheckDistance, groundLayer);

        GravityManager.IsGrounded = isGrounded;

        if (isGrounded)
        {
            Vector3 currentSurfaceRot = transform.rotation.eulerAngles;
            Vector3 targetSurfaceRotation = Quaternion.FromToRotation(transform.up, hit.normal).eulerAngles;
        }

        Debug.DrawRay(GroundCheckPos.position, -GroundCheckPos.up * groundCheckDistance, Color.red, 5f);
    }

    void Jump()
    {
        if (isGrounded)
        {
            isGrounded = false;
            GravityManager.IsGrounded = isGrounded;

            rb.AddForce(transform.up * jumpForce);
        }
    }

    void resetRotation()
    {
        /*if (isGrounded)
        {
            Vector3 currentRot = transform.rotation.eulerAngles;

            currentRot.x = currentRot.x <= -45 ? -90 : currentRot.x >= 45 ? 90 : 0;

            currentRot.z = currentRot.z <= -45 ? -90 : currentRot.z >= 45 ? 90 : 0;

            transform.eulerAngles = currentRot;
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PointCube"))
        {
            OnPlayerCollectedCube.Invoke();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Finish"))
        {
            PauseInputs();
            OnPlayerLeavePlayArea.Invoke();
        }
    }

    public void PauseInputs()
    {
        GameOver = true;
        GravityManager.PauseInputs();
    }
}
