using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    private Transform originalTransform;

    // Start is called before the first frame update
    void Start()
    {
        originalTransform = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = originalTransform.rotation;
    }
}
