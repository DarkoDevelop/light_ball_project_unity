using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastLight : MonoBehaviour
{

    public float offsetVariable = 0.001f;   //Offset for rays
    public float helper;
    public Vector2 helpMe;
    public int maxNum;
    public Material materialPointOne;
    public Material materialPointTwo;
    public BoxCollider2D[] gameObjectCollider; //Game objects(2D) with 2dBoxCollider included

    //Lists for positions and helper lists
    #region Lists
    public List<Vector2> fieldToSave;
    public List<Vector2> firstRayCast;
    public List<Vector2> secondRayCast;
    public List<Vector2> currentObjectPosition;
    public List<Vector2> finalList;
    #endregion

    #region triangle variables
    public float[] angles;
    Mesh mesh;
    public Vector3[] vertices;
    public int[] triangles;
    #endregion

    void Start()
    {
        //Init for MeshFilter and MeshRenderer
        #region triangleInitialization
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        #endregion

    }
    void Update()
    {
        //Clearing all lists for new frame
        fieldToSave.Clear();
        firstRayCast.Clear();
        secondRayCast.Clear();
        currentObjectPosition.Clear();
        finalList.Clear();

        for (int i = 0; i < gameObjectCollider.Length; i++)
        {
            //Assign locationCorner variable as Vector3 to GetObjectCorner() function 
            Vector3[] locationCorner = GetObjectCorners(gameObjectCollider[i]);

            //Clearing all lists

            for (int j = 0; j < 4; j++)
            {
                Vector3 myLocation = this.transform.position; //Saving location to vector3

                //Adds shit to location
                currentObjectPosition.Add(myLocation);

                //Making 2 variables of type raycast to store points in world where collision happend 
                RaycastHit2D hit = Physics2D.Raycast(myLocation, new Vector2(locationCorner[j].x - myLocation.x + offsetVariable, locationCorner[j].y - myLocation.y + offsetVariable), 100);
                RaycastHit2D hit2 = Physics2D.Raycast(myLocation, new Vector2(locationCorner[j].x - myLocation.x - offsetVariable, locationCorner[j].y - myLocation.y - offsetVariable), 100);
                Vector2 particleposition = hit.point;
                Vector2 particleposition2 = hit2.point;

                //Adds two positions of rays to list
                firstRayCast.Add(particleposition);
                secondRayCast.Add(particleposition2);

                //Draws 2 spheres on offset+- points in collision point
                DebugDraw.DrawSphere(particleposition, 0.2f, Color.red, materialPointOne);
                DebugDraw.DrawSphere(particleposition2, 0.2f, Color.green, materialPointTwo);
                if (hit.collider != null)
                {
                    //Display the point in world space where the ray hit the collider's surface.
                    Debug.DrawLine(myLocation, hit.point, Color.green);
                    Debug.DrawLine(myLocation, hit2.point, Color.red);
                }
                else
                    Debug.Log("Ray outside the boundaries");
            }
        }

        PopulateField(firstRayCast, secondRayCast); //populating field 
        FirstSortList(fieldToSave);
        FinalSortList(fieldToSave);
        MakeTriangles();
        DrawTriangles(finalList);

    }

    //Finding 2DBoxColliders vertices
    Vector3[] GetObjectCorners(BoxCollider2D obj)
    {
        Vector3[] corners = new Vector3[4];
        corners[0] = obj.transform.position + new Vector3(+obj.size.x * obj.transform.localScale.x / 2, +obj.size.y * obj.transform.localScale.y / 2);
        corners[1] = obj.transform.position + new Vector3(-obj.size.x * obj.transform.localScale.x / 2, +obj.size.y * obj.transform.localScale.y / 2);
        corners[2] = obj.transform.position + new Vector3(+obj.size.x * obj.transform.localScale.x / 2, -obj.size.y * obj.transform.localScale.y / 2);
        corners[3] = obj.transform.position + new Vector3(-obj.size.x * obj.transform.localScale.x / 2, -obj.size.y * obj.transform.localScale.y / 2);
        return corners;
    }

    //Add 2 lists into one called fieldToSave, starting with firstpoint continuing with secondpoint position 
    void PopulateField(List<Vector2> firstRayCast, List<Vector2> secondRayCast)
    {
        for (int i = 0; i < secondRayCast.Count; i++)
        {
            
            fieldToSave.Add(firstRayCast[i]);
            fieldToSave.Add(secondRayCast[i]);
        }
    }

    //Sorting ray points by angle
    void FirstSortList(List<Vector2> fieldToSave) {
        angles = new float[fieldToSave.Count];
        for (int i = 0; i < angles.Length; i++)
        {
            angles[i] = Quaternion.FromToRotation(Vector2.right, fieldToSave[i] - currentObjectPosition[0]).eulerAngles.z;
        }

        //Sorting list finalSortedList according to angles array 
        bool didSwap;
        do
        {
            didSwap = false;
            for (int i = 0; i < angles.Length - 1; i++)
            {
                if (angles[i] > angles[i + 1])
                {
                    helpMe = fieldToSave[i + 1];
                    helper = angles[i + 1];

                    angles[i + 1] = angles[i];
                    fieldToSave[i + 1] = fieldToSave[i];

                    angles[i] = helper;
                    fieldToSave[i] = helpMe;

                    didSwap = true;
                }
            }
        } while (didSwap);
    }

    //Sorting finalSortedList into new one and adding mouse position to finish triangle
    void FinalSortList(List<Vector2> preFinalSortedList)
    {

        for (int i = 0; i < preFinalSortedList.Count - 1; i++)
        {
            finalList.Add(currentObjectPosition[0]);
            finalList.Add(preFinalSortedList[i + 1]);
            finalList.Add(preFinalSortedList[i]);
        }
    }


    //make array of triangles, starting from last points to first
    void MakeTriangles() {
        int triangleNumbers = finalList.Count + 3;
        triangles = new int[triangleNumbers];
        
        for (int i = 0 ; i < finalList.Count ; i++)
        {
            triangles[i] = i;
        }

        //Adding one more triangle starting with mouse position, last point and first point
        triangles[triangleNumbers - 3] = triangles[0];
        triangles[triangleNumbers - 2] = triangles[2];
        triangles[triangleNumbers - 1] = triangles[triangleNumbers - 5];
    }

    //Drawing triangles - saving vertices of each poing of triangle to screen position and applying mesh to them
    void DrawTriangles(List<Vector2> finalSortedList)
    {
        vertices = new Vector3[finalSortedList.Count];
        int cardinal = finalSortedList.Count;
        for (int i = 0; i < cardinal; i++)
        {
            vertices[i] = new Vector3(finalSortedList[i].x - this.transform.position.x, finalSortedList[i].y - this.transform.position.y, 0);

        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}



