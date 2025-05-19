using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ENUMS;
using static GameGUI;
using static Game;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System;
using DG.Tweening;

public class City : MonoBehaviour
{

    //Multiplied with 0.0336 to account for my rescaled canvas, removed 0.05f from left value on cubes! for 0.0005 scale we use 0.028f 
    public static readonly float[][] offsetCubesRed = new float[][]
    {
        new float[] { 0.5f*0.0336f, +0.4f * 0.0336f, -0.0f*0.0336f  },
        new float[] { 0.5f*0.0336f, +0.4f * 0.0336f, -0.30f*0.0336f },
        new float[] { 0.5f*0.0336f, +0.4f * 0.0336f, -0.60f*0.0336f }
    };

    public static readonly float[][] offsetCubesYellow = new float[][]
    {
        new float[] { 0f*0.0336f, -0.1f*0.0336f, -0.0f*0.0336f },
        new float[] { 0f*0.0336f, -0.1f*0.0336f, -0.30f*0.0336f},
        new float[] { 0f*0.0336f, -0.1f*0.0336f, -0.60f*0.0336f }
    };
    public static readonly float[][] offsetCubesBlue = new float[][]
    {
        new float[] { -0.5f*0.0336f, +0.4f*0.0336f, -0.0f*0.0336f },
        new float[] { -0.5f*0.0336f, +0.40f*0.0336f, -0.30f*0.0336f },
        new float[] { -0.5f*0.0336f, +0.40f*0.0336f, -0.60f*0.0336f }
    };

    public static readonly float[][] offsetPawns = new float[][]
    {
        new float[] { -0.1f*0.0336f, 0.55f*0.0336f },
        new float[] { 0.1f*0.0336f, 0.55f*0.0336f },
        new float[] { 0.3f*0.0336f, 0.45f*0.0336f },
        new float[] { -0.3f*0.0336f, 0.45f*0.0336f }
    };

    public CityCard city;

    public Transform floatingName;

    private int numberOfInfectionCubesRed = 0;
    private int numberOfInfectionCubesYellow = 0;
    private int numberOfInfectionCubesBlue = 0;
    public GameObject CubesGameObject;
    public GameObject PawnsGameObject;

    public List<Player> PlayersInCity = new List<Player>();
    public Pawn[] PawnsInCity;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private Dictionary<VirusName, GameObject> grabCubes;


    void Start()
    {
        PawnsInCity = new Pawn[4];
        rectTransform = GetComponent<RectTransform>();
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        /*grabCubes = new Dictionary<VirusName, GameObject>()
        {
            {VirusName.Red, InstatiateGrabCube(offsetCubesRed, gui.VirusInfos[0])},
            {VirusName.Yellow, InstatiateGrabCube(offsetCubesYellow, gui.VirusInfos[1])},
            {VirusName.Blue,InstatiateGrabCube(offsetCubesBlue, gui.VirusInfos[2]) }
        };*/

    }


    public int getNumberOfCubes(VirusName virusName)
    {
        switch (virusName)
        {
            case VirusName.Red:
                return numberOfInfectionCubesRed;
            case VirusName.Yellow:
                return numberOfInfectionCubesYellow;
            case VirusName.Blue:
                return numberOfInfectionCubesBlue;
        }
        return 0;
    }

    public void ResetCubesOfColor(VirusName virusName)
    {
        switch (virusName)
        {
            case VirusName.Red:
                resetRedCubes();
                break;
            case VirusName.Blue:
                resetBlueCubes();
                break;
            case VirusName.Yellow:
                resetYellowCubes();
                break;
        }
    }
    
    private void resetRedCubes()
    {
        numberOfInfectionCubesRed = 0;
    }

    private void resetYellowCubes()
    {
        numberOfInfectionCubesYellow = 0;
    }

    private void resetBlueCubes()
    {
        numberOfInfectionCubesBlue = 0;
    }

    public bool IncrementNumberOfCubes(VirusName virusName, int increment)
    {
        switch (virusName)
        {
            case VirusName.Red:
                numberOfInfectionCubesRed += increment;
                if (numberOfInfectionCubesRed > 3)
                {
                    numberOfInfectionCubesRed = 3;
                    return true;
                }
                break;
            case VirusName.Yellow:
                numberOfInfectionCubesYellow += increment;
                if (numberOfInfectionCubesYellow > 3)
                {
                    numberOfInfectionCubesYellow = 3;
                    return true;
                }
                break;
            case VirusName.Blue:
                numberOfInfectionCubesBlue += increment;
                if (numberOfInfectionCubesBlue > 3)
                {
                    numberOfInfectionCubesBlue = 3;
                    return true;
                }
                break;
        }
        return false;
    }

    public void AddPawn(Player player)
    {
        if(!PlayersInCity.Contains(player))
            PlayersInCity.Add(player);
    }

    public void RemovePawn(Player player)
    {
        //remove a player and then sort the list so that null values are not first
        PlayersInCity.Remove(player);
    }

    public void Draw()
    {
        Pawn[] Pawns = new Pawn[4];
        Vector3 worldPoint = new Vector3(0, 0, 0);
        CubesGameObject.DestroyChildrenImmediate();
        PawnsGameObject.DestroyChildrenImmediate();

        DrawCubes();

        if (PlayersInCity.Count > 0)
        {
            for (int i = 0; i < PlayersInCity.Count; i++)
            {
                int playerPosition = PlayersInCity[i].Position;
                GameObject pawn = Instantiate(gui.PawnPrefab, PawnsGameObject.transform.position, PawnsGameObject.transform.rotation, PawnsGameObject.transform);
                pawn.transform.Translate(offsetPawns[i][0], offsetPawns[i][1], 0);
                PawnsInCity[playerPosition] = pawn.GetComponent<Pawn>();
                PawnsInCity[playerPosition].SetRoleAndPlayer(PlayersInCity[i]);
            }
        }
        //GameObject currentPawn = gui.Pawns[(int)PlayerRole];
        //currentPawn.SetActive(true);
        //Image currentPawnImage = currentPawn.GetComponent<Image>();
        //City initialCity = game.Cities[game.InitialCityID];
        //Vector3 pawnPosition = initialCity.getPawnPosition(PlayerRole);
    }

    private void DrawCubes()
    {
        InstantiateCubes(numberOfInfectionCubesRed, offsetCubesRed, gui.VirusInfos[0]);
        InstantiateCubes(numberOfInfectionCubesYellow, offsetCubesYellow, gui.VirusInfos[1]);
        InstantiateCubes(numberOfInfectionCubesBlue, offsetCubesBlue, gui.VirusInfos[2]);
    }

    public Vector3 getCubePosition(VirusName virusName, int cubeNumber)
    {
        if(cubeNumber >= offsetCubesRed.Length || cubeNumber < 0)
        {
            //Fallback
            return CubesGameObject.transform.position;
        }
        switch (virusName)
        {
            case VirusName.Red:
                return CubesGameObject.transform.position + new Vector3(offsetCubesRed[cubeNumber][0], offsetCubesRed[cubeNumber][1], offsetCubesRed[cubeNumber][2]);
            case VirusName.Yellow:
                return CubesGameObject.transform.position + new Vector3(offsetCubesYellow[cubeNumber][0], offsetCubesYellow[cubeNumber][1], offsetCubesYellow[cubeNumber][2]);
            case VirusName.Blue:
                return CubesGameObject.transform.position + new Vector3(offsetCubesBlue[cubeNumber][0], offsetCubesBlue[cubeNumber][1], offsetCubesBlue[cubeNumber][2]);
        }
        return CubesGameObject.transform.position;
    }
    private void InstantiateCubes(int numberOfCubes, float[][] offsets, VirusInfo info)
    {
        //grabCubes[info.virusName].SetActive(false);
       
        for (int i = 0; i < numberOfCubes; i++)
        {
            GameObject cube = Instantiate(gui.cubePrefab, CubesGameObject.transform);
            cube.transform.Translate(offsets[i][0], offsets[i][1], offsets[i][2]);
            cube.GetComponent<Cube>().virusInfo = info;
            cube.GetComponent<Cube>().cityCard = city; //Niklas Added.
            cube.GetComponentInChildren<InteractableUnityEventWrapper>().WhenSelect.AddListener(() => cubeClicked(info.virusName));
            //cube.GetComponentInChildren<InteractableUnityEventWrapper>().WhenUnselect.AddListener(() => cubeClicked(info.virusName));
            //cube.GetComponentInChildren<Button>().onClick.AddListener(() => cubeClicked(info.virusName));
            /*if(i == numberOfCubes - 1)
            {
                grabCubes[info.virusName].SetActive(true);
                grabCubes[info.virusName].transform.position = CubesGameObject.transform.position;
                grabCubes[info.virusName].transform.Translate(offsets[i][0], offsets[i][1], offsets[i][2]);
            }*/
        }
        
    }
    private GameObject InstatiateGrabCube(float[][] offsets, VirusInfo info)
    {
        GameObject cube = Instantiate(gui.cubeGrabBoxPrefab, transform);
        cube.transform.position = CubesGameObject.transform.position;
        cube.transform.Translate(offsets[0][0], offsets[0][1], offsets[0][2]);
        cube.GetComponentInChildren<InteractableUnityEventWrapper>().WhenSelect.AddListener(() => cubeClicked(info.virusName));
        cube.SetActive(false);
        //cube.GetComponentInChildren<InteractableUnityEventWrapper>().WhenUnselect.AddListener(() => cubeClicked(info.virusName));
        return cube;
    }

    public void handleCubeClicked(VirusName virusName)
    {
        //grabCubes[virusName].SetActive(false); //Prevent spamming it
        theGame.CubeClicked(this, virusName);
    }
    private void cubeClicked(VirusName virusName)
    {
        if (!NetworkPlayer.LocalInstance.blockTreating)
        {
            NetworkPlayer.LocalInstance.blockTreating = true;
            NetworkPlayer.LocalInstance.RPC_CubeClicked(virusName, city.cityID);
        }
        else
        {
            Debug.Log("Cube clicked Blocked!");
        }
        
    }

    public void HandleClicked()
    {
        GameGUI.CurrentPlayerPad().CityClicked(this);
    }
    public void Clicked()
    {
        NetworkPlayer.LocalInstance.RPC_CityClicked(city.cityID);
    }

    public bool CubesInCity()
    {
        if (numberOfInfectionCubesRed > 0 || numberOfInfectionCubesYellow > 0 || numberOfInfectionCubesBlue > 0)
        {
            return true;
        }
        return false;
    }

    public VirusName? FirstVirusFoundInCity()
    {
        if (numberOfInfectionCubesRed > 0)
            return VirusName.Red;
        if (numberOfInfectionCubesYellow > 0)
            return VirusName.Yellow;
        if (numberOfInfectionCubesBlue > 0)
            return VirusName.Blue;
        return null;
    }

    public void Update()
    {
        if(floatingName != null)
        {
            floatingName.gameObject.SetActive(false);
            float distance = Vector3.Distance(EyeTracker.instance.lastGazePoint, floatingName.position);
            if (distance < 0.1f)
            {
                floatingName.gameObject.SetActive(true);
                Vector3 lookAt = AnimationTemplates.CalculateLookTarget(gui.CenterEyeAnchor.position, floatingName.position);
                floatingName.LookAt(lookAt);
            }
            
        }
    }
}
