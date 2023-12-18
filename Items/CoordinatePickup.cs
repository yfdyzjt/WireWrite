using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WireWrite.Items
{
    public class CoordinatePickup : ModItem
    {
        // The Display Name and Tooltip of this item can be edited in the Localization/en-US_Mods.WireWrite.hjson file.
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Master;
            Item.value = Item.sellPrice(platinum: 100);
            Item.mech = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.Register();
        }

        public override bool? UseItem(Player player)
        {
            Main.NewText($"You clicked: {Player.tileTargetX},{Player.tileTargetY}");
            Main.chatText += $"{Player.tileTargetX},{Player.tileTargetY}";
            return true;
        }

    }
}