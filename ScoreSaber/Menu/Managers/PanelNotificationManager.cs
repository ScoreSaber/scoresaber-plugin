using ScoreSaber.Interfaces;
using SiraUtil.Logging;
using TMPro;
using Tweening;
using UnityEngine;
using Zenject;

namespace ScoreSaber.Menu.Managers;

internal class PanelNotificationManager : INotificationManager, ITickable {

    private readonly SiraLog _siraLog;
    private readonly TimeTweeningManager _timeTweeningManager;

    private const float AnimationDuration = 0.3f;
    private static readonly Color _defaultColor = Color.white;

    private float? _timeToDismiss;
    private TMP_Text? _textComponent;
    private Vector3 _hidePosition = Vector3.zero;
    private Vector3 _showPosition = Vector3.zero;

    public PanelNotificationManager(SiraLog siraLog, TimeTweeningManager timeTweeningManager) {

        _siraLog = siraLog;
        _timeTweeningManager = timeTweeningManager;
    }

    public void Present(string text, INotificationManager.Options? options = null) {

        options ??= new INotificationManager.Options();
        options.Text = text;
        Present(options);
    }

    internal void Setup(TMP_Text textComponent) {

        _textComponent = textComponent;

        _textComponent.alpha = 0f;
        var transform = textComponent.transform;
        _showPosition = transform.localPosition;
        _hidePosition = _showPosition + new Vector3(0f, -5f, 0f);
        transform.localPosition = _hidePosition;
    }

    public void Present(INotificationManager.Options options) {

        if (_textComponent == null)
            return;

        _textComponent.text = options.Text;
        var transform = _textComponent.transform;
        var startPos = transform.localPosition;
        var startColor = _textComponent.color;
        var endColor = options.Color ??= _defaultColor;

        _siraLog.Trace($"Presenting the text '{options.Text}'");
        _timeTweeningManager.KillAllTweens(_textComponent);
        _timeTweeningManager.AddTween(new FloatTween(0f, 1f, value => {

            _textComponent.color = Color.Lerp(startColor, endColor, value).ColorWithAlpha(Mathf.Lerp(startColor.a, 1f, value));
            transform.localPosition = Vector3.Lerp(startPos, _showPosition, value);

        }, AnimationDuration, EaseType.OutCubic), _textComponent);

        if (options.Duration.HasValue && options.Duration.Value != 0)
            _timeToDismiss = Time.time + options.Duration.Value;
        else
            _timeToDismiss = null;
    }

    public void Tick() {

        // If there's nothing to dismiss, do nothing.
        if (!_timeToDismiss.HasValue)
            return;

        // If we haven't reached the time we want to dismiss yet, do nothing.
        if (_timeToDismiss.Value > Time.time)
            return;

        if (_textComponent == null)
            return;

        // Dismiss the notification
        _timeToDismiss = null;
        var transform = _textComponent.transform;
        var startPos = transform.localPosition;
        var startColor = _textComponent.color;
        var endColor = _defaultColor;

        _siraLog.Debug("Dismissing active notification");
        _timeTweeningManager.KillAllTweens(_textComponent);
        _timeTweeningManager.AddTween(new FloatTween(0f, 1f, value => {

            _textComponent.color = Color.Lerp(startColor, endColor, value).ColorWithAlpha(Mathf.Lerp(startColor.a, 0f, value));
            transform.localPosition = Vector3.Lerp(startPos, _hidePosition, value);

        }, AnimationDuration, EaseType.OutQuint), _textComponent);
    }
}