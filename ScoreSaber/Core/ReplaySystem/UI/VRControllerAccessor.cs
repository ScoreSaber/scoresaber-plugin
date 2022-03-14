namespace ScoreSaber.Core.ReplaySystem.UI
{
    internal class VRControllerAccessor
    {
        public VRController leftController { get; }
        public VRController rightController { get; }

        public VRControllerAccessor(PauseMenuManager pauseMenuManager) {

            leftController = pauseMenuManager.transform.Find("MenuControllers/ControllerLeft").GetComponent<VRController>();
            rightController = pauseMenuManager.transform.Find("MenuControllers/ControllerRight").GetComponent<VRController>();
        }
    }
}
