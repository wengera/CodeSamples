using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using BOItem;
using System.Linq;
using TMPro;
using Game.Inventory;
using System.Collections;
using System.Collections.Specialized;
using UnityStandardAssets.CrossPlatformInput;

namespace UI.Menus
{
    public class UIToolbelt : UIManagerBase
    {
        private Transform menuContainer;
        private Image consumableImage;
        private Image weaponImage;

        void Awake()
        {
            menuContainer = transform.Find("MenuContainer");
            consumableImage = transform.Find("Consumable").Find("Image").GetComponent<Image>();
            weaponImage = transform.Find("Weapon").Find("Image").GetComponent<Image>();
            Inventory.weaponToolbelt.Tools.CollectionChanged += UpdateWeaponUI;
            Inventory.consumableToolbelt.Tools.CollectionChanged += UpdateConsumableUI;
            StartCoroutine(WaitForInventory());
        }

        private IEnumerator WaitForInventory()
        {
            do
            {
                yield return null;
            } while (!ItemFactory.Loaded);

            Inventory.consumableToolbelt.CycleForward();
            Inventory.weaponToolbelt.CycleForward();

            yield return null;
        }

        void OnEnable()
        {
            if (menuContainer == null)
                menuContainer = transform.Find("MenuContainer");

            if (consumableImage == null)
                consumableImage = transform.Find("Consumable").Find("Image").GetComponent<Image>();

            if (weaponImage == null)
                weaponImage = transform.Find("Weapon").Find("Image").GetComponent<Image>();
        }

        void Update()
        {
            if (ConsumableCycleForwardDown())
            {
                Debug.Log($"Consumable Toolbelt Forward");

                Inventory.consumableToolbelt.CycleForward();
                UpdateConsumableUI();
            }
            else if (WeaponCycleForwardDown())
            {
                Debug.Log($"Weapon Toolbelt Forward");

                Inventory.weaponToolbelt.CycleForward();
                CoreGameObjects.Value.PlayerActor.playerCombatController.Equip();
                UpdateWeaponUI();
            }else if (ConsumableCycleBackwardDown())
            {
                Debug.Log($"Consumable Toolbelt Backward");

                Inventory.consumableToolbelt.CycleBackward();
                UpdateConsumableUI();
            }
            else if (WeaponCycleBackwardDown())
            {
                Debug.Log($"Weapon Toolbelt Backward");

                Inventory.weaponToolbelt.CycleBackward();
                CoreGameObjects.Value.PlayerActor.playerCombatController.Equip();
                UpdateWeaponUI();
            }

        }

        private void UpdateWeaponUI(object? sender = null, NotifyCollectionChangedEventArgs? e = null)
        {
            Debug.Log("[Toolbelt] Update Weapon UI");
            if (Inventory.weaponToolbelt.SelectedItem != null)
            {
                weaponImage.sprite = Inventory.weaponToolbelt.SelectedItem.GetIcon();
                weaponImage.color = new Color(255, 255, 255, 1);
            }
            else
            {
                weaponImage.sprite = null;
                weaponImage.color = new Color(0, 0, 0, 0);
            }
        }
        private void UpdateConsumableUI(object? sender = null, NotifyCollectionChangedEventArgs? e = null)
        {
            Debug.Log("[Toolbelt] Update Consumable UI");
            if (Inventory.consumableToolbelt.SelectedItem != null)
            {
                consumableImage.color = new Color(1, 1, 1, 1);
                consumableImage.sprite = Inventory.consumableToolbelt.SelectedItem.GetIcon();
            }
            else
            {
                consumableImage.sprite = null;
                consumableImage.color = new Color(0, 0, 0, 0);
            }
        }
        private static bool WeaponCycleForwardDown() {
            var controllerInput = CrossPlatformInputManager.GetAxis("DPAD - Horizontal") > 0;
            var keyboardInput = Input.GetAxis(nameof(UserKeyBinds.ToolbeltForward)) > 0 && Input.GetAxis(nameof(UserKeyBinds.ActionModifier1)) <= 0;

            return CoreGameObjects.Value.GameController.AllowPlayerInput() && (controllerInput || keyboardInput);
        }
        private static bool WeaponCycleBackwardDown() {
            var controllerInput = CrossPlatformInputManager.GetAxis("DPAD - Horizontal") < 0;
            var keyboardInput = Input.GetAxis(nameof(UserKeyBinds.ToolbeltForward)) < 0 && Input.GetAxis(nameof(UserKeyBinds.ActionModifier1)) <= 0;

            return CoreGameObjects.Value.GameController.AllowPlayerInput() && (controllerInput || keyboardInput);
        }
        private static bool ConsumableCycleForwardDown() {
            var controllerInput = CrossPlatformInputManager.GetAxis("DPAD - Vertical") > 0;
            var keyboardInput = Input.GetAxis(nameof(UserKeyBinds.ToolbeltForward)) > 0 && Input.GetAxis(nameof(UserKeyBinds.ActionModifier1)) > 0;

            return CoreGameObjects.Value.GameController.AllowPlayerInput() && (controllerInput || keyboardInput);
        }
        private static bool ConsumableCycleBackwardDown()
        {
            var controllerInput = CrossPlatformInputManager.GetAxis("DPAD - Vertical") < 0;
            var keyboardInput = Input.GetAxis(nameof(UserKeyBinds.ToolbeltForward)) < 0 && Input.GetAxis(nameof(UserKeyBinds.ActionModifier1)) > 0;

            return CoreGameObjects.Value.GameController.AllowPlayerInput() && (controllerInput || keyboardInput);
        }

    }
}