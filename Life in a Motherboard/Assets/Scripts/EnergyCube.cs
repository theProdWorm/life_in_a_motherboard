using System;
using UnityEngine;

public class EnergyCube : MonoBehaviour
{
    const float appropriateNameForAConstantFloarVariable = 0.0078947368421f;

    public float energy;
    public float minimumEnergy;
    public float maximumEnergy;

    public LayerMask whatIsBoundaries;
    public float shellRadius;

    public float throwForceOrigin;

    private float throwForce;

    private float energyDelta;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        transform.localScale = Vector3.one * (appropriateNameForAConstantFloarVariable * energy);

        throwForce = throwForceOrigin / Mathf.Pow(transform.localScale.x, 1 / 3);

        if (energyDelta == energy)
            rb.gravityScale = 2;
        else
            rb.gravityScale = 0;

        rb.mass = transform.localScale.magnitude * 15;

        energyDelta = energy;
    }
    public bool CanGrow()
    {
        bool canGrow = !(Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0, whatIsBoundaries).Length > 1);

        return canGrow;
    }

    public void Throw(Vector2 velocity)
    {
        transform.SetParent(null);

        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.velocity = new Vector2(velocity.x * throwForce, velocity.y * throwForce);
    }
}
