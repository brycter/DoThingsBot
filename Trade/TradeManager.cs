using System;
using System.IO;
using System.Collections.ObjectModel;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace DoThingsBot.Trade {

    class TradeManager : IDisposable {
        public TradeManager() {
            try {
                CoreManager.Current.WorldFilter.ResetTrade += new EventHandler<ResetTradeEventArgs>(WorldFilter_ResetTrade);
                CoreManager.Current.WorldFilter.EndTrade += new EventHandler<EndTradeEventArgs>(WorldFilter_EndTrade);
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private bool disposed;
        private WorldObject tradePartner;
        

        public void Dispose() {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!disposed) {
                if (disposing) {

                    CoreManager.Current.WorldFilter.ResetTrade -= new EventHandler<ResetTradeEventArgs>(WorldFilter_ResetTrade);
                    CoreManager.Current.WorldFilter.EndTrade -= new EventHandler<EndTradeEventArgs>(WorldFilter_EndTrade);
                }

                // Indicate that the instance has been disposed.
                disposed = true;
            }
        }

        public void SetTradePartner(WorldObject partner) {
            tradePartner = partner;
        }

        public void StartTrade() {
            CoreManager.Current.Actions.UseItem(tradePartner.Id, 0);
            //CoreManager.Current.Actions.
            //CoreManager.Current.Actions.SelectItem(tradePartner.Id);

            //CoreManager.Current.
            //CoreManager.Current.WorldFilter.
        }

        void WorldFilter_ResetTrade(object sender, ResetTradeEventArgs e) {
            try {

                int traderId = 0;

                // This is a little trick.
                // When someone initiates the trade, they will have both the trader and tradee ID's be accurate.
                // When someone initiates a trade with you, you will have them as both ID's.
                // This will prevent our mule from auto-muling back to us.
                /*
				if (e.TradeeId == CoreManager.Current.CharacterFilter.Id)
					traderId = e.TraderId;
				else if (e.TraderId == CoreManager.Current.CharacterFilter.Id)
					traderId = e.TradeeId;
                */

                if (tradePartner == null) {
                    return;
                }

                if (tradePartner.ObjectClass == ObjectClass.Player) {
                    traderId = tradePartner.Id;
                }

                if (traderId == 0 || traderId == CoreManager.Current.CharacterFilter.Id)
                    return;

                //Start(fileInfo);
            }
            catch (FileNotFoundException) { Util.WriteToChat("<{" + PluginCore.PluginName + "}>: " + "Unable to start Auto Add to Trade. Is Virindi Tank running?"); }
            catch (Exception ex) { Util.LogException(ex); }
        }

        void WorldFilter_EndTrade(object sender, EndTradeEventArgs e) {
            try {
                
            }
            catch (Exception ex) { Util.LogException(ex); }
        }
    }
}
