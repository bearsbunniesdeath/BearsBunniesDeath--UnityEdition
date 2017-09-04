using UnityEngine;
using System.Collections;
using Completed;

public class CameraFollow : MonoBehaviour
{
    public float xMargin = 2;      // Distance in the x axis the player can move before the camera follows.
    public float yMargin = 2;      // Distance in the y axis the player can move before the camera follows.

    //TODO: To Players speed into account and either zoom out and follow faster
    private float xSmooth = 1;      // How smoothly the camera catches up with it's target movement in the x axis.
    private float ySmooth = 1;      // How smoothly the camera catches up with it's target movement in the y axis.

    private float xySmoothMin = 0.25f; 
    private float xySmoothMax = 1.25f;

    private float marginToBorder = 2;

    public Vector2 maxXAndY;        // The maximum x and y coordinates the camera can have.
    public Vector2 minXAndY;        // The minimum x and y coordinates the camera can have.


    private Transform playerTrans;       // Reference to the player's transform.
    private PlayerBehaviour playerScript;

    void Awake()
    {
        // Setting up the reference.
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }


    bool CheckXMargin()
    {
        // Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
        return Mathf.Abs(transform.position.x - playerTrans.position.x) > xMargin;
    }


    bool CheckYMargin()
    {
        // Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
        return Mathf.Abs(transform.position.y - playerTrans.position.y) > yMargin;
    }


    void Update()
    {
        TrackPlayer();
    }


    void TrackPlayer()
    {

        bool dimLights = false;

        // By default the target x and y coordinates of the camera are it's current x and y coordinates.
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        float diffFromMargin;
        
        // If the player has moved beyond the x margin...
        if (CheckXMargin())
        {            // ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
            diffFromMargin = Mathf.Abs(transform.position.x - playerTrans.position.x) - xMargin;
            xSmooth = (diffFromMargin / marginToBorder * (xySmoothMax - xySmoothMin)) + xySmoothMin;
            targetX = Mathf.Lerp(transform.position.x, playerTrans.position.x, xSmooth * Time.deltaTime);
        }

        // If the player has moved beyond the y margin...
        
        if (CheckYMargin()) {
            // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
            diffFromMargin = Mathf.Abs(transform.position.y - playerTrans.position.y) - yMargin;
            ySmooth = (diffFromMargin / marginToBorder * (xySmoothMax - xySmoothMin)) + xySmoothMin;
            targetY = Mathf.Lerp(transform.position.y, playerTrans.position.y, ySmooth * Time.deltaTime);
        }

        // The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
        //targetX = Mathf.Clamp(targetX, minXAndY.x, maxXAndY.x);
        //targetY = Mathf.Clamp(targetY, minXAndY.y, maxXAndY.y);

        if (!playerScript.IsInThickGrass())
        {
            //Get a darkness factor based on player distace from camera from 0 - 2 units
            float maxDistance = 2f;
            float darknessFactor = System.Math.Max(maxDistance - GetDistanceIgnoringZCoord(transform.position, playerTrans.position), 0) / maxDistance;
            float maxBrightness = 0.1f;
            Color skyColor = new Color(darknessFactor * maxBrightness, darknessFactor * maxBrightness, darknessFactor * maxBrightness, 1F);
            RenderSettings.ambientSkyColor = skyColor;

            // Set the camera's position to the target position with the same z component.
            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }
        else
        {
            RenderSettings.ambientSkyColor = Color.black;
        }

    }

    private float GetDistanceIgnoringZCoord(Vector3 pos1, Vector3 pos2) {
        Vector2 asVect2_1 = new Vector2(pos1.x, pos1.y);
        Vector2 asVect2_2 = new Vector2(pos2.x, pos2.y);

        return (asVect2_1 - asVect2_2).magnitude;
    }

}
