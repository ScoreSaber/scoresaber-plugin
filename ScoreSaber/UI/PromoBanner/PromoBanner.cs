using BeatSaberMarkupLanguage.FloatingScreen;
using IPA.Utilities;
using ScoreSaber.Core.Services;
using ScoreSaber.UI.Leaderboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace ScoreSaber.UI.PromoBanner {
    internal class PromoBanner {

        internal FloatingScreen _floatingScreen = null;

        [Inject] internal PromoBannerView _promoBannerView = null;

        [Inject] private readonly PhysicsRaycasterWithCache _physicsRaycasterWithCache = null;

        [Inject] private readonly TweeningService _tweeningService = null;

        public Action<bool> ShowBanner;

        public Action Dismiss;

        internal bool Dismissed { get; set; } = false;

        public void CreatePromoBanner() {
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(150f, 50f), false, new Vector3(3.48f, 3.6f, 2.2f), Quaternion.Euler(359.5f, 57.04f, 359.5f));
            _floatingScreen.GetComponent<VRGraphicRaycaster>().SetField("_physicsRaycaster", _physicsRaycasterWithCache);
            _floatingScreen.SetRootViewController(_promoBannerView, HMUI.ViewController.AnimationType.Out);
            ShowBanner += Show;
            Dismiss += DismissBanner;
            _floatingScreen.gameObject.SetActive(false);
        }

        public void Show(bool show) {
            if (Dismissed) return;
            _tweeningService.FadeHorizontalLayoutGroup(_promoBannerView.container, show, 0.2f);
        }

        public void DismissBanner() {
            Show(false);
            Dismissed = true;
        }
    }
}
