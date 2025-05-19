using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class EyeTracker : MonoBehaviour
{
    [SerializeField] private float maxDistance = 10.0f;
    [SerializeField] private GameObject eyeHitPrefab;
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] bool UseSmoothingForTracking = false;
    [SerializeField] private bool showGazePoint = false;

    [SerializeField] private bool printFixations = false;

    public Canvas boardCanvas;

    //Editor title
    [Header("Eye Track Logger")]
    [SerializeField] private string folderName = "EyeTrackData";
    //Tooltip for editor
    [Tooltip("If left empty, a timestamp will be used")]
    [SerializeField] private string fileName = null;

    private EyeTrackingLogger eyeTrackingLogger;

    [Header("Smoothing")]

    [Tooltip("Number of gaze points to use for smoothing. Needs to be an odd number for Median.")]
    [SerializeField] private int smoothingWindow = 5;
    [SerializeField] private SmoothingMode smoothingMode = SmoothingMode.Mean;
    private GameObject visualizerPOG;


    private FixationLogger fixationLogger;
    public Vector3 lastGazePoint;


    // Fixation tracking
    private AreaOfInterest currentAOI = null;
    private float firstHitTime = 0.0f;
    private List<Tuple<string, float>> fixations = new List<Tuple<string, float>>();

    // Smoothing test for eye tracking
    private Queue<Vector3> gazePoints;

    public static EyeTracker instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        visualizerPOG = Instantiate(eyeHitPrefab, Vector3.zero, Quaternion.identity);
        visualizerPOG.SetActive(false);

        eyeTrackingLogger = GetComponent<EyeTrackingLogger>();

        fixationLogger = new FixationLogger();
    }
    private void CombinedTracking() {
        if(showGazePoint)
            visualizerPOG.SetActive(true);
        Vector3 leftEyeDirection = leftEye.TransformDirection(Vector3.forward);
        Vector3 rightEyeDirection = rightEye.TransformDirection(Vector3.forward);
        Vector3 combinedDirection = (leftEyeDirection + rightEyeDirection) / 2;
        Vector3 normalizedDirection = combinedDirection.normalized;
        Vector3 smoothDirection = UseSmoothingForTracking ? GetSmoothedGazePoint(combinedDirection) : combinedDirection; //Where should this smoothing realistically be?
        Vector3 combinedPosition = (leftEye.position + rightEye.position) / 2;

        RaycastHit combinedHit;
        AreaOfInterest aoiHit = null;
        Vector3 boardPosition = new Vector3(0, 0, 0);
        Vector2 relativeBoardPosition = new Vector2(0, 0);
        Vector3 worldPosition = new Vector3(0, 0, 0);
        if (Physics.Raycast(combinedPosition, smoothDirection, out combinedHit, maxDistance, layerMask)) {
            // Visualize the hit point
            if(showGazePoint){
                visualizerPOG.SetActive(true);
                visualizerPOG.transform.position = combinedHit.point;
            }
                

            GameObject hitObject = combinedHit.collider.gameObject;
            if (hitObject != null) {
                boardPosition = ObjectPos2BoardPos(combinedHit.point);
                relativeBoardPosition = BoardPos2RelativeBoardPos(boardPosition);

                AreaOfInterest aoi = hitObject.GetComponentInParent<AreaOfInterest>();

                float hitTime = Time.time - MainMenu.startTimestamp; // To align with the timestamp in the event logger
                TrackFixations(aoi, hitTime);
                if (aoi == null) {
                    Debug.Log("No AOI found for object: " + hitObject.name);
                }
                aoiHit = aoi;
                lastGazePoint = combinedHit.point;


            }
            else {
                //No object hit, still need to update fixation
                TrackFixations(null, Time.time - MainMenu.startTimestamp);
            }
            worldPosition = combinedHit.point;
        } else {
            visualizerPOG.SetActive(false);

            //No object hit, still need to update fixation
            TrackFixations(null, Time.time - MainMenu.startTimestamp);
        }

        EyeTrackDataPoint gazePoint = new EyeTrackDataPoint(
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0),
            leftEyeDirection,
            rightEyeDirection,
            combinedPosition,
            combinedDirection,
            boardPosition,
            relativeBoardPosition,
            worldPosition,
            Time.time - MainMenu.startTimestamp,
            aoiHit
        );

        eyeTrackingLogger.Save(gazePoint);

    }

    private Vector3 GetSmoothedGazePoint(Vector3 newGazePoint) {
        if (smoothingMode == SmoothingMode.None) {
            return newGazePoint;
        }
        if (gazePoints == null) {
            gazePoints = new Queue<Vector3>();
        }
        if (gazePoints.Count >= smoothingWindow) {
            gazePoints.Dequeue();
        }
        gazePoints.Enqueue(newGazePoint);


        switch (smoothingMode) {
            case SmoothingMode.Mean:
                return GetMeanGazePoint();
            case SmoothingMode.Median:
                return GetMedianGazePoint();
            default:
                return newGazePoint;
        }
    }

    private Vector3 GetMeanGazePoint() {
        Vector3 smoothedGazePoint = new Vector3(0, 0, 0);
        foreach (Vector3 gazePoint in gazePoints) {
            smoothedGazePoint += gazePoint;
        }
        smoothedGazePoint /= gazePoints.Count;
        return smoothedGazePoint;
    }
    private Vector3 GetMedianGazePoint() {
        // Extract median
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        List<float> z = new List<float>();
        foreach (Vector3 gazePoint in gazePoints) {
            x.Add(gazePoint.x);
            y.Add(gazePoint.y);
            z.Add(gazePoint.z);
        }
        x.Sort();
        y.Sort();
        z.Sort();
        Vector3 smoothedGazePoint = new Vector3(x[x.Count / 2], y[y.Count / 2], z[z.Count / 2]);
        return smoothedGazePoint;
    }

    private void TrackFixations(AreaOfInterest aoi, float hitTime) {
        if (currentAOI == null) {
            currentAOI = aoi;
            firstHitTime = hitTime;
        } else if ((aoi == null || currentAOI != aoi) && !IsSameCubeTypeInCity(aoi)) {
            float fixationTimeMilli = (hitTime - firstHitTime) * 1000;
            fixations.Add(new Tuple<string, float>(currentAOI.GetAoILog(), fixationTimeMilli));

            if (fixationTimeMilli > 10) { //Small filtering here just to remove to most irrelevant fixations
                fixationLogger.LogFixation(new Fixation(fixationTimeMilli, firstHitTime, hitTime, currentAOI));
                if (printFixations)
                    Debug.Log("Fixation: " + currentAOI.GetAoILog() + " for " + fixationTimeMilli + "ms");
            }
            currentAOI = aoi;
            firstHitTime = hitTime;


        }
    }
    private bool IsSameCubeTypeInCity(AreaOfInterest aoi)
    {
        if (currentAOI == null || aoi == null)
        {
            return false;
        }
        Cube cube = aoi.GetComponent<Cube>();
        Cube currentCube = currentAOI.GetComponent<Cube>();
        if (cube == null ||  currentCube == null)
        { return false; }
        if(cube.cityCard == null)
        {
            return false; //Don't care about the connects of the unused ones on the board
        }
        if(currentCube.cityCard == null)
        {
            return false;
        }
        if(cube.cityCard.cityName == currentCube.cityCard.cityName && cube.virusInfo.virusName == currentCube.virusInfo.virusName)
        {
            return true;
        }
        return false;
    }


    private Vector3 ObjectPos2BoardPos(Vector3 objectPosition){
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(objectPosition);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardCanvas.GetComponent<RectTransform>(),
            screenPosition,
            boardCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out localPoint
        );
        return localPoint;
    }

    private Vector2 BoardPos2RelativeBoardPos(Vector3 boardPosition){
        //Converts the board position to a relative position on the board (0,0) to (1,1), the board position is 0,0 in the center of the board
        Vector2 relativeBoardPosition = new Vector2(
            (boardPosition.x + boardCanvas.GetComponent<RectTransform>().rect.width / 2) / boardCanvas.GetComponent<RectTransform>().rect.width,
            (boardPosition.y + boardCanvas.GetComponent<RectTransform>().rect.height / 2) / boardCanvas.GetComponent<RectTransform>().rect.height
        );
        return relativeBoardPosition;
    }


    void Update()
    {
        if(boardCanvas.gameObject.activeSelf){
            visualizerPOG.SetActive(false);
            CombinedTracking();
        }
    }


    public enum SmoothingMode
    {
        None,
        Median,
        Mean
    }
}
