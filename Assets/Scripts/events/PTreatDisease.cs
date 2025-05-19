
using Unity.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static ENUMS;
using System.Collections.Generic;
using DG.Tweening;

public class PTreatDisease : PlayerEvent, IInitializableEvent
{
    
    /*public int cityId { get; set; }
    public string virusName { get; set; }*/
    
    [JsonIgnore]
    private City city;
    [JsonIgnore]
    private VirusName virusNameEnum;
    [JsonIgnore]
    private bool defaultClick = true;

    private int numberOfCubesToTreat = 0;
    private int firstDisabledCubeIndex = 0;
    private int numberOfCubesInCity = 0;

    public PTreatDisease(Player player) : base(player) {}
    
    public PTreatDisease(City city, VirusName virusNameEnum): base(Game.theGame.CurrentPlayer)
    {
        this.city = city;
        this.virusNameEnum = virusNameEnum;
    }

    public PTreatDisease(City city) : base(Game.theGame.CurrentPlayer)
    {
        this.city = city;
        virusNameEnum = city.city.virusInfo.virusName;
        defaultClick = false;
    }

    public override void Do(Timeline timeline)
    {
        VirusName? virus = city.FirstVirusFoundInCity();

        numberOfCubesToTreat = 0;
        if (defaultClick)
            virus = virusNameEnum;
        
        if (virus != null)
        {
            if ((game.RedCure && virus == VirusName.Red)
                || (game.BlueCure && virus == VirusName.Blue) 
                || (game.YellowCure && virus == VirusName.Yellow))
            {
                firstDisabledCubeIndex = game.getNumberOfCubesOnBoard((VirusName)virus);
                numberOfCubesInCity = city.getNumberOfCubes((VirusName)virus);
                game.IncrementNumberOfCubesOnBoard((VirusName) virus, city.getNumberOfCubes((VirusName) virus));
                numberOfCubesToTreat = city.getNumberOfCubes((VirusName)virus);
                city.ResetCubesOfColor((VirusName)virus);
                //Debug.Log("A cure has been found!");
            }
            else
            {
                numberOfCubesToTreat = 1;
                numberOfCubesInCity = city.getNumberOfCubes((VirusName)virus);
                firstDisabledCubeIndex = game.getNumberOfCubesOnBoard((VirusName)virus);
                city.IncrementNumberOfCubes((VirusName)virus, -1);
                game.IncrementNumberOfCubesOnBoard((VirusName)virus, 1); 
            }
        }
        _player.DecreaseActionsRemaining(1);
    }

    public override float Act(bool qUndo = false)
    {
        city.Draw();

        //Animation for moving cubes from city back to board
        List<GameObject> cubes = new List<GameObject>();
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < numberOfCubesToTreat; i++)
        {
            GameObject cube = Object.Instantiate(GameGUI.gui.cubePrefab, GameGUI.gui.AnimationCanvas.transform);
            cube.GetComponent<Cube>().virusInfo = city.city.virusInfo;
            //Based on how many cubes the were, have the cube start from the right position.
            cube.transform.position = city.getCubePosition(virusNameEnum, numberOfCubesInCity-1- i);
            cube.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            cubes.Add(cube);
            Vector3 positionToMove = GameGUI.gui.getCubePosition(virusNameEnum, firstDisabledCubeIndex + i).transform.position;
            sequence.Join(cubes[i].transform.DOMove(positionToMove, ANIMATIONDURATION * 2));
            if (i == numberOfCubesToTreat - 1)
                sequence.AppendCallback(() =>
                {
                    foreach (GameObject cube in cubes)
                        Object.Destroy(cube);
                });
        }
        sequence.Play().OnComplete(() =>
        {
            
            _playerGui.Draw();
            gameGUI.DrawBoard();
            NetworkPlayer.LocalInstance.blockTreating = false;
        });
        return sequence.Duration();
    }

    public override string GetLogInfo()
    {
        return $@" ""city"" : {city.city.cityID},
                    ""virusName"" : ""{virusNameEnum}""
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["city"] is JValue jsonCity)
        {
            city = Game.theGame.GetCityById(jsonCity.Value<int>());
        }

        if (jsonData["virusName"] is JValue jsonVirusName)
        {
            if (System.Enum.TryParse(jsonVirusName.Value<string>(), out VirusName parsedVirusName))
            {
                virusNameEnum = parsedVirusName;
            }
            else
            {
                Debug.LogError($"Invalid VirusName: {jsonVirusName}");
            }
        }
    }
    
}