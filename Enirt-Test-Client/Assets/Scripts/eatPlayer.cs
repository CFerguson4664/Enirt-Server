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
            if(GetComponent<eatDots>().size > other.gameObject.GetComponent<markerEatDots>().size + 10) 
            {
                float distance = Vector2.Distance(transform.position, other.gameObject.transform.position);
                float radius = Mathf.Sqrt(GetComponent<eatDots>().size / 50f / Mathf.PI);

                if (distance < radius)
                {
                    //Increase our size 
                    GetComponent<eatDots>().size += other.gameObject.GetComponent<markerEatDots>().size;
                    objectManager.removePlayers.Add(new IdPair(other.gameObject.GetComponent<markerEatDots>().clientId, other.gameObject.GetComponent<markerEatDots>().playerId));
                }
            }
            //If the opponents size is larger than our size
            else if (other.gameObject.GetComponent<markerEatDots>().size > GetComponent<eatDots>().size + 10)
            {
                float distance = Vector2.Distance(transform.position, other.gameObject.transform.position);
                float radius = Mathf.Sqrt(other.gameObject.GetComponent<markerEatDots>().size / 50f / Mathf.PI);

                if(distance < radius)
                {
                    other.gameObject.GetComponent<markerEatDots>().size += GetComponent<eatDots>().size;
                    playerManager.RemovePlayer(GetComponent<PlayerMovement>().Id);
                }
            }
        }
    }
}
