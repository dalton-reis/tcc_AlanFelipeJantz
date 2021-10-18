using System;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public GameObject PointSphere;
    public GameObject LevelInfo;

    private readonly List<GameObject> _pointSpheres = new List<GameObject>();
    private int _expectedSpheres { get; set; }
    private int _secondsInterval = 5;
    private Level _currentLevel { get; set; }
    private bool _userPaused { get; set; }
    private Game CurrentGame => GameManager.CurrentGame;
    private int CurrentStage => GameManager.CurrentLevel?.Stage ?? 0;
    private const string defaultCurrentLevelMessage = "Total de pontos";
    private const string loadingMessage = "Carregando...";

    private static GameHandler _instance;
    public static GameHandler GetInstance() => _instance;
    public static DateTime GameStartTime { get; private set; }
    public bool HasController { get; set; }

    private void Start()
    {
        _instance = this;
        GameStartTime = DateTime.Now;
        GameManager.CreateNewGame(GameStartTime);
        ReportManager.CreateGameReport(CurrentGame.Levels.Count);
    }

    private void Update()
    {
        if (HasController)
        {
            if (!_userPaused && CurrentGame.Paused)
                Resume();

            string currentLevel = defaultCurrentLevelMessage;

            if (GameManager.Active)
            {
                ControlLevels();

                if (_pointSpheres.Count == 0)
                    currentLevel = loadingMessage;
                else
                    currentLevel = $"N�vel {CurrentStage} ({_expectedSpheres - _pointSpheres.Count}/{_expectedSpheres})";
            }
            else
            {
                SaveGameReport();
            }

            LevelInfo.SetTextMeshProValue(currentLevel, "Label");
            LevelInfo.SetTextMeshProValue(CurrentGame.Score.ToString());
        }
        else
        {
            _userPaused = false;
            if (!CurrentGame.Paused)
                Pause();

            ShowNoControllerConnected();
        }
    }

    private void OnGiveUp()
    {
        EndGame();
    }

    private void OnPause()
    {
        _userPaused = true;
        Pause();
    }

    private void OnResume()
    {
        _userPaused = false;
        Resume();
    }

    private void ControlLevels()
    {
        if (!CurrentGame.Paused && _pointSpheres.Count == 0)
        {
            ReportManager.EndLevel(DateTime.Now);

            if (_currentLevel == null)
            {
                if (CurrentGame.Levels.Count == 0)
                {
                    GameManager.EndCurrentGame();
                    return;
                }

                _currentLevel = CurrentGame.NextLevel();
                if (!_currentLevel.StartTime.HasValue)
                    _currentLevel.SetStartTime(DateTime.Now.AddSeconds(_secondsInterval));

                ReportManager.AddLevel(_currentLevel);
            }

            if (_currentLevel.StartTime.GetValueOrDefault() <= DateTime.Now)
            {
                for (int i = 0; i < _currentLevel.Positions.Count; i++)
                {
                    var point = Instantiate(PointSphere, _currentLevel.Positions[i], Quaternion.identity);
                    point.name = $"Point Sphere {i + 1}";
                    _pointSpheres.Add(point);
                }
                _expectedSpheres = _pointSpheres.Count;
                _currentLevel = null;
            }
        }
    }

    public void RemovePoint(GameObject point)
    {
        ReportManager.AddPoint(Platform.CurrentPlatformHeight, point.transform.position.y.GetHeightValue());
        SoundHandler.GetInstance().PlaySound(Sounds.Point);
        _pointSpheres.Remove(point);
        Destroy(point);
    }

    public void EndGame()
    {
        if (GameManager.Active)
        {
            GameManager.EndCurrentGame();

            foreach (var point in _pointSpheres)
                Destroy(point);

            ReportManager.EndLevel(DateTime.Now);
            Platform.GetInstance().StopPlatform();
        }
    }

    public void SaveGameReport()
    {
        if (!CurrentGame.ReportCreated)
        {
            GameManager.CreateGameReportFile();
        }
    }

    private void Pause()
    {
        CurrentGame.Pause();
    }

    public void Resume()
    {
        CurrentGame.Resume();
    }

    public void ShowControls()
    {
        CityUIManager.GetInstance().Show(CityUIMenuStates.Controllers);
    }

    private void ShowNoControllerConnected()
    {
        CityUIManager.GetInstance().Show(CityUIMenuStates.NoControllerConnected);
    }

    public void ReturnToMainScreen()
    {
        EndGame();
        LoaderManager.Load(GameScene.Main);
    }
}
