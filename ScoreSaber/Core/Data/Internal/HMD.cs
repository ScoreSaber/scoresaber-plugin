
#region

using System;
using System.Collections.Generic;
using System.Linq;
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
                var inputDevices = new List<InputDevice>();
                InputDevices.GetDevices(inputDevices);
#pragma warning disable CS0618 // Type or member is obsolete
                return inputDevices.Any(device => device.name.ToLower().Contains("knuckles")) ? Index : Get(XRDevice.model);
            } catch (Exception) {
                return 0;
            }
        }

        // TODO: These methods are a mess, refactor soon (tm)
        private static int Get(string hmdName) {

            string hmd = hmdName.ToLower();
            if (hmd.Contains("vive")) {
                if (hmd.Contains("pro")) {
                    return VivePro;
                }

                return hmd.Contains("cosmos") ? Cosmos : Vive;
            }
            if (hmd.Contains("quest")) {
                return Quest;
            }
            if (hmd.Contains("oculus")) {
                if (hmd.Contains("cv1")) {
                    return CV1;
                }

                return hmd.Contains("quest") ? Quest : RiftS;
            }
            if (hmdName.ToLower().Contains("index")) {
                return Index;
            }

            return WMRBrands.Any(brand => hmdName.ToLower().Contains(brand)) ? Windows : Unknown;
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