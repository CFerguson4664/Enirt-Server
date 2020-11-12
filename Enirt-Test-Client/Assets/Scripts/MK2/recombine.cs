using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class recombine : MonoBehaviour
{
    public float recombineTime;

    // Start is called before the first frame update
    void Awake()
    {
        recombineTime = 1;
    }

    // Update is called once per frame
    void Update()
    { 
        if(recombineTime > 0)
        {
            recombineTime -= Time.deltaTime;
        }
    }

    public void SetRecombine()
    {
        recombineTime = (float)(Math.Pow(Math.Log(GetComponent<eatDots>().size), 1.7) * 3);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(recombineTime <= 0 && other.otherCollider.gameObject.GetComponent<recombine>().recombineTime <= 0)
            {
                if(!GetComponent<PlayerMovement>().glide && !other.otherCollider.gameObject.GetComponent<PlayerMovement>().glide)
                {
                    if (GetComponent<eatDots>().size > other.otherCollider.gameObject.GetComponent<eatDots>().size)
                    {
                        Debug.Log(other.otherCollider.gameObject.GetComponent<PlayerMovement>().Id);

                        Debug.Log("recombine");
                        GetComponent<eatDots>().size += playerManager.ourPlayers[other.otherCollider.gameObject.GetComponent<PlayerMovement>().Id].Size;
                        playerManager.RemovePlayer(other.otherCollider.gameObject.GetComponent<PlayerMovement>().Id);
                        other.otherCollider.gameObject.GetComponent<eatDots>().size = 0;
                    }
                }
            }
        }
    }
}
