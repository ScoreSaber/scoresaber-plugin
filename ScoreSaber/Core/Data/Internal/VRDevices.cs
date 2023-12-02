using ScoreSaber.Core.Utils;
using System.Collections.Generic;
using UnityEngine.XR;

namespace ScoreSaber.Core.Data {
    internal static class VRDevices {
        internal static string GetDeviceHMD() {

#pragma warning disable CS0618 // Type or member is obsolete
            var hmd = $"(xrdevice):{XRDevice.model}";
#pragma warning restore CS0618 // Type or member is obsolete

            if (SteamSettings.hmdName != null) {
                hmd = $"{hmd}:(steamcfg):{SteamSettings.hmdName}";
            }

            return $"legacy:{hmd}";
        }

        internal static string GetDeviceControllerLeft() {
            try {
                var leftHandedControllers = new List<InputDevice>();
                var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
                InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);

                string device = string.Empty;
                if (leftHandedControllers != null && leftHandedControllers.Count > 0) {
                    device = $"(inputdevice):{leftHandedControllers[0].name}";
                }

                var leftXRNode = GetDeviceName(XRNode.LeftHand);
                if (leftXRNode != null) {
                    device = $"{device}:{leftXRNode}";
                }

                if (device != string.Empty) {
                    return $"legacy:{device}";
                }

                return $"legacy:unknown";
            } catch {
                return $"legacy:unknown";
            }
        }

        internal static string GetDeviceControllerRight() {
            try {
                var rightHandedControllers = new List<InputDevice>();
                var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
                InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, rightHandedControllers);

                string device = string.Empty;
                if (rightHandedControllers != null && rightHandedControllers.Count > 0) {
                    device = $"(inputdevice):{rightHandedControllers[0].name}";
                }

                var rightXRNode = GetDeviceName(XRNode.RightHand);
                if (rightXRNode != null) {
                    device = $"{device}:{rightXRNode}";
                }

                if (device != string.Empty) {
                    return $"legacy:{device}";
                }

                return $"legacy:unknown";
            } catch {
                return $"legacy:unknown";
            }
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
