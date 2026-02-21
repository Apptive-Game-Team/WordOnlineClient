using System.Collections;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace TutorialScene
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;
        [SerializeField] TutorialCardSender _cardSender;
        [SerializeField] TutorialData _tutorialData;
        [SerializeField] TextMeshProUGUI _dialogueText;
        [SerializeField] ManaMocker _manaMocker;
        
        private bool _advanceRequested;
        private bool _usedShotFire;
        private bool _usedWaterArcher;
        private bool _usedAnyCard;
        private bool _enemyDead;

        private void Awake()
        {        
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _cardSender.MagicUsed += OnMagicUsed;
            _cardSender.SingleCardUsed += OnSingleCardUsed;
        }

        private void OnDestroy()
        {
            _cardSender.MagicUsed -= OnMagicUsed;
            _cardSender.SingleCardUsed -= OnSingleCardUsed;
        }

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            for (int i = 0; i < _tutorialData.steps.Length; i++)
            {
                var step = _tutorialData.steps[i];

                if (step.shouldClearCards)
                {
                    ClearCard();
                }
                
                foreach (var name in step.cardNames)
                {
                    GiveCard(name);
                }
                
                yield return SetLocalizedDialogue(step.localizationKey);

                switch (step.waitType)
                {
                    case TutorialWaitType.Next:
                        yield return WaitAdvance();
                        break;
                    case TutorialWaitType.UsedShotFire:
                        yield return WaitUntil(() => _usedShotFire);
                        break;
                    case TutorialWaitType.UsedWaterArcher:
                        yield return WaitUntil(() => _usedWaterArcher);
                        break;
                    case TutorialWaitType.UsedAnyCard:
                        yield return WaitUntil(() => _usedAnyCard);
                        break;
                    case TutorialWaitType.EnemyDead:
                        yield return WaitUntil(() => _enemyDead);
                        break;
                }
            }

            SceneManager.LoadScene(_tutorialData.lobbySceneName);
        }

        private IEnumerator SetLocalizedDialogue(string key)
        {
            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(_tutorialData.stringTableName, key);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                _dialogueText.text = handle.Result;
            else
                _dialogueText.text = key;
        }

        private IEnumerator WaitAdvance()
        {
            _advanceRequested = false;

            while (Input.anyKey)
                yield return null;

            while (!_advanceRequested)
            {
                if (Input.anyKeyDown)
                    _advanceRequested = true;

                yield return null;
            }
        }

        private IEnumerator WaitUntil(System.Func<bool> condition)
        {
            while (!condition())
                yield return null;
        }

        public void RequestAdvance()
        {
            _advanceRequested = true;
        }

        public void NotifyEnemyDead()
        {
            _enemyDead = true;
        }
        public void NotifyShotFire()
        {
            _usedShotFire = true;
        }
        
        private void OnSingleCardUsed()
        {
            _usedAnyCard = true;
        }

        private void OnMagicUsed(System.Collections.Generic.IReadOnlyList<CardType> types)
        {
            if (IsExact(types, CardType.Shoot, CardType.Fire))
            {
                _usedShotFire = true;
                _manaMocker.UseMana(25);
            }
                
            if (IsExact(types, CardType.Spawn, CardType.Shoot, CardType.Water))
            {
                _usedWaterArcher = true;
            _manaMocker.UseMana(45);
            }
            
        }

        private bool IsExact(System.Collections.Generic.IReadOnlyList<CardType> types, params CardType[] expected)
        {
            if (types.Count != expected.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < types.Count; j++)
                {
                    if (types[j] == expected[i])
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        void GiveCard(string name)
        {
            TutorialSceneUIController.Instance.AddCard(name);
        }

        void ClearCard()
        {
            TutorialSceneUIController.Instance.ClearAllCards();
            _cardSender.CancelAll();
        }
    }
}
