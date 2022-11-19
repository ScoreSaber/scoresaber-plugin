#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Tags;
using System.Reflection;
using UnityEngine;

#endregion

namespace ScoreSaber.UI.Elements.Profile {
    internal class ProfileDetailViewTag : BSMLTag {
        private readonly string _content;

        public ProfileDetailViewTag(Assembly asm) {
            _content = Utilities.GetResourceContent(asm, "ScoreSaber.UI.Elements.Profile.ProfileDetailView.bsml");
        }

        public override string[] Aliases => new[] { "ss-profile" };

        public override GameObject CreateObject(Transform parent) {
            GameObject gameObj = new GameObject("ScoreSaberProfileModal");
            gameObj.transform.SetParent(parent, false);
            ProfileDetailView host = gameObj.AddComponent<ProfileDetailView>();
            BSMLParser.instance.Parse(_content, gameObj, host);
            host.SetProfileBadges(null);
            return gameObj;
        }
    }
}