using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CubeSphere
{
    //[CustomEditor(typeof(Planet))]
    public class PlanetEditor : Editor
    {
        private Planet _planet;

        void OnEnable()
        {
            _planet = target as Planet;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawSettingsEditor(_planet.ColorSettings);
            DrawSettingsEditor(_planet.ShapeSettings);
            if (GUILayout.Button("Reset"))
            {
                _planet.Cleanup();
                _planet.GeneratePlanet();
            }
        }

        void DrawSettingsEditor(UnityEngine.Object settings)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                //EditorGUILayout.InspectorTitlebar(true, settings);
                var editor = CreateEditor(settings);
                editor.OnInspectorGUI();

                if (check.changed)
                {
                    _planet.GeneratePlanet();
                }
            }
        }
    }
}
