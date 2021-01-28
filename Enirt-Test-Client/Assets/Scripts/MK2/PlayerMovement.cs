using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public ParticleSystem boost;
    public float speed;
    public float slack;
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;

    public Text name;
    public float glideTime;
    public float glideSpeed;

    public Vector3 currentPosition;
    public int currentSize;

    public bool glide = false;
	public static bool keyboardEnable;
	
    private Vector3 glideDirection;
    private float glideTimer = 0;
    float effectiveSpeed;

    public static Player smallestPlayer = null;
	
	//Multiplying by time.deltaTime compensates for varying frame rates
	float adjustedSpeed;

    float boostCost = 0;
	
    public int Id;

    // Update is called once per frame
    void Update()
    {
        effectiveSpeed = (float)(speed * (1 / Math.Log(Math.Pow(GetComponent<EatDots>().size, 0.5))));

        //If we push space try to split
        if (Input.GetKeyDown("space"))
        {
            if(GetComponent<EatDots>().size >= EatDots.minSize * 2)
            {
                GetComponent<EatDots>().size /= 2;
                playerManager.ourPlayers[Id].Size = GetComponent<EatDots>().size;
                PlayerData newPlayer = new PlayerData(currentSize / 2, currentPosition.x, currentPosition.y, currentPosition.z, manager.clientName, manager.clientColor);
                playerManager.ourPlayers[Id].SetRecombine();
                playerManager.AddPlayer(newPlayer, GetComponent<EatDots>().size, true);
            }
        }

        

        //If we are supposed to glide, then glide
        if (glide)
        {
            Glide();
        }
        else
        {
            //calculate the player's speed
			adjustedSpeed = effectiveSpeed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (GetComponent<EatDots>().size > EatDots.minSize)
                {
                    boost.Play();
                }
            }

            if (Input.GetKey(KeyCode.E)) 
            {
                if(GetComponent<EatDots>().size > EatDots.minSize)
                {
                    boostCost += (0.8f * Time.deltaTime);

                    if (boostCost >= 1)
                    {
                        GetComponent<EatDots>().size -= 1;
                        boostCost -= 1;
                    }


                    adjustedSpeed = adjustedSpeed * 1.3f;
                }
                else
                {
                    boost.Stop();
                }
            }

            if(Input.GetKeyUp(KeyCode.E))
            {
                boost.Stop();
            }

            // Keyboard controls
            if (keyboardEnable==true){
				Vector2 target = transform.position;

                
                int smallestSize = int.MaxValue;

                
                //Figure out which player object is in charge
                if(smallestPlayer == null || Input.GetKey(KeyCode.R))
                {
                    foreach (Player player in playerManager.ourPlayers.Values)
                    {
                        if (player.Size < smallestSize)
                        {
                            smallestSize = player.Size;
                            smallestPlayer = player;
                        }
                    }
                }

                Debug.Log(smallestPlayer.Id);
                //If we are in charge find our new position
                if (Id == smallestPlayer.Id)
                {
                    if (Input.GetKey(KeyCode.D))
                    {
                        target.x += adjustedSpeed;
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        target.x -= adjustedSpeed;
                    }
                    if (Input.GetKey(KeyCode.W))
                    {
                        target.y += adjustedSpeed;
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        target.y -= adjustedSpeed;
                    }
                }
                else
                {
                    //Otherwise follow the object in charge
                    target = new Vector3(smallestPlayer.XPos, smallestPlayer.YPos, smallestPlayer.ZPos);
                }
				
                //Move the player object
                transform.position = Vector2.MoveTowards(transform.position, target, adjustedSpeed);
				playerManager.ourPlayers[Id].UpdateData(netComs.GetTime(), GetComponent<EatDots>().size, transform.position.x, transform.position.y, transform.position.z);
				currentSize = GetComponent<EatDots>().size;
				
			}
			else{
                //If we are using mouse controls, follow the mouse
				FollowMouse();
			}
        }
        CheckBounds();
    }

    public void StartGlide()
    {
        //Causes the player object to start gliding instead of folloing the movement commands
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 offset = mousePosition - transform.position;
        Debug.Log(mousePosition);
        Debug.Log(transform.position);
        Debug.Log(offset);

        //Glide in the designated direction
        glideDirection = new Vector3((offset.x * 1000) + transform.position.x, (offset.y * 1000) + transform.position.y, transform.position.z);
        glideTimer = glideTime;
        glide = true;

        //Make sure we dont collide with ourselves
        gameObject.layer = 9;

        Debug.Log(glideDirection);
    }

    void Glide()
    {
        //Called every frame to make the player glide
        if(glideTimer > 0)
        {
            float adjustedSpeed = glideSpeed * Time.deltaTime * (glideTimer / glideTime);

            //Translate the player object by adjusted speed in the needed direction
            transform.position = Vector2.MoveTowards(transform.position, glideDirection, adjustedSpeed);
            playerManager.ourPlayers[Id].UpdateData(netComs.GetTime(), GetComponent<EatDots>().size, transform.position.x, transform.position.y, transform.position.z);

            currentPosition = transform.position;
            glideTimer -= Time.deltaTime;
        }
        else
        {
            //If we are done gliding, re enable collisions
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
        playerManager.ourPlayers[Id].UpdateData(netComs.GetTime(), GetComponent<EatDots>().size, transform.position.x, transform.position.y, transform.position.z);
        currentSize = GetComponent<EatDots>().size;
    }

    void CheckBounds()
    {
        //Check to make sure the player is in bounds
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
