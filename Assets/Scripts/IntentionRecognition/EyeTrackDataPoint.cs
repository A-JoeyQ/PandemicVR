using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EyeTrackDataPoint
{

    public Vector3 leftEyeDirection;
    public Vector3 rightEyeDirection;
    //Position of center between eyes
    public Vector3 centerEyePosition;
    //Normalized direction vector of center between eyes
    public Vector3 centerEyeDirection;

    //(x,y,z) position of the point on the board
    public Vector3 boardHitPosition;
    public Vector2 relativeBoardHitPosition;
    public Vector3 worldPosition;
    public Vector3 headPosition;
    public Vector3 headRotation;

    //Timestamp of the point
    public float timeStamp;

    //Object hit by the ray
    private AreaOfInterest objectHit;
    public string objectHitLog;



    public EyeTrackDataPoint(
        Vector3 headPosition,
        Vector3 headRotation,
        Vector3 leftEyeDirection,
        Vector3 rightEyeDirection,
        Vector3 centerEyePosition,
        Vector3 centerEyeDirection,
        Vector3 boardHitPosition,
        Vector2 relativeBoardHitPosition,
        Vector3 worldPosition,
        float timeStamp,
        AreaOfInterest objectHit = null
    )
    {
       
        this.headPosition = headPosition;
        this.headRotation = headRotation;
        this.leftEyeDirection = leftEyeDirection;
        this.rightEyeDirection = rightEyeDirection;
        this.centerEyePosition = centerEyePosition;
        this.centerEyeDirection = centerEyeDirection;
        this.boardHitPosition = boardHitPosition;
        this.relativeBoardHitPosition = relativeBoardHitPosition;
        this.worldPosition = worldPosition;
        this.timeStamp = timeStamp;
        this.objectHit = objectHit;
        this.objectHitLog = objectHit == null ? "None" : objectHit.GetAoILog();
    }

    public static string GetHeader(){
        return "dirx,diry,dirz,bx,by,bz,timeStamp,object";
    }

    public string GetLogInfo(){
        
        return JsonUtility.ToJson(this);
    }
}
