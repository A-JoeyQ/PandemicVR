using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Game;
using static GameGUI;

internal class EOutbreak : EngineEvent, IIndirectEvent
{
    private City originOfOutbreak;
    private float ANIMATIONDURATION = 2f / gui.AnimationTimingMultiplier;
    private List<int> infectCities = new List<int>();
    private List<int> quarantineSpecialistExceptions;

    public EOutbreak(City origin)
    {
        this.originOfOutbreak = origin;
    }

    public override void Do(Timeline timeline)
    {
        theGame.SetCurrentGameState(GameState.OUTBREAK); // TODO: needs refactoring, not MVC compliant
        quarantineSpecialistExceptions = new List<int>();
        foreach (Player player in PlayerList.Players)
        {
            if (player.Role == Player.Roles.QuarantineSpecialist)
            {
                quarantineSpecialistExceptions.Add(player.GetCurrentCity());
                quarantineSpecialistExceptions.AddRange(player.GetCurrentCityScript().city.neighbors);
            }
        }

        Debug.Log("Do of EOutbreak, OutbreakTracker before addition:" + string.Join(", ", theGame.OutbreakTracker));
        bool recurrentOutbreak = false;
        
        theGame.OutbreakTracker.Add(originOfOutbreak.city.cityID);
        Timeline.theTimeline.AddEvent(new EIncreaseOutbreak());
        //theGame.OutbreakCounter++;
        
        foreach (int neighbor in originOfOutbreak.city.neighbors)
        {
            City neighborCity = gui.Cities[neighbor].GetComponent<City>();

            if (quarantineSpecialistExceptions.Contains(neighborCity.city.cityID))
                continue;

            if (!theGame.OutbreakTracker.Contains(neighborCity.city.cityID))
            {
                infectCities.Add(neighbor);
                if (neighborCity.IncrementNumberOfCubes(originOfOutbreak.city.virusInfo.virusName,
                        1)) // True when Outbreak happens 
                {
                    Timeline.theTimeline.AddEvent(new EOutbreak(neighborCity));
                    recurrentOutbreak = true;
                }

            }
        }
        
        Debug.Log("Do of EOutbreak, OutbreakTracker after addition:" + string.Join(", ", theGame.OutbreakTracker) +
                  " infectCities = " + string.Join(", ", infectCities));
        
        if(!recurrentOutbreak)
            theGame.actionCompleted = true;
    }

    public override float Act(bool qUndo = false)
    {
        Sequence sequence = DOTween.Sequence();
        
        if (infectCities.Any())
        {
            sequence.Append(originOfOutbreak.transform.DOShakeRotation(ANIMATIONDURATION, new Vector3(0f, 0f, 3f), 10,
                90, false));
            List<GameObject> listCubes = new List<GameObject>();
            foreach (int neighbor in infectCities)
            {
                //City neighborCity = gui.Cities[neighbor].GetComponent<City>();
                if (quarantineSpecialistExceptions.Contains(neighbor))
                    continue;

                GameObject cube = Object.Instantiate(gui.cubePrefab, originOfOutbreak.transform.position,
                    originOfOutbreak.transform.rotation, gui.AnimationCanvas.transform);
                cube.GetComponent<Cube>().virusInfo = originOfOutbreak.city.virusInfo;
                listCubes.Add(cube);
                sequence.Join(cube.transform.DOMove(gui.Cities[neighbor].transform.position, ANIMATIONDURATION));
            }

            sequence.AppendCallback(() =>
            {
                foreach (GameObject cube in listCubes)
                {
                    Object.Destroy(cube);
                }

                foreach (int neighbor in originOfOutbreak.city.neighbors)
                {
                    City neighborCity = gui.Cities[neighbor].GetComponent<City>();
                    neighborCity.Draw();
                }
            });
        }

        return sequence.Duration();
    }

    public override string GetLogInfo()
    {
        string infectCitiesIds = string.Join(", ", infectCities);
        string quarantineSpecialistExceptionIds = string.Join(", ", quarantineSpecialistExceptions);
        return $@" ""originOfOutbreak"" : {originOfOutbreak.city.cityID},
                   ""infectCities"" : [{infectCitiesIds}],
                   ""quarantineSpecialistException"" : [{quarantineSpecialistExceptionIds}]
                ";
    }
}