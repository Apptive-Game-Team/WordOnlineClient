namespace TutorialScene
{
    public enum OnboardingProgress
    {
        None,
        Completed,
        Skipped,

        // Battle
        Battle,

        // Deck
        Deck_EnterScene,
        Deck_ExplainCard,
        Deck_ExplainDeck,
        Deck_SelectCreateDeck,
        Deck_CreateDeck,
        Deck_SaveDeck,
        Deck_ReturnToLobby,

        // MagicBook
        MagicBook_EnterScene,
        MagicBook_ExplainMagicBook,
        MagicBook_SelectMagic,
        MagicBook_ExplainMagicInfo,
        MagicBook_OpenElementChart,
        MagicBook_ReturnToLobby,

        // Hospitality battle
        HospitalityBattle,

        // Common
        Common_SelectDeck,
        Common_ExplainMatch,
        Common_ExplainBot,
        Common_ExplainAdventure,
        Common_ExplainMenu,
        Common_Complete
    }
}
