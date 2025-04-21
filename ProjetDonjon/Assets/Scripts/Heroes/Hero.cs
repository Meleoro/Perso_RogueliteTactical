using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Unit
    {
    [Header("Parameters")]
    public Inventory heroInventoryPrefab;
    [SerializeField] private HeroData heroData;

    [Header("Public Infos")]
    public HeroData HeroData { get { return heroData; } }
    public Loot[] EquippedLoot { get { return equippedLoot; } }
    public Inventory Inventory { get { return inventory; } }

    [Header("Private Infos")]
    private Loot[] equippedLoot = new Loot[6];
    private Inventory inventory;

    [Header("References")]
    [SerializeField] private HeroController _controller;


    #region Battle Functions

    public override void EnterBattle(BattleTile startTile)
    {

    }

    public override void ExitBattle()
    {

    }

    #endregion


    #region Inventory Functions

    public void SetupInventory(Inventory inventory)
    {
        this.inventory = inventory;
    }

    public void AddEquipment(Loot loot, int equipSlotIndex)
    {
        equippedLoot[equipSlotIndex] = loot;
    }

    public void RemoveEquipment(Loot removedLoot, int unequipSlotIndex)
    {
        equippedLoot[unequipSlotIndex] = null;
    }

    #endregion
}
