using HMUI;
using ScoreSaber.Interfaces;
using SiraUtil.Logging;
using Tweening;
using UnityEngine;

namespace ScoreSaber.Menu.Managers;

internal class PanelThemingManager : IThemingManager {

    private readonly SiraLog _siraLog;
    private readonly TimeTweeningManager _timeTweeningManager;

    private ImageView? _background;
    private const float AnimationDuration = 0.5f;

    public PanelThemingManager(SiraLog siraLog, TimeTweeningManager timeTweeningManager) {

        _siraLog = siraLog;
        _timeTweeningManager = timeTweeningManager;
    }

    public void SetColor(Color color) {

        if (_background == null)
            return;

        _siraLog.Trace($"Setting color to '{color}'");
        _timeTweeningManager.AddTween(new ColorTween(_background.color, color, value => {

            _background.color = value;

        }, AnimationDuration, EaseType.OutCubic), _background);
    }

    internal void Setup(ImageView background) {

        _background = background;
    }
}