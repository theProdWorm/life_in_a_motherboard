using UnityEngine;

public class ClampScale : MonoBehaviour
{
    private void Update()
    {
        var parentScale = transform.parent.localScale;

        if (parentScale.x < 1 || parentScale.y < 1)
        {
            transform.localScale = CreateScale(parentScale);
        }
        else
            transform.localScale = Vector3.one;
    }

    private Vector3 CreateScale(Vector3 parentScale)
    {
        parentScale.x = 1 / parentScale.x;
        parentScale.y = 1 / parentScale.y;
        parentScale.z = 1 / parentScale.z;

        return parentScale;
    }
}
