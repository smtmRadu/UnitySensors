using System.Collections;
using UnityEngine;

namespace DeepUnity
{
    public interface ISensor
    {
        /// <summary>
        /// Returns all sensor's observation in array format.
        /// </summary>
        /// <returns></returns>
        public float[] GetObservationsVector();
        /// <summary>
        /// Returns all sensor's important observations in array format.
        /// </summary>
        /// <returns></returns>
        //public float[] GetCompressedObservationsVector();
    }
    public enum World
    {
        World3d,
        World2d,
    }
    public enum CaptureType
    {
        RGB,
        Grayscale,
    }
    public struct RayInfo
    {
        /// <summary>
        /// Whether or not the ray hit anything.
        /// </summary>
        public bool HasHit;
        /// <summary>
        /// Normalized distance to the hit object.
        /// </summary>
        public float HitFraction;
        /// <summary>
        /// Whether or not the ray hit an object whose tag is in the input's DetectableTags list.
        /// </summary>
        public bool HitTaggedObject;
        /// <summary>
        /// The index of the hit object's tag in the DetectableTags list, or -1 if there was no hit, or the hit object has a different tag.
        /// </summary>
        public int HitTagIndex;
    }
    public struct GridCellInfo
    {
        /// <summary>
        /// Whether or not the box overlaps anything.
        /// </summary>
        public bool HasOverlap;
        /// <summary>
        /// Whether or not the box overlapped an object whose tag is in the input's DetectableTags list.
        /// </summary>
        public bool OverlappedTaggedObject;
        /// <summary>
        /// The index of the overlapped object's tag in the DetectableTags list, or -1 if there was no overlap, or the overlapped object has a different tag.
        /// </summary>
        public int OverlapTagIndex;
    }
}

