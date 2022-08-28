using UnityEngine;

namespace ScoreSaber.Interfaces;

internal interface INotificationManager {

    void Present(Options options);
    void Present(string text, Options? options = null);

    public class Options {

        public Color? Color { get; set; }
        public float? Duration { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}