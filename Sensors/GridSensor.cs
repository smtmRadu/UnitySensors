using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace DeepUnity
{
    /// <summary>
    /// GridSensor returns the observations of all objects reached by their tags. <br></br>
    /// No hit -> value 0    <br />
    /// Tag 1 hit -> value 1 <br />
    /// Tag 2 hit -> value 2 <br />
    /// ...
    /// </summary>
    [AddComponentMenu("DeepUnity/Grid Sensor")]
    public class GridSensor : MonoBehaviour, ISensor
    {
        private GridCellInfo[,,] Observations;
        [SerializeField, Tooltip("@scene type")] World world = World.World3d;
        [SerializeField, Tooltip("@LayerMask used when casting the rays")] LayerMask layerMask = ~0;
        [SerializeField] string[] detectableTags = new string[1];
        [SerializeField, Range(0.01f, 100f)] float scale = 1f;
        [SerializeField, Range(0.01f, 1f), Tooltip("@cast overlap raio")] float castScale = 0.95f;
        [SerializeField, Range(1, 20f)] int width = 8;
        [SerializeField, Range(1, 20f)] int height = 8;
        [SerializeField, Range(1, 20f)] int depth = 8;

        [Space(10)]
        [SerializeField, Range(-4.5f, 4.5f), Tooltip("@grid X axis offset")] float xOffset = 0;
        [SerializeField, Range(-4.5f, 4.5f), Tooltip("@grid Y axis offset")] float yOffset = 0;
        [SerializeField, Range(-4.5f, 4.5f), Tooltip("@grid Z axis offset\n@not used in 2D world")] float zOffset = 0;

        [Space(10)]
        [SerializeField] Color missColor = Color.gray;
        Color defaultHitColor = Color.black;

        private void Start()
        {
            Observations = new GridCellInfo[depth, height, width];
            CastGrid();
        }
        private void Update()
        {
            CastGrid();
        }
        private void OnDrawGizmos()
        {
            Vector3 origin000 = transform.position + (Vector3.one - new Vector3(width, height, depth)) * scale / 2f + new Vector3(xOffset, yOffset, zOffset) * scale;

            // Compute positions
            for (int d = 0; d < depth; d++)
            {
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        Vector3 position = origin000 + new Vector3(w, h, d) * scale;

                        if (world == World.World3d)
                        {
                            Collider[] hits = Physics.OverlapBox(position, Vector3.one * scale * castScale / 2f, new Quaternion(0, 0, 0, 1), layerMask);
 
                            if (hits.Length > 0)
                            {
                                Renderer rend;
                                hits[0].gameObject.TryGetComponent(out rend);
                                Gizmos.color = rend != null ? rend.sharedMaterial.color : defaultHitColor;
                            }
                            else
                                Gizmos.color = missColor;

                            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.6f);
                            Gizmos.DrawWireCube(position, Vector3.one * scale * castScale);
                            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.2f);
                            Gizmos.DrawCube(position, Vector3.one * scale * castScale);
                            
                           
                        }
                        else if(world == World.World2d)
                        {
                            if (d > 0)
                                return;

                            Collider2D hit = Physics2D.OverlapBox(position, Vector2.one * scale * castScale, 0);
                            bool gotHit = hit != null;

                            if (gotHit)
                            {
                                SpriteRenderer sr;
                                hit.gameObject.TryGetComponent(out sr);
                                Gizmos.color = sr != null ? sr.color : defaultHitColor;
                            }
                            else
                                Gizmos.color = missColor;
                            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.6f);
                            Gizmos.DrawWireCube(new Vector3(position.x, position.y, transform.position.z), Vector3.one * scale * castScale);
                            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.4f);
                            Gizmos.DrawCube(new Vector3(position.x, position.y, transform.position.z), Vector3.one * scale * castScale);
                        }
                       
                    }
                }
            }
           

        }

        public float[] GetObservationsVector()
        {
            int cellDataSize = 2 + detectableTags.Length;
            float[] vector = new float[cellDataSize * Observations.GetLength(0) * Observations.GetLength(1) * Observations.GetLength(2)];
            int index = 0; 
            for (int k = 0; k < depth; k++)
            {
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        GridCellInfo cell = Observations[k, h, w];
                        vector[index++] = cell.HasOverlap ? 1f : 0f;
                        vector[index++] = cell.OverlappedTaggedObject ? 1f : 0f;
                        vector[index++] = cell.OverlapTagIndex;
                    }
                }
            }
            return vector;
        }
        public GridCellInfo[,,] GetObservationsGridCells()
        {
            return Observations.Clone() as GridCellInfo[,,];
        }

        private void CastGrid()
        {
            Vector3 origin000 = transform.position + (Vector3.one - new Vector3(width, height, depth)) * scale / 2f + new Vector3(xOffset, yOffset, zOffset) * scale;

            // Compute positions
            for (int d = 0; d < depth; d++)
            {
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        Vector3 position = origin000 + new Vector3(w, h, d) * scale;
                        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

                        if (world == World.World3d)
                        {
                            Collider[] hits = Physics.OverlapBox(position, Vector3.one * scale * castScale / 2f, new Quaternion(0, 0, 0, 1), layerMask);

                            GridCellInfo cellInfo = new GridCellInfo();
                            cellInfo.HasOverlap = hits.Length > 0;
                            cellInfo.OverlappedTaggedObject = hits.Length > 0 && detectableTags != null? detectableTags.Contains(hits[0].tag) : false;
                            cellInfo.OverlapTagIndex = hits.Length > 0 && detectableTags != null ? Array.IndexOf(detectableTags, hits[0].tag) : -1;
                            Observations[d,h,w] = cellInfo;
                        }
                        else if (world == World.World2d)
                        {
                            if (d > 0)
                                return;

                            Collider2D hit = Physics2D.OverlapBox(position, Vector2.one * scale * castScale, 0);
                            GridCellInfo cellInfo = new GridCellInfo();
                            cellInfo.HasOverlap = hit;
                            cellInfo.OverlappedTaggedObject = hit && detectableTags != null ? detectableTags.Contains(hit.tag) : false;
                            cellInfo.OverlapTagIndex = hit && detectableTags != null ? Array.IndexOf(detectableTags, hit.tag) : -1;
                            Observations[d, h, w] = cellInfo;

                        }

                    }
                }
            }

        }
    }
    [CustomEditor(typeof(GridSensor)), CanEditMultipleObjects]
    class ScriptlessGridSensor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var script = target as GridSensor;

            List<string> _dontDrawMe = new List<string>() { "m_Script" };


            SerializedProperty sr = serializedObject.FindProperty("world");

            if (sr.enumValueIndex == (int)World.World2d)
            {
                _dontDrawMe.Add("deep");
                _dontDrawMe.Add("zOffset");

            }

            SerializedProperty detTags = serializedObject.FindProperty("detectableTags");
            SerializedProperty width = serializedObject.FindProperty("width");
            SerializedProperty height = serializedObject.FindProperty("height");
            SerializedProperty depth = serializedObject.FindProperty("depth");

           
            DrawPropertiesExcluding(serializedObject, _dontDrawMe.ToArray());

            int totalInfo = sr.enumValueIndex == (int)World.World2d ?
              (2 + detTags.arraySize) * width.intValue * height.intValue :
              (2 + detTags.arraySize) * width.intValue * height.intValue * depth.intValue;
            EditorGUILayout.HelpBox($"Observation Vector contains {totalInfo} float values.", MessageType.Info);


            serializedObject.ApplyModifiedProperties();
        }
    }
}

