using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiveRabbitsDemo
{
    public class DemoCamera : MonoBehaviour
    {
        [SerializeField]
        private Transform m_rabbit;

        private float m_distance = 1f;
        private float m_height = 1f;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.position = m_rabbit.position + (Vector3.up * m_height - Vector3.forward * m_distance);

            transform.LookAt(m_rabbit);

        }
    }
}