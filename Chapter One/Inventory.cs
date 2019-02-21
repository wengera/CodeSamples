using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using BOItem;
namespace Game.Inventory {

    public interface IInventory
    {
        List<Item> GetInventory();
        Item GetItem(int itemId);
        int GetItemId(string itemName);
        int GetItemIndex(int itemId);
        int GetEquippedItemIndex(int itemId);
        int GetEquippedItemIndex(EquipSlot slot);
        int GetEquippedItemId(EquipSlot slot);
        void RemoveItem(int itemId, int count);
        void AddItem(int itemId, int count);
        bool HasItemAmount(int itemId, int count);
        int GetItemQuantity(int itemId);
        void SwapItems(int firstItemIndex, int secondItemIndex);
        bool EquipItem(int itemId);
        bool UnEquipItem(int itemId);
        ArmourEquipSerializable SerializeEquip();
        void Save();

    }

    public class Inventory : IInventory
    {

        public static ItemDetailsObject[] itemDetailsLibrary;
        public static List<Item> playerInventory = new List<Item>();
        public static List<Item> equippedItems = new List<Item>();
        public static bool dataLoaded = false;

        public Inventory()
        {
            if (!Inventory.dataLoaded)
                LoadInventoryData();
        }

        public AttributesSerializable GetArmourAttributes()
        {
            //Debug.Log("Getting Armour Attributes");
            float armour = 0f;
            float magicResist = 0f;
            foreach (Item item in equippedItems)
            {
                if (item.GetItemDetails().combatData.isArmour())
                {
                    armour += item.GetItemDetails().combatData.statsDefensive.armour;
                    magicResist += item.GetItemDetails().combatData.statsDefensive.magicResist;
                }
            }
            return new AttributesSerializable(0, armour, magicResist, 0, 0, 0, 0, (new Level()));
        }

        //Returns a List of the Items in the player's inventory
        public List<Item> GetInventory(){
	    	return playerInventory;
	    }

	    //Returns an item object from the playerInventory
	    //This function seems pointless but I can't bring myself to get rid of it yet
	    public Item GetItem(int itemId){
	        foreach (Item item in playerInventory){
	            if (item.id == itemId)
	                return item;
	        }
	        return new Item();
	    }

        //Returns an item object from the playerInventory
        //This function seems pointless but I can't bring myself to get rid of it yet
        public Item GetEquippedItem(int itemId)
        {
            foreach (Item item in equippedItems)
            {
                if (item.id == itemId)
                    return item;
            }
            return new Item();
        }

        //Returns an item object from the playerInventory
        //This function seems pointless but I can't bring myself to get rid of it yet
        public Item GetEquippedItem(EquipSlot slot)
        {
            foreach (Item item in equippedItems)
            {
                if (item.itemDetails.combatData.equipSlot == slot)
                    return item;
            }
            return new Item();
        }

        //returns the item's index in the player inventory
        public int GetItemIndex(int itemId){
	    	for (int x = 0; x < playerInventory.Count; ++x){
	    		if (playerInventory[x].id == itemId)
	    			return x;
	    	}

	    	return -1;
	    }

	    //returns the item's index in the player's equipped items
	    public int GetEquippedItemIndex(int itemId){
	    	for (int x = 0; x < equippedItems.Count; ++x){
	    		if (equippedItems[x].id == itemId)
	    			return x;
	    	}

	    	return -1;
	    }

	    public int GetEquippedItemIndex(EquipSlot slot){
	    	for (int x = 0; x < equippedItems.Count; ++x){
	    		if (equippedItems[x].itemDetails.combatData.equipSlot == slot)
	    			return x;
	    	}

	    	return -1;
	    }

        public int GetEquippedItemId(EquipSlot slot)
        {
            for (int x = 0; x < equippedItems.Count; ++x)
            {
                if (equippedItems[x].itemDetails.combatData.equipSlot == slot)
                    return equippedItems[x].id;
            }

            return -1;
        }

        //Removes an amount of an item from the inventory
        public void RemoveItem(int itemId, int count){
	    	int itemIndex = GetItemIndex(itemId);

	    	if (itemIndex >= 0){
	    		playerInventory[itemIndex].count = Mathf.Max(0, playerInventory[itemIndex].count - count);

		    	//Remove if item count falls below 1
		    	if (playerInventory[itemIndex].count < 1){
		    		playerInventory.RemoveAt(itemIndex);

		    		//Check if removed item is equipped
		    		int equippedIndex = GetEquippedItemIndex(itemId);
		    		if (equippedIndex >= 0)
		    			equippedItems.RemoveAt(equippedIndex);

		    	}
	    	}
	    }

	    //Adds an amount of an item to the inventory
	    public void AddItem(int itemId, int count){
	    	int itemIndex = GetItemIndex(itemId);

	    	if (itemIndex < 0)
	    		playerInventory.Add(new Item(itemId, count));
    		else
    			playerInventory[itemIndex].count += count;
	    }

	    //Checks if the player has a certain amount of items in the inventory
	    public bool HasItemAmount(int itemId, int count){
	    	int itemIndex = GetItemIndex(itemId);

	    	if (itemIndex >= 0)
	    		return (playerInventory[itemIndex].count >= count);
    		else
    			return false;
	    } 
        
        //Returns the quantity of an item in the inventory
	    public int GetItemQuantity(int itemId){
	    	foreach (Item item in playerInventory)
            {
                if (item.id == itemId)
                    return item.count;
            }
            return 0;
	    }

        //Returns the quantity of an item in the inventory
	    public int GetItemId(string itemName){
	    	foreach (ItemDetailsObject item in itemDetailsLibrary)
            {
                if (item.name == itemName)
                    return item.id;
            }
            return -1;
	    }

	    //Swaps position of two items in player inventory
	    public void SwapItems(int firstItemIndex, int secondItemIndex){
	    	Item temp = playerInventory[firstItemIndex];
	    	playerInventory[firstItemIndex] = playerInventory[secondItemIndex];
	    	playerInventory[secondItemIndex] = temp;
	    }

        //Equips Item
        public bool EquipItem(int itemId){
            Debug.Log("Equip Item: " + itemId);
	    	Item toEquip = GetItem(itemId);

	    	if (toEquip.id != -1){
                //Verify item is equippable in the first place
                if (toEquip.itemDetails.combatData.equipSlot == EquipSlot.None)
		    		return false;

                //swap equipped items if item with same equip slot exists
		    	for (int x = 0; x < equippedItems.Count; ++x){
		    		if (equippedItems[x].itemDetails.combatData.equipSlot == toEquip.itemDetails.combatData.equipSlot)
                    {
		    			equippedItems[x] = toEquip;
		    			return true;
		    		}
		    	}

                //Add to equipped items
		    	equippedItems.Add(toEquip);

		    	return true;
	    	}else{
				Debug.Log("Item Not Found");
				return false;
	    	}
	    }

        //UnEquips Item
        public bool UnEquipItem(int itemId)
        {
            Debug.Log("UnEquip Item: " + itemId);
            Item toUnEquip = GetItem(itemId);

            if (toUnEquip.id != -1)
            {
                //Verify item is equippable in the first place
                if (toUnEquip.EquipSlot == "")
                    return false;

                //Find and remove equipped item
                for (int x = 0; x < equippedItems.Count; ++x)
                {
                    if (equippedItems[x].id == toUnEquip.id)
                    {
                        equippedItems.RemoveAt(x);
                        return true;
                    }
                }

                //Item was never equipped in the first place
                return true;
            }
            else
            {
                Debug.Log("Item Not Found");
                return false;
            }
        }

        public bool LoadAmmunition(int itemId = -1){
	    	//a specific ammunition was requested to be equipped
	    	if (itemId >= 0){
	    		int index = GetItemIndex(itemId);
	    		if (index >= 0){
	    			if (playerInventory[index].itemDetails.combatData.equipSlot == EquipSlot.Ammunition){
	    				EquipItem(itemId);
	    				return true;
	    			}else
	    				return false;
	    		}else
    				return false;
	    	}else{
	    		//Load first item in player inventory that is ammunition
	    		foreach(Item item in playerInventory){
	    			if (item.itemDetails.combatData.equipSlot == EquipSlot.Ammunition){
	    				LoadAmmunition(item.id);
	    			}
	    		}
	    		return false;
	    	}
	    }

	    //Subtracts x amount from item count based on itemId
	    //Drops if item count falls to or below 0
	    /*public void DropAmountFromItem(int itemId, int amount){
	    	Item item = GetItem(itemId);
	    	if (item.id != -1){
		    	int index = GetItemIndex(itemId);
		    	int itemCount = item.count;
		    	int newItemCount = itemCount - amount;

		    	if (newItemCount <= 0)
		    		playerInventory.RemoveAt(index);
	    		else
	    			playerInventory[index].count = newItemCount;
			}else
				Debug.Log("Item Not Found");
	    }

	    //Adds x amount to item count based on itemId
	    public void AddAmountToItem(int itemId, int amount){
	    	Item item = GetItem(itemId);
	    	if (item.id != -1){
	    		int index = GetItemIndex(itemId);
		    	int itemCount = item.count;
		    	int newItemCount = itemCount + amount;

				playerInventory[index].count = newItemCount;
	    	}else
				Debug.Log("Item Not Found");
	    }*/

	    //Saves the inventory
	    public void Save(){
	        Debug.Log("Saving Inventory...");

	        string inv_json = JsonHelper.ToJson<BaseItem>(playerInventory.ToArray());
	        writeInventoryToFile(inv_json);

        	string eqp_json = JsonHelper.ToJson<BaseItem>(equippedItems.ToArray());
	        writeEquippablesToFile(eqp_json);
	    }

	    //Prints the Inventory
	    public void PrintInventory(){
	    	foreach (Item i in playerInventory){
	            Debug.Log("Item: " + i.ToString());
	        }  
	    }

	    //Prints the Inventory
	    public void PrintEquipped(){
	    	foreach (Item i in equippedItems){
	            Debug.Log("Equipped: " + i.ToString());
	        }  
	    }

        public ArmourEquipSerializable SerializeEquip()
        {
            ArmourEquipSerializable armourEquip = new ArmourEquipSerializable();
            armourEquip.primary = GetEquippedItemId(EquipSlot.Primary);
            armourEquip.secondary = GetEquippedItemId(EquipSlot.Secondary);
            armourEquip.ammunition = GetEquippedItemId(EquipSlot.Ammunition);
            armourEquip.chest = GetEquippedItemId(EquipSlot.Chest);
            armourEquip.helmet = GetEquippedItemId(EquipSlot.Helmet);
            armourEquip.legs = GetEquippedItemId(EquipSlot.Legs);
            armourEquip.boots = GetEquippedItemId(EquipSlot.Boots);
            return armourEquip;
        }



        /*
			---------------------------------------------------------------------------------------------------------
	    	Test Functions
			---------------------------------------------------------------------------------------------------------
		*/

            public bool FireWeapon(){
				int ammoIndex = GetEquippedItemIndex(EquipSlot.Ammunition);

				if (ammoIndex < 0)
					if (LoadAmmunition())
						ammoIndex = GetEquippedItemIndex(EquipSlot.Ammunition);
					else{
						return false;
						Debug.Log("Out of Ammunition!");
					}

				Debug.Log("Ammo Index: " + ammoIndex);
				if (HasItemAmount(equippedItems[ammoIndex].id, 1)){
					Debug.Log("Fire!: " + (GetItem(equippedItems[ammoIndex].id).count - 1));
					RemoveItem(equippedItems[ammoIndex].id, 1);
					return true;
				}else{
					return false;
				}





			}


	    /*
			---------------------------------------------------------------------------------------------------------
	    	Don't need to look below this unless you want to understand how I implented the rest of the functionality
			---------------------------------------------------------------------------------------------------------
		*/






	    //Likely don't need to use (DO NOT DELETE THOUGH).
	    //This method is called fom the Item class to load the item's details
	    public ItemDetailsObject GetItemDetails(int itemId){
	        foreach (ItemDetailsObject item in itemDetailsLibrary){
	            if (item.id == itemId)
	                return item;
	        }
	        return new ItemDetailsObject();
	    }

	    //Function that calls: GetItemDetailsLibrary and GetPlayerInventory()
	    private void LoadInventoryData(){
            dataLoaded = true;
	        itemDetailsLibrary = GetItemDetailsLibrary();
	        playerInventory = GetPlayerInventory();
	        equippedItems = GetPlayerEquipped();
	    }

	    private ItemDetailsObject[] GetItemDetailsLibrary(){
	        StreamReader reader = new StreamReader(@"GameData\itemData.txt"); 
	        string json = reader.ReadToEnd();
	        reader.Close();
	        return JsonHelper.FromJson<ItemDetailsObject>(json);        
	    }

	    private List<Item> GetPlayerInventory(){
	        StreamReader reader = new StreamReader(@"GameData\playerInventory.txt"); 
            ItemFactory factory = new ItemFactory();
	        string json = reader.ReadToEnd();
        	reader.Close();
	        BaseItem[] baseItems = JsonHelper.FromJson<BaseItem>(json); 
	        List<Item> newInventory = new List<Item>();

	        for (int x = 0; x < baseItems.Length; ++x){
	           // Item i = new Item(baseItems[x].id, baseItems[x].count);
	            Item i = factory.CreateItem(baseItems[x].id, baseItems[x].count);
	            newInventory.Add(i);
	        }        
	        
	        return newInventory;
	    }

	    private List<Item> GetPlayerEquipped(){
	        StreamReader reader = new StreamReader(@"GameData\playerEquipped.txt"); 
	        string json = reader.ReadToEnd();
        	reader.Close();
            BaseItem[] baseItems = new BaseItem[] { };
            try
            {
                baseItems = JsonHelper.FromJson<BaseItem>(json);
            }catch
            {
                Debug.LogError("Failed to Load Base Item Json...");
            }
	        List<Item> newInventory = new List<Item>();

	        for (int x = 0; x < baseItems.Length; ++x){
	            Item i = new Item(baseItems[x].id, baseItems[x].count);
	            newInventory.Add(i);
	        }        
	        
	        return newInventory;
	    }
	        
	    private void writeInventoryToFile(string lines){

	        string fileName = @"GameData\playerInventory.txt";

	        if (File.Exists(fileName))
	        {
	            Debug.Log(fileName+" already exists.");

	            // Write the string to a file.
	            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
	            file.WriteLine(lines);

	            file.Close();

	            return;
	        }else{

	            Debug.Log("Creating File: " + fileName);

	            var vr = File.CreateText(fileName);
	            vr.Close();

	            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
	            List<BaseItem> newList = new List<BaseItem>();
	            //GameSaveSerializable g_save = new GameSaveSerializable();
	            file.WriteLine(JsonHelper.ToJson<BaseItem>(newList.ToArray()));

	            file.Close();
	        }
	        
	    }

	    private void writeEquippablesToFile(string lines){

	        string fileName = @"GameData\playerEquipped.txt";

	        if (File.Exists(fileName))
	        {
	            Debug.Log(fileName+" already exists.");

	            // Write the string to a file.
	            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
	            file.WriteLine(lines);

	            file.Close();

	            return;
	        }else{

	            Debug.Log("Creating File: " + fileName);

	            var vr = File.CreateText(fileName);
	            vr.Close();

	            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
	            //List<BaseItem> newList = new List<BaseItem>();
	            //GameSaveSerializable g_save = new GameSaveSerializable();
	            file.WriteLine(lines);

	            file.Close();
	        }
	        
	    }
	}
}

//Unity's Json Utility does not support Json Arrays so I found this custom Wrapper class to handle Serialization
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}