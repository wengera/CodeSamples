using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.IO;

namespace Player{
    public class Inventory : MonoBehaviour {
        public GameObject player;
        public PlayerStats p_stats;
        public string inventoryID = "";
        public List<Item> inventory;
        //public int inventorySize;
        private TextAsset ItemLibrary;
        private TextAsset ItemLibraryPaths;
        private StreamWriter invWriter;
        GameObject Grid;
        GameObject ItemButton;
        public bool display;
        public Transform game;
        
        void Awake(){
            game = GameObject.Find("Game").transform;
            inventory = new List<Item>();
            player = GameObject.Find("Player");
            p_stats = player.GetComponent<PlayerStats>();
            ItemButton = Resources.Load("ItemButton") as GameObject;
            Grid = transform.GetChild(0).transform.Find("InventoryGrid").gameObject;
            ItemLibrary = Resources.Load("InventoryUI/PlayerInventory") as TextAsset;
            ItemLibraryPaths = Resources.Load("InventoryUI/ItemLibraryPaths") as TextAsset;
            string library = getInventory();
            string[] lines = library.Split("\n"[0]);

            inventory = new List<Item>();
            for (int x = 0; x < lines.Length; ++x){
                List<string> parameters = lines[x].Split(","[0]).ToList();
                if (parameters[0] == "0"){
                    if (parameters[1] == "1"){
                        parameters.RemoveAt(0);
                        parameters.RemoveAt(0);
                        parameters.Add("" + x);
                        OneHanded item = new OneHanded(parameters);
                        inventory.Add(item);
                    }
                }else if (parameters[0] == "1"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Food item = new Food(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "2"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Drink item = new Drink(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "3"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    QuestItem item = new QuestItem(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "4"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Ammunition item = new Ammunition(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "5"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Material item = new Material(parameters);
                    inventory.Add(item);
                }
            }
        }

        string getItemPath(string itemName){
            string library = getInventory();
            string[] lines = library.Split("\n"[0]);
            for (int x = 0; x < lines.Length; ++x){
                List<string> parameters = lines[x].Split(","[0]).ToList();
                if (parameters[0] == itemName){
                    return parameters[1];
                }
            }
            return "-1";
        }

        GameObject getItem(Item item){
            string itemPath = "InventoryUI/" + getItemPath(item.getName()) + item.getName();
            Debug.Log(itemPath);
            GameObject obj = Resources.Load(itemPath) as GameObject;

            return obj;
        }
        
        void FixedUpdate(){
            if (display){
                //Follower cam_Script;
                //MyController p_Controller;
                displayInventory();
                display = false;
            }
        }
        
        
        
        void displayInventory(){
            game.SendMessage("MenusOpen");
            foreach(Transform child in Grid.transform) {
                    if (child.name != "Title"){
                        Destroy(child.gameObject);
                    }
                }
            for (int x = 0; x < inventory.Count; ++x){
                if (inventory[x] is OneHanded){
                    OneHanded item = inventory[x] as OneHanded;
                    GameObject temp = (GameObject)Instantiate(ItemButton);
                    temp.GetComponent<Button>().onClick.AddListener(() => selectItem(Int32.Parse(temp.name)));
                    temp.transform.parent = Grid.transform;
                    temp.transform.localScale = new Vector3(1f,1f,1f);
                    temp.transform.name = "" + item.getID();
                    temp.transform.GetChild(0).GetComponent<Text>().text = item.getDescription();
                }else if (inventory[x] is Food){
                    Food item = inventory[x] as Food;
                    GameObject temp = (GameObject)Instantiate(ItemButton);
                    temp.GetComponent<Button>().onClick.AddListener(() => selectItem(Int32.Parse(temp.name)));
                    temp.transform.parent = Grid.transform;
                    temp.transform.localScale = new Vector3(1f,1f,1f);
                    temp.transform.name = "" + item.getID();
                    temp.transform.GetChild(0).GetComponent<Text>().text = item.getDescription();
                }else if (inventory[x] is Drink){
                    Drink item = inventory[x] as Drink;
                    GameObject temp = (GameObject)Instantiate(ItemButton);
                    temp.GetComponent<Button>().onClick.AddListener(() => selectItem(Int32.Parse(temp.name)));
                    temp.transform.parent = Grid.transform;
                    temp.transform.localScale = new Vector3(1f,1f,1f);
                    temp.transform.name = "" + item.getID();
                    temp.transform.GetChild(0).GetComponent<Text>().text = item.getDescription();
                }else if (inventory[x] is QuestItem){
                    QuestItem item = inventory[x] as QuestItem ;
                    GameObject temp = (GameObject)Instantiate(ItemButton);
                    temp.GetComponent<Button>().onClick.AddListener(() => selectItem(Int32.Parse(temp.name)));
                    temp.transform.parent = Grid.transform;
                    temp.transform.localScale = new Vector3(1f,1f,1f);
                    temp.transform.name = "" + item.getID();
                    temp.transform.GetChild(0).GetComponent<Text>().text = item.getDescription();
                }else if (inventory[x] is Ammunition){
                    Ammunition item = inventory[x] as Ammunition ;
                    GameObject temp = (GameObject)Instantiate(ItemButton);
                    temp.GetComponent<Button>().onClick.AddListener(() => selectItem(Int32.Parse(temp.name)));
                    temp.transform.parent = Grid.transform;
                    temp.transform.localScale = new Vector3(1f,1f,1f);
                    temp.transform.name = "" + item.getID();
                    temp.transform.GetChild(0).GetComponent<Text>().text = item.getDescription();
                }else if (inventory[x] is Material){
                    Material item = inventory[x] as Material ;
                    GameObject temp = (GameObject)Instantiate(ItemButton);
                    temp.GetComponent<Button>().onClick.AddListener(() => selectItem(Int32.Parse(temp.name)));
                    temp.transform.parent = Grid.transform;
                    temp.transform.localScale = new Vector3(1f,1f,1f);
                    temp.transform.name = "" + item.getID();
                    temp.transform.GetChild(0).GetComponent<Text>().text = item.getDescription();
                }
                
            }
        }
        
        public void selectItem(int id){
            if (inventory[id] is OneHanded){
                OneHanded weap = inventory[id] as OneHanded;
                Debug.Log("Weapon Selected... [" + id + "]");
                player.GetComponent<PlayerCombatController>().EquipItem(weap);
            }else if (inventory[id] is Food){
                Debug.Log("Food Selected...[" + id + "]");
                GameObject f = (GameObject)Resources.Load("Food");
                Food invF = inventory[id] as Food;
                f.GetComponent<ConsumeFood>().Modifier = invF.getModifier();
                f.GetComponent<ConsumeFood>().Duration = invF.getDuration();
                Instantiate(f);
                inventory[id].updateCount(-1);
            }else if (inventory[id] is Drink){
                Debug.Log("Drink Selected...[" + id + "]");
                inventory[id].updateCount(-1);
            }else if (inventory[id] is Ammunition){
                Debug.Log("Ammunition Selected...[" + inventory[id].getName() + "]");
                inventory[id].updateCount(-1);
            }else if (inventory[id] is Material){
                Debug.Log("Material Selected...[" + inventory[id].getName() + "]");
                inventory[id].updateCount(-1);
            }
            Debug.Log("Inventory Count:" + inventory[id].getCount());
            if (inventory[id].getCount() > 0){
                display = true;
            }else{
                RemoveItem(id);
            }
        }
        
        public void RemoveItem(int i){
            inventory.RemoveAt(i);
        }

        public void removeAmountFromItem(string itemName, int count){
            int itemIndex = inventory.FindIndex(x => x.getName() == itemName);
            inventory[itemIndex].updateCount(-count);
            if (inventory[itemIndex].getCount() <= 0){
                RemoveItem(itemIndex);
            }
            saveInventory();
        }

        public bool checkItemQuantity(string name, int quantity){
            int itemIndex = inventory.FindIndex(x => x.getName() == name);
            Debug.Log("Item Index: " + itemIndex);
            if (itemIndex >= 0){
                return (inventory[itemIndex].getCount() >= quantity);
            }else{
                return false;
            }
        }

        public int getItemQuantity(string name){
            int itemIndex = inventory.FindIndex(x => x.getName() == name);
            Debug.Log("Item Index: " + itemIndex);
            if (itemIndex >= 0){
                return inventory[itemIndex].getCount();
            }else{
                return 0;
            }
        }
        
        public void updateInventory(){
            string library = getInventory();
            string[] lines = library.Split("\n"[0]);
            Debug.Log("Lines Length: " + lines.Length);
            inventory = new List<Item>();
            for (int x = 0; x < lines.Length; ++x){
                List<string> parameters = lines[x].Split(","[0]).ToList();
                if (parameters[0] == "0"){
                    if (parameters[1] == "1"){
                        parameters.Add("" + x);
                        parameters.RemoveAt(0);
                               parameters.RemoveAt(0);
                        OneHanded item = new OneHanded(parameters);
                        inventory.Add(item);
                    }
                }else if (parameters[0] == "1"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Food item = new Food(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "2"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Drink item = new Drink(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "3"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    QuestItem item = new QuestItem(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "4"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Ammunition item = new Ammunition(parameters);
                    inventory.Add(item);
                }else if (parameters[0] == "5"){
                    parameters.RemoveAt(0);
                    parameters.Add("" + x);
                    Material item = new Material(parameters);
                    inventory.Add(item);
                }
            }
        }
        
        public void itemRemoved(int i){
            for (int x = i; x < inventory.Count; ++x){
                if (inventory[x] is OneHanded){
                    OneHanded item = inventory[x] as OneHanded;
                    item.setID(item.getID() - 1);
                    Debug.Log("Item: " + item.getName() + ", " + item.getID());
                }
                if (inventory[x] is Food){
                    Food item = inventory[x] as Food;
                    item.setID(item.getID() - 1);
                    Debug.Log("Item: " + item.getName() + ", " + item.getID());
                }
                if (inventory[x] is Drink){
                    Drink item = inventory[x] as Drink;
                    item.setID(item.getID() - 1);
                    Debug.Log("Item: " + item.getName() + ", " + item.getID());
                }
                if (inventory[x] is Ammunition){
                    Ammunition item = inventory[x] as Ammunition;
                    item.setID(item.getID() - 1);
                    Debug.Log("Item: " + item.getName() + ", " + item.getID());
                }
                if (inventory[x] is Material){
                    Material item = inventory[x] as Material;
                    item.setID(item.getID() - 1);
                    Debug.Log("Item: " + item.getName() + ", " + item.getID());
                }
                
            }
        }

        public List<string> getItemParametersByID(int itemID){
            Debug.Log("Item ID: " + itemID);
            TextAsset itemLibrary = Resources.Load("InventoryUI/ItemLibrary") as TextAsset;
            string library = itemLibrary.text;
            string[] lines = library.Split("\n"[0]);

            List<string> param = new List<string>(lines[itemID].Split(","[0]).ToList());

            return param;
        }

        public void addItemByID(int itemID, int count){
            List<string> parameters = new List<string>(getItemParametersByID(itemID));
            Debug.Log("Parameters: " + string.Join(",", parameters.ToArray()));
            if (parameters[0] == "0"){
                if (parameters[1] == "1"){
                    parameters.RemoveAt(0);
                    parameters.RemoveAt(0);
                    parameters.Add("0");
                    OneHanded item = new OneHanded(parameters);
                    int index = inventory.FindIndex(x => x.getName() == item.getName());
                    if (index >= 0) 
                    {
                       inventory[index].updateCount(count); // element exists, do what you need
                    }else{
                        item.updateCount(count - 1);
                        inventory.Add(item);
                    }
                    
                }
            }else if (parameters[0] == "1"){
                parameters.RemoveAt(0);
                parameters.Add("1");
                Food item = new Food(parameters);

                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(count); // element exists, do what you need
                }else{
                    item.updateCount(count - 1);
                    inventory.Add(item);
                }
            }else if (parameters[0] == "2"){
                parameters.RemoveAt(0);
                parameters.Add("2");
                Drink item = new Drink(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(count); // element exists, do what you need
                }else{
                    item.updateCount(count - 1);
                    inventory.Add(item);
                }
            }else if (parameters[0] == "3"){
                parameters.RemoveAt(0);
                parameters.Add("3");
                QuestItem item = new QuestItem(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(count); // element exists, do what you need
                }else{
                    item.updateCount(count - 1);
                    inventory.Add(item);
                }
            }else if (parameters[0] == "4"){
                parameters.RemoveAt(0);
                parameters.Add("4");
                Ammunition item = new Ammunition(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(count); // element exists, do what you need
                }else{
                    item.updateCount(count - 1);
                    inventory.Add(item);
                }
            }else if (parameters[0] == "5"){
                parameters.RemoveAt(0);
                parameters.Add("5");
                Material item = new Material(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(count); // element exists, do what you need
                }else{
                    item.updateCount(count - 1);
                    inventory.Add(item);
                }
            }else{
                Debug.Log("Invalid Item");
            }
            
            saveInventory();

        }

        public void AddItem(List<string> i){
            List<string> parameters = new List<string>(i);
            Debug.Log("Parameters: " + string.Join(",", parameters.ToArray()));
            if (parameters[0] == "0"){
                if (parameters[1] == "1"){
                    
                    parameters.Add("" + inventory.Count);
                    parameters.RemoveAt(0);
                    parameters.RemoveAt(0);
                    
                    
                    OneHanded item = new OneHanded(parameters);

                    int index = inventory.FindIndex(x => x.getName() == item.getName());

                    if (index >= 0) 
                    {
                       inventory[index].updateCount(1); // element exists, do what you need
                    }else{
                        inventory.Add(item);
                    }
                    
                }
            }else if (parameters[0] == "1"){
                parameters.RemoveAt(0);
                parameters.Add("1");
                Food item = new Food(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(1); // element exists, do what you need
                }else{
                    inventory.Add(item);
                }
            }else if (parameters[0] == "2"){
                parameters.RemoveAt(0);
                parameters.Add("2");
                Drink item = new Drink(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(1); // element exists, do what you need
                }else{
                    inventory.Add(item);
                }
            }else if (parameters[0] == "3"){
                parameters.RemoveAt(0);
                parameters.Add("3");
                QuestItem item = new QuestItem(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(1); // element exists, do what you need
                }else{
                    inventory.Add(item);
                }
            }else if (parameters[0] == "4"){
                parameters.RemoveAt(0);
                parameters.Add("4");
                Ammunition item = new Ammunition(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(1); // element exists, do what you need
                }else{
                    inventory.Add(item);
                }
            }else if (parameters[0] == "5"){
                parameters.RemoveAt(0);
                parameters.Add("5");
                Material item = new Material(parameters);
                int index = inventory.FindIndex(x => x.getName() == item.getName());
                if (index >= 0) 
                {
                   inventory[index].updateCount(1); // element exists, do what you need
                }else{
                    inventory.Add(item);
                }
            }else{
                Debug.Log("Invalid Item");
            }
            
            saveInventory();
        }

        public void saveInventory(){
            Debug.Log("Saving Inventory...");
            List<Item> newInv = new List<Item>(inventory);
            Item[] lines = newInv.ToArray();
            string[] pInvFile = new string[lines.Length];
            string fileLines = "";
            for (int x = 0; x < lines.Length; ++x){
                pInvFile[x] = lines[x].toInv();
                if (x > 0){
                   fileLines += "\r\n"; 
                }
                fileLines += lines[x].toInv();
            }

            for (int x = 0; x < pInvFile.Length; ++x){
                Debug.Log(pInvFile[x]);
            }            

            writeToFile(fileLines);

            Debug.Log("Inventory Saved...");
        }
        
        void writeToFile(string lines){

            string fileName = "PlayerInventory.txt";

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
                var sr = File.CreateText(fileName);
                sr.Close();
            }
            
        }

        string getInventory(){
            StreamReader reader = new StreamReader(inventoryID + ".txt"); 
            return reader.ReadToEnd();
        }
        
    }
}