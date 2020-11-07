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
            //If our size is larger than the opponents size
            if(GetComponent<eatDots>().size > other.gameObject.GetComponent<eatDots>().size) 
            {
                //And their size is greater than 0
                if(other.gameObject.GetComponent<eatDots>().size > 0)
                {
                    //Increase our size 
                    GetComponent<eatDots>().size++;
                    //And decrease their size
                    other.gameObject.GetComponent<eatDots>().size--;
                }
            }
            //If the opponents size is larger than our size
            else
            {
                //And our size is greater than 0
                if (GetComponent<eatDots>().size > 0)
                {
                    //Decrease our size
                    GetComponent<eatDots>().size--;
                    //And increase their size
                    other.gameObject.GetComponent<eatDots>().size++;
                }
            }
        }
    }
}
