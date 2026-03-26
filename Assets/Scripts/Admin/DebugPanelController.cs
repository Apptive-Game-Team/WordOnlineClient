using System;
using Admin.Dto;
using Global;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Admin
{
    public class DebugPanelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AdminViewModel adminViewModel;
        [SerializeField] private DebugItemUI itemPrefab;
        [SerializeField] private GameObject magicParent;
        [SerializeField] private GameObject prefabParent;
        [SerializeField] private GameObject selectionIndicator; // Optional: visual feedback for selection mode

        private enum SelectionType { None, Magic, Prefab }
        private SelectionType currentSelectionType = SelectionType.None;
        private int selectedMagicId;
        private string selectedPrefabId;

        private void Start()
        {
            if (adminViewModel == null) adminViewModel = FindObjectOfType<AdminViewModel>();
            if (selectionIndicator != null) selectionIndicator.SetActive(false);
            
            RefreshLists();
        }

        public void RefreshLists()
        {
            // Clear existing items
            foreach (Transform child in magicParent.transform) Destroy(child.gameObject);
            foreach (Transform child in prefabParent.transform) Destroy(child.gameObject);

            // Fetch Magics
            adminViewModel.FetchMagics(magics =>
            {
                if (magics == null) return;
                foreach (var magic in magics)
                {
                    var item = Instantiate(itemPrefab, magicParent.transform);
                    item.SetItem(magic.name, () => SelectMagic(magic.id));
                }
            });

            // Fetch Prefabs
            adminViewModel.FetchPrefabs(prefabs =>
            {
                if (prefabs == null) return;
                foreach (var prefab in prefabs)
                {
                    var item = Instantiate(itemPrefab, prefabParent.transform);
                    item.SetItem(prefab.name, () => SelectPrefab(prefab.id));
                }
            });
        }

        private void SelectMagic(int id)
        {
            selectedMagicId = id;
            currentSelectionType = SelectionType.Magic;
            if (selectionIndicator != null) selectionIndicator.SetActive(true);
            WDebug.Log($"Magic {id} selected. Click on field to summon.");
        }

        private void SelectPrefab(string id)
        {
            selectedPrefabId = id;
            currentSelectionType = SelectionType.Prefab;
            if (selectionIndicator != null) selectionIndicator.SetActive(true);
            WDebug.Log($"Prefab {id} selected. Click on field to spawn.");
        }

        private void Update()
        {
            if (currentSelectionType == SelectionType.None) return;

            // Update indicator position to mouse
            if (selectionIndicator != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                selectionIndicator.transform.position = mousePos;
            }

            // Cancel selection on Right Click
            if (Input.GetMouseButtonDown(1))
            {
                CancelSelection();
                return;
            }

            // Confirm selection on Left Click
            if (Input.GetMouseButtonDown(0))
            {
                // Check if clicking on UI
                if (EventSystem.current.IsPointerOverGameObject()) return;

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                ExecuteSelection(worldPos);
            }
        }

        private void ExecuteSelection(Vector3 position)
        {
            string sessionId = SceneContext.MatchInfo?.sessionId ?? "debug-1";
            string master = SceneContext.Me ?? "None";

            if (currentSelectionType == SelectionType.Magic)
            {
                var dto = new DebugSummonMagicRequestDto
                {
                    sessionId = sessionId,
                    master = master,
                    magicId = selectedMagicId,
                    position = position
                };
                adminViewModel.SummonMagic(dto, response => 
                {
                    if (!response.success) WDebug.LogError("Summon Magic Failed: " + response.message);
                });
            }
            else if (currentSelectionType == SelectionType.Prefab)
            {
                var dto = new DebugSpawnPrefabRequestDto
                {
                    sessionId = sessionId,
                    master = master,
                    prefabId = selectedPrefabId,
                    position = position
                };
                adminViewModel.SpawnPrefab(dto, response => 
                {
                    if (!response.success) WDebug.LogError("Spawn Prefab Failed: " + response.message);
                });
            }

            // Keep selection active for multiple spawns? 
            // The user didn't specify, but usually for debug it's nice.
            // Let's reset it for now to avoid accidental spawns.
            CancelSelection();
        }

        private void CancelSelection()
        {
            currentSelectionType = SelectionType.None;
            if (selectionIndicator != null) selectionIndicator.SetActive(false);
        }
    }
}
