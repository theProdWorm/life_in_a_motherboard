using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region public
    public float speed;
    public float jumpForce;
    public float jumpVelocityLoss;

    public Transform groundCheck;
    public Transform facingCheck;
    public float groundCheckRadius;
    public LayerMask whatIsGround;

    public Transform energyTank;
    public float energy;
    public float energyCap;
    public float drainSpeed;
    public float efficiency;
    #endregion

    #region private
    private float xMovement;
    private bool isGrounded;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer renderer;
    private bool flipped;

    private List<EnergyCube> energyCubes;
    private EnergyCube energyCube;
    #endregion

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();
        renderer = gameObject.GetComponent<SpriteRenderer>();

        energyCubes = new List<EnergyCube>();
    }

    private void Update()
    {
        Jump();

        anim.SetBool("isJumping", !isGrounded);

        // Increase/decrease size of closest cube in the direction the player is facing.
        if (energyCubes.Count != 0)
        {
            ClosestCube();

            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.JoystickButton7))
            {
                Transfer();
            }
            if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.JoystickButton6))
            {
                Drain();
            }
        }

        energyTank.localScale = new Vector3(1, energy / energyCap, 1);

        if (energy >= energyCap)
            Die();

        Grab();
    }

    private void Grab()
    {

    }

    private void Jump()
    {
        if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton1)) && isGrounded)
        {
            rb.velocity = Vector2.up * jumpForce;
        }
        else if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton1)) && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpVelocityLoss);
        }
    }

    private void ClosestCube()
    {
        float smallestDistance = 1000;

        foreach (EnergyCube cube in energyCubes)
        {
            float distance = Vector3.Distance(cube.transform.position, facingCheck.position);

            if (distance < smallestDistance)
            {
                energyCube = cube;
                smallestDistance = distance;
            }
        }
    }

    private void Transfer() // Transfer energy from player to energy ball
    {
        if (energy >= drainSpeed)
        {
            if (energyCube.energy + drainSpeed * efficiency <= energyCube.maximumEnergy)
            {
                energy -= drainSpeed;
                energyCube.energy += drainSpeed * efficiency;
            }
            else
            {
                energy -= energyCube.maximumEnergy - energyCube.energy;
                energyCube.energy = energyCube.maximumEnergy;
            }
        }
        else if (energy > 0)
        {
            if (energyCube.energy + energy * efficiency <= energyCube.maximumEnergy)
            {
                energy = 0;
                energyCube.energy += energy * efficiency;
            }
            else
            {
                energy -= energyCube.maximumEnergy - energyCube.energy;
                energyCube.energy = energyCube.maximumEnergy;
            }
        }
    }

    private void Drain() // Drain energy from cube
    {
        if (energyCube.energy - drainSpeed >= energyCube.minimumEnergy)
        {
            energy += drainSpeed * efficiency;
            energyCube.energy -= drainSpeed;
        }
        else
        {
            energy += energyCube.energy - energyCube.minimumEnergy;
            energyCube.energy = energyCube.minimumEnergy;
        }
    }

    private void Die()
    {
        // Death code in here
        print("died");
    }

    private void FixedUpdate()
    {
        xMovement = Input.GetAxisRaw("Horizontal") * speed;
        anim.SetBool("isRunning", xMovement > 0 || xMovement < 0);

        if ((xMovement < 0 && !flipped) || (xMovement > 0 && flipped))
            FlipSprite();

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        rb.velocity = new Vector2(xMovement, rb.velocity.y);
    }

    private void FlipSprite()
    {
        flipped = !flipped;

        transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("EnergyCube") && !energyCubes.Contains(other.gameObject.GetComponentInParent<EnergyCube>()))
                energyCubes.Add(other.gameObject.GetComponentInParent<EnergyCube>());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("EnergyCube"))
            energyCubes.Remove(other.gameObject.GetComponentInParent<EnergyCube>());
    }
}