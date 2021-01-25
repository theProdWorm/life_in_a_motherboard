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
    public Transform grabbingPoint;
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
    private bool flipped;

    private List<EnergyCube> energyCubes;
    private EnergyCube closestCube;
    private EnergyCube grabbedCube;
    #endregion

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();

        energyCubes = new List<EnergyCube>();
    }

    private void Update()
    {
        Jump();

        anim.SetBool("isJumping", !isGrounded);

        // Do energy cube logic
        if (energyCubes.Count != 0)
        {
            ClosestCube();
            Transfer();
            Drain();
            Grab();
            Throw();
        }

        energyTank.localScale = new Vector3(1, energy / energyCap, 1);

        if (energy >= energyCap)
            Die();
    }

    private void Grab()
    {
        if (!(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0)))
            return;

        if (grabbedCube != null)
            return;

        closestCube.transform.SetParent(grabbingPoint);
        closestCube.transform.position = grabbingPoint.position;

        grabbedCube = closestCube;

        closestCube.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    private void Throw()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Mouse2) || Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            closestCube.Throw(rb.velocity);
            grabbedCube = null;
        }
    }

    private void Jump()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1)) && isGrounded)
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
                closestCube = cube;
                smallestDistance = distance;
            }
        }
    }

    private void Transfer() // Transfer energy from player to energy ball
    {
        if (!(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.JoystickButton7)))
            return;

        if (!closestCube.CanGrow())
            return;

        if (energy >= drainSpeed)
        {
            if (closestCube.energy + drainSpeed * efficiency <= closestCube.maximumEnergy)
            {
                energy -= drainSpeed;
                closestCube.energy += drainSpeed * efficiency;
            }
            else
            {
                energy -= closestCube.maximumEnergy - closestCube.energy;
                closestCube.energy = closestCube.maximumEnergy;
            }
        }
        else if (energy > 0)
        {
            if (closestCube.energy + energy * efficiency <= closestCube.maximumEnergy)
            {
                energy = 0;
                closestCube.energy += energy * efficiency;
            }
            else
            {
                energy -= closestCube.maximumEnergy - closestCube.energy;
                closestCube.energy = closestCube.maximumEnergy;
            }
        }
    }

    private void Drain() // Drain energy from cube
    {
        if (!(Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.JoystickButton6)))
            return;

        if (closestCube.energy - drainSpeed >= closestCube.minimumEnergy)
        {
            energy += drainSpeed * efficiency;
            closestCube.energy -= drainSpeed;
        }
        else
        {
            energy += closestCube.energy - closestCube.minimumEnergy;
            closestCube.energy = closestCube.minimumEnergy;
        }
    }

    private void Die()
    {
        // Death code in here
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