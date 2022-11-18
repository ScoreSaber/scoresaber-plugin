#if RELEASE
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

internal class Internals {
    internal string A() {

        string localPath = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Plugins", "ScoreSaber.dll");
        string dataPath = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Beat Saber_Data");
        string oculusPlatform = Path.Combine(dataPath, "Managed", "Oculus.Platform.dll");
        string steamWorks = Path.Combine(dataPath, "Managed", "Steamworks.NET.dll");

        using (var md5 = MD5.Create()) {

            string localHash = string.Empty;
            string oculusHash = string.Empty;
            string steamHash = string.Empty;
            string mainHash = string.Empty;

            using (var stream = File.OpenRead(localPath)) {
                var hasher = md5.ComputeHash(stream);
                localHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
            }

            using (var stream = File.OpenRead(oculusPlatform)) {
                var hasher = md5.ComputeHash(stream);
                oculusHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
            }

            using (var stream = File.OpenRead(steamWorks)) {
                var hasher = md5.ComputeHash(stream);
                steamHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
            }

            string hash = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}", localHash, oculusHash, steamHash)))).Replace("-", "").ToLowerInvariant();

            return hash;
        }
    }
}

#endif
