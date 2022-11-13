using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneBehaviour : MonoBehaviour
{
    public GameObject timer;
    public float pauseTime;
    //public float movingTime;
    public GameObject zoneInt;
    public GameObject zoneExt;
    public float shrinkingSpeed;

    private Vector3 currentPos;
    private Vector3 newPos;
    private float currentRadius;
    private float newRadius;
    private float shrinkingRadius;
    private bool newZoneIsNeeded;
    private bool isShrinking;
    private int timeToShrink = 3;
    private float currentTime;
    private bool zoneIsChosen = false;
    //public float pauseTime;

    private float distanceToMoveCenter;

    //public float checkRadius;

    // Start is called before the first frame update
    void Start()
    {
        currentRadius = 225;
        currentTime = Time.time;
        //newRadius = currentRadius;
        //newPos = transform.position;
        shrinkingRadius = currentRadius;
        currentPos = transform.position;
        pauseTime = Random.Range(5, 10);
        timer.GetComponent<Timer>().timeDuration = pauseTime;
        timer.GetComponent<Timer>().ResetTimer();
        //checkRadius = currentRadius;
        //zoneWall.transform.localScale = new Vector3(zoneWall.transform.localScale.x * currentRadius, zoneWall.transform.localScale.y, zoneWall.transform.localScale.z * currentRadius);
    }

    // Update is called once per frame
    void Update()
    {
        zoneInt.transform.localScale = new Vector3(shrinkingRadius * 2, 1, shrinkingRadius * 2);
        zoneExt.transform.localScale = new Vector3(shrinkingRadius * 2, 1, shrinkingRadius * 2);

        zoneExt.transform.position = currentPos;
        zoneInt.transform.position = currentPos;

        if ( Time.time >= (currentTime + pauseTime))
        {
            if (!zoneIsChosen)
            {
                newZoneIsNeeded = true;
                zoneIsChosen = true;
            }
            
        }

        if (newZoneIsNeeded)
        {
            newRadius = currentRadius * Random.Range(0.5f, 0.9f);
            float spawningRadius = currentRadius - newRadius;

            newPos = PickRandomPos(spawningRadius);
            newZoneIsNeeded = false;
            isShrinking = true;
        }

        if (isShrinking)
        {
            Vector3 posRef = currentPos;
            currentPos = Vector3.MoveTowards(currentPos, newPos, shrinkingSpeed * Time.deltaTime);
            shrinkingRadius = Mathf.MoveTowards(shrinkingRadius, newRadius, shrinkingSpeed * Time.deltaTime);
            if (Vector3.Distance(currentPos, newPos) <= 0 && shrinkingRadius <= newRadius)
            {
                zoneIsChosen = false;
                currentRadius = newRadius;
                isShrinking = false;
                currentTime = Time.time;
                pauseTime = Random.Range(5, 10);
                timer.GetComponent<Timer>().timeDuration = pauseTime;
                timer.GetComponent<Timer>().ResetTimer();
                //newZoneIsNeeded = true;
            }


        }
        /*private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(new Vector3 (currentPos.x, 300, currentPos.z), currentRadius - newRadius);
        }*/

        Vector3 PickRandomPos(float radius)
        {
            Vector3 newPos = (Random.insideUnitSphere * radius + new Vector3(currentPos.x, 0, currentPos.z));
            //newRadius = currentRadius * 0.7f;
            return new Vector3(newPos.x, 0, newPos.z);
        }

        /* private float[] ShrinkCircle(float amount)
         {
             float newXR = circle.radii[0] - amount;
             float newYR = circle.radii[1] - amount;
             float[] retVal = new float[2];
             retVal[0] = newXR;
             retVal[1] = newYR;
             return retVal;
         }*/
    }
}
