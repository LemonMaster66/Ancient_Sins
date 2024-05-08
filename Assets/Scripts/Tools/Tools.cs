using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering;

namespace PalexUtilities
{
    public static class Tools
    {


        #region Text
            

            public static string Remove(this string s, string toRemove)
            {
                if (toRemove == "") return s;
                return s.Replace(toRemove, "");
            }


        #endregion



        #region Math
            

            public static bool IsOdd(this int i) => i % 2 == 1;
            public static bool IsEven(this int i) => i % 2 == 0;


        #endregion



        #region Textures
            

            public static Texture2D CreateTexture2D(int width, int height, GraphicsFormat graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB, bool useMips = false)
            {
                return new Texture2D(width, height, graphicsFormat, useMips ? TextureCreationFlags.MipChain : TextureCreationFlags.None);
            }

            public static void ReadPixelsFrom(this Texture2D texture2D, RenderTexture renderTexture)
            {
                var prevActive = RenderTexture.active;

                RenderTexture.active = renderTexture;

                texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

                RenderTexture.active = prevActive;

            }

            public static Texture2D ToTexture2D(this RenderTexture rt)
            {
                var texture2D = CreateTexture2D(rt.width, rt.height, rt.graphicsFormat, rt.useMipMap);

                texture2D.ReadPixelsFrom(rt);
                texture2D.Apply();

                return texture2D;
            }

            //public static void SavePNG(this Texture2D texture2d, string path) => File.WriteAllBytes(path, texture2d.EncodeToPNG());


        #endregion



        #region Transform
            

            public static List<Transform> GetChildren(this Transform transform)
            {
                var list = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                    list.Add(transform.GetChild(i));

                return list;
            }

            public static Transform DestroyChildren(this Transform transform)
            {
                while (transform.childCount > 0)
                    if (Application.isPlaying)
                        Object.Destroy(transform.GetChild(0).gameObject);
                    else
                        Object.DestroyImmediate(transform.GetChild(0).gameObject);

                return transform;
            }


        #endregion



        #region Navmesh
            

            public static float CalculatePathDistance(Vector3 startPos, Vector3 endPos, NavMeshAgent agent)
            {
                NavMeshPath path = new NavMeshPath();
                float distance = 0;

                if(NavMesh.CalculatePath(startPos, endPos, agent.areaMask, path))
                {
                    for(int i = 1; i < path.corners.Length; i++)
                    {
                        distance += Vector3.Distance(path.corners[i-1], path.corners[i]);
                    }
                }
                return distance;
            }


        #endregion



        #region Logic
            

            public static bool FrustumCheck(Collider collider, Camera cam)   // True if its in the Cameras Bounds
            {
                Bounds bounds = collider.bounds;
                Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

                return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
            }
            
            public static bool OcclusionCheck(this Transform transform, Transform target) // True if its Occluded
            {
                if(Physics.Raycast(transform.position, target.position-transform.position, out RaycastHit hit, 100000, Physics.AllLayers))
                {
                    return hit.transform.tag != "Player";
                }
                return true;
            }
            public static bool OcclusionCheck(this Transform transform, Transform target, LayerMask layerMask = default) // True if its Occluded
            {
                if(Physics.Raycast(transform.position, target.position-transform.position, out RaycastHit hit, 100000, layerMask))
                {
                    return hit.transform.tag != "Player";
                }
                return true;
            }
            public static bool OcclusionCheck(Transform[] transforms, Transform target, LayerMask layerMask) // True if all Points are Occluded
            {
                foreach(Transform Point in transforms)
                {
                    if(Physics.Raycast(Point.position, target.position-Point.position, out RaycastHit hit, 100000, layerMask))
                    {
                        if(hit.transform.tag == "Player") return false;
                    }
                }
                return true;
            }

            
        #endregion


        
        #region Bonus Functions
            

            public static void DrawThickRay(Vector3 start, Vector3 dir, Color color, float duration, float Thickness, int Iterations = 200)
            {
                for(int i = 0; i < 200; i++)
                {
                    start.x += UnityEngine.Random.Range(Thickness, -Thickness);
                    start.y += UnityEngine.Random.Range(Thickness, -Thickness);
                    start.z += UnityEngine.Random.Range(Thickness, -Thickness);
                    Debug.DrawRay(start, dir, color, duration);
                }
            }


        #endregion
    }
}
