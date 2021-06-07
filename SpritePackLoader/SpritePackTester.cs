using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueLibsCore;
using SpritePackLoader.Hexagons;

namespace SpritePackLoader
{
	[ItemCategories(RogueCategories.Technology, RogueCategories.Usable, RogueCategories.NPCsCantPickUp)]
	public class SpritePackTester : CustomItem, IItemUsable
	{
		public override void SetupDetails()
		{
			Item.itemType = ItemTypes.Tool;
			Item.itemValue = 488755541;
			Item.initCount = 1;
			Item.rewardCount = 1;
			Item.hasCharges = true;
			Item.goesInToolbar = true;
			Item.LoadItemSprite("PropertyDeed");
		}
		private static bool displayed;
		private static bool activating;
		private static readonly List<InvItem> decoys = new List<InvItem>();
		private IEnumerable<Vector3> EnumerateLocationsNearby(float scale)
			=> Hexagon.Spiral(new Hexagon(0, 0, 0), int.MaxValue).Select(h => h.ToVector(scale));
		public void UseItem()
		{
			if (activating) return;
			activating = true;
			Item.database.StartCoroutine(UseItem2());
		}
		private IEnumerator UseItem2()
		{
			try
			{
				if (displayed = !displayed)
				{
					Vector3 center = Owner.tr.position;
					IEnumerator<Vector3> locationsEnumerator = EnumerateLocationsNearby(0.32f).GetEnumerator();

					foreach (ItemUnlock item in RogueLibsInternals.Unlocks.OfType<ItemUnlock>())
					{
						InvItem invItem = new InvItem { invItemName = item.Name };
						invItem.SetupDetails(false);
						invItem.invItemCount = 0;
						invItem.Categories.Add("Decoy");

						if (!locationsEnumerator.MoveNext()) break;
						Vector3 location = locationsEnumerator.Current + center;
						Item spawned = GameController.gameController.spawnerMain.SpawnItem(location, invItem);
						GameController.gameController.audioHandler.Play(spawned, "Spawn");
						decoys.Add(invItem);
						yield return new WaitForFixedUpdate();
					}
				}
				else
				{
					foreach (Item item in GameController.gameController.itemList.ToList())
						if (item.invItem?.Categories?.Contains("Decoy") == true)
						{
							GameController.gameController.audioHandler.Play(item, "Spawn");
							item.DestroyMe();
							yield return new WaitForFixedUpdate();
						}

					foreach (Agent obj in GameController.gameController.agentList)
					{
						InvDatabase db = obj.agentInvDatabase ?? obj.objectInvDatabase ?? obj.specialInvDatabase ?? obj.playerInvDatabase;
						bool playSound = false;
						if (db != null)
							foreach (InvItem item in db.InvItemList)
								if (item.Categories.Contains("Decoy"))
								{
									db.DestroyItem(item);
									playSound = true;
								}
						if (playSound) GameController.gameController.audioHandler.Play(db.objectReal, "Spawn");
					}

					decoys.Clear();
				}
			}
			finally
			{
				activating = false;
			}
		}
	}
}
