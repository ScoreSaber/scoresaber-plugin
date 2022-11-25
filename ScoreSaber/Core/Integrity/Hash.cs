#if RELEASE

#region

using IPA.Utilities;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace ScoreSaber.Core.AC {
    /// <summary>
    /// This class is used in the creation of the ScoreSaberUploadData object.
    /// The hash is used to verify the integrity of the plugin.
    /// </summary>
    internal class Hash {
        internal string Create() {

            string localPath = Path.Combine(UnityGame.InstallPath, "Plugins", "ScoreSaber.dll");
            string dataPath = Path.Combine(UnityGame.InstallPath, "Beat Saber_Data");
            // Possibly removable now, unless those two variables are used to detect the platform the player is on.
            string oculusPlatform = Path.Combine(dataPath, "Managed", "Oculus.Platform.dll");
            string steamWorks = Path.Combine(dataPath, "Managed", "Steamworks.NET.dll");

            // MD5 is one of the fastest algorithm
            using (var md5 = MD5.Create()) {

                string localHash = string.Empty;
                string oculusHash = string.Empty;
                string steamHash = string.Empty;

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

                string hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(
                    $"{localHash}{oculusHash}{steamHash}"))).Replace("-", "").ToLowerInvariant();

                return hash;
            }
        }
    }
}

#endif