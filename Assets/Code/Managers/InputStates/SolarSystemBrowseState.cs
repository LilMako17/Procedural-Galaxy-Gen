using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;
using UnityEngine;

public class SolarSystemBrowseState : GalaxyBrowseState
{
    protected override bool _forceRotationSnapBack => false;

    public SolarSystemBrowseState(Camera camera) : base(camera)
    {
    }

    protected override void UpdateObjectInspect()
    {
        // do nothing
    }
}

