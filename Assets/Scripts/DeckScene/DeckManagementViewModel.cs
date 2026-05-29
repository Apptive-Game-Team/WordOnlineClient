using System;
using System.Collections;
using System.Linq;
using Data.Deck;

namespace DeckScene
{
    public enum DeckEditMode
    {
        None,
        Create,
        Update
    }

    public enum DeckValidationError
    {
        None,
        CardCount,
        AttributeCount,
        MagicCount
    }

    public class DeckManagementViewModel
    {
        private static DeckResponseDto[] cachedUserDecks;
        private static CardDto[] cachedOwnedCards;

        private readonly DeckApiClient deckApiClient = new DeckApiClient();

        public DeckResponseDto[] UserDecks => cachedUserDecks ?? Array.Empty<DeckResponseDto>();
        public CardDto[] OwnedCards => cachedOwnedCards ?? Array.Empty<CardDto>();
        public DeckResponseDto CurrentDeck { get; private set; }
        public DeckEditMode CurrentMode { get; private set; } = DeckEditMode.None;
        public bool HasCachedData => UserDecks.Length > 0 && OwnedCards.Length > 0;
        public bool CanDeleteCurrentDeck => CurrentMode == DeckEditMode.Update && CurrentDeck != null;

        public void SelectDeck(DeckResponseDto deck)
        {
            CurrentDeck = CloneDeck(deck);
            CurrentMode = DeckEditMode.Update;
            DeckSceneContext.CurrentDeck = CurrentDeck;
        }

        public void SelectNewDeck(string deckName)
        {
            CurrentDeck = new DeckResponseDto
            {
                id = -1,
                name = deckName,
                cards = Array.Empty<CardDto>()
            };
            CurrentMode = DeckEditMode.Create;
            DeckSceneContext.CurrentDeck = CurrentDeck;
        }

        public bool TryAddOwnedCard(CardDto card)
        {
            if (CurrentDeck == null || card == null || !card.unlocked)
            {
                return false;
            }

            int ownedCount = card.count;
            int inDeckCount = CurrentDeck.cards.Count(c => c.id == card.id);
            if (CurrentDeck.cards.Length >= 15 || inDeckCount >= ownedCount)
            {
                return false;
            }

            CurrentDeck.cards = CurrentDeck.cards.Append(card).ToArray();
            DeckSceneContext.CurrentDeck = CurrentDeck;
            return true;
        }

        public bool TryRemoveCard(CardDto card)
        {
            if (CurrentDeck == null || card == null)
            {
                return false;
            }

            var cardList = CurrentDeck.cards.ToList();
            CardDto toDelete = cardList.FirstOrDefault(c => c.id == card.id);
            if (toDelete == null)
            {
                return false;
            }

            cardList.Remove(toDelete);
            CurrentDeck.cards = cardList.ToArray();
            DeckSceneContext.CurrentDeck = CurrentDeck;
            return true;
        }

        public DeckValidationError ValidateCurrentDeck()
        {
            if (CurrentDeck == null)
            {
                return DeckValidationError.CardCount;
            }

            int typeCount = CurrentDeck.cards
                .Where(c => c.type == "Type")
                .Select(c => c.name)
                .Distinct()
                .Count();
            int magicCount = CurrentDeck.cards
                .Where(c => c.type == "Magic")
                .Select(c => c.name)
                .Distinct()
                .Count();

            if (CurrentDeck.cards.Length != 15)
            {
                return DeckValidationError.CardCount;
            }

            if (typeCount < 2)
            {
                return DeckValidationError.AttributeCount;
            }

            if (magicCount < 3)
            {
                return DeckValidationError.MagicCount;
            }

            return DeckValidationError.None;
        }

        public IEnumerator LoadAll(Action<bool> callback)
        {
            CardDto[] ownedCards = null;
            yield return deckApiClient.GetOwnedCards(cards => ownedCards = cards);
            if (ownedCards == null)
            {
                callback?.Invoke(false);
                yield break;
            }

            DeckResponseDto[] userDecks = null;
            yield return deckApiClient.GetDecks(decks => userDecks = decks);
            if (userDecks == null)
            {
                callback?.Invoke(false);
                yield break;
            }

            cachedOwnedCards = ownedCards;
            cachedUserDecks = userDecks;
            DeckSceneContext.OwnedCards = cachedOwnedCards;
            callback?.Invoke(true);
        }

        public IEnumerator SubmitCurrentDeck(string deckName, Action<DeckEditMode, bool> callback)
        {
            if (CurrentDeck == null)
            {
                callback?.Invoke(CurrentMode, false);
                yield break;
            }

            DeckRequestDto requestDto = new DeckRequestDto
            {
                name = deckName,
                cardIds = CurrentDeck.cards.Select(c => c.id).ToArray()
            };

            if (CurrentMode == DeckEditMode.Create)
            {
                yield return deckApiClient.CreateDeck(requestDto, isSuccess => callback?.Invoke(DeckEditMode.Create, isSuccess));
                yield break;
            }

            yield return deckApiClient.UpdateDeck(CurrentDeck.id, requestDto, isSuccess => callback?.Invoke(DeckEditMode.Update, isSuccess));
        }

        public IEnumerator DeleteCurrentDeck(Action<bool> callback)
        {
            if (!CanDeleteCurrentDeck)
            {
                callback?.Invoke(false);
                yield break;
            }

            yield return deckApiClient.DeleteDeck(CurrentDeck.id, callback);
        }

        private static DeckResponseDto CloneDeck(DeckResponseDto deck)
        {
            if (deck == null)
            {
                return null;
            }

            return new DeckResponseDto
            {
                id = deck.id,
                name = deck.name,
                cards = deck.cards?.ToArray() ?? Array.Empty<CardDto>()
            };
        }
    }
}
