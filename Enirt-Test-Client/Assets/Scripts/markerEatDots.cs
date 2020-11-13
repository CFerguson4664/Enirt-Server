using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class markerEatDots : MonoBehaviour
{
    //Stores the number of orbs the player has eaten
    public static int minSize = 20;
    public int size = 20;

    public int clientId;
    public int playerId;

    // Start is called before the first frame update
    void Start()
    {
        //Calculate the player's radius and scale their model accordingly
        float radius = Mathf.Sqrt(size / 50f / Mathf.PI);
        transform.localScale = new Vector3(radius, radius, transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        //Calculate the player's radius and scale their model accordingly
        float radius = Mathf.Sqrt(size / 50f / Mathf.PI);
        transform.localScale = new Vector3(radius, radius, transform.localScale.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Check to see if the tag on the collider is equal to an Orb
        if (other.gameObject.tag == "orb")
        {
            //If it is an orb, eat it and increase the player's size
            objectManager.removeOrbs.Add(other.gameObject.GetComponent<orbData>().Id);
            size += 1;
        }
    }
}
