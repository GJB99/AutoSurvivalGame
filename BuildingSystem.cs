using UnityEngine;
using System.Collections;
using TMPro;

public class BuildingSystem : MonoBehaviour
{
    public GameObject chestPrefab;
    public GameObject smelterPrefab;
    private PlayerInventory playerInventory;
    public TextMeshProUGUI messageText;
    public float messageDisplayTime = 2f;
    public float fabricationTime = 1f; // Time to fabricate an item

    void Start()
    {
        playerInventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (playerInventory != null && playerInventory.IsBuildingMenuVisible())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(TryBuildStonePickaxe());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(TryBuildIronPickaxe());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartCoroutine(TryBuildSmelter());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                StartCoroutine(TryBuildProcessor());
            }
        }
    }

    IEnumerator TryBuildChest()
    {
        if (playerInventory.CanBuild("Rock", 5))
        {
            playerInventory.RemoveItems("Rock", 5);
            ShowMessage("Fabricating Chest...");
            yield return new WaitForSeconds(fabricationTime);
            playerInventory.AddItem("Chest", 1);
            ShowMessage("Chest fabricated and added to inventory!");
        }
        else
        {
            ShowMessage("Not enough resources to build a chest!");
        }
    }

    IEnumerator TryBuildStonePickaxe()
    {
        if (playerInventory.CanBuild("Rock", 5))
        {
            playerInventory.RemoveItems("Rock", 5);
            ShowMessage("Crafting Stone Pickaxe...");
            yield return new WaitForSeconds(fabricationTime);
            playerInventory.AddItem("Stone Pickaxe", 1);
            ShowMessage("Stone Pickaxe crafted and added to inventory!");
        }
        else
        {
            ShowMessage("Not enough resources to craft a Stone Pickaxe!");
        }
    }

    IEnumerator TryBuildIronPickaxe()
    {
        if (playerInventory.CanBuild("Stone Pickaxe", 1) && playerInventory.CanBuild("Iron", 10))
        {
            playerInventory.RemoveItems("Stone Pickaxe", 1);
            playerInventory.RemoveItems("Iron", 10);
            ShowMessage("Crafting Iron Pickaxe...");
            yield return new WaitForSeconds(fabricationTime);
            playerInventory.AddItem("Iron Pickaxe", 1);
            ShowMessage("Iron Pickaxe crafted and added to inventory!");
        }
        else
        {
            ShowMessage("Not enough resources to craft an Iron Pickaxe!");
        }
    }

    IEnumerator TryBuildSmelter()
    {
        if (playerInventory.CanBuild("Copper", 5) && playerInventory.CanBuild("Iron", 5) && playerInventory.CanBuild("Rock", 10))
        {
            playerInventory.RemoveItems("Copper", 5);
            playerInventory.RemoveItems("Iron", 5);
            playerInventory.RemoveItems("Rock", 10);
            ShowMessage("Fabricating Smelter...");
            yield return new WaitForSeconds(fabricationTime);
            playerInventory.AddItem("Smelter", 1);
            ShowMessage("Smelter fabricated and added to inventory!");
        }
        else
        {
            ShowMessage("Not enough resources to build a Smelter!");
        }
    }

    IEnumerator TryBuildProcessor()
    {
        if (playerInventory.CanBuild("Iron Ingot", 10) && playerInventory.CanBuild("Copper Ingot", 10))
        {
            playerInventory.RemoveItems("Iron Ingot", 10);
            playerInventory.RemoveItems("Copper Ingot", 10);
            ShowMessage("Fabricating Processor...");
            yield return new WaitForSeconds(fabricationTime);
            playerInventory.AddItem("Processor", 1);
            ShowMessage("Processor fabricated and added to inventory!");
        }
        else
        {
            ShowMessage("Not enough resources to build a Processor!");
        }
    }

    void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            CancelInvoke("ClearMessage");
            Invoke("ClearMessage", messageDisplayTime);
        }
        Debug.Log(message);
    }

    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}