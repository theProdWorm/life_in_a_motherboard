using UnityEngine;

public class EnergyCube : MonoBehaviour
{
    public float energy;
    public float minimumEnergy;
    public float maximumEnergy;

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

        energyDelta = energy;
    }
}
