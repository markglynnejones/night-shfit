using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityObject = UnityEngine.Object;

public sealed class PrototypeSaveLoadController : MonoBehaviour
{
    private const int SaveFormatVersion = 2;
    private const string SaveFileName = "prototype-save.json";
    private const string BackupSaveExtension = ".bak";
    private const string TemporarySaveExtension = ".tmp";
    private const string LooseState = "Loose";
    private const string ShelvedState = "Shelved";
    private const string CurrentVisualScaleSignature = "jewel-cases-0.22";

    [SerializeField] private float autosaveDebounceSeconds = 2f;
    [SerializeField] private float periodicAutosaveSeconds = 120f;

    private static PrototypeSaveLoadController activeController;

    private bool hasCompletedStartupLoad;
    private bool isLoading;
    private bool hasDirtyChanges;
    private bool expandedStockPendingSave;
    private float nextAutosaveTime;
    private float nextPeriodicAutosaveTime;

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
    private static string BackupSavePath => SavePath + BackupSaveExtension;
    private static string TemporarySavePath => SavePath + TemporarySaveExtension;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureControllerExists()
    {
        PrototypeSaveLoadController[] existingControllers =
            UnityObject.FindObjectsByType<PrototypeSaveLoadController>(FindObjectsInactive.Exclude);

        if (existingControllers.Length > 0)
        {
            return;
        }

        new GameObject("Prototype Save Load Controller").AddComponent<PrototypeSaveLoadController>();
    }

    public static void NotifyPersistentStateChanged()
    {
        if (activeController == null)
        {
            return;
        }

        activeController.MarkPersistentStateChanged();
    }

    private void Awake()
    {
        activeController = this;
        nextPeriodicAutosaveTime = Time.unscaledTime + periodicAutosaveSeconds;
    }

    private void Start()
    {
        StartCoroutine(LoadAfterPrototypeSetup());
    }

    private void OnDestroy()
    {
        if (activeController == this)
        {
            activeController = null;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.f5Key.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
            {
                SaveGame("Manual save");
            }

            if (keyboard.f9Key.wasPressedThisFrame || keyboard.oKey.wasPressedThisFrame)
            {
                LoadGame(LoadMode.Manual);
            }
        }

        if (!hasCompletedStartupLoad || isLoading)
        {
            return;
        }

        if (hasDirtyChanges && Time.unscaledTime >= nextAutosaveTime)
        {
            SaveGame("Autosave");
            return;
        }

        if (periodicAutosaveSeconds > 0f && Time.unscaledTime >= nextPeriodicAutosaveTime)
        {
            SaveGame("Periodic autosave");
        }
    }

    private void OnApplicationQuit()
    {
        if (!hasCompletedStartupLoad || isLoading)
        {
            return;
        }

        SaveGame("Quit save");
    }

    private IEnumerator LoadAfterPrototypeSetup()
    {
        yield return null;

        if (!PrototypeSortingSetup.HasCompletedSetup
            && UnityObject.FindObjectsByType<PrototypeSortingSetup>(FindObjectsInactive.Exclude).Length > 0)
        {
            bool setupCompleted = false;
            void HandleSetupCompleted()
            {
                setupCompleted = true;
            }

            PrototypeSortingSetup.SetupCompleted += HandleSetupCompleted;
            while (!setupCompleted && !PrototypeSortingSetup.HasCompletedSetup)
            {
                yield return null;
            }

            PrototypeSortingSetup.SetupCompleted -= HandleSetupCompleted;
        }

        LoadGame(LoadMode.Automatic);
        hasCompletedStartupLoad = true;
        hasDirtyChanges = expandedStockPendingSave;
        if (expandedStockPendingSave)
        {
            nextAutosaveTime = Time.unscaledTime + Mathf.Max(2f, autosaveDebounceSeconds);
        }

        nextPeriodicAutosaveTime = Time.unscaledTime + periodicAutosaveSeconds;
    }

    private void MarkPersistentStateChanged()
    {
        if (!hasCompletedStartupLoad || isLoading)
        {
            return;
        }

        hasDirtyChanges = true;
        nextAutosaveTime = Time.unscaledTime + Mathf.Max(0.1f, autosaveDebounceSeconds);
    }

    private void SaveGame(string reason)
    {
        PrototypeSaveData saveData = new()
        {
            version = SaveFormatVersion,
            shelfSlotLayoutVersion = 1,
            player = CapturePlayer(),
            visualScaleSignature = CurrentVisualScaleSignature
        };

        // The hand stack is saved as an ordered list of physical IDs. Carried
        // cases are also included in looseMediaItems for backwards-compatible
        // world state and as a safe fallback if the list cannot be restored.
        PlayerInteraction[] carryingPlayers =
            UnityObject.FindObjectsByType<PlayerInteraction>(FindObjectsInactive.Exclude);
        if (carryingPlayers.Length > 0 && carryingPlayers[0] != null)
        {
            saveData.carriedPhysicalIds = carryingPlayers[0].GetCarriedPhysicalIds();
            saveData.selectedCarriedIndex = carryingPlayers[0].SelectedCarriedIndex;
        }

        MediaItem[] mediaItems = UnityObject.FindObjectsByType<MediaItem>(FindObjectsInactive.Exclude);
        Array.Sort(mediaItems, CompareMediaItemsByPhysicalId);

        HashSet<string> savedPhysicalIds = new();
        for (int i = 0; i < mediaItems.Length; i++)
        {
            MediaItem mediaItem = mediaItems[i];
            if (mediaItem == null || string.IsNullOrWhiteSpace(mediaItem.PhysicalItemId))
            {
                Debug.LogWarning($"Skipping media item '{mediaItem?.name}' because it has no physical item ID.");
                continue;
            }

            if (!savedPhysicalIds.Add(mediaItem.PhysicalItemId))
            {
                Debug.LogWarning($"Skipping duplicate physical item ID '{mediaItem.PhysicalItemId}'.");
                continue;
            }

            PhysicalInteractable interactable = mediaItem.GetComponent<PhysicalInteractable>();
            ShelfSlot currentShelfSlot = interactable != null ? interactable.CurrentShelfSlot : null;
            int shelfOrder = currentShelfSlot != null ? currentShelfSlot.IndexOf(interactable) : -1;

            if (currentShelfSlot != null && shelfOrder >= 0)
            {
                saveData.shelvedMediaItems.Add(new ShelvedMediaItemSaveData
                {
                    physicalItemId = mediaItem.PhysicalItemId,
                    state = ShelvedState,
                    shelfSectionId = currentShelfSlot.ShelfSectionId,
                    shelfOrder = shelfOrder,
                    physicalSlot = currentShelfSlot.PhysicalSlotOf(interactable)
                });

                continue;
            }

            // If saved while carrying an item, restore it as loose stock on a surface
            // rather than leaving it floating at the camera hold position after load.
            Vector3 loosePosition = mediaItem.transform.position;
            Quaternion looseRotation = mediaItem.transform.rotation;
            if (interactable != null && interactable.IsHeld)
            {
                Vector3 rayOrigin = loosePosition + Vector3.up * 0.2f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit ground, 6f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    loosePosition.y = ground.point.y + 0.04f;
                }
                else
                {
                    loosePosition.y = 0.04f;
                }

                looseRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            saveData.looseMediaItems.Add(new LooseMediaItemSaveData
            {
                physicalItemId = mediaItem.PhysicalItemId,
                state = LooseState,
                position = loosePosition,
                rotation = looseRotation
            });
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            SafeWriteSaveFile(json);
            hasDirtyChanges = false;
            expandedStockPendingSave = false;
            nextPeriodicAutosaveTime = Time.unscaledTime + periodicAutosaveSeconds;
            Debug.Log($"Game saved ({reason}): {SavePath}");
        }
        catch (Exception exception)
        {
            nextAutosaveTime = Time.unscaledTime + Mathf.Max(1f, autosaveDebounceSeconds);
            nextPeriodicAutosaveTime = Time.unscaledTime + Mathf.Max(1f, periodicAutosaveSeconds);
            Debug.LogError($"Game save failed: {exception.Message}");
        }
    }

    private void LoadGame(LoadMode loadMode)
    {
        if (!TryReadSaveWithBackup(loadMode, out PrototypeSaveData saveData))
        {
            return;
        }

        isLoading = true;
        try
        {
            EnsureSaveLists(saveData);
            if (!string.Equals(saveData.visualScaleSignature,
                CurrentVisualScaleSignature, StringComparison.Ordinal))
            {
                // Older v2 saves contain the 78 cm prototype cases stacked at
                // 7.1 cm intervals. Keep each case's ID and X/Z location while
                // lowering it onto the same support at the new case thickness.
                UpgradeLegacyLooseCaseHeights(saveData.looseMediaItems);
                Debug.Log("Adjusted legacy CD stack heights for smaller jewel cases.");
            }

            ClearHeldItems();

            Dictionary<string, MediaItem> mediaItemsById = BuildMediaItemsById();
            Dictionary<string, ShelfSlot> shelvesById = BuildShelvesById();

            ResetSceneMediaToLoose(UnityObject.FindObjectsByType<MediaItem>(FindObjectsInactive.Exclude));
            ClearShelfOccupancy(shelvesById.Values);
            HashSet<string> restoredPhysicalIds = new();
            RestoreLooseItems(saveData.looseMediaItems, mediaItemsById, restoredPhysicalIds);
            RestoreShelvedItems(saveData.shelvedMediaItems, mediaItemsById,
                shelvesById, restoredPhysicalIds, saveData.shelfSlotLayoutVersion);

            // Existing v2 saves may contain only the original 140 slice IDs.
            // Append new catalogue stock without changing those IDs, shelf
            // positions, physical order, or any carried selected cases.
            int expanded = PrototypeSortingSetup.RestoreMissingExpansionStock(
                restoredPhysicalIds, mediaItemsById, shelvesById);
            expandedStockPendingSave = expanded > 0;
            if (expanded > 0)
            {
                Debug.Log("Kept your existing save and added " + expanded
                    + " newly introduced stock items.");
            }

            RestorePlayer(saveData.player);
            RestoreCarriedItems(saveData.carriedPhysicalIds,
                saveData.selectedCarriedIndex, mediaItemsById, restoredPhysicalIds);

            hasDirtyChanges = expandedStockPendingSave;
            nextAutosaveTime = Time.unscaledTime + Mathf.Max(2f, autosaveDebounceSeconds);
            nextPeriodicAutosaveTime = Time.unscaledTime + periodicAutosaveSeconds;
            Debug.Log($"Game loaded: {SavePath}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private static bool TryReadSaveWithBackup(LoadMode loadMode, out PrototypeSaveData saveData)
    {
        saveData = null;
        bool mainExists = File.Exists(SavePath);
        if (!mainExists)
        {
            // A crash during the atomic replacement can leave only the backup.
            if (File.Exists(BackupSavePath)
                && TryReadSaveFile(BackupSavePath, out saveData, out _))
            {
                Debug.LogWarning("Main save absent; recovered backup: " + BackupSavePath);
                return true;
            }

            if (loadMode == LoadMode.Manual)
            {
                Debug.LogWarning($"No save file found at {SavePath}.");
            }
            else
            {
                Debug.Log("No save file found; using authored prototype starting state.");
            }

            return false;
        }

        if (TryReadSaveFile(SavePath, out saveData, out string mainFailureReason))
        {
            return true;
        }

        Debug.LogWarning($"Could not load save file at {SavePath}: {mainFailureReason}");

        string backupFailureReason = string.Empty;
        if (File.Exists(BackupSavePath)
            && TryReadSaveFile(BackupSavePath, out saveData, out backupFailureReason))
        {
            Debug.LogWarning($"Loaded backup save from {BackupSavePath}.");
            return true;
        }

        if (File.Exists(BackupSavePath))
        {
            Debug.LogWarning($"Could not load backup save at {BackupSavePath}: {backupFailureReason}");
        }

        Debug.LogWarning("Falling back to authored prototype starting state.");
        return false;
    }

    private static bool TryReadSaveFile(string path, out PrototypeSaveData saveData, out string failureReason)
    {
        saveData = null;
        failureReason = string.Empty;

        try
        {
            saveData = JsonUtility.FromJson<PrototypeSaveData>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            failureReason = exception.Message;
            return false;
        }

        if (saveData == null)
        {
            failureReason = "save data was empty or unreadable";
            return false;
        }

        if (!IsSupportedVersion(saveData.version, out failureReason))
        {
            return false;
        }

        return true;
    }

    private static bool IsSupportedVersion(int version, out string failureReason)
    {
        if (version == SaveFormatVersion)
        {
            failureReason = string.Empty;
            return true;
        }

        failureReason = $"save format version {version} is not supported by this prototype";
        return false;
    }

    private static void SafeWriteSaveFile(string json)
    {
        Directory.CreateDirectory(Application.persistentDataPath);

        File.WriteAllText(TemporarySavePath, json);
        FileInfo temporaryFile = new(TemporarySavePath);
        if (!temporaryFile.Exists || temporaryFile.Length <= 0)
        {
            throw new IOException($"Temporary save file was not written correctly: {TemporarySavePath}");
        }

        if (!File.Exists(SavePath))
        {
            File.Move(TemporarySavePath, SavePath);
            return;
        }

        try
        {
            File.Replace(TemporarySavePath, SavePath, BackupSavePath);
        }
        catch (PlatformNotSupportedException)
        {
            ReplaceSaveWithBackupFallback();
        }
        catch (IOException)
        {
            ReplaceSaveWithBackupFallback();
        }
    }

    private static void ReplaceSaveWithBackupFallback()
    {
        File.Copy(SavePath, BackupSavePath, true);
        File.Delete(SavePath);
        File.Move(TemporarySavePath, SavePath);
    }

    private static void EnsureSaveLists(PrototypeSaveData saveData)
    {
        saveData.player ??= new PlayerSaveData();
        saveData.looseMediaItems ??= new List<LooseMediaItemSaveData>();
        saveData.shelvedMediaItems ??= new List<ShelvedMediaItemSaveData>();
        saveData.carriedPhysicalIds ??= new List<string>();
    }

    private static PlayerSaveData CapturePlayer()
    {
        SimpleFirstPersonController[] players =
            UnityObject.FindObjectsByType<SimpleFirstPersonController>(FindObjectsInactive.Exclude);

        if (players.Length == 0 || players[0] == null)
        {
            Debug.LogWarning("No player found while saving.");
            return new PlayerSaveData();
        }

        return new PlayerSaveData
        {
            position = players[0].WorldPosition,
            bodyRotation = players[0].BodyRotation,
            cameraLocalRotation = players[0].CameraLocalRotation
        };
    }

    private static void RestorePlayer(PlayerSaveData playerSaveData)
    {
        SimpleFirstPersonController[] players =
            UnityObject.FindObjectsByType<SimpleFirstPersonController>(FindObjectsInactive.Exclude);

        if (players.Length == 0 || players[0] == null)
        {
            Debug.LogWarning("No player found while loading.");
            return;
        }

        players[0].RestorePose(playerSaveData.position, playerSaveData.bodyRotation, playerSaveData.cameraLocalRotation);
    }

    private static Dictionary<string, MediaItem> BuildMediaItemsById()
    {
        Dictionary<string, MediaItem> mediaItemsById = new();
        MediaItem[] mediaItems = UnityObject.FindObjectsByType<MediaItem>(FindObjectsInactive.Exclude);

        for (int i = 0; i < mediaItems.Length; i++)
        {
            MediaItem mediaItem = mediaItems[i];
            if (mediaItem == null || string.IsNullOrWhiteSpace(mediaItem.PhysicalItemId))
            {
                Debug.LogWarning($"Media item '{mediaItem?.name}' has no physical item ID and cannot be restored from save data.");
                continue;
            }

            if (mediaItemsById.ContainsKey(mediaItem.PhysicalItemId))
            {
                Debug.LogWarning($"Duplicate scene physical item ID '{mediaItem.PhysicalItemId}' found while loading.");
                continue;
            }

            mediaItemsById.Add(mediaItem.PhysicalItemId, mediaItem);
        }

        return mediaItemsById;
    }

    private static Dictionary<string, ShelfSlot> BuildShelvesById()
    {
        Dictionary<string, ShelfSlot> shelvesById = new();
        ShelfSlot[] shelves = UnityObject.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);

        for (int i = 0; i < shelves.Length; i++)
        {
            ShelfSlot shelf = shelves[i];
            if (shelf == null || string.IsNullOrWhiteSpace(shelf.ShelfSectionId))
            {
                Debug.LogWarning($"Shelf slot '{shelf?.name}' has no shelf section ID and cannot be restored from save data.");
                continue;
            }

            if (shelvesById.ContainsKey(shelf.ShelfSectionId))
            {
                Debug.LogWarning($"Duplicate shelf section ID '{shelf.ShelfSectionId}' found while loading.");
                continue;
            }

            shelvesById.Add(shelf.ShelfSectionId, shelf);
        }

        return shelvesById;
    }

    private static void ResetSceneMediaToLoose(IEnumerable<MediaItem> mediaItems)
    {
        foreach (MediaItem mediaItem in mediaItems)
        {
            if (mediaItem == null)
            {
                continue;
            }

            PhysicalInteractable interactable = mediaItem.GetComponent<PhysicalInteractable>();
            if (interactable != null)
            {
                interactable.RestoreLooseState(mediaItem.transform.position, mediaItem.transform.rotation);
            }
        }
    }

    private static void ClearShelfOccupancy(IEnumerable<ShelfSlot> shelves)
    {
        foreach (ShelfSlot shelf in shelves)
        {
            shelf?.ClearPlacedItemsForLoad();
        }
    }

    private static void UpgradeLegacyLooseCaseHeights(
        List<LooseMediaItemSaveData> looseItems)
    {
        foreach (LooseMediaItemSaveData item in looseItems)
        {
            if (item == null)
            {
                continue;
            }

            Vector3 position = item.position;
            float castHeight = Mathf.Max(3f, position.y + 0.5f);
            RaycastHit[] hits = Physics.RaycastAll(
                new Vector3(position.x, castHeight, position.z),
                Vector3.down, 10f, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            float supportHeight = float.NegativeInfinity;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || hit.point.y > position.y + 0.06f)
                {
                    continue;
                }

                // Stock piles must not support themselves during migration.
                if (hit.collider.GetComponentInParent<MediaItem>() != null
                    || hit.collider.GetComponentInParent<ShelfSlot>() != null)
                {
                    continue;
                }

                supportHeight = Mathf.Max(supportHeight, hit.point.y);
            }

            if (float.IsNegativeInfinity(supportHeight))
            {
                continue;
            }

            float legacyLayers = Mathf.Max(0f,
                (position.y - supportHeight - 0.04f) / 0.071f);
            position.y = supportHeight + 0.011f
                + legacyLayers * PrototypeSortingSetup.PileLayerSpacing;
            item.position = position;
        }
    }

    private static void RestoreLooseItems(
        List<LooseMediaItemSaveData> looseItems,
        Dictionary<string, MediaItem> mediaItemsById,
        HashSet<string> restoredPhysicalIds)
    {
        for (int i = 0; i < looseItems.Count; i++)
        {
            LooseMediaItemSaveData looseItem = looseItems[i];
            if (looseItem == null || string.IsNullOrWhiteSpace(looseItem.physicalItemId))
            {
                Debug.LogWarning("A loose save entry has no physical item ID.");
                continue;
            }

            if (!restoredPhysicalIds.Add(looseItem.physicalItemId))
            {
                Debug.LogWarning($"Duplicate save entry for physical item ID '{looseItem.physicalItemId}'.");
                continue;
            }

            if (!mediaItemsById.TryGetValue(looseItem.physicalItemId, out MediaItem mediaItem))
            {
                Debug.LogWarning($"Save references missing physical item ID '{looseItem.physicalItemId}'.");
                continue;
            }

            PhysicalInteractable interactable = mediaItem.GetComponent<PhysicalInteractable>();
            if (interactable != null)
            {
                interactable.RestoreLooseState(looseItem.position, looseItem.rotation);
            }
        }
    }

    private static void RestoreShelvedItems(
        List<ShelvedMediaItemSaveData> shelvedItems,
        Dictionary<string, MediaItem> mediaItemsById,
        Dictionary<string, ShelfSlot> shelvesById,
        HashSet<string> restoredPhysicalIds,
        int savedShelfLayoutVersion)
    {
        shelvedItems.Sort(CompareShelvedItems);

        for (int i = 0; i < shelvedItems.Count; i++)
        {
            ShelvedMediaItemSaveData shelvedItem = shelvedItems[i];
            if (shelvedItem == null || string.IsNullOrWhiteSpace(shelvedItem.physicalItemId))
            {
                Debug.LogWarning("A shelved save entry has no physical item ID.");
                continue;
            }

            if (!restoredPhysicalIds.Add(shelvedItem.physicalItemId))
            {
                Debug.LogWarning($"Duplicate save entry for physical item ID '{shelvedItem.physicalItemId}'.");
                continue;
            }

            if (!mediaItemsById.TryGetValue(shelvedItem.physicalItemId, out MediaItem mediaItem))
            {
                Debug.LogWarning($"Save references missing physical item ID '{shelvedItem.physicalItemId}'.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(shelvedItem.shelfSectionId)
                || !shelvesById.TryGetValue(shelvedItem.shelfSectionId, out ShelfSlot shelf))
            {
                Debug.LogWarning($"Save references missing shelf section ID '{shelvedItem.shelfSectionId}' for '{shelvedItem.physicalItemId}'.");
                continue;
            }

            PhysicalInteractable interactable = mediaItem.GetComponent<PhysicalInteractable>();
            // Old v2 saves used shelfOrder as the physical layer-first index.
            // New v2 saves additionally store each CD's independent slot so
            // removing one case no longer shuffles other columns on reload.
            int physicalSlot = savedShelfLayoutVersion >= 1
                ? shelvedItem.physicalSlot : shelvedItem.shelfOrder;
            if (interactable == null || !shelf.RestorePlacedItem(interactable, physicalSlot))
            {
                Debug.LogWarning($"Could not restore '{shelvedItem.physicalItemId}' to shelf '{shelvedItem.shelfSectionId}'.");
            }
        }
    }

    private static void RestoreCarriedItems(List<string> carriedPhysicalIds,
        int selectedIndex, Dictionary<string, MediaItem> mediaItemsById,
        HashSet<string> restoredPhysicalIds)
    {
        if (carriedPhysicalIds == null || carriedPhysicalIds.Count == 0)
        {
            return;
        }

        PlayerInteraction[] players =
            UnityObject.FindObjectsByType<PlayerInteraction>(FindObjectsInactive.Exclude);
        if (players.Length == 0 || players[0] == null)
        {
            Debug.LogWarning("Could not restore held CD stack: player interaction is absent.");
            return;
        }

        HashSet<string> unique = new();
        List<PhysicalInteractable> carried = new();
        foreach (string id in carriedPhysicalIds)
        {
            if (string.IsNullOrWhiteSpace(id) || !unique.Add(id)
                || carried.Count >= PlayerInteraction.MaxCarryItems)
            {
                continue;
            }

            // A carried case must already be restored as loose stock; never
            // steal something from its authored shelf when loading a bad save.
            if (!restoredPhysicalIds.Contains(id)
                || !mediaItemsById.TryGetValue(id, out MediaItem media))
            {
                Debug.LogWarning("Could not restore missing carried CD: " + id);
                continue;
            }

            PhysicalInteractable item = media.GetComponent<PhysicalInteractable>();
            if (item != null && item.CurrentShelfSlot == null)
            {
                carried.Add(item);
            }
        }

        players[0].RestoreCarriedStock(carried, selectedIndex);
    }

    private static void ClearHeldItems()
    {
        PlayerInteraction[] playerInteractions =
            UnityObject.FindObjectsByType<PlayerInteraction>(FindObjectsInactive.Exclude);

        for (int i = 0; i < playerInteractions.Length; i++)
        {
            playerInteractions[i]?.ClearHeldItemForPersistence();
        }
    }

    private static int CompareMediaItemsByPhysicalId(MediaItem left, MediaItem right)
    {
        string leftId = left != null ? left.PhysicalItemId : string.Empty;
        string rightId = right != null ? right.PhysicalItemId : string.Empty;
        return string.Compare(leftId, rightId, StringComparison.OrdinalIgnoreCase);
    }

    private static int CompareShelvedItems(ShelvedMediaItemSaveData left, ShelvedMediaItemSaveData right)
    {
        string leftShelf = left != null ? left.shelfSectionId : string.Empty;
        string rightShelf = right != null ? right.shelfSectionId : string.Empty;
        int shelfComparison = string.Compare(leftShelf, rightShelf, StringComparison.OrdinalIgnoreCase);
        if (shelfComparison != 0)
        {
            return shelfComparison;
        }

        int leftOrder = left != null ? left.shelfOrder : -1;
        int rightOrder = right != null ? right.shelfOrder : -1;
        return leftOrder.CompareTo(rightOrder);
    }

    [Serializable]
    private sealed class PrototypeSaveData
    {
        public int version = SaveFormatVersion;
        public int shelfSlotLayoutVersion; // 0: legacy shelfOrder, 1: fixed facing/depth slots
        public string visualScaleSignature = string.Empty;
        public PlayerSaveData player = new();
        public List<LooseMediaItemSaveData> looseMediaItems = new();
        public List<ShelvedMediaItemSaveData> shelvedMediaItems = new();
        public List<string> carriedPhysicalIds = new();
        public int selectedCarriedIndex;
    }

    [Serializable]
    private sealed class PlayerSaveData
    {
        public Vector3 position;
        public Quaternion bodyRotation = Quaternion.identity;
        public Quaternion cameraLocalRotation = Quaternion.identity;
    }

    [Serializable]
    private sealed class LooseMediaItemSaveData
    {
        public string physicalItemId = string.Empty;
        public string state = LooseState;
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
    }

    [Serializable]
    private sealed class ShelvedMediaItemSaveData
    {
        public string physicalItemId = string.Empty;
        public string state = ShelvedState;
        public string shelfSectionId = string.Empty;
        public int shelfOrder = -1;
        public int physicalSlot = -1;
    }

    private enum LoadMode
    {
        Automatic,
        Manual
    }
}
