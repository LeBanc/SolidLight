using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// RunnerController class defines the character/runner behaviours
/// </summary>
public class RunnerController : MonoBehaviour
{
    #region Parameters
    public float maxSpeed = 4f;
    public JumpTrigger jumpRightTrigger;
    public JumpTrigger jumpLeftTrigger;
    public GroundTrigger groundTrigger;
    public WallDetectionTrigger wallDetectRightTrigger;
    public WallDetectionTrigger wallDetectLeftTrigger;
    public WallJumpDetectionTrigger wallJumpDetectRightTrigger;
    public WallJumpDetectionTrigger wallJumpDetectLeftTrigger;

    private Rigidbody2D runnerRigBody;
    private Collider2D col2D;
    private Animator runnerAnimator;
    private SpriteRenderer runnerSpriteRenderer;
    private Vector2 direction = Vector2.right;


    // Physics acceleration/deceleration
    private float runAccel = 5f;
    private float minVelocity = 0.5f;

    // Parameters for physics hyperbole on several movements
    private float jumpOverBlockXh = 0.5f;
    private float jumpOverBlockH = 1.5f;

    private float jumpOverGapXh = 1f;
    private float jumpOverGapH = 1.2f;

    private float jumpFromWallXh = 0.4f;
    private float jumpFromWallH = 1f;

    private float fallXh = 0.4f;
    private float fallH = 1f;

    // Booleans for register jump while landing
    private bool jumpGapAfterLanding = false;
    private bool jumpBlockAfterLanding = false;

    private Vector3 spawnPoint;
    private Vector2 spawnDirection;
    private bool spawnIsSet;

    #endregion

    #region StateMachine states
    // States of character state machine
    private enum CharacterState
    {
        Idle,
        StartRunning,
        Running,
        JumpOverGap,
        JumpOverBlock,
        FallFromGround,
        Jumping,
        HangOnWall,
        FallFromWall,
        Falling,
        Land,
        Die
    }
    private CharacterState state = CharacterState.Idle;
    private CharacterState nextState = CharacterState.Idle;
    #endregion

    /// <summary>
    /// At Start, fetch the components, subscribe to the initial events and init the statemachine
    /// </summary>
    void Start()
    {
        // Fetching the components
        runnerRigBody = GetComponent<Rigidbody2D>();
        col2D = GetComponent<Collider2D>();
        runnerAnimator = GetComponent<Animator>();
        runnerSpriteRenderer = GetComponent<SpriteRenderer>();

        // Init the events subscription
        jumpRightTrigger.OnJump += JumpOverGap;
        groundTrigger.OnGrounded += Ground;
        groundTrigger.OnAir += Fall;
        wallDetectRightTrigger.OnJump += JumpOverBlock;
        wallJumpDetectRightTrigger.OnHang += WallJump;
        wallJumpDetectLeftTrigger.OnFall += Fall;

        // Init at null gravity for spawning animation
        Physics2D.gravity = Vector2.zero;

        // Init spawn position at current transform poistion if not already initialized
        if (!spawnIsSet)
        {
            spawnPoint = transform.position;
            spawnDirection = Vector2.right;
            spawnIsSet = true;
        }

        // For Debug
        DebugText.ChangeDebugText(state.ToString());
    }

    /// <summary>
    /// On Destroy, unsubscribe to all events
    /// </summary>
    private void OnDestroy()
    {
        jumpRightTrigger.OnJump -= JumpOverGap;
        wallDetectRightTrigger.OnJump -= JumpOverBlock;
        wallJumpDetectRightTrigger.OnHang -= WallJump;
        wallJumpDetectRightTrigger.OnFall -= Fall;
        jumpLeftTrigger.OnJump -= JumpOverGap;
        wallDetectLeftTrigger.OnJump -= JumpOverBlock;
        wallJumpDetectLeftTrigger.OnHang -= WallJump;
        wallJumpDetectLeftTrigger.OnFall -= Fall;
        groundTrigger.OnGrounded -= Ground;
        groundTrigger.OnAir -= Fall;
    }

    #region StateMachine - Update methods
    /// <summary>
    /// On Update, execute the state machine and change state if needed
    /// </summary>
    private void Update()
    {
        if (nextState != state)
        {
            switch (state)
            {
                case CharacterState.Idle:
                    if (nextState == CharacterState.StartRunning)
                    {
                        state = CharacterState.StartRunning;
                        SetAnimationTrigger("Start");
                    }
                    break;

                case CharacterState.StartRunning:
                    if (nextState == CharacterState.Running) state = CharacterState.Running;
                    break;

                case CharacterState.Running:
                    if (nextState == CharacterState.JumpOverGap)
                    {
                        state = CharacterState.JumpOverGap;
                        SetAnimationTrigger("Jump");
                    }
                    if (nextState == CharacterState.JumpOverBlock)
                    {
                        state = CharacterState.JumpOverBlock;
                        SetAnimationTrigger("Jump");
                    }
                    if (nextState == CharacterState.FallFromGround)
                    {
                        state = CharacterState.FallFromGround;
                        // Null gravity for Coyote Time
                        Physics2D.gravity = new Vector2(0f, 0f);
                        SetAnimationTrigger("Fall");
                    }
                    if (nextState == CharacterState.Die)
                    {
                        state = CharacterState.Die;
                        SetAnimationTrigger("Die");
                    }
                    break;

                case CharacterState.JumpOverGap:
                    if (nextState == CharacterState.Jumping)
                    {
                        state = CharacterState.Jumping;
                        StopAllCoroutines(); // To stop the deceleration in the jump coroutine
                        // Set the JumpOverGap velocity & gravity and reset jumpGapAfterLanding
                        runnerRigBody.velocity = new Vector2(direction.x * maxSpeed / 2, 2 * jumpOverGapH * (maxSpeed / 2) / jumpOverGapXh);
                        Physics2D.gravity = new Vector2(0f, -2 * jumpOverGapH * (maxSpeed / 2) * (maxSpeed / 2) / (jumpOverGapXh * jumpOverGapXh));
                        jumpGapAfterLanding = false;
                    }
                    break;

                case CharacterState.JumpOverBlock:
                    if (nextState == CharacterState.Jumping)
                    {
                        state = CharacterState.Jumping;
                        StopAllCoroutines(); // To stop the deceleration in the jump coroutine
                        // Set the JumpOverBlock velocity & gravity and reset jumpBlockAfterLanding
                        runnerRigBody.velocity = new Vector2(direction.x * maxSpeed / 2, 2 * jumpOverBlockH * (maxSpeed / 2) / jumpOverBlockXh);
                        Physics2D.gravity = new Vector2(0f, -2 * jumpOverBlockH * (maxSpeed / 2) * (maxSpeed / 2) / (jumpOverBlockXh * jumpOverBlockXh));
                        jumpBlockAfterLanding = false;
                    }
                    break;

                case CharacterState.Jumping:
                    if (nextState == CharacterState.Falling)
                    {
                        state = CharacterState.Falling;
                        Physics2D.gravity = new Vector2(0f, -2 * fallH * (maxSpeed / 2) * (maxSpeed / 2) / (fallXh * fallXh));
                        SetAnimationTrigger("Fall");
                    }
                    if (nextState == CharacterState.HangOnWall)
                    {
                        state = CharacterState.HangOnWall;
                        // Change the character direction (x axis) and stick it to the wall while the animation is done (JumpToWall or FallToWall)
                        ChangeDirection();
                        runnerRigBody.velocity = -1f * direction;
                        Physics2D.gravity = Vector2.zero;

                        SetAnimationTrigger("Hang");
                    }
                    if (nextState == CharacterState.Die)
                    {
                        state = CharacterState.Die;
                        SetAnimationTrigger("Die");
                    }
                    break;

                case CharacterState.Falling:
                    if (nextState == CharacterState.Land)
                    {
                        state = CharacterState.Land;
                        // Set the character velocity and the gravity
                        runnerRigBody.velocity = new Vector2(maxSpeed / 10 * direction.x, 0f);
                        Physics2D.gravity = new Vector2(0f, -9.81f);
                        SetAnimationTrigger("Land");
                    }
                    if (nextState == CharacterState.HangOnWall)
                    {
                        state = CharacterState.HangOnWall;
                        // Change the character direction (x axis) and stick it to the wall while the animation is done (JumpToWall or FallToWall)
                        ChangeDirection();
                        runnerRigBody.velocity = -1f * direction;
                        Physics2D.gravity = Vector2.zero;
                        SetAnimationTrigger("Hang");
                    }
                    if (nextState == CharacterState.Die)
                    {
                        state = CharacterState.Die;
                        SetAnimationTrigger("Die");
                    }
                    break;

                case CharacterState.HangOnWall:
                    if (nextState == CharacterState.Jumping)
                    {
                        state = CharacterState.Jumping;
                        // Set the velocity and the gravity to the JumpFromWall values and set the next state to Jumping
                        runnerRigBody.velocity = new Vector2(direction.x * maxSpeed / 2, 2 * jumpFromWallH * (maxSpeed / 2) / jumpFromWallXh);
                        Physics2D.gravity = new Vector2(0f, -2 * jumpFromWallH * (maxSpeed / 2) * (maxSpeed / 2) / (jumpFromWallXh * jumpFromWallXh));
                        SetAnimationTrigger("Jump");
                    }
                    if (nextState == CharacterState.FallFromWall)
                    {
                        state = CharacterState.FallFromWall;
                        runnerRigBody.velocity = Vector2.zero;
                        SetAnimationTrigger("Fall");
                    }
                    break;

                case CharacterState.FallFromGround:
                    if (nextState == CharacterState.Falling)
                    {
                        state = CharacterState.Falling;
                        // Set default gravity (earth) and a a small horizontal velocity
                        Physics2D.gravity = new Vector2(0f, -9.81f);
                        runnerRigBody.velocity = minVelocity * direction;
                    }
                    break;

                case CharacterState.FallFromWall:
                    if (nextState == CharacterState.Falling)
                    {
                        state = CharacterState.Falling;
                        // Set default gravity (earth) and a a small horizontal velocity
                        Physics2D.gravity = new Vector2(0f, -9.81f);
                        runnerRigBody.velocity = minVelocity * direction;
                    }
                    break;

                case CharacterState.Land:
                    if (nextState == CharacterState.Running) state = CharacterState.Running;
                    if (nextState == CharacterState.JumpOverBlock)
                    {
                        state = CharacterState.JumpOverBlock;
                        SetAnimationTrigger("Jump");
                    }
                    if (nextState == CharacterState.JumpOverGap)
                    {
                        state = CharacterState.JumpOverGap;
                        SetAnimationTrigger("Jump");
                    }
                    if (nextState == CharacterState.FallFromGround)
                    {
                        state = CharacterState.FallFromGround;
                        // Null gravity for Coyote Time
                        Physics2D.gravity = new Vector2(0f, 0f);
                        SetAnimationTrigger("Fall");
                    }
                    break;

                case CharacterState.Die:
                    if (nextState == CharacterState.Idle)
                    {
                        state = CharacterState.Idle;
                        SetAnimationTrigger("Reset");
                    }
                    break;

                default:
                    state = nextState;
                    break;
            }
            nextState = state;
            DebugText.ChangeDebugText(state.ToString());
            Debug.Log(state.ToString());
        }
    }

    /// <summary>
    /// On FixedUpdate, defines character rigidbody velocity and physics gravity
    /// </summary>
    void FixedUpdate()
    {
        // Addforce to character rigidbody
        switch (state)
        {
            case CharacterState.Running:
                // In running, accelerates
                runnerRigBody.velocity = new Vector2(runnerRigBody.velocity.x + Time.fixedDeltaTime * runAccel * direction.x, 0f);
                break;
            case CharacterState.Jumping:
                // In Jumping, if the vertical velocity is negative, change the gravity and the next state to Falling
                if (runnerRigBody.velocity.y < 0)
                {
                    nextState = CharacterState.Falling;
                }
                break;
            default: // In all other states, keep the current settings
                break;
        }

        // Limit character speed
        if (Mathf.Abs(runnerRigBody.velocity.x) > maxSpeed)
        {
            if (runnerRigBody.velocity.x > 0f)
            {
                runnerRigBody.velocity = new Vector2(maxSpeed, runnerRigBody.velocity.y);
            }
            else
            {
                runnerRigBody.velocity = new Vector2(-maxSpeed, runnerRigBody.velocity.y);
            }
        }
    }
    #endregion

    #region Spawn, Idle, StartRunning & Die states
    /// <summary>
    /// Running method is called by Animation event to go to Running state at the end of StartRunning animation
    /// </summary>
    private void Running()
    {
        nextState = CharacterState.Running;
    }

    /// <summary>
    /// Die method is called by Border collider when the character hits a border
    /// </summary>
    public void Die()
    {
        nextState = CharacterState.Die;
        runnerRigBody.velocity = Vector2.zero;
        Physics2D.gravity = Vector2.zero;
    }

    /// <summary>
    /// Spawn method is called by Animation event to go to Idle state at the end of Die animation
    /// </summary>
    private void Spawn()
    {
        GoToSpawnPoint();
        nextState = CharacterState.Idle;
        runnerRigBody.velocity = Vector2.zero;
        Physics2D.gravity = Vector2.zero;
    }

    /// <summary>
    /// SpawnToIdle is called by animation event to change gravity at the end of Spawn animation
    /// </summary>
    private void SpawnToIdle()
    {
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
    #endregion

    #region Jumps
    /// <summary>
    /// JumpOverGap method is called by the jumpTriggers OnJump event
    /// </summary>
    void JumpOverGap()
    {
        // If in Running state or the jump is register during landing, start the coroutine
        if (state.Equals(CharacterState.Running) || jumpGapAfterLanding)
        {
            StartCoroutine(JumpOverGapCoroutine());
        }
        // If in Landing state, register the jump for after the landing
        else if (state.Equals(CharacterState.Land)) jumpGapAfterLanding = true;
    }

    /// <summary>
    /// JumpOverGapCoroutine coroutine is called when the character has to jump over a gap
    /// </summary>
    /// <returns></returns>
    IEnumerator JumpOverGapCoroutine()
    {
        // Ground trigger and jumpTriggers are on the same horizontal line and that can cause an hazard in the jump detection
        // So wait for 1 frame at the beginning 
        yield return null;

        // And test again if in Running state or the jump is register during landing, change the next step to JumpOverGap and continue
        if (state.Equals(CharacterState.Running) || jumpGapAfterLanding)
        {
            nextState = CharacterState.JumpOverGap;
        }
        else // break (quit coroutine)
        {
            yield break;
        }

        // For 0.2 seconds, if the character horizontal velocity is gt maxSpeed/4, decelerates
        float _startTime = Time.time;
        while(Time.time - _startTime < 0.2f)
        {
            if(Mathf.Abs(runnerRigBody.velocity.x) > maxSpeed / 4)
            {
                runnerRigBody.velocity = new Vector2(runnerRigBody.velocity.x - 5 * Time.deltaTime * runAccel * direction.x, 0f);
            }            
            yield return null;
        }        

        // Set the next step to Jumping
        nextState = CharacterState.Jumping;
    }

    /// <summary>
    /// JumpOverGap method is called by the wallDetectTriggers OnJump event
    /// </summary>
    void JumpOverBlock()
    {
        // If in Running state or the jump is register during landing, set the nexState to JumpOverBlock and start the coroutine
        if (state.Equals(CharacterState.Running) || jumpBlockAfterLanding)
        {
            nextState = CharacterState.JumpOverBlock;
            StartCoroutine(JumpOverBlockCoroutine());
        }
        // If in Landing state, register the jump for after the landing
        else if (state.Equals(CharacterState.Land)) jumpBlockAfterLanding = true;
    }

    /// <summary>
    /// JumpOverBlockCoroutine coroutine is called when the character has to jump to a higher block
    /// </summary>
    /// <returns></returns>
    IEnumerator JumpOverBlockCoroutine()
    {
        // For 0.2 seconds, if the character horizontal velocity is gt maxSpeed/4, decelerates
        float _startTime = Time.time;
        while (Time.time - _startTime < 0.2f)
        {
            if(Mathf.Abs(runnerRigBody.velocity.x) > maxSpeed / 4)
            {
                runnerRigBody.velocity = new Vector2(runnerRigBody.velocity.x - 5 * Time.deltaTime * runAccel * direction.x, 0f);
            }
            yield return null;
        }

        // Set the next step to Jumping
        nextState = CharacterState.Jumping;
    }
    #endregion

    #region Wall
    /// <summary>
    /// WallJump method is called by the wallJumpDetectTriggers OnHang event
    /// </summary>
    void WallJump()
    {
        // If in Jumping or Falling state, chenge to HangOnWall
        if (state.Equals(CharacterState.Jumping) || state.Equals(CharacterState.Falling))
        {
            // Set the next state to HangOnWall
            nextState = CharacterState.HangOnWall;
        }
    }

    /// <summary>
    /// StickToWall method is called by animation event at the end of the transition from movement (jump/fall) to HangOnWall state
    /// </summary>
    private void StickToWall()
    {
        runnerRigBody.velocity = Vector2.zero;
    }

    /// <summary>
    /// JumpFromWall method is called by animation event to make the transition from HangOnWall to Jumping states
    /// </summary>
    private void JumpFromWall()
    {
        nextState = CharacterState.Jumping;
    }

    /// <summary>
    /// ChangeDirection method changes the character horizontal direction (x axis * -1), flips the SpriteRenderer on X axis and set the triggers for event listening
    /// </summary>
    void ChangeDirection()
    {
        direction = new Vector2(-direction.x, 0f);
        runnerSpriteRenderer.flipX = !runnerSpriteRenderer.flipX;
        if (direction.x < 0)
        {
            jumpRightTrigger.OnJump -= JumpOverGap;
            wallDetectRightTrigger.OnJump -= JumpOverBlock;
            wallJumpDetectRightTrigger.OnHang -= WallJump;
            wallJumpDetectLeftTrigger.OnFall -= Fall;

            jumpLeftTrigger.OnJump += JumpOverGap;
            wallDetectLeftTrigger.OnJump += JumpOverBlock;
            wallJumpDetectLeftTrigger.OnHang += WallJump;
            wallJumpDetectRightTrigger.OnFall += Fall;
        }
        else
        {
            jumpLeftTrigger.OnJump -= JumpOverGap;
            wallDetectLeftTrigger.OnJump -= JumpOverBlock;
            wallJumpDetectLeftTrigger.OnHang -= WallJump;
            wallJumpDetectRightTrigger.OnFall -= Fall;

            jumpRightTrigger.OnJump += JumpOverGap;
            wallDetectRightTrigger.OnJump += JumpOverBlock;
            wallJumpDetectRightTrigger.OnHang += WallJump;
            wallJumpDetectLeftTrigger.OnFall += Fall;
        }
        Debug.Log("Direction changed");
    }
    #endregion

    #region Landing
    /// <summary>
    /// Ground method is called by the groundTrigger OnGrounded event
    /// </summary>
    void Ground()
    {
        // If the current state is Falling, set the nextState to Land and start the coroutine
        if (state.Equals(CharacterState.Falling))
        {
            nextState = CharacterState.Land;
        }
    }

    /// <summary>
    /// EndOfLanding is called by animation event when the land is ended
    /// </summary>
    private void EndOfLanding()
    {
        // Check if a jump has been register
        if (jumpGapAfterLanding)
        {
            JumpOverGap();
        }
        else if (jumpBlockAfterLanding)
        {
            JumpOverBlock();
        }
        else //else set nextstate to Running
        {
            nextState = CharacterState.Running;
        }
    }
    #endregion

    #region Fall from position (Ground/Wall)
    /// <summary>
    /// Fall method is called by groundTrigger OnAir event and wallJumpDetectTriggers OnFall event
    /// </summary>
    void Fall()
    {
        // If in Running state, set the next state to FallFromGround, stop all coroutines and start Decel and CoyoteTime ones
        if (state.Equals(CharacterState.Running) || state.Equals(CharacterState.Land))
        {
            nextState = CharacterState.FallFromGround;
            jumpBlockAfterLanding = false;
            jumpGapAfterLanding = false;
            StartCoroutine(Decel());
        }
        // If in HangOnWall state, set the next state to FallFromWall, stop all coroutines and start the CoyoteTime one
        else if(state.Equals(CharacterState.HangOnWall))
        {
            nextState = CharacterState.FallFromWall;
        }
    }

    /// <summary>
    /// EndOfCoyoteTime is called by animation events (RunToFall & WallToFall) to trigger the start of the falling
    /// </summary>
    private void EndOfCoyoteTime()
    {
        StopAllCoroutines(); // To Stop Decel coroutine if needed
        nextState = CharacterState.Falling;
    }

    /// <summary>
    /// Decel coroutine decrease the horizontal velocity of the character
    /// </summary>
    /// <returns></returns>
    IEnumerator Decel()
    {
        while(Mathf.Abs(runnerRigBody.velocity.x)> minVelocity)
        {
            runnerRigBody.velocity = new Vector2(runnerRigBody.velocity.x - 1.5f* (Time.deltaTime *runAccel) * direction.x, 0f);
            yield return null;
        }
        runnerRigBody.velocity = Vector2.zero;
    }
    #endregion

    #region Animation triggers
    /// <summary>
    /// SetAnimationTrigger method resets all animation trigger before setting the one in parameters
    /// </summary>
    /// <param name="_trigger">Name of the trigger (string)</param>
    private void SetAnimationTrigger(string _trigger)
    {
        ResetAllAnimationTriggers();
        runnerAnimator.SetTrigger(_trigger);
    }

    /// <summary>
    /// ResetAllAnimationTriggers method resets all the animation triggers
    /// </summary>
    private void ResetAllAnimationTriggers()
    {
        runnerAnimator.ResetTrigger("Start");
        runnerAnimator.ResetTrigger("Jump");
        runnerAnimator.ResetTrigger("Fall");
        runnerAnimator.ResetTrigger("Land");
        runnerAnimator.ResetTrigger("Hang");
        runnerAnimator.ResetTrigger("Die");
    }
    #endregion

    #region Spawn position
    /// <summary>
    /// SetSpawnPoint method defines a new spawn point
    /// </summary>
    /// <param name="_pos">New position of the spawn point (Vector3)</param>
    /// <param name="_dir">Direction of the character at the new position (Vector2)</param>
    public void SetSpawnPoint(Vector3 _pos, Vector2 _dir)
    {
        spawnPoint = _pos;
        spawnDirection = (_dir.x > 0f) ? Vector2.right : Vector2.left;
        spawnIsSet = true;
    }

    /// <summary>
    /// GoToSpawnPoint method moves the transform to the spawn point (reset position)
    /// </summary>
    private void GoToSpawnPoint()
    {
        transform.position = spawnPoint;
        if (!direction.Equals(spawnDirection)) ChangeDirection();
    }
    #endregion

    #region EventSystem
    /// <summary>
    /// OnPress method is called when the mouse button is pressed to go from Idle to StartRunning states
    /// </summary>
    void OnPress()
    {
        if(state.Equals(CharacterState.Idle))
        {
            nextState = CharacterState.StartRunning;
        }
    }
    #endregion
}
