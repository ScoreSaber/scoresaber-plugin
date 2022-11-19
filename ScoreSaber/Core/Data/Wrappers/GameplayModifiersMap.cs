namespace ScoreSaber.Core.Data.Wrappers {
    internal class GameplayModifiersMap {
        internal GameplayModifiersMap() { }

        internal GameplayModifiersMap(GameplayModifiers _gameplayModifiers) {
            gameplayModifiers = _gameplayModifiers;
        }

        internal GameplayModifiers gameplayModifiers { get; set; }
        internal double totalMultiplier { get; set; } = 1;
    }
}