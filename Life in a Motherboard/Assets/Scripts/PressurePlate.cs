using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressurePlate : MonoBehaviour
{
    public float requiredPressure;

    public GameObject plopp;
    public Text text;

    private float rbMass = 0;

    private List<Rigidbody2D> rigidbodies = new List<Rigidbody2D>();

    private void Update()
    {
        rbMass = 0;

        foreach (Rigidbody2D _rb in rigidbodies) 
        {
            rbMass += _rb.mass;
        }

        plopp.SetActive(!(rbMass >= requiredPressure));



        text.text = Mathf.Clamp(Mathf.RoundToInt(requiredPressure - rbMass), 0, (int)requiredPressure).ToString();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PhysicsObject"))
        {
            if (rigidbodies.Contains(other.GetComponent<Rigidbody2D>()))
                return;

            var _rb = other.GetComponent<Rigidbody2D>();

            rigidbodies.Add(_rb);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PhysicsObject"))
        {
            var _rb = other.GetComponent<Rigidbody2D>();

            rigidbodies.Remove(_rb);
        }
    }
}
