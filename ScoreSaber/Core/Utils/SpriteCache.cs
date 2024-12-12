using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScoreSaber.Core.Utils {
    internal static class SpriteCache {
        internal static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
        private static int MaxSpriteCacheSize = 150;
        internal static Queue<string> spriteCacheQueue = new Queue<string>();
        internal static void MaintainSpriteCache() {
            while (cachedSprites.Count > MaxSpriteCacheSize) {
                string oldestUrl = spriteCacheQueue.Dequeue();
                cachedSprites.Remove(oldestUrl);
            }
        }

        internal static void AddSpriteToCache(string url, Sprite sprite) {
            if (cachedSprites.ContainsKey(url)) {
                return;
            }
            cachedSprites.Add(url, sprite);
            spriteCacheQueue.Enqueue(url);
        }
    }
}
