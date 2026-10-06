using Backpack.Config;
using UnityEngine;
using HarmonyLib;

#if IL2CPP
using MelonLoader;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Levelling;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Product.Packaging;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Tools;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Phone;
using Il2CppSystem.Linq;
#elif MONO
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Storage;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.UI.Phone;
#endif

namespace Backpack;

#if IL2CPP
[RegisterTypeInIl2Cpp]
#endif
public class PlayerBackpack : MonoBehaviour
{
    public const string StorageName = "Backpack";
    public const int MaxStorageSlots = 128;

    private bool _backpackEnabled = true;
    private StorageEntity _storage;

#if IL2CPP
    public PlayerBackpack(IntPtr ptr) : base(ptr)
    {
    }
#endif

    public static PlayerBackpack Instance { get; private set; }

    public bool IsUnlocked => NetworkSingleton<LevelManager>.Instance.GetFullRank() >= Configuration.Instance.UnlockLevel;
    public bool IsOpen => Singleton<StorageMenu>.Instance != null
                          && Singleton<StorageMenu>.Instance.IsOpen
                          && Singleton<StorageMenu>.Instance.TitleLabel.text == StorageName;
#if IL2CPP
    public Il2CppSystem.Collections.Generic.List<ItemSlot> ItemSlots => _storage.ItemSlots.Cast<Il2CppSystem.Collections.Generic.IEnumerable<ItemSlot>>().ToList();
#elif MONO
    public List<ItemSlot> ItemSlots => _storage.ItemSlots.ToList();
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Logger.Warning("Multiple PlayerBackpack components found; discarding the duplicate.");
            Destroy(this);
            return;
        }

        var player = gameObject.GetComponentInParent<Player>();
        try
        {
            _storage = player?.GetBackpackStorage();
        }
        catch (InvalidOperationException)
        {
            _storage = null;
        }

        if (_storage == null)
        {
            Logger.Error("Player does not have a BackpackStorage component!");
            Destroy(this);
            return;
        }

        Logger.Info("Configuring backpack storage...");
        UpdateSize(Configuration.Instance.StorageSlots);
        Instance = this;
    }

    private void Update()
    {
        if (!_backpackEnabled || !IsUnlocked || !Input.GetKeyDown(Configuration.Instance.ToggleKey))
            return;

        try
        {
            if (IsOpen)
                Close();
            else
                Open();
        }
        catch (Exception e)
        {
            Logger.Error("Error toggling backpack: " + e.Message);
        }
    }

    public void SetBackpackEnabled(bool state)
    {
        if (!state)
            Close();

        _backpackEnabled = state;
    }

    public void Open()
    {
        var storageMenu = Singleton<StorageMenu>.Instance;
        if (!_backpackEnabled || !IsUnlocked || storageMenu == null
            || (Singleton<ManagementClipboard>.Instance?.IsEquipped ?? false)
            || storageMenu.IsOpen || (Phone.Instance?.IsOpen ?? false))
            return;

        storageMenu.SlotGridLayout.constraintCount = _storage.DisplayRowCount;
#if IL2CPP
        storageMenu.Open(_storage.Cast<IItemSlotOwner>(), StorageName, string.Empty, null);
#elif MONO
        storageMenu.Open(_storage, StorageName, string.Empty, null);
#endif
        _storage.SendAccessor(Player.Local.NetworkObject);
    }

    public void Close()
    {
        if (!_backpackEnabled || !IsOpen)
            return;

        Singleton<StorageMenu>.Instance.Close();
        _storage.SendAccessor(null);
    }

    public bool ContainsItemsOfInterest(EStealthLevel maxStealthLevel)
    {
        for (var i = 0; i < _storage.ItemSlots.Count; i++)
        {
#if IL2CPP
            var itemSlot = _storage.ItemSlots[new Index(i)].Cast<ItemSlot>();
#elif MONO
            var itemSlot = _storage.ItemSlots[i];
#endif
            if (itemSlot?.ItemInstance == null)
                continue;

#if IL2CPP
            var productInstance = itemSlot.ItemInstance.TryCast<ProductItemInstance>();
#elif MONO
            var productInstance = itemSlot.ItemInstance as ProductItemInstance;
#endif
            if (productInstance == null)
            {
                if (!IsLegal(itemSlot.ItemInstance.Definition))
                    return true;

                continue;
            }

            if (productInstance.AppliedPackaging == null || productInstance.AppliedPackaging.StealthLevel <= maxStealthLevel)
                return true;
        }

        return false;
    }

    private static bool IsLegal(object definition)
    {
        if (definition == null)
            return false;

        var definitionType = definition.GetType();
        var property = AccessTools.Property(definitionType, "LegalStatus")
                       ?? AccessTools.Property(definitionType, "legalStatus");
        var value = property?.GetValue(definition);
        if (value == null)
        {
            var field = AccessTools.Field(definitionType, "LegalStatus")
                        ?? AccessTools.Field(definitionType, "legalStatus");
            value = field?.GetValue(definition);
        }

        return string.Equals(value?.ToString(), "Legal", StringComparison.Ordinal);
    }

    /// <summary>
    /// Adds the specified number of slots to the backpack.
    /// </summary>
    /// <remarks>The maximum number of slots for storage is 128.</remarks>
    /// <param name="slotCount">The number of slots to add.</param>
    public void Upgrade(int slotCount)
    {
        if (slotCount is < 1 or > MaxStorageSlots)
            return;

        var newSlotCount = _storage.SlotCount + slotCount;
        if (newSlotCount > MaxStorageSlots)
        {
            Logger.Warning("Cannot upgrade backpack to more than {0} slots.", MaxStorageSlots);
            return;
        }

        UpdateSize(newSlotCount);
    }

    /// <summary>
    /// Removes the specified number of slots from the backpack.
    /// </summary>
    /// <param name="slotCount">The number of slots to remove.</param>
    /// <param name="force">If true, will remove slots even if they contain items.</param>
    public void Downgrade(int slotCount, bool force = false)
    {
        if (slotCount < 1)
            return;

        if (!force && slotCount >= _storage.SlotCount)
        {
            Logger.Warning("Cannot downgrade backpack to zero slots. A minimum of one must remain.");
            return;
        }

        var newSlotCount = _storage.SlotCount - slotCount;
        if (newSlotCount < 1)
            newSlotCount = 1;

        if (force)
        {
            UpdateSize(newSlotCount);
            return;
        }

        var isSafeToRemove = true;
        var removedSlots = _storage.ItemSlots.GetRange(newSlotCount, _storage.SlotCount - newSlotCount);
        for (var i = 0; i < removedSlots.Count; i++)
        {
#if IL2CPP
            var itemSlot = removedSlots[new Index(i)].Cast<ItemSlot>();
#elif MONO
            var itemSlot = removedSlots[new Index(i)] as ItemSlot;
#endif
            if (itemSlot?.ItemInstance == null)
                continue;

            Logger.Warning("Downgrading backpack will remove item: " + itemSlot.ItemInstance.Definition.name);
            isSafeToRemove = false;
        }

        if (!isSafeToRemove)
        {
            Logger.Warning("Cannot downgrade backpack due to items present in removed slots.");
            return;
        }

        UpdateSize(newSlotCount);
    }

    private void UpdateSize(int newSize)
    {
        _storage.SlotCount = newSize;
        _storage.DisplayRowCount = newSize switch
        {
            <= 20 => (int) Math.Ceiling(newSize / 5.0),
            <= 80 => (int) Math.Ceiling(newSize / 10.0),
            _ => (int) Math.Ceiling(newSize / 16.0)
        };

        if (_storage.ItemSlots.Count > newSize)
        {
            _storage.ItemSlots.RemoveRange(newSize, _storage.ItemSlots.Count - newSize);
            return;
        }

        for (var i = _storage.ItemSlots.Count; i < newSize; i++)
        {
            var itemSlot = new ItemSlot();
            var slotCountBeforeOwnerAssignment = _storage.ItemSlots.Count;
#if IL2CPP
            if (itemSlot.onItemDataChanged == null)
                itemSlot.onItemDataChanged = (Il2CppSystem.Action) _storage.ContentsChanged;
            else
                itemSlot.onItemDataChanged.CombineImpl((Il2CppSystem.Action) _storage.ContentsChanged);

            itemSlot.SetSlotOwner(_storage.Cast<IItemSlotOwner>());
#elif MONO
            itemSlot.onItemDataChanged += _storage.ContentsChanged;
            itemSlot.SetSlotOwner(_storage);
#endif

            // Some game versions register the slot during SetSlotOwner while others do not.
            if (_storage.ItemSlots.Count == slotCountBeforeOwnerAssignment)
                _storage.ItemSlots.Add(itemSlot);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null!;
        }
    }
}
