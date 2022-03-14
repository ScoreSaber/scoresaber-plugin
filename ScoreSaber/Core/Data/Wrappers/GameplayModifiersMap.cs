namespace ScoreSaber.Core.Data.Wrappers {
    internal class GameplayModifiersMap {
        internal GameplayModifiers gameplayModifiers { get; set; }
        internal double totalMultiplier { get; set; } = 1;
        internal GameplayModifiersMap() { }
        internal GameplayModifiersMap(GameplayModifiers _gameplayModifiers) {
            gameplayModifiers = _gameplayModifiers;
        }
    }
}
