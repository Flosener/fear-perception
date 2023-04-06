using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiveRabbitsDemo
{
    public class AnimatorParamatersChange : MonoBehaviour
    {

        private string[] m_buttonNames = new string[] { "Idle", "Run", "Eat", "Damage", "Dead" };

        private Animator m_animator;

        // Use this for initialization
        void Start()
        {

            m_animator = GetComponent<Animator>();

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnGUI()
        {
            GUI.BeginGroup(new Rect(0, 0, 150, 1000));

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150));
            GUILayout.Label("Run Magnitude");
            var magnitude = m_animator.GetFloat("RunMagnitude");
            magnitude = GUILayout.HorizontalSlider(magnitude, -1, 1, GUILayout.Width(140));
            m_animator.SetFloat("RunMagnitude", magnitude);
            GUILayout.EndVertical();

            if (GUILayout.Button("Reset Magnitude", GUILayout.Width(150))) m_animator.SetFloat("RunMagnitude", 0f);

            GUILayout.Space(20);

            for (int i = 0; i < m_buttonNames.Length; i++)
            {
                if (GUILayout.Button(m_buttonNames[i], GUILayout.Width(150)))
                {
                    m_animator.SetInteger("AnimIndex", i);
                    m_animator.SetTrigger("Next");
                }
            }

            GUI.EndGroup();
        }
    }
}
