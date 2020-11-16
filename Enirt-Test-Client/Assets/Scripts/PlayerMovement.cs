using JetBrains.Annotations;
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
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;

    public float glideTime;
    public float glideSpeed;

    public Vector3 currentPosition;
    public int currentSize;

    public bool glide = false;
	public static bool keyboardEnable;
	
    private Vector3 glideDirection;
    private float glideTimer = 0;
    float effectiveSpeed;
	
	//Multiplying by time.deltaTime compensates for varying frame rates
	float adjustedSpeed;
	
    public int Id;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        effectiveSpeed = (float)(speed * (1 / Math.Log(Math.Pow(GetComponent<eatDots>().size, 0.5))));

        if (Input.GetKeyDown("space"))
        {
            if(GetComponent<eatDots>().size >= eatDots.minSize * 2)
            {
                GetComponent<eatDots>().size /= 2;
                playerManager.ourPlayers[Id].Size = GetComponent<eatDots>().size;
                PlayerData newPlayer = new PlayerData(currentSize / 2, currentPosition.x, currentPosition.y, currentPosition.z);
                playerManager.ourPlayers[Id].SetRecombine();
                playerManager.AddPlayer(newPlayer, GetComponent<eatDots>().size, true);
            }
        }

        if (glide)
        {
            Glide();
        }
        else
        {
			adjustedSpeed = effectiveSpeed * Time.deltaTime;
			
			// Keyboard controls
			if(keyboardEnable==true){
				Vector2 target = transform.position;
				
				if(Input.GetKey(KeyCode.D)){
					target.x += adjustedSpeed;
				}
				if(Input.GetKey(KeyCode.A)){
					target.x -= adjustedSpeed;
				}
				if(Input.GetKey(KeyCode.W)){
					target.y += adjustedSpeed;
				}
				if(Input.GetKey(KeyCode.S)){
					target.y -= adjustedSpeed;
				}
				
				transform.position = target;
				playerManager.ourPlayers[Id].UpdateData(netComs.GetTime(), GetComponent<eatDots>().size, transform.position.x, transform.position.y, transform.position.z);
				currentSize = GetComponent<eatDots>().size;
				
			}
			
			else{
				FollowMouse();
			}
        }
        checkBounds();
    }

    public void StartGlide()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 offset = mousePosition - transform.position;
        Debug.Log(mousePosition);
        Debug.Log(transform.position);
        Debug.Log(offset);

        glideDirection = new Vector3((offset.x * 1000) + transform.position.x, (offset.y * 1000) + transform.position.y, transform.position.z);
        glideTimer = glideTime;
        glide = true;

        
        gameObject.layer = 9;

        Debug.Log(glideDirection);
    }

    void Glide()
    {
        if(glideTimer > 0)
        {
            float adjustedSpeed = glideSpeed * Time.deltaTime * (glideTimer / glideTime);

            //Translate the player object by adjusted speed in the needed direction
            transform.position = Vector2.MoveTowards(transform.position, glideDirection, adjustedSpeed);
            playerManager.ourPlayers[Id].UpdateData(netComs.GetTime(), GetComponent<eatDots>().size, transform.position.x, transform.position.y, transform.position.z);

            currentPosition = transform.position;
            glideTimer -= Time.deltaTime;
        }
        else
        {
            gameObject.layer = 8;
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
            //Translate the player object by adjusted speed in the needed direction
            transform.position = Vector2.MoveTowards(transform.position, mousePosition, adjustedSpeed);
        }

        currentPosition = transform.position;
        playerManager.ourPlayers[Id].UpdateData(netComs.GetTime(), GetComponent<eatDots>().size, transform.position.x, transform.position.y, transform.position.z);
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
