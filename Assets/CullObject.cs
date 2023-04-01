using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullObject : MonoBehaviour
{
    public bool doNotCull = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    }

    private void FixedUpdate()
    {
        
    }

    public void Cull()
    {
        /*
        if (GetComponent<Renderer>().enabled == false)
        {
            return;
        }


        var groupCull = this.GetComponentInParent<CullGroup>();

        if (groupCull != null)
        {
            groupCull.Cull();
        } else
        {
            if (doNotCull)
            {
                return;
            }
            GetComponent<Renderer>().enabled = false;
        }   
        */
    }

    public void UnCull()
    {
        /*
        if (GetComponent<Renderer>().enabled == true)
        {
            return;
        }
        var groupCull = this.GetComponentInParent<CullGroup>();

        if (groupCull != null)
        {
            groupCull.UnCull();
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
        }
        */
    }

    public void Hide()
    {
        /*
        if (doNotCull)
        {
            return;
        }
        GetComponent<Renderer>().enabled = false;
        */
    }

    public void Show()
    {
        //GetComponent<Renderer>().enabled = true;
    }

}
