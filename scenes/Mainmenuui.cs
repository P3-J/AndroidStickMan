using Godot;
using System;
using System.Collections.Generic;

public partial class Mainmenuui : CanvasLayer
{
    
    [Export] Animroot animController;
    [Export] AudioStreamPlayer uiBoppp;
    Globals glob;
    
    private Dictionary<int, string> levels = new()
    {
        [0] = "res://scenes/prod/infinity_world.tscn",
    };
    public override void _Ready()
    {
        base._Ready();
        glob = GetNode<Globals>("/root/Globals");
    }



    [Export] VBoxContainer mainmenu;
    [Export] Control customizeRoot;
    [Export] RichTextLabel itemLabel;
    [Export] OptionButton OpButton;
    [Export] StickRoot stickmanModel;
    [Export] Node3D boardModel;
    [Export] Button buyButton;
    public void ManageUiBasedOnView(string viewName)
    {
        

        mainmenu.Visible = false;
        customizeRoot.Visible = false;

        switch (viewName)
        {
            
            case "customize":
                CustomizeScreenActivate();
                break;
            case "mainview":
                mainmenu.Visible = true;
                break;
            default:
                return;
        }


    }



    private void CustomizeScreenActivate()
    {
        customizeRoot.Visible = true;
        SelectionUiSetup();
    }


    private void ChangeModelVisibilityBasedOnOptionSelection()
    {
        
        switch (OpButton.Selected)
        {
            case (0 or 1):
                boardModel.Visible = false;
                stickmanModel.Visible = true;
                break;
            case (2):
                boardModel.Visible = true;
                stickmanModel.Visible = false;
                break;
        }
    }


    private void EquipCurrentItems()
    { 
        stickmanModel.SpawnHatOnHead(glob.equipedItems["head"]);
    }


    List<string> currentSelectionNames = [];
    string currentSelection = "";
    string category = "";
    private void SelectionUiSetup()
    {
        // this is also procced when changing view
        ChangeModelVisibilityBasedOnOptionSelection();
        EquipCurrentItems();
        Dictionary<string, Dictionary<string, Variant>> slotItems;
        currentSelectionNames = [];

        switch (OpButton.Selected)
        {
            case 0:
                category = "color";
                break;
            case 1:
                category = "head";
                break;
            case 2: 
                category = "board";
                break;
        }

        slotItems = glob.allItems[category];
        itemLabel.Text = glob.equipedItems[category];
        currentSelection = glob.equipedItems[category];

        foreach (string Name in slotItems.Keys)
        {
            currentSelectionNames.Add(Name);
        }

        int cItemPrice = (int)glob.allItems[category][glob.equipedItems[category]]["price"];
        bool cItemOwned = (bool)glob.allItems[category][glob.equipedItems[category]]["owned"];
        ManageBuyButtonText(cItemOwned, cItemPrice);
    }
    private void ManageBuyButtonText(bool owned, int price)
    {
        if (owned)
        {
            if (currentSelection == glob.equipedItems[category])
            {
                buyButton.Text = "Equipped";
                return;
            }
            buyButton.Text = "Equip";
            return;
        }
        buyButton.Text = "Buy " + price.ToString() + "G";
    }


    private void SelectionChange(string dir)
    {
        
        int direction = dir == "left" ? -1 : 1;
        int cItemIndex = currentSelectionNames.IndexOf(currentSelection);
        int count = currentSelectionNames.Count;
        int nextIndex = ((cItemIndex + direction) % count + count) % count;

        string nextItem = currentSelectionNames[nextIndex];

        Dictionary<string, Variant> nItemDict = glob.allItems[category][nextItem];

        currentSelection = nextItem;
        itemLabel.Text = nextItem;
        ManageBuyButtonText((bool)nItemDict["owned"], (int)nItemDict["price"]);
        SpawnItemBasedOnSelection(nextItem);

    }

    private void SpawnItemBasedOnSelection(string ItemName)
    {
        switch (OpButton.Selected)
        {
            case 0:
                //"color";
                break;
            case 1:
                //"head";
                stickmanModel.SpawnHatOnHead(ItemName);
                break;
            case 2: 
                //"board";
                break;
        }
    }

    private void BuyCurrentlySelectedItem()
    {
        
        Dictionary<string, Variant> cItem = glob.allItems[category][currentSelection];

        bool owned = (bool)cItem["owned"];
        int price = (int)cItem["price"];

        if (owned) {
            
            if (currentSelection != glob.equipedItems[category])
            {
                glob.equipedItems[category] = currentSelection;
                ManageBuyButtonText(true, 0);
            }
            return;
        }
        if (glob.playerGold < price) return;
        
        glob.playerGold -= price;
        GD.Print("gleft ", glob.playerGold.ToString());
        cItem["owned"] = true;
        ManageBuyButtonText(true, 0);

    }

    private void PlayUiSound()
    {
        uiBoppp.Play();
    }

    private void _on_left_pressed()
    {
        SelectionChange("left");
        PlayUiSound();
    }

    private void _on_right_pressed()
    {
        SelectionChange("right");
        PlayUiSound();
    }

    private void _on_options_item_selected(int Selection)
    {
        SelectionUiSetup();
        PlayUiSound();
    }

    private void _on_buybutton_pressed()
    {
        BuyCurrentlySelectedItem();
        PlayUiSound();
    }

    private void _on_go_to_menu_pressed()
    {
        string screen = "mainview";
        animController.RotateCamTo(screen, 90f);
        ManageUiBasedOnView(screen);
        PlayUiSound();
    }

    public void _on_play_button_pressed()
    {
        animController.RotateCamTo("rooftoplevel", 50f);
        GetTree().ChangeSceneToFile(levels[0]);
        PlayUiSound();
    }

    public void _on_cus_button_pressed()
    {
        string screen = "customize";
        animController.RotateCamTo(screen, 52f);
        ManageUiBasedOnView(screen);
        PlayUiSound();
    }


}
