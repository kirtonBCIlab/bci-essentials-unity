using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: ADD REQUIREMENT FOR OTHER COMPONENTS ON HERE

//This class contains the functions needed to set-up the P300 matrices for flashing.
public class P300_Setup : MonoBehaviour
{
    //This is the most important game object which needs to be returned
    private GameObject[,] object_matrix;
    private List<GameObject> object_list = new List<GameObject>(); 

    [SerializeField] P300_Controller p300_Controller;
    private GameObject objects; //This name is a left-over from previous iterations. However it works fine for here.
    private int matrixCounter = 0;

    private void Awake()
    {
        p300_Controller = GetComponent<P300_Controller>(); 
    }
    private void Start()
    {
        //Get all of the information from the P300_Controller that is needed here.
        
    }

    /* Configure matrix and display this on the screen */
    public GameObject[,] SetUpMatrix(List<GameObject> objectList)
    {
        /*Reference Matrix and thought process:

            0   1   2
            3   4   5
            6   7   8
        
            C0 = 0, 3, 6
            C1 = 1, 4, 7
            C2 = 2, 5, 8

            R0 = 0, 1, 2
            R1 = 3, 4, 5
            R2 = 6, 7, 8

        */
        //Get variables from the p300 Controller for each of the variables
        int numColumns      = p300_Controller.numColumns;
        int numRows         = p300_Controller.numRows;
        GameObject myObject = p300_Controller.myObject;
        double startX       = p300_Controller.startX;       //Initial position of X for drawing in the objects
        double startY       = p300_Controller.startY;       //Initial position of Y for drawing in the objects
        float startZ        = p300_Controller.startZ;        //Initial position of Z for drawing in the objects
        double distanceX    = p300_Controller.distanceX;    //Distance between objects in X-plane
        double distanceY    = p300_Controller.distanceY;


    //Initial set up
    object_matrix = new GameObject[numColumns, numRows];
        objects = new GameObject { name = "Objects"};

        /* Dynamic Matrix Setup */
        int object_counter = 0;
        for (int y = numRows - 1; y > -1; y--)
        {
            for (int x = 0; x < numColumns; x++)
            {
                //Instantiating prefabs
                GameObject new_obj = Instantiate(myObject);

                //Renaming objects
                new_obj.name = "Object" + object_counter.ToString();
                //new_obj.GetComponent<ActionController>().id = object_counter;

                //Adding to list
                objectList.Add(new_obj);

                //Adding to Parent GameObject
                new_obj.transform.parent = objects.transform;

                //Setting position of object
                new_obj.transform.position = new Vector3((float)((x + startX) * distanceX), (float)((y + startY) * distanceY), startZ);

                //Activating objects
                new_obj.SetActive(true);
                object_counter++;
            }
        }

        //Now update the object list in the primary controller
        p300_Controller.object_list = objectList;

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

        //Setting up object matrix to be used during RC Flashes
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numColumns; x++)
            {
                object_matrix[x, y] = objectList[matrixCounter];
                matrixCounter++;
            }
        }
        
        return object_matrix;
    }

    /* Sets all objects in a list to a desired colour */
    public void Recolour(List<GameObject> object_list, Color color)
    {

        for (int i = 0; i < object_list.Count; i++)
        {
            object_list[i].GetComponent<Renderer>().material.color = color;
        }
    }

    public void ResetMatrixCount()
    {
        matrixCounter = 0;
    }

    public void DestroyMatrix()
    {

        //Destroy Parent Objects
        Destroy(objects);

    }
}
