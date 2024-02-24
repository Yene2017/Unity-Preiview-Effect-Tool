#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Tools.PreviewModule
{
    [ExecuteAlways]
    public class PreviewSceneCameraHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            cam = GetComponent<Camera>();
        }

        public void Update()
        {
            if (isManualUpdate) return;
            if (!target) return;
            if (!isSmart)
            {
                ControlCamera(target.GetComponentsInChildren<Renderer>(true));
                return;
            }

            _CacheList.Clear();
            foreach (var r in target.GetComponentsInChildren<Renderer>(true))
            {
                if (!(r is ParticleSystemRenderer))
                {
                    _CacheList.Add(r);
                    continue;
                }
                if (!r.TryGetComponent<ParticleSystem>(out var ps)) continue;
                if (!ps.emission.enabled) continue;
                _CacheList.Add(r);
            }
            ControlCamera(_CacheList.ToArray());
        }

        public void ControlCamera(Renderer[] rs)
        {
            if (rs == null || rs.Length < 1) return;
            if (!cam) return;
            switch (viewMode)
            {
                case ViewMode.Min:
                    bounds = GetMinBounds(rs);
                    break;
                case ViewMode.Max:
                    bounds = GetMaxBounds(rs);
                    break;
                case ViewMode.Avg:
                    bounds = GetAvgBounds(rs);
                    break;
                case ViewMode.Smart:
                    bounds = GetBounds(rs);
                    break;
            }

            cam.fieldOfView = 40;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 400f;

            var aim = bounds.center;
            var radius = Mathf.Max(bounds.extents.magnitude, cam.nearClipPlane);
            var distance = radius / Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2);
            var curPos = bounds.center + new Vector3(0, 0, distance);
            var curRot = Quaternion.Euler(0, 180, 0);
            cam.transform.SetPositionAndRotation(curPos, curRot);
            cam.transform.RotateAround(aim, Vector3.up, 30);
            cam.transform.RotateAround(aim, cam.transform.right, 30);
        }

        private Bounds GetBounds(Renderer[] rs)
        {
            var pivot = new Vector3();
            var size = 0f;
            var count = 0;
            foreach (var r in rs)
            {
                if (!r.enabled) continue;
                count++;
            }
            foreach (var r in rs)
            {
                if (!r.enabled) continue;
                pivot += r.bounds.center / count;
                size += Mathf.Pow(r.bounds.size.magnitude, 1f / 3) / count;
            }
            var b = new Bounds(pivot, size * size * size * Vector3.one);
            return b;
        }

        private Bounds GetAvgBounds(Renderer[] rs)
        {
            var min = GetMinBounds(rs);
            var max = GetMaxBounds(rs);
            var p = 0.7f;
            max.center = min.center * p + max.center * (1 - p);
            max.size = min.size * p + max.size * (1 - p);
            return max;
        }

        private Bounds GetMaxBounds(Renderer[] rs)
        {
            var b = new Bounds();
            foreach(var r in rs)
            {
                if (!r.enabled) continue;
                if (r.bounds.size.magnitude > b.size.magnitude)
                {
                    b = r.bounds;
                }
            }
            return b;
        }

        private Bounds GetMinBounds(Renderer[] rs)
        {
            var b = new Bounds();
            foreach (var r in rs)
            {
                if (!r.enabled) continue;
                if (r.bounds.size.magnitude < b.size.magnitude)
                {
                    b = r.bounds;
                }
            }
            return b;
        }

        public bool isManualUpdate;
        public bool isSmart;
        public GameObject target;
        public Camera cam;
        public Bounds bounds;

        private List<Renderer> _CacheList = new List<Renderer>();

        public static ViewMode viewMode = ViewMode.Smart;


        public enum ViewMode
        {
            Smart,Avg,Min,Max
        }
    }
}
#endif
