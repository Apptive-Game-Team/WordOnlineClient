using System;
using System.Collections;
using Data.Magic;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace TutorialScene
{
    public class BattleTutorialManager : LocalSingletonObject<BattleTutorialManager>
    {
        [SerializeField] TutorialCardSender _cardSender;
        [SerializeField] TutorialData _tutorialData;
        [SerializeField] TextMeshProUGUI _dialogueText;
        [SerializeField] ManaMocker _manaMocker;
        
        private bool _advanceRequested;
        private bool _usedShotFire;
        private bool _usedWaterArcher;
        private bool _usedAnyCard;
        private bool _enemyDead;
        public event Action OnEnd; 

        protected override void Awake()
        {        
            base.Awake();
            
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

            OnEnd?.Invoke();
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

        // TODO(#579): 튜토리얼 대본이 아직 옛 조합(Shoot+Fire, Spawn+Shoot+Water)을 가리킨다.
        // 그 조합의 결과 마법 이름은 각각 fireShot 과 aquaArcher 였으므로 이름으로 바꿔 두었다.
        // 튜토리얼을 새 모델로 다시 쓸 때 이 두 이름과 아래 마나 숫자를 함께 손본다.
        private const string ShotFireMagicName = "fire_shot";
        private const string WaterArcherMagicName = "aqua_archer";

        private void OnMagicUsed(System.Collections.Generic.IReadOnlyList<CombinedMagicData> magics)
        {
            if (UsedMagic(magics, ShotFireMagicName))
            {
                _usedShotFire = true;
                _manaMocker.UseMana(25);
            }

            if (UsedMagic(magics, WaterArcherMagicName))
            {
                _usedWaterArcher = true;
                _manaMocker.UseMana(45);
            }
        }

        private static bool UsedMagic(
            System.Collections.Generic.IReadOnlyList<CombinedMagicData> magics,
            string serverName)
        {
            for (int i = 0; i < magics.Count; i++)
            {
                if (magics[i] != null &&
                    string.Equals(magics[i].serverName, serverName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
