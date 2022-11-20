namespace ScoreSaber.Core.ReplaySystem.UI {
    internal class VRControllerAccessor {
        public VRController LeftController { get; }
        public VRController RightController { get; }

        public VRControllerAccessor(PauseMenuManager pauseMenuManager) {

            LeftController = pauseMenuManager.transform.Find("MenuControllers/ControllerLeft").GetComponent<VRController>();
            RightController = pauseMenuManager.transform.Find("MenuControllers/ControllerRight").GetComponent<VRController>();
        }
    }
}