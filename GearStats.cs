using UnityEngine;

[System.Serializable]
public class GearStats
{
    public int physicalResistance = 0;
    public int heatResistance = 0;
    public int poisonResistance = 0;
    public int radiationResistance = 0;
    public int physicalDamage = 0;
    public int meleeAttackSpeed = 0;
    public int rangedAttackSpeed = 0;
    public int critChance = 0;
    public float critMultiplier = 0;
    public int moveSpeed = 0;

    public static GearStats GetGearStats(string itemName)
    {
        GearStats stats = new GearStats();
        
        switch (itemName)
        {
            case "Wood Helmet":
                stats.physicalResistance = 5;
                stats.heatResistance = 2;
                break;

            case "Iron Helmet":
                stats.physicalResistance = 5;
                stats.heatResistance = 2;
                break;
                
            case "Iron Armor":
                stats.physicalResistance = 10;
                stats.heatResistance = 5;
                break;
                
            case "Leather Boots":
                stats.physicalResistance = 2;
                stats.moveSpeed = 2;
                break;
                
            case "Ring of Power":
                stats.physicalDamage = 5;
                stats.critChance = 10;
                stats.critMultiplier = 0.2f;
                break;
                
            case "Speed Belt":
                stats.moveSpeed = 5;
                break;
                
            default:
                Debug.LogWarning($"No stats defined for item: {itemName}");
                break;
        }
        
        return stats;
    }
}