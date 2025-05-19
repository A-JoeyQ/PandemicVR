using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

internal class PContainSpecialistRemoveWhenEntering : PlayerEvent, IIndirectEvent
{
    private City city;

    private int redCount, yellowCount, blueCount;

    private VirusName cubeRemoved;
    
    float ANIMATIONDURATION = 1f / GameGUI.gui.AnimationTimingMultiplier;

    private int numberOfCubesToTreat;
    private int firstDisabledCubeIndex = 0;
    private int numberOfCubesInCity = 0;

    private bool cubeWasRemoved = false;

    public PContainSpecialistRemoveWhenEntering(City city, int redCount, int yellowCount, int blueCount) : base(Game.theGame.CurrentPlayer)
    {
        this.city = city;
        this.redCount = redCount;
        this.yellowCount = yellowCount;
        this.blueCount = blueCount;
    }

    public override void Do(Timeline timeline)
    {
        
        if (redCount >= 2)
        {
            cubeWasRemoved = true;
            cubeRemoved = VirusName.Red;
            firstDisabledCubeIndex = game.getNumberOfCubesOnBoard((VirusName)cubeRemoved);
            numberOfCubesInCity = city.getNumberOfCubes((VirusName)cubeRemoved);
            city.IncrementNumberOfCubes(VirusName.Red, -1);
            game.IncrementNumberOfCubesOnBoard(VirusName.Red, 1);
            
        }

        if (yellowCount >= 2)
        {
            cubeWasRemoved = true;
            cubeRemoved = VirusName.Yellow;
            firstDisabledCubeIndex = game.getNumberOfCubesOnBoard((VirusName)cubeRemoved);
            numberOfCubesInCity = city.getNumberOfCubes((VirusName)cubeRemoved);
            city.IncrementNumberOfCubes(VirusName.Yellow, -1);
            game.IncrementNumberOfCubesOnBoard(VirusName.Yellow, 1);
            
        }

        if (blueCount >= 2)
        {
            cubeWasRemoved = true;
            cubeRemoved = VirusName.Blue;
            firstDisabledCubeIndex = game.getNumberOfCubesOnBoard((VirusName)cubeRemoved);
            numberOfCubesInCity = city.getNumberOfCubes((VirusName)cubeRemoved);
            city.IncrementNumberOfCubes(VirusName.Blue, -1);
            game.IncrementNumberOfCubesOnBoard(VirusName.Blue, 1);
            
        }
    }

    public override float Act(bool qUndo = false)
    {
        city.Draw();
        if (cubeWasRemoved)
        {
            Sequence sequence = DOTween.Sequence();

            GameObject cube = Object.Instantiate(GameGUI.gui.cubePrefab, GameGUI.gui.AnimationCanvas.transform);
            cube.GetComponent<Cube>().virusInfo = city.city.virusInfo;
            //Based on how many cubes the were, have the cube start from the right position.
            cube.transform.position = city.getCubePosition(cubeRemoved, numberOfCubesInCity - 1);
            cube.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Vector3 positionToMove = GameGUI.gui.getCubePosition(cubeRemoved, firstDisabledCubeIndex).transform.position;
            sequence.Join(cube.transform.DOMove(positionToMove, ANIMATIONDURATION * 2));
            sequence.AppendCallback(() =>
            {
                Object.Destroy(cube);
            });
            sequence.Play().OnComplete(() =>
            {

                _playerGui.Draw();
                gameGUI.DrawBoard();
            });
            return sequence.Duration();
        }
        else
        {
            _playerGui.Draw();
            gameGUI.DrawBoard();
            return 0f;
        }
        

        //Animation for moving cubes from city back to board
        
    }
    
    public override string GetLogInfo()
    {
        return $@" ""city"" : {city.city.cityID},
                     ""cubeRemoved"" : ""{cubeRemoved}""
                ";
    }
}