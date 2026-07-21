using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRootMotion : MonoBehaviour
{
    [Header("Slide / Dodge Movement")]
public float slideSpeed = 8f;

public float dodgeSpeed = 10f;
public bool IsCrouching => isCrouching;


private float slideTimer;
private float dodgeTimer;

private Vector3 storedPosition;


public float slideDuration = 0.6f;
public float dodgeDuration = 0.4f;


[Header("Slide Settings")]
public float slideCooldown = 1.2f;
[Header("Crouch Settings")]
public bool isCrouching;
private bool isjumping;
private bool isSliding;
private bool isRunning;
private float nextSlideTime;
public bool IsSliding => isSliding;
public bool IsDodging => isDodging;
public bool IsAttacking => isAttacking;
public bool IsBladeAttacking => isBladeAttacking;

public bool IsJumping => isJumping;


    [Header("Blade Attack Settings")]
public float bladeAttackCooldown = 1f;
private bool isBladeAttacking;


private float nextBladeAttackTime;

    [Header("Q Attack Settings")]
public float attackQCooldown = 1f;
private bool isAttackingQ;
private float nextAttackQTime;

    [Header("Dodge Settings")]
    public float dodgeCooldown = 1f;

    private bool isDodging;
    private float nextDodgeTime;
    private float dodgeX;
    private float dodgeY;

    [Header("Attack Settings")]
    public float attackCooldown = 0.8f;

    private bool isAttacking;
    private float nextAttackTime;

    public Animator animator;
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    private bool isJumping;

    private Rigidbody rb;
    private float horizontal;
    private float vertical;
    private bool isGrounded;

    

    void Awake()
{
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    rb = GetComponent<Rigidbody>();
    rb.freezeRotation = true;
    rb.interpolation = RigidbodyInterpolation.Interpolate;
    rb.linearDamping = 0f;  // Add this line to ensure drag is always 0

    Debug.Log("PlayerRootMotion initialized");
}


    void Update()
    {
        // Get input first
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        
        // Check ground state early - CRITICAL for preventing airborne animations
        CheckGround();
        
        // Cancel crouch if player becomes airborne
        if (isCrouching && !isGrounded)
        {
            StandUp();
        }
        
        // Cancel other ground-based actions if player becomes airborne
        if (!isGrounded)
        {
            if (isAttacking && !isDodging)
            {
                EndAttack();
            }
            if (isBladeAttacking)
            {
                EndBladeAttack();
            }
            if (isSliding)
            {
                EndSlide();
            }
        }

        

        // 🧎‍♂️ SLIDE INPUT - Only allow when grounded
        if (Input.GetKeyDown(KeyCode.C) && isGrounded)
        {
            if (isRunning && !isSliding && Time.time >= nextSlideTime)
            {
                Slide();
            }
            else if (!isRunning && !isSliding)
            {
                ToggleCrouch();
            }
        }

        // Blade Attack input - Only allow when grounded
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextBladeAttackTime && isGrounded && !isDodging)
        {
            BladeAttack();
        }

       

        // --- JUMP INPUT ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isDodging)
            Jump();

        // --- ATTACK INPUT ---
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime && isGrounded && !isDodging)
            Attack();

        // --- DODGE INPUT --- (Priority input - can interrupt other animations)
        if (Input.GetKeyDown(KeyCode.LeftAlt) && Time.time >= nextDodgeTime && isGrounded)
        {
            // Cancel any ongoing actions when dodging
            if (isCrouching)
                StandUp();
            if (isAttacking)
                EndAttack();
            if (isBladeAttacking)
                EndBladeAttack();
            if (isSliding)
                EndSlide();
            
            float ver = Input.GetAxisRaw("Vertical");
            float hor = Input.GetAxisRaw("Horizontal");
            
            float dodgeY = 0f;
            float dodgeX = 0f;
            
            if (ver > 0)
                dodgeY = 0.5f;
            else if (ver < 0)
                dodgeY = -0.5f;
            
            if (hor > 0)
                dodgeX = 0.5f;
            else if (hor < 0)
                dodgeX = -0.5f;
            
            Dodge(dodgeX, dodgeY);
        }

        // --- CROUCH BLEND TREE INPUT ---
        // Only set crouch parameters when grounded and actually crouching
       if (isCrouching && isGrounded)
{
    // Build camera-relative world movement direction
    Camera mainCam = Camera.main;
    Vector3 worldMoveDir = Vector3.zero;
    if (mainCam != null)
    {
        Vector3 camFwd = mainCam.transform.forward; camFwd.y = 0f; camFwd.Normalize();
        Vector3 camRight = mainCam.transform.right;  camRight.y = 0f; camRight.Normalize();
        worldMoveDir = camFwd * vertical + camRight * horizontal;
    }

    // Project onto player's local axes so blend tree matches actual facing
    float localForward = Vector3.Dot(worldMoveDir, transform.forward);
    float localRight   = Vector3.Dot(worldMoveDir, transform.right);

    animator.SetFloat("Crouch_Horizontal", localRight   * 0.5f, 0.15f, Time.deltaTime);
    animator.SetFloat("Crouch_Vertical",   localForward * 0.5f, 0.15f, Time.deltaTime);
}
else
{
    animator.SetFloat("Crouch_Horizontal", 0f);
    animator.SetFloat("Crouch_Vertical",   0f);
}

    }
void FixedUpdate()
{
    ApplyActionMovement();

    if (isGrounded && rb.linearVelocity.y <= 0f)
    {
        rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration);
    }
}



void OnAnimatorMove()
{
    if (!isSliding &&
        !isDodging &&
        !isAttacking &&
        !isBladeAttacking &&
        !isJumping)
        return;

    Vector3 delta = animator.deltaPosition;

    // Preserve Y velocity for gravity
    delta.y = rb.linearVelocity.y * Time.deltaTime;

    // Apply the root motion movement
    rb.MovePosition(rb.position + delta);
}







void ApplyActionMovement()
{
    if (isSliding)
    {
        rb.linearVelocity = new Vector3(
    transform.forward.x * slideSpeed,
    rb.linearVelocity.y,
    transform.forward.z * slideSpeed);

    }
    else if (isDodging)
    {
       rb.linearVelocity = new Vector3(
    transform.forward.x * dodgeSpeed,
    rb.linearVelocity.y,
    transform.forward.z * dodgeSpeed);

    }
}










    void RotatePlayer()
{
    // Only rotate when moving
    if (Mathf.Abs(horizontal) < 0.1f && Mathf.Abs(vertical) < 0.1f)
        return;

    // Don't rotate during these actions
    if (isBladeAttacking || isDodging)
        return;

    // Don't rotate on pure backward input
    if (vertical < -0.1f && Mathf.Abs(horizontal) < 0.1f)
        return;

    Camera mainCam = Camera.main;
    if (mainCam == null) return;

    Vector3 cameraForward = mainCam.transform.forward;
    cameraForward.y = 0;
    cameraForward.Normalize();

    Vector3 cameraRight = mainCam.transform.right;
    cameraRight.y = 0;
    cameraRight.Normalize();

    // Only use forward/strafe components for rotation target
    Vector3 moveDirection = (cameraForward * Mathf.Max(vertical, 0f) + cameraRight * horizontal).normalized;

    if (moveDirection.magnitude > 0.1f)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}



    // --- JUMP ---
    void Jump()
{
    if (isCrouching)
        StandUp();
    Debug.Log("JUMP");

    isSliding = false;
    isJumping = true;  // Add this line

    rb.linearVelocity = new Vector3(
        rb.linearVelocity.x,
        0f,
        rb.linearVelocity.z);

    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

    animator.ResetTrigger("Jump");
    animator.SetTrigger("Jump");

    isGrounded = false;
}

public void EndJump()
{
    isJumping = false;
}


void ToggleCrouch()
{
    if (isCrouching)
        StandUp();
    else
        StartCrouch();

        Debug.Log($"Toggle crouch. Now: {isCrouching}");
}

void StartCrouch()
{
    // Only allow crouch when grounded
    if (!isGrounded)
    {
        Debug.Log("Cannot crouch while airborne!");
        return;
    }
    
    Debug.Log("CROUCH");

    isCrouching = true;
    animator.SetBool("isCrouching", true);
}

void StandUp()
{
    Debug.Log("STAND");

    isCrouching = false;
    animator.SetBool("isCrouching", false);
}

void CancelSlide()
{
    isSliding = false;

    // clear slide trigger so it can be used again later
    animator.ResetTrigger("Slide");
}

    

    // --- ATTACK ---
    void Attack()
    {
        // Only allow attack when grounded
        if (!isGrounded)
        {
            Debug.Log("Cannot attack while airborne!");
            return;
        }
        
        Debug.Log("ATTACK");
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        animator.SetTrigger("Attack");
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    void BladeAttack()
{
    // Only allow blade attack when grounded
    if (!isGrounded)
    {
        Debug.Log("Cannot blade attack while airborne!");
        return;
    }
    
    Debug.Log("BladeAttack!");
    isBladeAttacking = true;
    nextBladeAttackTime = Time.time + bladeAttackCooldown;

    // Trigger the Animator
    animator.SetTrigger("BladeAttack");
}

public void EndBladeAttack()
{
    isBladeAttacking = false;
}

void Slide()
{
    if (isCrouching)
        StandUp();
    
    Debug.Log("SLIDE");

    isSliding = true;
    nextSlideTime = Time.time + slideCooldown;

    // CRITICAL: Reset trigger first to ensure it fires again
    animator.ResetTrigger("Slide");
    animator.SetTrigger("Slide");
}


public void EndSlide()
{
    isSliding = false;
    
    // CRITICAL: Reset trigger when slide ends so it can be used again
    animator.ResetTrigger("Slide");
}


    // --- DODGE ---
   void Dodge(float dodgeX, float dodgeY)
{
    if (!isGrounded)
    {
        Debug.Log("Cannot dodge while airborne!");
        return;
    }
    
    if (Mathf.Abs(dodgeX) < 0.1f && Mathf.Abs(dodgeY) < 0.1f)
    {
        dodgeY = -0.5f;
    }
    else
    {
        dodgeX = Mathf.Sign(dodgeX) * 0.5f;
        dodgeY = Mathf.Sign(dodgeY) * 0.5f;
    }

    Debug.Log($"DODGE! X: {dodgeX}, Y: {dodgeY}");
    
    storedPosition = transform.position;
    
    isDodging = true;
    nextDodgeTime = Time.time + dodgeCooldown;

    animator.SetFloat("DHorizontal", dodgeX);
    animator.SetFloat("DVertical", dodgeY);
    
    animator.ResetTrigger("Dodge");
    animator.SetTrigger("Dodge");
}






   public void EndDodge()
{
    isDodging = false;
    animator.SetFloat("DHorizontal", 0f);
    animator.SetFloat("DVertical", 0f);
    animator.ResetTrigger("Dodge");
    
    Vector3 currentVel = rb.linearVelocity;
    currentVel.x *= 0.3f;
    currentVel.z *= 0.3f;
    rb.linearVelocity = currentVel;
}



    // --- GROUND CHECK ---
    void CheckGround()
{
    Vector3 rayOrigin = transform.position;
    float checkDistance = groundCheckDistance + 0.5f;
    
    bool rayHit = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit raycastHit, checkDistance, groundLayer);
    
    bool sphereHit = Physics.SphereCast(rayOrigin, 0.1f, Vector3.down, out RaycastHit spherecastHit, checkDistance, groundLayer);
    
    isGrounded = rayHit || sphereHit;
    
    Debug.DrawRay(rayOrigin, Vector3.down * checkDistance, isGrounded ? Color.green : Color.red);
    Debug.Log($"Ground Check - RayHit: {rayHit}, SphereHit: {sphereHit}, Distance: {checkDistance}, Layer: {groundLayer.value}");
    
    if (rayHit)
        Debug.Log($"Raycast hit: {raycastHit.collider.name} at distance: {raycastHit.distance}");
    if (sphereHit)
        Debug.Log($"SphereCast hit: {spherecastHit.collider.name} at distance: {spherecastHit.distance}");
}




    // --- DEBUG UI ---
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Grounded: {isGrounded}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Horizontal: {horizontal}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Vertical: {vertical}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Dodge X: {dodgeX}");
        GUI.Label(new Rect(10, 90, 300, 20), $"Dodge Y: {dodgeY}");
    }
}
