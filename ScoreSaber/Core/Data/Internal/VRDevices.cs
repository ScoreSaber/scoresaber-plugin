using ScoreSaber.Core.Utils;
using System.Collections.Generic;
using UnityEngine.XR;

namespace ScoreSaber.Core.Data {
    internal static class VRDevices {
        internal static string GetDeviceHMD() {

            var currentRuntime = UnityEngine.XR.OpenXR.OpenXRRuntime.name.ToLower();

            var hmd = GetDeviceName(XRNode.Head);

            if (currentRuntime.Contains("steam")) {
                if (SteamSettings.hmdName != null)
                    hmd = $"{hmd}:(steamcfg):{SteamSettings.hmdName}";
            }

            if (OpenXRManager.hmdName != null)
                hmd = $"{hmd}:(openxr):{OpenXRManager.hmdName}";

            return $"{UnityEngine.XR.OpenXR.OpenXRRuntime.name}:{hmd}" ;
        }

        internal static string GetDeviceControllerLeft() {
            return $"{UnityEngine.XR.OpenXR.OpenXRRuntime.name}:{GetDeviceName(XRNode.LeftHand)}";
        }

        internal static string GetDeviceControllerRight() {
            return $"{UnityEngine.XR.OpenXR.OpenXRRuntime.name}:{GetDeviceName(XRNode.RightHand)}";
        }

        private static string GetDeviceName(XRNode node) {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(node, devices);
            if (devices.Count == 0) {
                return null;
            }
            return "(xrnode):" + devices[0].name;
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
