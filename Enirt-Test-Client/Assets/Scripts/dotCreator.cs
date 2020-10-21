using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotCreator : MonoBehaviour
{
    public GameObject orb;
    public GameObject player;
    public float spawnDelay;
    float elapsedTime = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        if(elapsedTime > spawnDelay)
        {
            float xRand = Random.value;
            float yRand = Random.value;

            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            float xSize = playerMovement.rightBound - playerMovement.leftBound;
            float ySize = playerMovement.topBound - playerMovement.bottomBound;

            float xPos = xRand * xSize - (0.5f * xSize);
            float yPos = yRand * ySize - (0.5f * ySize);

            Instantiate(orb, new Vector3(xPos, yPos, 0.0f), Quaternion.identity);

            elapsedTime -= spawnDelay;
        }
    }
}
