using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Protected Stats")]
    protected int currentHealth;
    protected int currentMaxHealth;
    protected int currentEnergy;
    protected int currentMaxEnergy;
    protected int currentStrength;
    protected int currentSpeed;
    protected int currentLuck;

    [Header("Protected Other Infos")]
    protected BattleTile currentTile;


    [Header("Public Infos")]
    public int CurrentHealth { get { return currentHealth; } }
    public int CurrentMaxHealth { get { return currentMaxHealth; } }
    public int CurrentStrength { get { return currentStrength; } }
    public int CurrentSpeed { get { return currentSpeed; } }


    [Header("References")]
    [SerializeField] private UnitUI _ui;



    #region Unit Infos Functions

    public void InitialiseUnitInfos(int maxHealth, int maxEnergy, int strength, int speed, int luck)
    {
        currentHealth = maxHealth;
        currentMaxHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentMaxEnergy = maxEnergy;
        currentStrength = strength;
        currentSpeed = speed;
        currentLuck = luck;
    }

    public void ActualiseUnitInfos(int newMaxHealth, int newStrength, int newSpeed, int newLuck)
    {
        currentMaxHealth = newMaxHealth;
        currentStrength = newStrength;
        currentSpeed = newSpeed;
        currentLuck = newLuck;
    }

    #endregion


    #region Move Functions

    public IEnumerator MoveUnitCoroutine(List<BattleTile> pathTiles)
    {
        for(int i = 0; i < pathTiles.Count; i++)
        {
            MoveUnit(pathTiles[i]);

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void MoveUnit(BattleTile tile)
    {
        currentTile = tile;

        transform.position = tile.transform.position;
    }

    #endregion


    #region Enter / Exit Functions

    public virtual void EnterBattle(BattleTile startTile)
    {

    }

    public virtual void ExitBattle()
    {

    }

    #endregion

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth < 0)
            Debug.Log("La Mooooort");
    }
}
