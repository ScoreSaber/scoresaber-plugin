using System;
using System.Runtime.InteropServices;
using System.Text;

// Adapted from https://forum.unity.com/threads/openxr-is-it-no-longer-possible-to-get-descriptive-device-names.1051493/#post-8316300

namespace ScoreSaber.Core.Utils {
    internal static unsafe class OpenXRManager {

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(
          string lpModuleName);

        // delegate for xrGetInstanceProcAddr() https://registry.khronos.org/OpenXR/specs/1.0/man/html/xrGetInstanceProcAddr.html
        private delegate ulong xrGetInstanceProcAddrDelegate(ulong instance, StringBuilder name, IntPtr* function);

        // delegate for xrGetSystemProperties() https://registry.khronos.org/OpenXR/specs/1.0/man/html/xrGetSystemProperties.html
        private delegate ulong xrGetSystemPropertiesDelegate(ulong instance, ulong systemId, void* properties);

        // delegate for xrGetSystemProperties() https://registry.khronos.org/OpenXR/specs/1.0/man/html/xrGetSystem.html
        private delegate ulong xrGetSystemDelegate(ulong instance, XrSystemGetInfo* getInfo, ulong* systemId);

        private delegate ulong xrGetCurrentInstanceDelegate();

        // XrSystemProperties structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemProperties.html
        struct XrSystemProperties {
            public ulong type;      // 64 bit
            public void* next;      // 64 bit
            public ulong systemId;  // 64 bit
            public int vendorId;    // 32 bit
            public fixed byte systemName[XR_MAX_SYSTEM_NAME_SIZE];
            public XrSystemGraphicsProperties graphicsProperties;
            public XrSystemTrackingProperties trackingProperties;
        };

        // XrSystemGraphicsProperties structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemGraphicsProperties.html
        struct XrSystemGraphicsProperties {
            public uint maxSwapchainImageHeight;
            public uint maxSwapchainImageWidth;
            public uint maxLayerCount;
        };

        // XrSystemTrackingProperties structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemTrackingProperties.html
        struct XrSystemTrackingProperties {
            public uint orientationTracking;
            public uint positionTracking;
        };

        // XrSystemGetInfo structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemGetInfo.html
        struct XrSystemGetInfo {
            public ulong type;      // 64 bit
            public void* next;      // 64 bit
            public ulong formFactor;// 64 bit
        }

        // https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrFormFactor.html
        private const int XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY = 1;

        // https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrStructureType.html
        private const int XR_TYPE_SYSTEM_GET_INFO = 4;
        private const int XR_TYPE_SYSTEM_PROPERTIES = 5;
        private const int XR_MAX_SYSTEM_NAME_SIZE = 256;

        internal static string GetDevice() {
            var openXRLoaderBaseAddress = GetModuleHandle("openxr_loader");

            if (openXRLoaderBaseAddress == IntPtr.Zero) {
                return null;
            }

            // Get our xrInstance
            var xrGetCurrentInstance = Marshal.GetDelegateForFunctionPointer<xrGetCurrentInstanceDelegate>(openXRLoaderBaseAddress + 0x38980);
            var xrCurrentInstance = new IntPtr(*(long*)xrGetCurrentInstance());
            var xrInstance = *(ulong*)new IntPtr((byte*)xrCurrentInstance.ToPointer() + 8);

            // Get our systemId
            var info = new XrSystemGetInfo {
                type = XR_TYPE_SYSTEM_GET_INFO,
                formFactor = XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY
            };

            ulong systemId = 0;
            var xrGetSystem = Marshal.GetDelegateForFunctionPointer<xrGetSystemDelegate>(openXRLoaderBaseAddress + 0x2D0B);
            var getSystemResult = xrGetSystem(xrInstance, &info, &systemId);

            // Get our system properties
            var xrGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer<xrGetInstanceProcAddrDelegate>(openXRLoaderBaseAddress + 0x5AE2);
            IntPtr xrGetSystemPropertiesAddr;
            ulong result = xrGetInstanceProcAddr(xrInstance, new StringBuilder("xrGetSystemProperties"), &xrGetSystemPropertiesAddr);
            var getSystemPropertiesAddr = Marshal.GetDelegateForFunctionPointer<xrGetSystemPropertiesDelegate>(xrGetSystemPropertiesAddr);

            XrSystemProperties properties;
            properties.type = XR_TYPE_SYSTEM_PROPERTIES;
            result = getSystemPropertiesAddr(xrInstance, systemId, &properties);

            var systemName = "";
            for (int charIndex = 0; charIndex < XR_MAX_SYSTEM_NAME_SIZE; charIndex++) {
                if (properties.systemName[charIndex] == 0) {
                    break;
                }
                systemName += ((char)properties.systemName[charIndex]);
            }
            return systemName;
        }
    }
}
