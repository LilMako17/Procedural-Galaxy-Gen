using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FSM;

public class InputManager : MonoBehaviour
{
    public const string STATE_DISABLED = "disabled";
    public const string STATE_GALAXY_BROWSE = "galaxyBrowse";
    public const string STATE_SOLAR_SYSTEM_BROWSE = "solarSystem";
    public const string STATE_PLANET_INSPECT = "planetInspect";

    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<InputManager>();
            }
            return _instance;
        }
    }

    private static InputManager _instance;
    private Camera _camera;
    private StateMachine _stateMachine;

    private void Awake()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            _camera = GameObject.FindObjectOfType<Camera>();
        }

        _stateMachine = new StateMachine();
        _stateMachine.AddState(STATE_DISABLED, new State());
        _stateMachine.AddState(STATE_GALAXY_BROWSE, new GalaxyBrowseState(_camera));
        _stateMachine.AddState(STATE_SOLAR_SYSTEM_BROWSE, new SolarSystemBrowseState(_camera));
        _stateMachine.SetStartState(STATE_DISABLED);
        _stateMachine.Init();

        SetState(STATE_GALAXY_BROWSE);
    }

    public void SetState(string state)
    {
        _stateMachine.RequestStateChange(state);
    }

    private void Update()
    {
        _stateMachine.OnLogic();
    }
}
