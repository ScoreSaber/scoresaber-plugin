using System;
using System.Runtime.InteropServices;
using System.Text;

// Adapted from https://forum.unity.com/threads/openxr-is-it-no-longer-possible-to-get-descriptive-device-names.1051493/#post-8316300

namespace ScoreSaber.Core.Utils {
    internal static unsafe class OpenXRManager {
        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("openxr_loader", EntryPoint = "xrGetSystemProperties", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong GetSystemProperties(long* instance, ulong systemId, XrSystemProperties* properties);

        [DllImport("openxr_loader", EntryPoint = "xrGetSystem", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong GetSystem(long* instance, XrSystemGetInfo* getInfo, ulong* systemId);

        // delegate for xrGetInstanceProcAddr() https://registry.khronos.org/OpenXR/specs/1.0/man/html/xrGetInstanceProcAddr.html
        private delegate ulong xrGetInstanceProcAddrDelegate(ulong instance, StringBuilder name, IntPtr* function);

        // delegate for xrGetSystemProperties() https://registry.khronos.org/OpenXR/specs/1.0/man/html/xrGetSystemProperties.html
        private delegate ulong xrGetSystemPropertiesDelegate(ulong instance, ulong systemId, void* properties);

        // delegate for xrGetSystemProperties() https://registry.khronos.org/OpenXR/specs/1.0/man/html/xrGetSystem.html
        private delegate ulong xrGetSystemDelegate(ulong instance, XrSystemGetInfo* getInfo, ulong* systemId);

        private delegate ulong xrGetCurrentInstanceDelegate();

        // XrSystemProperties structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemProperties.html
        private struct XrSystemProperties {
            public       ulong                      type;     // 64 bit
            public       void*                      next;     // 64 bit
            public       ulong                      systemId; // 64 bit
            public       int                        vendorId; // 32 bit
            public fixed byte                       systemName[XR_MAX_SYSTEM_NAME_SIZE];
            public       XrSystemGraphicsProperties graphicsProperties;
            public       XrSystemTrackingProperties trackingProperties;
        };

        // XrSystemGraphicsProperties structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemGraphicsProperties.html
        private struct XrSystemGraphicsProperties {
            public uint maxSwapchainImageHeight;
            public uint maxSwapchainImageWidth;
            public uint maxLayerCount;
        };

        // XrSystemTrackingProperties structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemTrackingProperties.html
        private struct XrSystemTrackingProperties {
            public uint orientationTracking;
            public uint positionTracking;
        };

        // XrSystemGetInfo structure https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSystemGetInfo.html
        private struct XrSystemGetInfo {
            public ulong type;       // 64 bit
            public void* next;       // 64 bit
            public ulong formFactor; // 64 bit
        }

        // https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrFormFactor.html
        private const int XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY = 1;

        // https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrStructureType.html
        private const int XR_TYPE_SYSTEM_GET_INFO   = 4;
        private const int XR_TYPE_SYSTEM_PROPERTIES = 5;
        private const int XR_MAX_SYSTEM_NAME_SIZE   = 256;

        internal static string hmdName = null;

        internal static void Initialize() {
            hmdName = AttemptGetHmd();
        }

        private static string AttemptGetHmd() {
            try {
                var openXRLoaderBaseAddress = GetModuleHandle("openxr_loader");

                if (openXRLoaderBaseAddress == IntPtr.Zero) {
                    throw new Exception("openxr_loader not found");
                }

                // Get our xrInstance
                var xrGetCurrentInstance = Marshal.GetDelegateForFunctionPointer<xrGetCurrentInstanceDelegate>(openXRLoaderBaseAddress + 0x39480);
                var xrCurrentInstance    = new IntPtr(*(long*)xrGetCurrentInstance());
                var xrInstance           = (long*)*(ulong*)new IntPtr((byte*)xrCurrentInstance.ToPointer() + 8);

                // Get our systemId
                var info = new XrSystemGetInfo {
                    type       = XR_TYPE_SYSTEM_GET_INFO,
                    formFactor = XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY
                };

                ulong systemId        = 0;
                var   getSystemResult = GetSystem(xrInstance, &info, &systemId);

                if (getSystemResult != 0) {
                    Plugin.Log.Info($"Failed to get system from OpenXR {getSystemResult}");
                    return null;
                }

                // Get our system properties
                XrSystemProperties properties;
                properties.type = XR_TYPE_SYSTEM_PROPERTIES;
                var result = GetSystemProperties(xrInstance, systemId, &properties);

                if (result != 0) {
                    Plugin.Log.Info($"Failed to get system properties from OpenXR {result}");
                    return null;
                }

                var systemNameStrBuilder = new StringBuilder();

                for (int charIndex = 0; charIndex < XR_MAX_SYSTEM_NAME_SIZE; charIndex++) {
                    if (properties.systemName[charIndex] == 0) {
                        break;
                    }

                    systemNameStrBuilder.Append(((char)properties.systemName[charIndex]));
                }

                return systemNameStrBuilder.ToString();
            } catch (Exception ex) {
                Plugin.Log.Info($"Failed to get hmd from OpenXR {ex}");
                return null;
            }
        }
    }
}