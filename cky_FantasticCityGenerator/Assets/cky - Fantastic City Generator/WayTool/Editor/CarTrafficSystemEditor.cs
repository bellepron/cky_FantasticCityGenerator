using UnityEngine;
using UnityEditor;

namespace cky.TrafficSystem
{
    [CustomEditor(typeof(TrafficSystem))]
    public class CarTrafficSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TrafficSystem myScript = (TrafficSystem)target;

            // Normal ÷zellikleri šizdirin
            DrawDefaultInspector();

            GUILayout.Space(30);
            if (GUILayout.Button("Create"))
            {
                DestroyImmediate(GameObject.Find("CarContainer"));

                myScript.LoadCars();
            }
        }
    }
}
