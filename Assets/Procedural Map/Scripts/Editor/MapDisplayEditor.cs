using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralMap
{
    [CustomEditor(typeof(MapDisplay))]
    public class MapDisplayEditor : Editor
    {

        MapDisplay display;

        private void OnEnable()
        {
            display = (MapDisplay)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create"))
                display.Create();
        }
    }
}
