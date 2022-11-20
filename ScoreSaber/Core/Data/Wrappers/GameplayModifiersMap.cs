namespace ScoreSaber.Core.Data.Wrappers {
    internal class GameplayModifiersMap {
        internal GameplayModifiers GameplayModifiers { get; set; }
        internal double TotalMultiplier { get; set; } = 1;
        internal GameplayModifiersMap() { }
        internal GameplayModifiersMap(GameplayModifiers _gameplayModifiers) {
            GameplayModifiers = _gameplayModifiers;
        }
    }
}