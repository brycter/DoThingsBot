using DoThingsBot.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoThingsBot.FSM.States {
    class BotStartState : IBotState {
        public string Name { get => "BotStartState"; }
        public ItemBundle itemBundle;

        public BotStartState(ItemBundle items) {
            itemBundle = items;
        }

        public BotStartState() {
            throw new Exception("BotStartState() called without itemBundle");
        }

        public void Enter(Machine machine) {
            if (itemBundle.HasOwner()) {
                if (Spells.DoesAnySpellNeedRefresh(DoThingsBot.ConfigurationManager().GetWantedTinkerEnchantments(), DoThingsBot.ConfigurationManager().GetBuffRefreshTime())) {
                    ChatManager.Tell(itemBundle.GetOwner(), "hold on a sec, gonna buff :)");


                    if (itemBundle.GetCraftMode() == CraftMode.CheckSkills) {
                        itemBundle.SetEquipMode(EquipMode.Buff);
                        machine.ChangeState(new BotEquipItemsState(itemBundle));
                    }
                    else {
                        itemBundle.SetEquipMode(EquipMode.Buff);
                        machine.ChangeState(new BotEquipItemsState(itemBundle));
                    }
                }
                else {
                    if (itemBundle.GetCraftMode() == CraftMode.CheckSkills) {
                        itemBundle.SetEquipMode(EquipMode.Tinker);
                        machine.ChangeState(new BotEquipItemsState(itemBundle));
                    }
                    else {
                        itemBundle.SetEquipMode(EquipMode.Idle);
                        machine.ChangeState(new BotTradingState(itemBundle));
                    }
                }
            }
            else {
                itemBundle.SetEquipMode(EquipMode.Buff);
                itemBundle.SetCraftMode(CraftMode.None);

                machine.ChangeState(new BotEquipItemsState(itemBundle));
            }
        }

        public void Exit(Machine machine) {

        }

        public void Think(Machine machine) {

        }

        public ItemBundle GetItemBundle() {
            return itemBundle;
        }
    }
}
