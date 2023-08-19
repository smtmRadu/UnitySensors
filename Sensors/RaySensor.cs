using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine.UIElements;
using System.Linq;
using Unity.VisualScripting;

namespace DeepUnity
{
    [AddComponentMenu("DeepUnity/Ray Sensor")]
    public class RaySensor : MonoBehaviour, ISensor
    {
        private readonly LinkedList<RayInfo> Observations = new LinkedList<RayInfo>();

        [SerializeField, Tooltip("@scene type")] World world = World.World3d;
        [SerializeField, Tooltip("@LayerMask used when casting the rays")] LayerMask layerMask = ~0;
        [SerializeField, Tooltip("@tags that can provide information")] string[] detectableTags = new string[1];
        [SerializeField, Range(1, 50), Tooltip("@size of the buffer equals the number of rays")] int rays = 5;
        [SerializeField, Range(1, 360)] int fieldOfView = 45;
        [SerializeField, Range(0, 359)] int rotationOffset = 0;
        [SerializeField, Range(1, 1000), Tooltip("@maximum length of the rays")] int distance = 100;
        [SerializeField, Range(0.01f, 10)] float sphereCastRadius = 0.5f;

        [Space(10)]
        [SerializeField, Range(-5, 5), Tooltip("@ray X axis offset")] float xOffset = 0;
        [SerializeField, Range(-5, 5), Tooltip("@ray Y axis offset")] float yOffset = 0;
        [SerializeField, Range(-5, 5), Tooltip("@ray Z axis offset\n@not used in 2D world")] float zOffset = 0;
        [SerializeField, Range(-45, 45), Tooltip("@ray vertical tilt\n@not used in 2D world")] float tilt = 0;

        [Space(10)]
        [SerializeField] Color rayColor = Color.green;
        [SerializeField] Color missRayColor = Color.red;


        private void Start()
        {
            CastRays();
        }
        private void Update()
        {
            CastRays();
        }
        private void OnDrawGizmos()
        {
            float oneAngle = rays == 1 ? 0 : -fieldOfView / (rays - 1f);

            float begin = -oneAngle * (rays - 1f) / 2f + rotationOffset;
            Vector3 startAngle;

            if (world == World.World3d)
            {
                Quaternion rotationToTheLeft = Quaternion.AngleAxis(begin, transform.up);
                Vector3 rotatedForward = rotationToTheLeft * transform.forward;
                Vector3 rotationAxis = Vector3.Cross(rotatedForward, transform.up).normalized;
                Quaternion secondaryRotation = Quaternion.AngleAxis(tilt, rotationAxis);
                startAngle = secondaryRotation * rotatedForward;
            }
            else //world2d
                startAngle = Quaternion.AngleAxis(begin, transform.forward) * transform.up;

            Vector3 castOrigin = transform.position + (transform.right * xOffset + transform.up * yOffset + transform.forward * zOffset) * transform.lossyScale.magnitude;

            float currentAngle = 0;

            for (int r = 0; r < rays; r++)
            {
                Vector3 rayDirection;
                if (world == World.World3d) //3d
                {
                    rayDirection = Quaternion.AngleAxis(currentAngle, transform.up) * startAngle;

                    RaycastHit hit;
                    bool isHit = Physics.SphereCast(castOrigin, sphereCastRadius, rayDirection, out hit, distance, layerMask);
                    
                    if (isHit)
                    {
                        Gizmos.color = rayColor;
                        Gizmos.DrawRay(castOrigin, rayDirection * hit.distance);
                        Gizmos.DrawWireSphere(castOrigin + rayDirection * hit.distance, sphereCastRadius);
                    }
                    else
                    {
                        Gizmos.color = missRayColor;
                        Gizmos.DrawRay(castOrigin, rayDirection * distance);
                    }
                }
                else //2d
                {
                    rayDirection = Quaternion.AngleAxis(currentAngle, transform.forward) * startAngle;
                    
                    RaycastHit2D hit2D = Physics2D.CircleCast(castOrigin, sphereCastRadius, rayDirection, distance, layerMask);
                    if (hit2D)
                    {
                        Gizmos.color = rayColor;
                        Gizmos.DrawRay(castOrigin, rayDirection * hit2D.distance);
                        Gizmos.DrawWireSphere(castOrigin + rayDirection * hit2D.distance, sphereCastRadius);
                    }
                    else
                    {
                        Gizmos.color = missRayColor;
                        Gizmos.DrawRay(castOrigin, rayDirection * distance);
                    }
                }

                currentAngle += oneAngle;
            }


        }

        /// <summary>
        /// Returns information of all rays.
        /// </summary>
        /// <returns></returns>
        public RayInfo[] GetObservationRays()
        {
            return Observations.ToArray();
        }
        public float[] GetObservationsVector()
        {
            int rayInfoDim = 3 + detectableTags.Length;
            float[] vector = new float[rays * rayInfoDim];
            int index = 0;
            foreach (var rayInfo in Observations)
            {
                vector[index++] = rayInfo.HasHit ? 1f : 0f;
                vector[index++] = rayInfo.HitFraction;
                vector[index++] = rayInfo.HitTaggedObject ? 1f : 0f;
                vector[index++] = rayInfo.HitTagIndex;
            }
            return vector;
        }
       
       

        /// <summary>
        /// This methods casts the necessary rays.
        /// </summary>
        private void CastRays()
        {
            Observations.Clear();
            float oneAngle = rays == 1 ? 0 : -fieldOfView / (rays - 1f);

            float begin = -oneAngle * (rays - 1f) / 2f + rotationOffset;
            Vector3 startAngle;

            if (world == World.World3d)
            {
                Quaternion rotationToTheLeft = Quaternion.AngleAxis(begin, transform.up);
                Vector3 rotatedForward = rotationToTheLeft * transform.forward;
                Vector3 rotationAxis = Vector3.Cross(rotatedForward, transform.up).normalized;
                Quaternion secondaryRotation = Quaternion.AngleAxis(tilt, rotationAxis);
                startAngle = secondaryRotation * rotatedForward;
            }
            else //world2d
                startAngle = Quaternion.AngleAxis(begin, transform.forward) * transform.up;


            Vector3 castOrigin = transform.position + (transform.right * xOffset + transform.up * yOffset + transform.forward * zOffset) * transform.lossyScale.magnitude;

            float currentAngle = 0;

            for (int r = 0; r < rays; r++)
            {
                
                if (world == World.World3d)
                {
                    Vector3 rayDirection = Quaternion.AngleAxis(currentAngle, transform.up) * startAngle;
                    CastRay3D(castOrigin, sphereCastRadius, rayDirection, distance, layerMask);
                }
                else
                {
                    Vector3 rayDirection = Quaternion.AngleAxis(currentAngle, transform.forward) * startAngle;
                    CastRay2D(castOrigin, sphereCastRadius, rayDirection, distance, layerMask);
                }
              
                currentAngle += oneAngle;
            }
        }
        /// <summary>
        /// This method casts only rays for 3D worlds. It is called from CastRays().
        /// </summary>
        private void CastRay3D(Vector3 castOrigin, float sphereCastRadius, Vector3 rayDirection, float distance, LayerMask layerMask)
        {
            RaycastHit hit;
            bool success = Physics.SphereCast(castOrigin, sphereCastRadius, rayDirection, out hit, distance, layerMask);
            
            RayInfo rayInfo = new RayInfo();
            rayInfo.HasHit = success;
            rayInfo.HitFraction = success ? hit.distance / distance : 0f;
            rayInfo.HitTaggedObject = success && detectableTags != null? detectableTags.Contains(hit.collider.tag) : false;
            rayInfo.HitTagIndex = success && detectableTags != null ? Array.IndexOf(detectableTags, hit.collider.tag) : -1;
            Observations.AddLast(rayInfo);
        }
        /// <summary>
        /// This method casts only rays for 2D worlds. It is called from CastRays().
        /// </summary>
        private void CastRay2D(Vector3 castOrigin, float sphereCastRadius, Vector3 rayDirection, float distance, LayerMask layerMask)
        {
            RaycastHit2D hit = Physics2D.CircleCast(castOrigin, sphereCastRadius, rayDirection, distance, layerMask);

            RayInfo rayInfo = new RayInfo();
            rayInfo.HasHit = hit;
            rayInfo.HitFraction = hit ? hit.distance / distance : 0f;
            rayInfo.HitTaggedObject = hit && detectableTags != null ? detectableTags.Contains(hit.collider.tag) : false;
            rayInfo.HitTagIndex = hit && detectableTags != null ? Array.IndexOf(detectableTags, hit.collider.tag) : -1;
            Observations.AddLast(rayInfo);
        }   
    }

    
  
   

    [CustomEditor(typeof(RaySensor)), CanEditMultipleObjects]
    class ScriptlessRaySensor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var script = target as RaySensor;

            List<string> _dontDrawMe = new List<string>() { "m_Script" };


            SerializedProperty sr = serializedObject.FindProperty("world");

            if (sr.enumValueIndex == (int)World.World2d)
            {
                _dontDrawMe.Add("tilt");
                _dontDrawMe.Add("zOffset");

            }



            DrawPropertiesExcluding(serializedObject, _dontDrawMe.ToArray());


            SerializedProperty detectableTags = serializedObject.FindProperty("detectableTags");
            int totalInfoSize = 1 + 1 + 1 + detectableTags.arraySize;
            EditorGUILayout.HelpBox($"Observation Vector contains {totalInfoSize} float values.", MessageType.Info);


            serializedObject.ApplyModifiedProperties();
        }
    }
   
}