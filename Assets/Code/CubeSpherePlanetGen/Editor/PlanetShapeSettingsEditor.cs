using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace CubeSphere
{
    //[CustomEditor(typeof(PlanetShapeSettings))]
    public class PlanetShapeSettingsEditor : OdinEditor
    {
        private void OnValidate()
        {
            UnityEngine.Debug.Log("VALIDATE shape settings");
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            UnityEngine.Debug.Log("SAVe shape settings");
        }
    }
}