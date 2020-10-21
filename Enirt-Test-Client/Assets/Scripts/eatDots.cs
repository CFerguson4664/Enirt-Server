using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eatDots : MonoBehaviour
{
    public int size;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Check to see if the tag on the collider is equal to Enemy
        if (other.gameObject.tag == "orb")
        {
            Destroy(other.gameObject);
            size += 1;

            float radius = Mathf.Sqrt(size / 50f / Mathf.PI);
            transform.localScale = new Vector3(radius + 0.5f, radius + 0.5f, transform.localScale.z);
        }
    }
}
