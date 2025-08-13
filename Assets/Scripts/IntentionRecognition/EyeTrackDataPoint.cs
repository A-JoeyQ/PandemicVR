using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EyeTrackDataPoint
{
    // ▼▼▼ 在这里添加新字段 ▼▼▼
    public string hitObjectName;        // 击中物体的名字
    public string hitObjectTag;         // 击中物体的标签
    public string areaOfInterestLog;    // 从AreaOfInterest脚本中获取的日志信息 (保留原有功能)
    // ▲▲▲ 添加结束 ▲▲▲
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

    /*0813    //Object hit by the ray
        private AreaOfInterest objectHit;
        public string objectHitLog;*/

    // ▼▼▼ 修改整个构造函数 ▼▼▼
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
        GameObject hitObject = null // <--- 参数类型已更改！
    )
    {
        // 1. 基础数据的赋值保持不变
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

        // 2. 新的信息提取逻辑
        if (hitObject != null)
        {
            // 直接记录射线射到的任何物体的名字和标签
            this.hitObjectName = hitObject.name;
            this.hitObjectTag = hitObject.tag;

            // 同时，我们仍然尝试获取AreaOfInterest信息，以实现兼容和更丰富的日志
            AreaOfInterest aoi = hitObject.GetComponentInParent<AreaOfInterest>();
            this.areaOfInterestLog = (aoi != null) ? aoi.GetAoILog() : "N/A"; // 如果有AOI脚本，就记录它的信息；如果没有，记为"N/A"
        }
        else
        {
            // 如果射线没有射到任何东西
            this.hitObjectName = "None";
            this.hitObjectTag = "None";
            this.areaOfInterestLog = "None";
        }
    }
    // ▲▲▲ 构造函数修改结束 ▲▲▲

    /*    public EyeTrackDataPoint(
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
        }*/

    public static string GetHeader(){
        return "dirx,diry,dirz,bx,by,bz,timeStamp,object";
    }

    public string GetLogInfo(){
        
        return JsonUtility.ToJson(this);
    }
}
