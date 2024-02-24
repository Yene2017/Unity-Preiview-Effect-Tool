using UnityEngine;

namespace Tools.PreviewModule
{
    [MarkPreview(typeof(ParticleSystem))]
    public class PreviewParticleSystem : PreviewController
    {
        public ParticleSystem ps;

        internal override void SetTarget(Component comp)
        {
            ps = comp as ParticleSystem;
        }

        protected override void UpdatePreview()
        {
            if (!ps) return;
            if (deltaTime > 0)
            {
                ps.Simulate((float)deltaTime, false, false);
            }
            else
            {
                ps.Simulate((float)current, false, true);
            }
        }

        protected override void OnPlay()
        {
            if (!ps) return;
            ps.Simulate(0.01f, false, true);
        }

        protected override void OnPause()
        {
            if (!ps) return;
            ps.Pause(false);
        }

        protected override void OnStop()
        {
            if (!ps) return;
            ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public override bool EnablePreview()
        {
            return true;
        }
    }

}