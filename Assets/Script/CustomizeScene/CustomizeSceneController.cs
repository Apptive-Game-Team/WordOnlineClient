using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Script.CustomizeScene;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeSceneController : MonoBehaviour
{
    [Header("API & DB")]
    [SerializeField] private DecorationsApiClient apiClient;
    [SerializeField] private DecorationDatabase dataDb;

    [Header("Character Preview")]
    [SerializeField] private Image baseCharacterImage; // 몸
    [SerializeField] private Image hatLayerImage;      // 모자 레이어
    [SerializeField] private Image capeLayerImage;     // 망토 레이어

    [Header("UI - Grids")]
    [SerializeField] private Transform hatGridRoot;        // GridLayoutGroup가 달린 오브젝트
    [SerializeField] private Transform capeGridRoot;
    [SerializeField] private DecorationIconView iconPrefab;

    private readonly Dictionary<long, DecorationIconView> iconById = new();

    private long? equippedHatId = null;
    private long? equippedCapeId = null;

    private void Start()
    {
        StartCoroutine(InitUI());
    }

    private IEnumerator InitUI()
    {
        // 1) 전체 목록 가져와서 아이콘 생성
        yield return StartCoroutine(apiClient.GetMyDecorations(false,
            resp =>
            {
                BuildIconGrids(resp);
            },
            err =>
            {
                Debug.LogError("GetMyDecorations failed: " + err);
            }));

        // 2) 장착 중인 것만 다시 가져와서 캐릭터에 반영
        yield return RefreshEquippedFromServer();
    }

    private void BuildIconGrids(DecorationsResponse response)
    {
        // 기존 아이콘 정리
        foreach (Transform child in hatGridRoot) Destroy(child.gameObject);
        foreach (Transform child in capeGridRoot) Destroy(child.gameObject);
        iconById.Clear();

        // 서버 목록 전부 순회해서, 로컬 메타 있으면 아이콘 생성
        foreach (var dto in response.decorations)
        {
            var meta = dataDb.Find(dto.decorationId);
            if (meta == null) continue; // 로컬에 없는 건 무시

            Transform parent = meta.type == DecorationType.Hat ? hatGridRoot : capeGridRoot;
            var icon = Instantiate(iconPrefab, parent);
            icon.Init(meta, dto.isEquipped, OnDecorationIconClicked);

            iconById[dto.decorationId] = icon;

            if (meta.type == DecorationType.Hat && dto.isEquipped)
                equippedHatId = dto.decorationId;
            else if (meta.type == DecorationType.Cape && dto.isEquipped)
                equippedCapeId = dto.decorationId;
        }
        
        dataDb.FindAllNotInList(response.decorations.Select(d => d.decorationId).ToList()).ToList().ForEach(meta =>
        {
            CreateLockIcon(meta);
        });
    }
    
    private void CreateLockIcon(DecorationData meta)
    {
        StartCoroutine(
        apiClient.GetQuestState(meta.decorationId,
            questState =>
            {
                Transform parent = meta.type == DecorationType.Hat ? hatGridRoot : capeGridRoot;
                var icon = Instantiate(iconPrefab, parent);
                icon.Init(meta, false, null, questState);
        
                iconById[meta.decorationId] = icon;
            },
            err =>
            {
                Debug.LogError("GetQuestState failed: " + err);
            }
        ));
    }

    private void OnDecorationIconClicked(DecorationData meta)
    {
        // 같은 아이템 다시 누르면 그냥 무시해도 되고,
        // 서버에 다시 보내도 큰 문제는 없음. 여기선 무시.
        if (meta.type == DecorationType.Hat && equippedHatId == meta.decorationId) return;
        if (meta.type == DecorationType.Cape && equippedCapeId == meta.decorationId) return;

        StartCoroutine(EquipAndRefresh(meta));
    }

    private IEnumerator EquipAndRefresh(DecorationData meta)
    {
        bool success = false;

        yield return StartCoroutine(apiClient.EquipDecoration(meta.decorationId,
            () =>
            {
                success = true;
                _ = true;
            },
            err =>
            {
                Debug.LogError("EquipDecoration failed: " + err);
                _ = true;
            }));

        if (!success) yield break;

        // 서버 기준 상태로 다시 동기화
        yield return RefreshEquippedFromServer();
    }

    private IEnumerator RefreshEquippedFromServer()
    {
        yield return StartCoroutine(apiClient.GetMyDecorations(true,
            resp =>
            {
                ApplyEquipped(resp);
            },
            err =>
            {
                Debug.LogError("GetMyDecorations(equippedOnly) failed: " + err);
            }));
    }

    private void ApplyEquipped(DecorationsResponse response)
    {
        equippedHatId = null;
        equippedCapeId = null;

        // 먼저 UI 아이콘 장착 표시 리셋
        foreach (var kv in iconById)
            kv.Value.SetEquipped(false);

        foreach (var dto in response.decorations)
        {
            var meta = dataDb.Find(dto.decorationId);
            if (meta == null) continue;

            if (meta.type == DecorationType.Hat)
            {
                equippedHatId = dto.decorationId;
                if (iconById.TryGetValue(dto.decorationId, out var icon))
                    icon.SetEquipped(true);

                // 캐릭터 모자 레이어 스프라이트 세팅
                hatLayerImage.sprite = meta.characterSprite;
                hatLayerImage.enabled = true;
            }
            else if (meta.type == DecorationType.Cape)
            {
                equippedCapeId = dto.decorationId;
                if (iconById.TryGetValue(dto.decorationId, out var icon))
                    icon.SetEquipped(true);

                capeLayerImage.sprite = meta.characterSprite;
                capeLayerImage.enabled = true;
            }
        }

        // 장착된 게 없으면 레이어 끄기
        if (equippedHatId == null) hatLayerImage.enabled = false;
        if (equippedCapeId == null) capeLayerImage.enabled = false;
    }
}
