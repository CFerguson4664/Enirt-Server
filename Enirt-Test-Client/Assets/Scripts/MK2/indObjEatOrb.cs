using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class indObjEatOrb : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        //Check to see if the tag on the collider is equal to an Orb
        if (other.gameObject.tag == "orb")
        {
            //If it is an orb, eat it and increase the player's size
            objectManager.removeOrbs.Add(other.gameObject.GetComponent<OrbId>().Id);
        }
    }
}
