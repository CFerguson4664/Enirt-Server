using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eatPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        //Check to see if the tag on the collider is equal to Enemy
        if (other.gameObject.tag == "marker")
        {

            if(GetComponent<eatDots>().size > other.gameObject.GetComponent<eatDots>().size) 
            {
                if(other.gameObject.GetComponent<eatDots>().size > 0)
                {
                    GetComponent<eatDots>().size++;
                    other.gameObject.GetComponent<eatDots>().size--;
                }
            }
            else
            {
                if(GetComponent<eatDots>().size > 0)
                {
                    GetComponent<eatDots>().size--;
                    other.gameObject.GetComponent<eatDots>().size++;
                }
            }
        }
    }
}
