using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    private BoxCollider2D coll;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private static Vector3 respawnPoint = new Vector3(1f, 1f, 1f);
    private static Vector3 checkpoint = new Vector3(0f, 0f, 0f);

    private static bool isNotFull;

    private static float hitPoints;

    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private Animator frogAime_1;
    [SerializeField] private Animator frogAime_2;
    [SerializeField] private Animator opossumAime_1;
    [SerializeField] private Animator opossumAime_2;
    [SerializeField] private Animator crank_1;
    [SerializeField] private Animator crank_2;
    [SerializeField] private Animator crank_3;
    [SerializeField] private Animator crank_4;
    [SerializeField] private Animator crank_5;
    [SerializeField] private BoxCollider2D frogColl_1;
    [SerializeField] private BoxCollider2D frogColl_2;
    [SerializeField] private BoxCollider2D opossumColl_1;
    [SerializeField] private BoxCollider2D opossumColl_2;
    [SerializeField] private Rigidbody2D frogRB_1;
    [SerializeField] private Rigidbody2D frogRB_2;
    [SerializeField] private Rigidbody2D opossumRB_1;
    [SerializeField] private Rigidbody2D opossumRB_2;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Vector2 standingOffset;
    [SerializeField] private Vector2 standingSize;
    [SerializeField] private Vector2 crouchingOffset;
    [SerializeField] private Vector2 crouchingSize;

    [SerializeField] private AudioSource hurtSoundEffect;
    [SerializeField] private AudioSource jumpSoundEffect;
    [SerializeField] private AudioSource crouchSoundEffect;
    [SerializeField] private AudioSource forgDeathSoundEffect;
    [SerializeField] private AudioSource opossumDeathSoundEffect;
    [SerializeField] private AudioSource HealthSoundEffect;
    [SerializeField] private AudioSource checkpointSoundEffect;
    [SerializeField] private AudioSource finishSoundEffect;

    [SerializeField] private LayerMask jumpableGround;

    private enum movementState {idle, run, jump, fall, climb, crouch, hurt}

    private movementState state;

    private float dirX = 0f;
    private float vertical;
    private bool isClimbing, isHurting, isCrouching, isJumping, isLadder, isCrank_1, isCrank_2, isCrank_3, isCrank_4, isCrank_5;
    private float maxHealth = 3f;
    public float currentHealth;

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float hurtForce = 5f;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        if (!isNotFull)
        {
            currentHealth = maxHealth;
            healthBar.SetMaxHealth(maxHealth);
        }
        else
        {
            if (hitPoints == 3)
            {
                currentHealth++;
            }

            currentHealth++;
            healthBar.SetHealth(currentHealth);
        }

        if (respawnPoint == checkpoint)
        {
            transform.position = checkpoint;
        }
        else { respawnPoint = transform.position; }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.timeScale == 0)
        {
            Cursor.visible = true;
        }
        else { Cursor.visible = false; }

        if (!isHurting)
        {
            Movement();
        }

        if (Input.GetButtonDown("Fire1") && IsGrounded() && Time.timeScale == 1)
        {
            crouchSoundEffect.Play();
        }

        if (rb.velocity.x == 0f || rb.velocity.y == 0f)
        {
            isHurting = false;
        }

        UpdateAnimationState();

        if (Time.timeScale == 0)
        {
            rb.velocity = Vector2.zero;

            anim.SetInteger("state", (int) - 1);
        }

    }

    public void MenuHealth()
    {
        isNotFull = false;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);

        if (currentHealth > 0)
        {
            hurtSoundEffect.Play();
        }
    }

    private void IncreaseHealth(float health)
    {
        currentHealth += health;
        healthBar.SetHealth(currentHealth);
    }

    private void Movement()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        vertical = Input.GetAxisRaw("Vertical");
        if (isLadder && Mathf.Abs(vertical) > 0f)
        {
            isClimbing = true;
            anim.speed = 1f;
        }
        else if (isLadder && vertical == 0f)
        {
            isClimbing = true;
            anim.speed = 0f;
        }
        else { anim.speed = 1f; }

        if (Input.GetButton("Fire1") && IsGrounded())
        {
            coll.offset = crouchingOffset;
            coll.size = crouchingSize;

            rb.velocity = new Vector2(0f, rb.velocity.y);

            isCrouching = true;
        }
        else
        {
            coll.offset = standingOffset;
            coll.size = standingSize;

            isCrouching = false;
        }

        if (Input.GetButtonDown("Jump") && IsGrounded() && Time.timeScale == 1)
        {
            jumpSoundEffect.Play();

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }
        else if (rb.velocity.y < .1f)
        { 
            isJumping = false; 
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, vertical * moveSpeed);
        }
        else { rb.gravityScale = 2.8f; }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (state == movementState.fall && collision.gameObject.name.Equals("Frog (1)"))
            {
                rb.velocity = new Vector2(rb.velocity.x, hurtForce * 2);
                frogAime_1.SetTrigger("Death");
                frogColl_1.GetComponent<BoxCollider2D>().isTrigger = true;
                frogRB_1.bodyType = RigidbodyType2D.Static;
                isJumping = true;

                forgDeathSoundEffect.Play();
            }
            else if (state == movementState.fall && collision.gameObject.name.Equals("Frog (2)"))
            {
                rb.velocity = new Vector2(rb.velocity.x, hurtForce * 2);
                frogAime_2.SetTrigger("Death");
                frogColl_2.GetComponent<BoxCollider2D>().isTrigger = true;
                frogRB_2.bodyType = RigidbodyType2D.Static;
                isJumping = true;

                forgDeathSoundEffect.Play();
            }
            else if (state == movementState.fall && collision.gameObject.name.Equals("Opossum (1)"))
            {
                rb.velocity = new Vector2(rb.velocity.x, hurtForce * 2);
                opossumAime_1.SetTrigger("Death");
                opossumColl_1.GetComponent<BoxCollider2D>().isTrigger = true;
                opossumRB_1.bodyType = RigidbodyType2D.Static;
                isJumping = true;

                opossumDeathSoundEffect.Play();
            }
            else if (state == movementState.fall && collision.gameObject.name.Equals("Opossum (2)"))
            {
                rb.velocity = new Vector2(rb.velocity.x, hurtForce * 2);
                opossumAime_2.SetTrigger("Death");
                opossumColl_2.GetComponent<BoxCollider2D>().isTrigger = true;
                opossumRB_2.bodyType = RigidbodyType2D.Static;
                isJumping = true;

                opossumDeathSoundEffect.Play();
            }

            else
            {
                isHurting = true;

                if (collision.gameObject.transform.position.x > transform.position.x)
                {
                    rb.velocity = new Vector2(-hurtForce / 2, 10f);
                }
                else { rb.velocity = new Vector2(hurtForce / 2, 10f); }

                TakeDamage(1f);

                if (currentHealth == 0)
                {
                    playerLife.Die();

                    hitPoints = currentHealth;

                    checkpoint = respawnPoint;

                    isNotFull = true;
                }

                isJumping = false;

            }
        }

        if (collision.gameObject.CompareTag("Trap"))
        {
            hitPoints = currentHealth;

            checkpoint = respawnPoint;

            isNotFull = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Health"))
        {
            HealthSoundEffect.Play();

            if (currentHealth < maxHealth)
            {
                IncreaseHealth(1f);
            }
            Destroy(collision.gameObject);
        }
        
        if (collision.CompareTag("Ladder"))
        { 
            isLadder = true;
        }

        if (collision.CompareTag("Door"))
        {
            finishSoundEffect.Play();

            anim.SetTrigger("End");

            rb.bodyType = RigidbodyType2D.Static;

            Invoke("CompleteLevel", 1.5f);
        }

        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            if (collision.gameObject.name.Equals("Checkpoint (1)"))
            {
                if (!isCrank_1)
                {
                    checkpointSoundEffect.Play();
                }

                respawnPoint = transform.position;
                isCrank_1 = true;
            }
            else if (collision.gameObject.name.Equals("Checkpoint (2)"))
            {
                if (!isCrank_2)
                {
                    checkpointSoundEffect.Play();
                }

                respawnPoint = transform.position;
                isCrank_2 = true;
            }
            else if (collision.gameObject.name.Equals("Checkpoint (3)"))
            {
                if (!isCrank_3)
                {
                    checkpointSoundEffect.Play();
                }

                respawnPoint = transform.position;
                isCrank_3 = true;
            }
            else if (collision.gameObject.name.Equals("Checkpoint (4)"))
            {
                if (!isCrank_4)
                {
                    checkpointSoundEffect.Play();
                }

                respawnPoint = transform.position;
                isCrank_4 = true;
            }
            else if (collision.gameObject.name.Equals("Checkpoint (5)"))
            {
                if (!isCrank_5)
                {
                    checkpointSoundEffect.Play();
                }

                respawnPoint = transform.position;
                isCrank_5 = true;
            }
        }

        if (collision.gameObject.CompareTag("Trap"))
        {
            hitPoints = currentHealth;

            checkpoint = respawnPoint;

            isNotFull = true;
        }
    }
    private void CompleteLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false;
        }

        if (rb.velocity.y > .1f)
        {
            isJumping = true;
        }
        else { isJumping = false; }
    }

    private void UpdateAnimationState()
    {
        if (rb.bodyType == RigidbodyType2D.Static)
        {
            
        }
        else
        {
            if (dirX > 0f)
            {
                state = movementState.run;
                if (Time.timeScale == 1)
                {
                    sprite.flipX = false;
                }
            }
            else if (dirX < 0f)
            {
                state = movementState.run;
                if (Time.timeScale == 1)
                {
                    sprite.flipX = true;
                }
            }
            else
            {
                state = movementState.idle;
            }

            if (rb.velocity.y > 0.1f && isJumping && !IsGrounded())
            {
                state = movementState.jump;
            }
            else if (rb.velocity.y < -.1f && !IsGrounded())
            {
                state = movementState.fall;
            }

            if (isCrouching)
            {
                state = movementState.crouch;
            }

            if (isClimbing)
            {
                state = movementState.climb;
            }

            if (isHurting)
            {
                state = movementState.hurt;
            }

            if (isCrank_1)
            {
                crank_1.SetTrigger("Down");
            }

            if (isCrank_2)
            {
                crank_2.SetTrigger("Down");
            }

            if (isCrank_3)
            {
                crank_3.SetTrigger("Down");
            }

            if (isCrank_4)
            {
                crank_4.SetTrigger("Down");
            }

            if (isCrank_5)
            {
                crank_5.SetTrigger("Down");
            }
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
}
