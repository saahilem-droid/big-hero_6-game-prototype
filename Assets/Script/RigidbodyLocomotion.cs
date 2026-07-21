using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class RigidbodyLocomotion : MonoBehaviour
{
    private PlayerRootMotion playerRootMotion;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 12f;

    [Header("Camera")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private Animator animator;

    private float horizontal;
    private float vertical;
    private bool isRunning;

    private Vector3 moveDir;

    void Awake()
    {
        
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerRootMotion = GetComponent<PlayerRootMotion>();


        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
{
    horizontal = Input.GetAxisRaw("Horizontal");
    vertical = Input.GetAxisRaw("Vertical");

    isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    float smoothH = Input.GetAxis("Horizontal");
    float smoothV = Input.GetAxis("Vertical");

    bool pureBackward = smoothV < -0.1f && Mathf.Abs(smoothH) < 0.1f;
    bool isMoving = Mathf.Abs(smoothH) > 0.1f || Mathf.Abs(smoothV) > 0.1f;

    float animForward = 0f;
    if (pureBackward)
        animForward = -0.5f;
    else if (isMoving)
        animForward = isRunning ? 1f : 0.5f;

    animator.SetFloat("xHorizontal", 0f, 0.1f, Time.deltaTime);
    animator.SetFloat("yVertical", animForward, 0.1f, Time.deltaTime);
    animator.SetBool("isRunning", isRunning);
}



    void FixedUpdate()
{
    if (playerRootMotion != null &&
       (playerRootMotion.IsSliding ||
        playerRootMotion.IsDodging ||
        playerRootMotion.IsAttacking ||
        playerRootMotion.IsBladeAttacking||
        playerRootMotion.IsJumping))
    {
        return;
    }

    CalculateDirection();
    Move();
    Rotate();
}



    // =============================
    // CAMERA RELATIVE DIRECTION
    // =============================
    void CalculateDirection()
    {
        if (!cameraTransform)
        {
            moveDir = new Vector3(horizontal, 0f, vertical);
            return;
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        moveDir =
            forward.normalized * vertical +
            right.normalized * horizontal;

        moveDir.Normalize();
    }

    // =============================
    // TRANSLATION
    // =============================
   void Move()
{
    float speed = isRunning ? runSpeed : walkSpeed;

    Vector3 velocity;

    // Pure backward input: move along player's own -forward, don't use camera-relative dir
    if (vertical < -0.1f && Mathf.Abs(horizontal) < 0.1f)
    {
        velocity = -transform.forward * speed;
    }
    else
    {
        velocity = moveDir * speed;
    }

    velocity.y = rb.linearVelocity.y;
    rb.linearVelocity = velocity;
}



    // =============================
    // ROTATION
    // =============================
    void Rotate()
{
    if (cameraTransform == null)
        return;

    // No input — don't rotate
    if (Mathf.Abs(horizontal) < 0.1f && Mathf.Abs(vertical) < 0.1f)
        return;

    // Pure S — don't rotate, player walks backward in place
    if (vertical < -0.1f && Mathf.Abs(horizontal) < 0.1f)
        return;

    if (moveDir.sqrMagnitude > 0.01f)
    {
        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 12f * Time.fixedDeltaTime));
    }
}



}
