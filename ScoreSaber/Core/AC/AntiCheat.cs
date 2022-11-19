#if RELEASE
using System;
using System.IO;
using System.Security.Cryptography;

namespace ScoreSaber.Core.AC
{
    internal abstract class AntiCheat {
        internal static string GetHash() {

            string localPath = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Plugins", "ScoreSaber.dll");
            string dataPath = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Beat Saber_Data");
            string oculusPlatform = Path.Combine(dataPath, "Managed", "Oculus.Platform.dll");
            string steamWorks = Path.Combine(dataPath, "Managed", "Steamworks.NET.dll") ;

            using (MD5 md5 = MD5.Create()) {

                string localHash;
                string oculusHash;
                string steamHash;
                
                // ? Why is this never used?
                // string mainHash = string.Empty;
            
                using (FileStream stream = File.OpenRead(localPath)) {
                    byte[] hasher = md5.ComputeHash(stream);
                    localHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
                }

                using (FileStream stream = File.OpenRead(oculusPlatform)) {
                    byte[] hasher = md5.ComputeHash(stream);
                    oculusHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
                }

                using (FileStream stream = File.OpenRead(steamWorks)) {
                    byte[] hasher = md5.ComputeHash(stream);
                    steamHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
                }

                string hash =
                    BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{localHash}{oculusHash}{steamHash}"))).Replace("-", "").ToLowerInvariant();

                return hash;
            }
        }
    }
}

#endif