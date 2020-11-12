using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float slack;
    public float cameraSlack;
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;

    public float glideTime;
    public float glideSpeed;


    public Vector3 currentPosition;
    public int currentSize;

    private bool glide = false;
    private Vector3 glideDirection;
    private float glideTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            PlayerData newPlayer = new PlayerData(currentSize / 2, currentPosition.x, currentPosition.y, currentPosition.z);
            GetComponent<eatDots>().size /= 2;
            playerManager.AddPlayer(newPlayer, true);
        }

        if (glide)
        {
            Glide();
        }
        else
        {
            FollowMouse();
        }
        checkBounds();
    }

    public void StartGlide()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 offset = mousePosition - currentPosition;

        float XPos = 0;
        float YPos = 0;

        if (Math.Abs(offset.x) > Math.Abs(offset.y))
        {
            YPos = 1000f;
            XPos = Math.Abs(offset.x) * (1000f / Math.Abs(offset.y));
        }
        else
        {
            XPos = 1000f;
            YPos = Math.Abs(offset.y) * (1000f / Math.Abs(offset.x));
        }
        
        if(offset.x < 0)
        {
            XPos *= -1;
        }

        if (offset.y < 0)
        {
            YPos *= -1;
        }

        glideDirection = new Vector3(XPos, YPos, currentPosition.z);
        glideTimer = glideTime;
        glide = true;
    }

    void Glide()
    {
        if(glideTimer > 0)
        {
            float adjustedSpeed = glideSpeed * Time.deltaTime;

            //Translate the player object by adjusted speed in the needed direction
            transform.position = Vector2.MoveTowards(transform.position, glideDirection, adjustedSpeed);

            currentPosition = transform.position;
            glideTimer -= Time.deltaTime;
        }
        else
        {
            glide = false;
        }
    }

    void FollowMouse()
    {
        //Get the coordinates of the mouse in pixels and convert it to the Unity world coordinate system
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Only move if the difference is significant (Eliminates bouncing when the player reaches the mouse)
        if (Vector3.Distance(mousePosition, transform.position) > slack)
        {
            //Multiplying by time.deltaTime compensates for varying frame rates
            float adjustedSpeed = speed * Time.deltaTime;

            //Translate the player object by adjusted speed in the needed direction
            transform.position = Vector2.MoveTowards(transform.position, mousePosition, adjustedSpeed);

            float cameraSpeed = Vector2.Distance(Camera.main.transform.position, transform.position) * adjustedSpeed * (1 / cameraSlack);

            Vector3 cameraGoalPos = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

            Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, cameraGoalPos, cameraSpeed);
        }

        currentPosition = transform.position;
        currentSize = GetComponent<eatDots>().size;
    }

    void checkBounds()
    {
        if (transform.position.x > rightBound)
        {
            transform.position = new Vector3(rightBound, transform.position.y, transform.position.z);
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

        currentPosition = transform.position;
    }
}
