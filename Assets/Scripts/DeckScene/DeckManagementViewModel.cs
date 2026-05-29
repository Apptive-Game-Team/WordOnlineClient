using System;
using System.Collections;
using System.Linq;
using Data.Deck;
using Global;

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
                bool isSuccess = false;
                DeckResponseDto createdDeck = null;
                yield return deckApiClient.CreateDeck(requestDto, (success, deck) =>
                {
                    isSuccess = success;
                    createdDeck = deck;
                });

                if (isSuccess)
                {
                    long createdDeckId = createdDeck?.id ?? 0;
                    if (createdDeckId <= 0)
                    {
                        yield return ResolveDeckId(requestDto, deckId => createdDeckId = deckId);
                    }

                    if (createdDeckId > 0)
                    {
                        CurrentDeck.id = createdDeckId;
                        yield return SelectSubmittedDeck(createdDeckId);
                    }
                }

                callback?.Invoke(DeckEditMode.Create, isSuccess);
                yield break;
            }

            bool updateSuccess = false;
            yield return deckApiClient.UpdateDeck(CurrentDeck.id, requestDto, isSuccess => updateSuccess = isSuccess);

            if (updateSuccess)
            {
                yield return SelectSubmittedDeck(CurrentDeck.id);
            }

            callback?.Invoke(DeckEditMode.Update, updateSuccess);
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

        private IEnumerator SelectSubmittedDeck(long deckId)
        {
            bool isSelected = false;
            yield return deckApiClient.SelectDeck(deckId, success => isSelected = success);

            if (isSelected && SceneContext.User != null)
            {
                SceneContext.User.selectedDeckId = deckId;
            }
        }

        private IEnumerator ResolveDeckId(DeckRequestDto requestDto, Action<long> callback)
        {
            DeckResponseDto[] decks = null;
            yield return deckApiClient.GetDecks(result => decks = result);

            DeckResponseDto matchingDeck = decks?
                .Where(deck => deck.name == requestDto.name && HasSameCards(deck, requestDto.cardIds))
                .LastOrDefault();

            callback?.Invoke(matchingDeck?.id ?? 0);
        }

        private static bool HasSameCards(DeckResponseDto deck, long[] cardIds)
        {
            long[] deckCardIds = deck.cards?.Select(card => card.id).OrderBy(id => id).ToArray() ?? Array.Empty<long>();
            long[] requestCardIds = cardIds?.OrderBy(id => id).ToArray() ?? Array.Empty<long>();
            return deckCardIds.SequenceEqual(requestCardIds);
        }
    }
}
