using System.Collections.Generic;
using System.Linq;
using TrackedRiderUtility;
using UnityEngine;

namespace CoasterAnarchy
{
    public class Main : IMod
    {

        public void onEnabled()
        {
            GameObject lsmFin = TrackRideHelper.GetTrackedRide("Floorless Coaster").meshGenerator.lsmFinGO;
            TrackRideHelper.GetTrackedRide("Wooden Coaster").canHaveLSM = true;
            TrackRideHelper.GetTrackedRide("Wooden Coaster").meshGenerator.lsmFinGO = lsmFin;
        }

        public void onDisabled()
        {

        }

        public string Name => "Coaster Anarchy";

        public string Description => "Allows more track pieces on more coasters...";

        string IMod.Identifier => "Marnit@ParkitectCoasterAnarchy";


        public string Path
        {
            get
            {
                return ModManager.Instance.getModEntries().First(x => x.mod == this).path;
            }
        }
    }
}
