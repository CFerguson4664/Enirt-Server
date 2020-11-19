using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatPlayer : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        //Check to see if the tag on the collider is equal to Enemy
        if (other.gameObject.tag == "marker")
        {

            //If our size is larger than the opponents size
            if(GetComponent<EatDots>().size > other.gameObject.GetComponent<MarkerEatDots>().size + 10) 
            {
                float distance = Vector2.Distance(transform.position, other.gameObject.transform.position);
                float radius = Mathf.Sqrt(GetComponent<EatDots>().size / 50f / Mathf.PI);

                if (distance < radius)
                {
                    //Increase our size 
                    GetComponent<EatDots>().size += other.gameObject.GetComponent<MarkerEatDots>().size;
                    ObjectManager.removePlayers.Add(new IdPair(other.gameObject.GetComponent<MarkerEatDots>().clientId, other.gameObject.GetComponent<MarkerEatDots>().playerId));
                }
            }
            //If the opponents size is larger than our size
            else if (other.gameObject.GetComponent<MarkerEatDots>().size > GetComponent<EatDots>().size + 10)
            {
                float distance = Vector2.Distance(transform.position, other.gameObject.transform.position);
                float radius = Mathf.Sqrt(other.gameObject.GetComponent<MarkerEatDots>().size / 50f / Mathf.PI);

                if(distance < radius)
                {
                    other.gameObject.GetComponent<MarkerEatDots>().size += GetComponent<EatDots>().size;
                    PlayerManager.RemovePlayer(GetComponent<PlayerMovement>().Id);
                }
            }
        }
    }
}
