using ScoreSaber.Core.Utils;
using System.Collections.Generic;
using UnityEngine.XR;

namespace ScoreSaber.Core.Data
{
    internal static class VRDevices 
    {
        internal static string GetDeviceHMD() {
            return OpenXRManager.GetDevice();
        }

        internal static string GetDeviceControllerLeft() {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            if (devices.Count == 0)
                return null;
            return devices[0].name;
        }

        internal static string GetDeviceControllerRight() {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count == 0)
                return null;
            return devices[0].name;
        }

        internal static string GetLegacyHmdFriendlyName(int hmd) {

            if (hmd == 0) { return "Unknown"; }
            if (hmd == 1) { return "Oculus Rift CV1"; }
            if (hmd == 2) { return "HTC VIVE"; }
            if (hmd == 4) { return "HTC VIVE Pro"; }
            if (hmd == 8) { return "Windows Mixed Reality"; }
            if (hmd == 16) { return "Oculus Rift S"; }
            if (hmd == 32) { return "Oculus Quest"; }
            if (hmd == 64) { return "Valve Index"; }
            if (hmd == 128) { return "HTC VIVE Cosmos"; }
            return "Unknown";
        }
    }
}
