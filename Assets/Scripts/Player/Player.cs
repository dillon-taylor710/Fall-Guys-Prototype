using UnityEngine;
using Mirror;
using System.Collections;
using Mirror.Examples.RigidbodyPhysics;
using System.Collections.Generic;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class Player : NetworkBehaviour
{
    // COMPONENTS
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private Animator anim;
    [HideInInspector] public CameraRotator playerCamera = null;

    [Header("Movement")]
    public bool startControllable = false;
    [SerializeField] private LayerMask walkableSurface;
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float maxSpeed = 10.0f;
    [SerializeField] private float counterMoveScale = 0.1f;
    [SerializeField] private float slopeThreshold = 0.2f;
    [SerializeField] private float gravForceMultiplier = 20.0f;
    [SerializeField] private float collisionForceThreshold = 50.0f;
    private float rotationAngle = 0.0f;

    private Vector3 inputDir = Vector3.zero;
    private bool isGrounded = false;
    private bool playerStable = true;
    private bool playerControl = true;
    private bool hasFallenOver = false;
    private bool smacked = false;

    [Header("Ability Values")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float jumpCheckDist = 0.25f;
    [SerializeField] private float diveWaitTime = 1.0f;
    [SerializeField] private Vector3 diveForce = Vector3.forward;
    [SerializeField] private float diveTorque = 90.0f;
    [SerializeField, Min(0.1f)] private float diveRecoveryMaxSpeed = 2.0f;
    [SerializeField] private float stableHitThreshold = 5.0f;

    [SerializeField] float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    Vector3 currentVelocity = Vector3.zero;

    [SyncVar]
    public bool isJumping = false;
    [SyncVar]
    public bool isRunning = false;
    [SyncVar]
    public bool isVictory = false;
    [SyncVar]
    public int skin_id = -1;
    private int prev_skin_id = -1;

    public Renderer character_renderer;
    public List<CharacterTexture> character_textures;

    public GameObject MineSymbol;

    public Vector3 last_point = Vector3.zero;

    #region Mirror Methods

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            //enabled = false;
            //rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
            MineSymbol.SetActive(true);
        }
    }

    void Awake()
    {
        if (isLocalPlayer)
        {
            skin_id = PlayerPrefs.GetInt("Char_SKIN_ID", 0);
            if (skin_id < character_textures.Count)
            {
                character_renderer.material.SetTexture("_MainTex", character_textures[skin_id].Main);
                //character_renderer.material.SetTexture("_MetallicGlossMap", character_textures[index].Metalic);
                //character_renderer.material.SetTexture("_BumpMap", character_textures[index].Normal);
            }
        }
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            skin_id = PlayerPrefs.GetInt("Char_SKIN_ID", 0);
            if (skin_id < character_textures.Count)
            {
                character_renderer.material.SetTexture("_MainTex", character_textures[skin_id].Main);
                //character_renderer.material.SetTexture("_MetallicGlossMap", character_textures[index].Metalic);
                //character_renderer.material.SetTexture("_BumpMap", character_textures[index].Normal);
            }
        }
        //FindObjectOfType<NetworkManagerHUD>().showGUI = false;

        //FindObjectOfType<NetworkManagerHUD>().useGUILayout = false;

        if (!startControllable)
            this.enabled = false;

        playerCamera = Camera.main.GetComponent<CameraRotator>();

        // Tell Server we're waiting to play
        if (GameManager.singleton)
            GameManager.singleton.CmdReadyPlayer(GetComponent<NetworkIdentity>());
        else
            playerCamera.cameraTarget = transform;
    }
    #endregion

    // UPDATE + FIXEDUPDATE aren't exactly 'Server Authorative'... but it's been hours, I can't be arsed and you shouldn't be able to read this
    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer)
        {
            if (skin_id != prev_skin_id)
            {
                prev_skin_id = skin_id;
                if (skin_id < character_textures.Count)
                {
                    character_renderer.material.SetTexture("_MainTex", character_textures[skin_id].Main);
                    //character_renderer.material.SetTexture("_MetallicGlossMap", character_textures[index].Metalic);
                    //character_renderer.material.SetTexture("_BumpMap", character_textures[index].Normal);
                }
                character_renderer.enabled = true;
                rb.useGravity = true;
            }
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;

        inputDir = direction;// Vector3.SmoothDamp(inputDir, direction, ref currentVelocity, turnSmoothTime);Debug.LogError(inputDir);

        // If can move...
        if (playerControl)
        {
            // DIVE
            /*if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                playerControl = false;
                playerStable = false;
                anim.SetBool("Stable", playerStable);
                anim.SetTrigger("Dive");

                rb.velocity = Vector3.zero;
                rb.AddForce(transform.rotation * diveForce, ForceMode.Impulse);
                rb.AddRelativeTorque(new Vector3(diveTorque, 0,0), ForceMode.Impulse);
            }*/

            // JUMP
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                anim.SetTrigger("Jump");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isJumping = true;
                anim.SetBool("isJumping", isJumping);
            }
        }

        anim.SetFloat("Movement", inputDir.magnitude);
        anim.SetBool("Grounded", isGrounded);
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isVictory", isVictory);

        if (!isLocalPlayer)
            return;

        // Gravity Multiplier - increased vertical accuracy
        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        isGrounded = GroundCheck();

        if (playerControl)
        {
            Move();
        }

        // Constrain rotation when stable
        if (playerStable)
            rb.MoveRotation(Quaternion.Euler(0, rotationAngle, 0));
    }

    public void SetVictory(bool vic)
    {
        isVictory = vic;
        anim.SetBool("isVictory", isVictory);
    }

    /// <summary>
    /// Handles Movement
    /// </summary>
    private void Move()
    {
        //Debug.DrawRay(transform.position, transform.forward * 10, Color.blue);
        if (inputDir.magnitude > 0.1f)
        {
            // Rotation
            rotationAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + playerCamera.transform.eulerAngles.y;
            rotationAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref turnSmoothVelocity, turnSmoothTime);
            inputDir = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;

            // Movement
            if (isGrounded && !hasFallenOver)
            {
                if (rb.velocity.magnitude < maxSpeed)
                    rb.AddForce(inputDir * speed, ForceMode.Force);// rb.AddForce(transform.forward * inputDir.magnitude * speed);
            }
            isRunning = true;
            anim.SetBool("isRunning", isRunning);

            if (isGrounded)
            {
                isJumping = false;
                anim.SetBool("isJumping", isJumping);
            }
        }
        else if (isGrounded)
        {
            // Counter Movement & Sliding Prevention (not particularly great method)
            rb.AddForce(speed * Vector3.forward * Time.deltaTime * -rb.velocity.z * counterMoveScale);
            rb.AddForce(speed * Vector3.right * Time.deltaTime * -rb.velocity.x * counterMoveScale);

            isJumping = false;
            isRunning = false;

            anim.SetBool("isRunning", isRunning);
            anim.SetBool("isJumping", isJumping);
        }
    }

    /// <summary>
    /// Returns whether player is stood on Ground/Walkable Surface
    /// </summary>
    private bool GroundCheck()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, jumpCheckDist, walkableSurface);

        if (hit.transform)
            return true;
        else
            return false;
    }

    [ClientCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!smacked)
        {
            float collisionForce = collision.impulse.magnitude;
            if (collisionForce > collisionForceThreshold)
            {
                playerStable = false;
                playerControl = false;
                rb.AddForce(collision.impulse * 0.25f, ForceMode.Impulse);
            }
        }

    }

    IEnumerator SetParent(Transform parent)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z));

        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();

        this.transform.parent = parent;
    }

    [ClientCallback]
    private void OnCollisionStay(Collision collision)
    {
        if (!hasFallenOver && !collision.transform.CompareTag("Player"))
        {
            HandleHasFallenOver();
        }
    }

    /// <summary>
    /// Handles fallen over check and resulting logic.
    /// </summary>
    private void HandleHasFallenOver()
    {
        // Fallen over Check
        if (Vector3.Dot(Vector3.down, transform.up) > -0.2f)
        {
            hasFallenOver = true;
            StartCoroutine(StandUp());
        }
    }

    /// <summary>
    /// Instantly returns Player back to standing after lying delay.
    /// </summary>
    private IEnumerator StandUp()
    {
        // Delay...
        yield return new WaitForSeconds(diveWaitTime);

        // Wait until speed while fallen is slowed
        yield return new WaitUntil(() => rb.velocity.magnitude < diveRecoveryMaxSpeed);

        // Reset pos + rot
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = transform.position + Vector3.up * 0.6f;
        rb.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z));
        
        hasFallenOver = false;
        playerControl = true;
        playerStable = true;
        smacked = false;
        anim.SetBool("Stable", playerStable);
        isRunning = false;
        isJumping = false;
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isJumping", isJumping);
    }

    private void OnEnable()
    {
        if(playerCamera)
            playerCamera.cameraTarget = transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, Vector3.down * jumpCheckDist);
    }
}