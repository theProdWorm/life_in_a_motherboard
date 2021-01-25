using System.Collections;
using UnityEngine;

public class EnergyCube : MonoBehaviour
{
    public float energy;
    public float minimumEnergy;
    public float maximumEnergy;

    public LayerMask whatIsBoundaries;

    public float throwVelocityOrigin;
    private float throwVelocity;

    private float energyDelta;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Default: 100 energy
        // Scale = energy / 100

        transform.localScale = Vector3.one * (energy / 100);

        rb.mass = transform.localScale.magnitude * 15;

        if (energyDelta == energy)
            rb.gravityScale = 2;
        else
            rb.gravityScale = 0;

        throwVelocity = throwVelocityOrigin * transform.localScale.x;

        energyDelta = energy;
    }

    public bool CanGrow()
    {
        bool canGrow = !Physics2D.OverlapBox(transform.position, transform.localScale, 0, whatIsBoundaries);

        return canGrow;
    }

    public void Throw(Vector2 velocity)
    {
        transform.SetParent(null);

        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.AddForce(velocity);
    }
}
