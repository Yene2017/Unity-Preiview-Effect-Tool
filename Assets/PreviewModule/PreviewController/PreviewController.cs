using UnityEditor;
using UnityEngine;


namespace Tools.PreviewModule
{
    public abstract class PreviewController
    {
        public double startTime;
        public double current;
        public double lastTime;
        public double deltaTime;
        public bool isPlaying;
        public bool isPaused;
        public float playRange;

        public bool isStoped
        {
            get
            {
                return !isPaused && !isPlaying;
            }
        }

        internal abstract void SetTarget(Component comp);
        protected abstract void OnPlay();
        protected abstract void OnPause();
        protected abstract void OnStop();
        protected abstract void UpdatePreview();

        public virtual bool EnablePreview()
        {
            return true;
        }

        public void Play()
        {
            startTime = EditorApplication.timeSinceStartup;
            lastTime = EditorApplication.timeSinceStartup;
            isPlaying = true;
            isPaused = false;
            OnPlay();
        }

        public void Pause()
        {
            isPlaying = false;
            isPaused = true;
            OnPause();
        }

        public void Stop()
        {
            isPlaying = false;
            isPaused = false;
            OnStop();
        }

        public void Step(double interval)
        {
            if (EnablePreview())
            {
                var time = EditorApplication.timeSinceStartup;
                startTime += time - current - interval;
                current = time - startTime;
                deltaTime = interval;
                lastTime = current - interval;
                UpdatePreview();
                lastTime = time;
            }
        }

        public void Sample()
        {
            var time = EditorApplication.timeSinceStartup;
            if (isPlaying && EnablePreview())
            {
                deltaTime = time - lastTime;
                current = time - startTime;
                UpdatePreview();
                lastTime = time;
                if (playRange > 1 && current > playRange)
                {
                    Stop();
                    Play();
                }
            }
            else
            {
                startTime = time;
            }
        }
    }
}