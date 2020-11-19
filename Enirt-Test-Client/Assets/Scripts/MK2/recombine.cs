using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Recombine : MonoBehaviour
{
    //Store all of the collisions occuring each frame
    List<int> collisions = new List<int>();


    // Update is called once per frame
    void FixedUpdate()
    {
        //If we still exist in the player list
        if(PlayerManager.ourPlayers.ContainsKey(GetComponent<PlayerMovement>().Id)) 
        {
            //update all of collisions
            PlayerManager.ourPlayers[GetComponent<PlayerMovement>().Id].currentPlayerCollisions = collisions;
            collisions = new List<int>();
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Add every player we are colliding with this frame to the list
            collisions.Add(other.gameObject.GetComponent<PlayerMovement>().Id);
        }
    }
}
