using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eatDots : MonoBehaviour
{
    //Stores the number of orbs the player has eaten
    public int size;

    // Start is called before the first frame update
    void Start()
    {
        //Calculate the player's radius and scale their model accordingly
        float radius = Mathf.Sqrt(size / 50f / Mathf.PI);
        transform.localScale = new Vector3(radius + 0.5f, radius + 0.5f, transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        //Calculate the player's radius and scale their model accordingly
        float radius = Mathf.Sqrt(size / 50f / Mathf.PI);
        transform.localScale = new Vector3(radius + 0.5f, radius + 0.5f, transform.localScale.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Check to see if the tag on the collider is equal to an Orb
        if (other.gameObject.tag == "orb")
        {
            //If it is an orb, eat it and increase the player's size
            Destroy(other.gameObject);
            size += 1;
        }
    }
}
