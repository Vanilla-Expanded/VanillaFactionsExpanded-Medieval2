using UnityEngine;

namespace VFEMedieval
{
    public class MerchantGuild_Tweener
    {
        private MerchantGuild merchantGuild;

        private Vector3 tweenedPos = Vector3.zero;

        private Vector3 lastTickSpringPos;

        private const float SpringTightness = 0.09f;

        public Vector3 TweenedPos => tweenedPos;

        public Vector3 LastTickTweenedVelocity => TweenedPos - lastTickSpringPos;

        public Vector3 TweenedPosRoot => MerchantGuildTweenerUtility.PatherTweenedPosRoot(merchantGuild) + MerchantGuildTweenerUtility.MerchantGuildCollisionPosOffsetFor(merchantGuild);

        public MerchantGuild_Tweener(MerchantGuild merchantGuild)
        {
            this.merchantGuild = merchantGuild;
        }

        public void TweenerTick()
        {
            lastTickSpringPos = tweenedPos;
            Vector3 vector = TweenedPosRoot - tweenedPos;
            tweenedPos += vector * 0.09f;
        }

        public void ResetTweenedPosToRoot()
        {
            tweenedPos = TweenedPosRoot;
            lastTickSpringPos = tweenedPos;
        }
    }
}