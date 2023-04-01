using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullGroup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Cull()
    {
        /*
        CullObject[] children = GetComponentsInChildren<CullObject>();

        foreach(CullObject child in children)
        {
            child.Hide();
        }
        */
    }

    public void UnCull()
    {
        /*
        CullObject[] children = GetComponentsInChildren<CullObject>();

        foreach (CullObject child in children)
        {
            child.Show();
        }
        */
    }
}
