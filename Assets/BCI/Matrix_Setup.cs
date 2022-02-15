using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written by Brian Irvine

// This function sets up a matrix of BCI objects
// All objects are given the tag "BCI" so that they can be collected by the controller script

//This class contains the functions needed to set-up the P300 matrices for flashing.
public class Matrix_Setup : MonoBehaviour
{
    public int numColumns;
    public int numRows;
    public GameObject myObject;
    private double startX;       //Initial position of X for drawing in the objects
    private double startY;       //Initial position of Y for drawing in the objects
    private float startZ;        //Initial position of Z for drawing in the objects
    public double distanceX;    //Distance between objects in X-plane
    public double distanceY;
    private List<GameObject> objectList = new List<GameObject>();
    private GameObject new_obj;
    //private GameObject objects; //This name is a left-over from previous iterations. However it works fine for here.

    // Setup the matrix
    // void SetUpMatrix(List<GameObject> objectList)
    public void SetUpMatrix()
    {
        //Initial set up
        //object_matrix = new GameObject[numColumns, numRows];
        //objects = new GameObject { name = "Objects" };

        /* Dynamic Matrix Setup */
        int object_counter = 0;
        for (int y = numRows - 1; y > -1; y--)
        {
            for (int x = 0; x < numColumns; x++)
            {
                //Instantiating prefabs
                new_obj = Instantiate(myObject);

                //Renaming objects
                new_obj.name = "Object" + object_counter.ToString();

                //Give the new object the BCI tag
                new_obj.tag = "BCI";

                //turn off
                new_obj.GetComponent<SPO>().TurnOff();

                //Adding to list
                objectList.Add(new_obj);

                //Adding to Parent GameObject
                //new_obj.transform.parent = objects.transform;

                //Setting position of object
                new_obj.transform.position = new Vector3((float)((x + startX) * distanceX), (float)((y + startY) * distanceY), startZ);

                //Activating objects
                new_obj.SetActive(true);
                object_counter++;
            }
        }

        //Position Camera to the centre of the objects
        float cameraX = (float)((((objectList[numColumns - 1].transform.position.x) - (objectList[0].transform.position.x)) / 2) + (startX * 2));
        float cameraY = (float)((((objectList[0].transform.position.y) - (objectList[object_counter - 1].transform.position.y)) / 2) + (startY * 2));
        float cameraSize;
        if (numRows > numColumns)
        {
            cameraSize = numRows;
        }
        else
        {
            cameraSize = numColumns;
        }


        GameObject.Find("Main Camera").transform.position = new Vector3(cameraX, cameraY, -10f + startZ);
        GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize = cameraSize;
        print("Camera Position: X: " + (cameraX) + " Y: " + (cameraY) + " Z: " + -10f);
    }

    //Destroy the matrix
    public void DestroyMatrix()
    {
        //Destroy Parent Objects
        int objectCount = objectList.Count;
        for (int i = 0; i < objectCount; i++)
        {
            Destroy(objectList[i]);
        }

    }




}