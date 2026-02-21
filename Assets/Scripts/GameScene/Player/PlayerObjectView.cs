using Scripts.CustomizeScene;
using Scripts.CustomizeScene.dto;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.GameScene.Player
{
    public class PlayerObjectView : MonoBehaviour
    {
        // 모자/망토 스프라이트 교체하는 컴포넌트들
        [SerializeField] private Image hatRenderer;
        [SerializeField] private Image capeRenderer;

        // DB 참조
        [SerializeField] private DecorationDatabase decorationDatabase;

        public void SetDecorations(DecorationsResponse resp)
        {
            if (resp == null || decorationDatabase == null)
            {
                Debug.LogWarning("[PlayerObjectView] DecorationsResponse 또는 DecorationDatabase가 null입니다.");
                return;
            }
        
            var list = resp.decorations; // 예: public List<DecorationDto> decorations;

            if (list == null)
            {
                Debug.LogWarning("[PlayerObjectView] resp.decorations 가 null입니다.");
                return;
            }

            // 모자 / 망토 각각 하나씩 찾기 (equippedOnly=true 로 받아오면 보통 1개씩일 거라 가정)
            DecorationDto hatInfo = null;
            DecorationDto capeInfo = null;
            foreach (var item in list)
            {
                if (decorationDatabase.Find(item.decorationId)?.type == DecorationType.Hat)
                {
                    hatInfo = item;
                }
                else if (decorationDatabase.Find(item.decorationId)?.type == DecorationType.Cape)
                {
                    capeInfo = item;
                }
            }


            // 모자 세팅
            if (hatRenderer != null)
            {
                if (hatInfo != null)
                {
                    var hatData = decorationDatabase.Find(hatInfo.decorationId);
                    if (hatData != null && hatData.characterSprite != null)
                    {
                        hatRenderer.sprite = hatData.characterSprite;
                        hatRenderer.enabled = true;
                    }
                    else
                    {
                        // 데이터 없으면 숨김
                        hatRenderer.sprite = null;
                        hatRenderer.enabled = false;
                    }
                }
                else
                {
                    hatRenderer.sprite = null;
                    hatRenderer.enabled = false;
                }
            }

            // 망토 세팅
            if (capeRenderer != null)
            {
                if (capeInfo != null)
                {
                    var capeData = decorationDatabase.Find(capeInfo.decorationId);
                    if (capeData != null && capeData.characterSprite != null)
                    {
                        capeRenderer.sprite = capeData.characterSprite;
                        capeRenderer.enabled = true;
                    }
                    else
                    {
                        capeRenderer.sprite = null;
                        capeRenderer.enabled = false;
                    }
                }
                else
                {
                    capeRenderer.sprite = null;
                    capeRenderer.enabled = false;
                }
            }
        }
    }
}
