using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiveRabbitsDemo
{
    public class MaterialsChange : MonoBehaviour
    {

        [SerializeField]
        private Renderer m_rabbitRenderer;
        [SerializeField]
        private Material[] m_materials = new Material[5];

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnGUI()
        {
            GUI.BeginGroup(new Rect(Screen.width - 100, 0, 100, 1000));

            for (int i = 0; i < m_materials.Length; i++)
            {
                if (GUILayout.Button("Rabbit " + (i + 1), GUILayout.Width(100)))
                {
                    m_rabbitRenderer.material = m_materials[i];
                }
            }

            GUI.EndGroup();
        }
    }
}