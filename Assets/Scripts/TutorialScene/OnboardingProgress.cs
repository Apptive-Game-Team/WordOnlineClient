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
        Deck_CreateDeck,
        Deck_ReturnToLobby,

        // MagicBook
        MagicBook_EnterScene,
        MagicBook_ExplainMagicBook,
        MagicBook_SelectMagic,
        MagicBook_ExplainMagicInfo,
        MagicBook_ReturnToLobby,
        
        // Common
        Common_SelectDeck,
        Common_ExplainMatch,
        Common_ExplainBot,
        Common_ExplainAdventure,
        Common_ExplainMenu,
        Common_Complete
    }
}
