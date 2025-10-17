using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRUIControls;
using Zenject;

namespace ScoreSaber.UI.Multiplayer {

    // Unused for now
    internal class ScoreSaberMultiplayerLobbyLeaderboardFlowManager : IInitializable, IDisposable {

        private readonly DiContainer _container;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ServerPlayerListViewController _serverPlayerListViewController;
        private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;

        private ClickableImage _image;

        public ScoreSaberMultiplayerLobbyLeaderboardFlowManager(DiContainer container, MainFlowCoordinator mainFlowCoordinator, ServerPlayerListViewController serverPlayerListViewController, PlatformLeaderboardViewController platformLeaderboardViewController) {

            _container = container;
            _mainFlowCoordinator = mainFlowCoordinator;
            _serverPlayerListViewController = serverPlayerListViewController;
            _platformLeaderboardViewController = platformLeaderboardViewController;
        }

        public void Initialize() {

            _serverPlayerListViewController.didActivateEvent += ServerPlayerListViewController_didActivateEvent;
            _serverPlayerListViewController.didDeactivateEvent += ServerPlayerListViewController_didDeactivateEvent;
        }

        private void ServerPlayerListViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (firstActivation)
                _image = SetupButton();

            _image.OnClickEvent += OnScoreSaberButtonClicked;
        }

        private void OnScoreSaberButtonClicked(PointerEventData _) {

            var youngest = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            if (!(youngest is GameServerLobbyFlowCoordinator))
                return;



            //_platformLeaderboardViewController.SetData() ;
            ReflectionUtil.InvokeMethod<object, FlowCoordinator>(youngest, "SetRightScreenViewController", _platformLeaderboardViewController, ViewController.AnimationType.In);
        }

        private void ServerPlayerListViewController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling) {

            _image.OnClickEvent -= OnScoreSaberButtonClicked;
        }

        public void Dispose() {

            _serverPlayerListViewController.didActivateEvent -= ServerPlayerListViewController_didActivateEvent;
            _serverPlayerListViewController.didDeactivateEvent -= ServerPlayerListViewController_didDeactivateEvent;
        }

        private ClickableImage SetupButton() {

            GameObject gameObject = new GameObject("ScoreSaber Icon");
            ClickableImage image = gameObject.AddComponent<ClickableImage>();
            image.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;

            image.rectTransform.SetParent(_serverPlayerListViewController.transform);
            image.rectTransform.localPosition = new Vector3(-52f, 36.5f, 0f);
            image.rectTransform.localScale = new Vector3(.5f, .5f, .5f);
            image.rectTransform.sizeDelta = new Vector2(20f, 20f);
            gameObject.AddComponent<LayoutElement>();

            image.color = image.DefaultColor = Color.white.ColorWithAlpha(0.1f);
            image.SetField<ImageView, float>("_skew", 0.18f);

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;
            _container.InstantiateComponent<VRGraphicRaycaster>(gameObject);

            BeatSaberMarkupLanguage.Utilities.LoadSpriteFromAssemblyAsync(Assembly.GetExecutingAssembly(), "ScoreSaber.Resources.logo.png").ContinueWith(spriteTask => {
                image.sprite = spriteTask.Result;
                image.sprite.texture.wrapMode = TextureWrapMode.Clamp;
            });

            return image;
        }
    }
}
