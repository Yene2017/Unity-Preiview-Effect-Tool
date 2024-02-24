using UnityEngine;


namespace Tools.PreviewModule
{
    [MarkPreview(typeof(Animator))]
    public class PreviewAnimator : PreviewController
    {
        public Animator target;
        public AnimationClip clip;

        internal override void SetTarget(Component comp)
        {
            target = comp as Animator;
        }

        protected override void UpdatePreview()
        {
            if (clip)
            {
                var current = this.current - startTime;
                clip.SampleAnimation(target.gameObject, (float)current);
            }
        }

        protected override void OnPlay()
        {
            if (target)
            {
                clip = target.GetCurrentAnimatorClipInfo(0)[0].clip;
            }
            if (clip)
            {
                clip.SampleAnimation(target.gameObject, 0);
            }
        }

        protected override void OnPause()
        {
        }

        protected override void OnStop()
        {
            if (clip)
            {
                clip.SampleAnimation(target.gameObject, clip.length);
            }
            clip = null;
        }

        public override bool EnablePreview()
        {
            return target && target.enabled;
        }

    }

}