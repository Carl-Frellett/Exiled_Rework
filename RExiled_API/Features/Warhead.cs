using System;

namespace RExiled.API.Features
{
    public static class Warhead
    {
        private static AlphaWarheadController controller;
        private static AlphaWarheadNukesitePanel sitePanel;
        private static AlphaWarheadOutsitePanel outsitePanel;

        public static AlphaWarheadController Controller
        {
            get
            {
                if (controller == null)
                    controller = PlayerManager.localPlayer.GetComponent<AlphaWarheadController>();

                return controller;
            }
        }

        public static AlphaWarheadNukesitePanel SitePanel
        {
            get
            {
                if (sitePanel == null)
                    sitePanel = UnityEngine.Object.FindObjectOfType<AlphaWarheadNukesitePanel>();

                return sitePanel;
            }
        }

        public static AlphaWarheadOutsitePanel OutsitePanel
        {
            get
            {
                if (outsitePanel == null)
                    outsitePanel = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();

                return outsitePanel;
            }
        }

        public static bool LeverStatus
        {
            get => SitePanel.Networkenabled;
            set => SitePanel.Networkenabled = value;
        }

        public static bool IsKeycardActivated
        {
            get => OutsitePanel.NetworkkeycardEntered;
            set => OutsitePanel.NetworkkeycardEntered = value;
        }

        public static bool IsDetonated => Controller.detonated;

        public static bool IsInProgress => Controller.NetworkinProgress;

        public static float DetonationTimer
        {
            get => Controller.NetworktimeToDetonation;
            set => Controller.NetworktimeToDetonation = value;
        }

        public static float RealDetonationTimer => Controller.RealDetonationTime();

        public static bool CanBeStarted => !Recontainer079.isLocked &&
            ((AlphaWarheadController._resumeScenario == -1 &&
            Math.Abs(Controller.scenarios_start[AlphaWarheadController._startScenario].SumTime() - Controller.timeToDetonation) < 0.0001f) ||
            (AlphaWarheadController._resumeScenario != -1 &&
            Math.Abs(Controller.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime() - Controller.timeToDetonation) < 0.0001f));

        public static void Start()
        {
            Controller.InstantPrepare();
            Controller.StartDetonation();
        }

        public static void Stop() => Controller.CancelDetonation();

        public static void Detonate()
        {
            Controller.InstantPrepare();
            Controller.Detonate();
        }

        public static void Shake() => Controller.RpcShake(true);
    }
}
