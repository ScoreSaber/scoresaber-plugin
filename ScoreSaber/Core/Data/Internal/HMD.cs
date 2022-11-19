#pragma warning disable IDE1006 // Naming Styles

#region

using System;
using System.Collections.Generic;
using UnityEngine.XR;

#endregion

namespace ScoreSaber.Core.Data.Internal {
    internal static class HMD {
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
                List<InputDevice> inputDevices = new List<InputDevice>();
                InputDevices.GetDevices(inputDevices);
                foreach (InputDevice device in inputDevices) {
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
                }

                return Vive;
            }

            if (hmd.Contains("quest")) {
                return Quest;
            }

            if (hmd.Contains("oculus")) {
                if (hmd.Contains("cv1")) {
                    return CV1;
                }

                if (hmd.Contains("quest")) {
                    return Quest;
                }

                return RiftS;
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
            switch (hmd) {
                case 0:
                    return "Unknown";
                case 1:
                    return "Oculus Rift CV1";
                case 2:
                    return "HTC VIVE";
                case 4:
                    return "HTC VIVE Pro";
                case 8:
                    return "Windows Mixed Reality";
                case 16:
                    return "Oculus Rift S";
                case 32:
                    return "Oculus Quest";
                case 64:
                    return "Valve Index";
                case 128:
                    return "HTC VIVE Cosmos";
                default:
                    return "Unknown";
            }
        }
    }
}