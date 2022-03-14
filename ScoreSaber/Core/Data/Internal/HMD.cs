
#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Collections.Generic;
using UnityEngine.XR;

namespace ScoreSaber.Core.Data
{
    internal static class HMD 
    {
        internal static int Unknown = 0;
        internal static int CV1 = 1;
        internal static int Vive = 2;
        internal static int VivePro = 4;
        internal static int Windows = 8;
        internal static int RiftS = 16;
        internal static int Quest = 32;
        internal static int Index = 64;
        internal static int Cosmos = 128;

        internal static string[] WMRBrands = {
            "lenovo",
            "microsoft",
            "acer",
            "dell",
            "acer",
            "wmr",
            "samsung",
            "asus",
            "reverb"
        };

        internal static int Get() {

            try {
                var inputDevices = new List<InputDevice>();
                InputDevices.GetDevices(inputDevices);
                foreach (var device in inputDevices) {
                    if (device.name.ToLower().Contains("knuckles")) {
                        return Index;
                    }
                }
#pragma warning disable CS0618 // Type or member is obsolete
                return Get(XRDevice.model);
#pragma warning restore CS0618 // Type or member is obsolete
            } catch (Exception) {
                return 0;
            }
        }

        private static int Get(string hmdName) {

            string hmd = hmdName.ToLower();
            if (hmd.Contains("vive")) {
                if (hmd.Contains("pro")) {
                    return VivePro;
                }
                if (hmd.Contains("cosmos")) {
                    return Cosmos;
                } else {
                    return Vive;
                }
            }
            if (hmd.Contains("quest")) {
                return Quest;
            }
            if (hmd.Contains("oculus")) {
                if (hmd.Contains("cv1")) {
                    return CV1;
                } else {
                    if (hmd.Contains("quest")) {
                        return Quest;
                    } else {
                        return RiftS;
                    }
                }
            }
            if (hmdName.ToLower().Contains("index")) {
                return Index;
            }
            foreach (string brand in WMRBrands) {
                if (hmdName.ToLower().Contains(brand)) {
                    return Windows;
                }
            }
            return Unknown;
        }

        internal static string GetFriendlyName(int hmd) {

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
