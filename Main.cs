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
            SpecialSegmentsList specials = TrackRideHelper.GetTrackedRide("Floorless Coaster").specialSegments;
            CoasterCarInstantiator[] carTypes = { };
            foreach (Attraction current in ScriptableSingleton<AssetManager>.Instance.getAttractionObjects())
            {
                var ride = current as TrackedRide;
                if (ride != null)
                {
                    carTypes = carTypes.Union(ride.carTypes).ToArray();
                }
            }
            foreach (Attraction current in ScriptableSingleton<AssetManager>.Instance.getAttractionObjects())
            {
                var ride = current as TrackedRide;
                if (ride != null)
                {
                    ride.canAdjustLiftSpeeds = true;
                    ride.canHaveLSM = true;
                    ride.canChangeLaps = true;
                    ride.canCurveLifts = true;
                    ride.canCurveSlopes = true;
                    ride.canCurveVertical = true;
                    if (ride.getUnlocalizedName().Contains("Coaster"))
                    {
                        ride.canHaveBrakes = true;
                    }
                    ride.canHaveHoldingBrakes = true;
                    if (ride.getUnlocalizedName().Contains("Coaster") || ride.everyUpIsLift)
                    {
                        ride.canHaveLifts = true;
                    }
                    ride.canInvertSlopes = true;
                    ride.canChangeCarRotation = true;
                    ride.everyUpIsLift = false;
                    ride.maxBankingAngle = 180;
                    ride.maxDeltaHeightDown = float.MaxValue;
                    ride.maxDeltaHeightForLift = float.MaxValue;
                    ride.maxDeltaHeightPerUnit = float.MaxValue;
                    ride.maxDeltaHeightUp = float.MaxValue;
                    ride.maxLiftSpeed = float.MaxValue;
                    ride.maxSupportHeight = int.MaxValue;
                    ride.meshGenerator.lsmFinGO = lsmFin;
                    ride.min90CurveSize = 1;
                    ride.minHalfHelixSize = 1;
                    if (ride.getUnlocalizedName() != "Hydraulically-Launched Coaster")
                    {
                        ride.specialSegments = specials;
                    }
                    ride.carTypes = ride.carTypes.Union(carTypes).ToArray();
                }

            }
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
