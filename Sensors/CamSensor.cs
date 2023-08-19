using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

namespace DeepUnity
{
    [AddComponentMenu("DeepUnity/Cam Sensor")]
    public class CamSensor : MonoBehaviour, ISensor
    {
        [SerializeField] private Camera cam;
        [SerializeField, Min(16)]private int width = 640;
        [SerializeField, Min(9)]private int height = 480;
        [SerializeField] private CaptureType type = CaptureType.RGB;

        private void Awake()
        {
            if (cam == null)
                Debug.Log("Please attach a camera to CamSensor");
        }
        /// <summary>
        /// Returns the image texture rendered by the camera.
        /// </summary>
        /// <returns>Texture2D</returns>
        public Texture2D GetObservationTexture()
        {
            if (cam == null)
            {
                Debug.LogError("<color=red>CamSensor Cam not set to an instance of an object.</color>");
                return null;
            }
            cam.targetTexture = new RenderTexture(width, height, 0);


            RenderTexture activeRT = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;

            cam.Render();

            Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
            image.Apply();
            RenderTexture.active = activeRT;

            switch (type)
            {
                case CaptureType.RGB:
                    break;
                case CaptureType.Grayscale:
                    image.SetPixels(image.GetPixels().Select(x => new Color(x.grayscale, x.grayscale, x.grayscale, x.a)).ToArray());
                    break;
            }

            return image;
        }
        public float[] GetObservationsVector()
        {
            Color[] pixels = GetObservationTexture().GetPixels();
            int channels = type == CaptureType.RGB ? 3 : 1;
            float[] vector = new float[pixels.Length * channels];
            int index = 0;
            foreach (var item in pixels)
            {
                if (type == CaptureType.RGB)
                    vector[index++] = item.grayscale;
                else
                {
                    vector[index++] = item.r;
                    vector[index++] = item.g;
                    vector[index++] = item.b;
                }
            }
            return vector;
        }



        public void TakeAShot()
        {
            if (cam == null)
            {
                Debug.LogError("CameraSensor Cam object reference not set to an instance of an object. Please assign a Camera for the first field!");
                return;
            }
            if (cam.targetTexture == null)
                cam.targetTexture = new RenderTexture(width, height, 0);


            if (!Directory.Exists("Assets/CamShots"))
                Directory.CreateDirectory("Assets/CamShots");

            string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets/CamShots" });

            if(guids.Length == 0)
            {
                File.WriteAllBytes("Assets/CamShots/Frame1.png", GetObservationTexture().EncodeToPNG());
            }
            else
            {
                string lastPath = AssetDatabase.GUIDToAssetPath(guids.Last());

                Debug.Log(guids.ToLineSeparatedString());
                // Extract the number from the last path using a regular expression
                Match match = Regex.Match(lastPath, @".*Frame(\d+)");
                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;
                    int newNumber = int.Parse(numberString) + 1;
                    string newPath = lastPath.ToString().Replace(numberString, newNumber.ToString());

                    Debug.Log(newNumber);
                    Debug.Log(newPath);
                    File.WriteAllBytes(newPath, GetObservationTexture().EncodeToPNG());

                }
                else
                {
                    File.WriteAllBytes("Assets/CamShots/Frame1.png", GetObservationTexture().EncodeToPNG());
                }
            }
            

            AssetDatabase.Refresh();
        }  
    }



    #region Editor
    [CustomEditor(typeof(CamSensor)), CanEditMultipleObjects]
    class ScriptlessCameraSensor : Editor
    {
        public static string[] dontInclude = new string[] { "m_Script" };
        public override void OnInspectorGUI()
        {
            CamSensor script = (CamSensor)target;

            SerializedProperty cam = serializedObject.FindProperty("cam");

            if (cam.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Camera not attached to Cam Sensor.", MessageType.Warning);


            DrawPropertiesExcluding(serializedObject, dontInclude);

            SerializedProperty type = serializedObject.FindProperty("type");
            SerializedProperty w = serializedObject.FindProperty("width");
            SerializedProperty h = serializedObject.FindProperty("height");

            int vecDim = type.enumValueIndex == (int)CaptureType.Grayscale ?
                            w.intValue * h.intValue :
                            3 * w.intValue * h.intValue;

            if(cam.objectReferenceValue != null)
                EditorGUILayout.HelpBox($"Observations Vector contains {vecDim} float values.", MessageType.Info);
            else
                EditorGUILayout.HelpBox($"Cannot compute Observations Vector size until attaching a Camera.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Separator();
            if(GUILayout.Button("Take a shot"))
            {
                script.TakeAShot();
            }
        }
    }
    #endregion
}
