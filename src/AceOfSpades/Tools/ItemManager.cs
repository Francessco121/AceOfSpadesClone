using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;
using System;

/* ItemManager.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class ItemManager : IDisposable
    {
        public Player OwnerPlayer { get; }
        public World World { get; }

        public Item SelectedItem { get; private set; }
        public int SelectedItemIndex { get; private set; }

        public int MuzzleFlashIterations { get; set; }

        public bool DontUpdateItems;
        public bool IsReplicated { get; set; }

        static Key[] equipKeys = new Key[]
        {
            Key.Tilde,
            Key.Number1, Key.Number2, Key.Number3, Key.Number4, Key.Number5,
            Key.Number6, Key.Number7, Key.Number8, Key.Number9, Key.Number0
        };

        MasterRenderer renderer;
        SimpleCamera camera { get { return OwnerPlayer.GetCamera(); } }

        Item[] items;

        IMuzzleFlash muzzleFlash;
        ItemViewbob viewbob;

        public ItemManager(MasterRenderer renderer, Player ownerPlayer, World world, ItemViewbob viewbob)
        {
            this.renderer = renderer;
            this.viewbob = viewbob;
            OwnerPlayer = ownerPlayer;
            World = world;

            if (GlobalNetwork.IsClient)
                muzzleFlash = new ClientMuzzleFlash(renderer, ownerPlayer);
            else
                muzzleFlash = new ServerMuzzleFlash();

            SelectedItemIndex = -1;
        }

        public bool RefillAllGuns()
        {
            bool refilledAny = false;
            foreach (Item item in items)
            {
                Gun gun = item as Gun;
                if (gun != null)
                {
                    gun.CancelReload();

                    if (gun.CurrentMag < gun.GunConfig.MagazineSize
                        || gun.StoredAmmo < gun.GunConfig.MaxStoredMags * gun.GunConfig.MagazineSize)
                        refilledAny = true;

                    gun.CurrentMag = gun.GunConfig.MagazineSize;
                    gun.StoredAmmo = gun.GunConfig.MaxStoredMags * gun.GunConfig.MagazineSize;
                }
            }

            return refilledAny;
        }

        public IMuzzleFlash GetMuzzleFlash()
        {
            return muzzleFlash;
        }

        public void SetItems(Item[] items, int defaultItem)
        {
            foreach (Item item in items)
                World.AddGameObject(item);

            this.items = items;
            Equip(defaultItem);
        }

        public void Equip(int index, bool forceEquip = false)
        {
            if (SelectedItemIndex == index)
                return;

            SelectedItemIndex = index;

            if (SelectedItem != null)
                SelectedItem.OnUnequip();

            if (index < 0 || index >= items.Length)
                SelectedItem = null;
            else
            {
                Item item = items[index];

                if (forceEquip || item.CanEquip())
                {
                    SelectedItem = item;
                    SelectedItem.OnEquip();

                    // Ensure new item doesnt update if specified
                    if (DontUpdateItems)
                        SelectedItem.IsEnabled = false;

                    if (GlobalNetwork.IsClient)
                        viewbob.OnItemEquipped();
                }
            }

            if (GlobalNetwork.IsClient)
                muzzleFlash.Hide();
        }

        public void TryInvokePrimaryFire()
        {
            if (SelectedItem != null)
                SelectedItem.ForcePrimaryFire();
        }

        public bool CanPrimaryFire()
        {
            return SelectedItem != null && SelectedItem.CanPrimaryFire() && !OwnerPlayer.IsSprinting;
        }

        public void Update(bool primaryFire, bool primaryFireHold, bool secondaryFire, 
            bool secondaryFireHold, bool reload, float deltaTime)
        {
            // Equip an item
            if (!GlobalNetwork.IsServer && OwnerPlayer.AllowUserInput)
            {
                for (int i = 0; i < equipKeys.Length; i++)
                {
                    if (Input.GetKeyDown(equipKeys[i]))
                    {
                        Equip(i - 1);
                        break;
                    }
                }

                if (!OwnerPlayer.IsRenderingThirdperson && Input.ScrollDeltaY != 0)
                {
                    int eq = -1;
                    int selectedItemIndex = SelectedItemIndex;
                    for (int i = 0; i < items.Length; i++)
                    {
                        int eqmove = Input.ScrollDeltaY > 0 ? 1 : -1;
                        eq = selectedItemIndex - eqmove;
                        if (eq < 0) eq += items.Length;
                        if (eq >= items.Length) eq -= items.Length;

                        if (items[eq].CanEquip())
                            break;
                        else
                            selectedItemIndex = eq;
                    }

                    Equip(eq);
                }
            }

            if (SelectedItem != null)
            {
                // Handle primary fire input
                if (SelectedItem.Config.IsPrimaryAutomatic && primaryFireHold || primaryFire)
                    SelectedItem.PrimaryFire();

                // Handle secondary fire input
                if (SelectedItem.Config.IsSecondaryAutomatic && secondaryFireHold || secondaryFire)
                    SelectedItem.SecondaryFire();

                // Handle gun specific input
                if (SelectedItem.Type.HasFlag(ItemType.Gun))
                {
                    Gun gun = (Gun)SelectedItem;

                    if (reload)
                        gun.Reload();

                    // Update the muzzle flash
                    UpdateMuzzleFlash(gun, deltaTime);
                }
            }
        }

        public void UpdateReplicated(float deltaTime)
        {
            if (SelectedItem != null)
            {
                SelectedItem.UpdateReplicated(deltaTime);

                // Handle gun specific input
                if (SelectedItem.Type.HasFlag(ItemType.Gun))
                {
                    Gun gun = (Gun)SelectedItem;

                    // Update the muzzle flash
                    UpdateMuzzleFlash(gun, deltaTime);
                }
            }
        }

        void UpdateMuzzleFlash(Gun gun, float deltaTime)
        {
            if (!GlobalNetwork.IsServer)
            {
                ((ClientMuzzleFlash)muzzleFlash).Update(deltaTime);
            }

            if (GlobalNetwork.IsClient)
            {
                if (((ClientMuzzleFlash)muzzleFlash).UpdateReplicated(gun, MuzzleFlashIterations, deltaTime))
                {
                    gun.OnReplicatedPrimaryFire();
                    viewbob.ApplyKickback(gun.GunConfig.ModelKickback);
                    MuzzleFlashIterations--;
                }
            }
            else if (GlobalNetwork.IsServer)
            {
                ServerMuzzleFlash svMuzzleFlash = (ServerMuzzleFlash)muzzleFlash;
                if (svMuzzleFlash.Visible)
                {
                    svMuzzleFlash.Hide();
                    MuzzleFlashIterations++;
                }
            }
        }

        public void Draw(EntityRenderer entRenderer, ItemViewbob viewbob)
        {
            // Render item in hand
            if (SelectedItem != null)
            {
                if (SelectedItem.Type.HasFlag(ItemType.Gun))
                {
                    Gun gun = (Gun)SelectedItem;

                    // Render muzzle flash
                    ((ClientMuzzleFlash)muzzleFlash).Render(gun, entRenderer, viewbob);
                }

                SelectedItem.Draw(viewbob);
            }
        }

        public void Dispose()
        {
            muzzleFlash.Dispose();

            foreach (Item item in items)
                item.Dispose();
        }
    }
}
