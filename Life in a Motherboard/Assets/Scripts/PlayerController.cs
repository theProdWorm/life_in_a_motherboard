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
    public float maxCarryCap;
    public float drainSpeed;
    public float efficiency;

    public GameObject energyCube;
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
            bool grabbedThisFrame = false;

            ClosestCube();
            if (closestCube != null)
            {
                Drain();
                Transfer();

                grabbedThisFrame = Grab();
            }

            if (!grabbedThisFrame)
                Throw();

            print(grabbedCube);
        }

        energyTank.localScale = new Vector3(1, energy / energyCap, 1);

        if (energy >= energyCap)
            Die();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Instantiate(energyCube, new Vector3(5, -5, 0), Quaternion.identity);
        }
    }

    private bool Grab()
    {
        if (!(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5)))
            return false;

        if (grabbedCube != null)
            return false;

        if (closestCube.transform.localScale.x >= maxCarryCap)
            return false;

        grabbedCube = closestCube;
        grabbedCube.transform.SetParent(grabbingPoint);
        grabbedCube.transform.position = grabbingPoint.position;
        grabbedCube.transform.rotation = Quaternion.identity;

        var _rb = grabbedCube.GetComponent<Rigidbody2D>();

        _rb.bodyType = RigidbodyType2D.Kinematic;

        _rb.rotation = 0;
        _rb.angularVelocity = 0;
        _rb.velocity = Vector2.zero;

        closestCube = null;

        return true;
    }


    private void Throw()
    {
        if (!(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5)))
            return;

        if (grabbedCube == null)
            return;

        grabbedCube.Throw(rb.velocity * 0.75f + new Vector2(transform.localScale.x, transform.localScale.y));

        grabbedCube = null;
    }

    private void Jump()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.JoystickButton1)) && isGrounded)
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

        if (closestCube == grabbedCube)
            closestCube = null;

        foreach (EnergyCube cube in energyCubes)
        {
            if (cube == grabbedCube)
                continue;

            float distance = Vector3.Distance(cube.transform.position, facingCheck.position);

            if (distance < smallestDistance)
            {
                closestCube = cube;
                smallestDistance = distance;
            }
        }
    }

    private void Drain() // Drain energy from cube
    {
        if (!(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.JoystickButton7)))
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

    private void Transfer() // Transfer energy from player to energy ball
    {
        if (!(Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.JoystickButton6)))
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
        {
            energyCubes.Add(other.gameObject.GetComponentInParent<EnergyCube>());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("EnergyCube"))
        {
            energyCubes.Remove(other.gameObject.GetComponentInParent<EnergyCube>());

            if (closestCube = other.gameObject.GetComponentInParent<EnergyCube>())
                closestCube = null;
        }
    }
}