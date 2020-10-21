using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float slack;
    public float cameraSlack;
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Get the coordinates of the mouse in pixels and convert it to the Unity world coordinate system
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Only move if the difference is significat (Eliminates bouncing when the player reaches the mouse)
        if (Vector3.Distance(mousePosition,transform.position) > slack)
        {
            //Multiplying by time.deltaTime compensates for varying frame rates
            float adjustedSpeed = speed * Time.deltaTime;

            //Translate the player object by adjusted speed in the needed direction
            transform.position = Vector2.MoveTowards(transform.position, mousePosition, adjustedSpeed);

            float cameraSpeed = Vector2.Distance(Camera.main.transform.position, transform.position) * adjustedSpeed * (1 / cameraSlack);

            Vector3 cameraGoalPos = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

            Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position,cameraGoalPos,cameraSpeed);

            if(transform.position.x > rightBound)
            {
                transform.position = new Vector3(rightBound,transform.position.y,transform.position.z);
            }
            if (transform.position.x < leftBound)
            {
                transform.position = new Vector3(leftBound, transform.position.y, transform.position.z);
            }
            if (transform.position.y > topBound)
            {
                transform.position = new Vector3(transform.position.x, topBound, transform.position.z);
            }
            if (transform.position.y < bottomBound)
            {
                transform.position = new Vector3(transform.position.x, bottomBound, transform.position.z);
            }
        }
    }
}
