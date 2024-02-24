using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools.PreviewModule
{
    public class PreviewHandle
    {
        static Dictionary<Type, Type> markPreviewMap = new Dictionary<Type, Type>();

        static PreviewHandle()
        {
            foreach (var t in TypeCache.GetTypesWithAttribute<MarkPreviewAttribute>())
            {
                var atts = t.GetCustomAttributes(typeof(MarkPreviewAttribute), false).Cast<MarkPreviewAttribute>();
                foreach(var att in atts)
                {
                    markPreviewMap.Add(att.type, t);
                }
            }
        }

        public PreviewHandle()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private void Update()
        {
            if (_Controllers == null) return;
            foreach (var c in _Controllers)
            {
                c.Sample();
            }
        }

        public GameObject selectEffect;
        public bool isPlaying;

        public bool isPaused;
        public double stepTime = 0.1f;
        public PreviewController[] _Controllers;

        public void ChangeHandleEffect(GameObject newEf)
        {
            Clear();
            selectEffect = newEf;
            if (!newEf) return;
            var targetCompList = new List<Component>();
            foreach (var comp in selectEffect.GetComponentsInChildren<Component>(true))
            {
                if (!markPreviewMap.ContainsKey(comp.GetType())) continue;
                targetCompList.Add(comp);
            }
            _Controllers = new PreviewController[targetCompList.Count];
            var index = 0;
            foreach (var comp in targetCompList)
            {
                _Controllers[index] = Activator.CreateInstance(markPreviewMap[comp.GetType()]) as PreviewController;
                _Controllers[index].SetTarget(comp);
                index++;
            }
        }

        public void Play()
        {
            if (_Controllers == null) return;
            isPlaying = true;
            isPaused = false;
            var ef = selectEffect;
            foreach (var c in _Controllers)
            {
                c.Play();
            }
        }

        public void Pause()
        {
            if (_Controllers == null) return;
            isPlaying = false;
            isPaused = true;
            var ef = selectEffect;
            foreach (var c in _Controllers)
            {
                c.Pause();
            }
        }

        public void Stop()
        {
            if (_Controllers == null) return;
            isPlaying = false;
            isPaused = false;
            foreach (var c in _Controllers)
            {
                c.Stop();
            }
        }

        public void UpdatePlayRange(float range)
        {
            if (!selectEffect) return;
            foreach (var c in _Controllers)
            {
                c.playRange = range;
            }
        }

        public void Clear()
        {
            Stop();
            _Controllers = null;
        }

        public static implicit operator bool(PreviewHandle handle)
        {
            return handle != null;
        }

        public void Step()
        {
            if (_Controllers == null) return;
            foreach (var c in _Controllers)
            {
                c.Step(stepTime);
            }
        }

        public void Strides()
        {
            if (_Controllers == null) return;
            foreach (var c in _Controllers)
            {
                c.Step(stepTime);
                c.Step(stepTime);
                c.Step(stepTime);
            }
        }
    }

}
