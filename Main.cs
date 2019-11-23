using System.Collections.Generic;
using System.Linq;
using TrackedRiderUtility;
using UnityEngine;

namespace CoasterAnarchy
{
    public class Main : IMod
    {
        Dictionary<string, TrackedRide> originalSettings = new Dictionary<string, TrackedRide>();
        Dictionary<string, List<SpecialSegmentSettings>> originalSegments = new Dictionary<string, List<SpecialSegmentSettings>>();

        public void onEnabled()
        {
            originalSettings = new Dictionary<string, TrackedRide>();
            originalSegments = new Dictionary<string, List<SpecialSegmentSettings>>();
            Global.NO_TRACKBUILDER_RESTRICTIONS = true;
            GameObject lsmFin = TrackRideHelper.GetTrackedRide("Floorless Coaster").meshGenerator.lsmFinGO;
            CoasterCarInstantiator[] carTypes = { };
            foreach (Attraction current in ScriptableSingleton<AssetManager>.Instance.getAttractionObjects())
            {
                var ride = current as TrackedRide;
                if (ride != null)
                {
                    carTypes = carTypes.Union(ScriptableSingleton<AssetManager>.Instance.getCoasterCarInstantiatorsFor(ride.getReferenceName())).ToArray();
                }
            }
            foreach (Attraction current in ScriptableSingleton<AssetManager>.Instance.getAttractionObjects())
            {
                var ride = current as TrackedRide;
                TrackedRide originalRide = new TrackedRide();
                if (ride != null)
                {
                    originalRide.canBuildRideCamera = ride.canBuildRideCamera;
                    ride.canBuildRideCamera = true;
                    originalRide.canChangeSpinLock = ride.canChangeSpinLock;
                    ride.canChangeSpinLock = true;
                    originalRide.canBuildMagneticKickers = ride.canBuildMagneticKickers;
                    ride.canBuildMagneticKickers = true;
                    originalRide.canBuildSlopeTransitionBrakes = ride.canBuildSlopeTransitionBrakes;
                    ride.canBuildSlopeTransitionBrakes = true;
                    originalRide.maxSegmentWidth = ride.maxSegmentWidth;
                    ride.maxSegmentWidth = 10f;
                    originalRide.canAdjustLiftSpeeds = ride.canAdjustLiftSpeeds;
                    ride.canAdjustLiftSpeeds = true;
                    originalRide.canHaveLSM = ride.canHaveLSM;
                    ride.canHaveLSM = true;
                    originalRide.canChangeLaps = ride.canChangeLaps;
                    ride.canChangeLaps = true;
                    originalRide.canCurveLifts = ride.canCurveLifts;
                    ride.canCurveLifts = true;
                    originalRide.canCurveSlopes = ride.canCurveSlopes;
                    ride.canCurveSlopes = true;
                    originalRide.canCurveVertical = ride.canCurveVertical;
                    ride.canCurveVertical = true;
                    originalRide.canHaveBrakes = ride.canHaveBrakes;
                    originalRide.canHaveBlockBrakes = ride.canHaveBlockBrakes;
                    if (ride.getUnlocalizedName().Contains("Coaster"))
                    {
                        ride.canHaveBrakes = true;
                        ride.canHaveBlockBrakes = true;
                    }
                    originalRide.canHaveHoldingBrakes = ride.canHaveHoldingBrakes;
                    ride.canHaveHoldingBrakes = true;
                    originalRide.canHaveLifts = ride.canHaveLifts;
                    if (ride.getUnlocalizedName().Contains("Coaster") || ride.everyUpIsLift)
                    {
                        ride.canHaveLifts = true;
                    }
                    originalRide.canInvertSlopes = ride.canInvertSlopes;
                    ride.canInvertSlopes = true;
                    originalRide.canChangeCarRotation = ride.canChangeCarRotation;
                    ride.canChangeCarRotation = true;
                    originalRide.everyUpIsLift = ride.everyUpIsLift;
                    ride.everyUpIsLift = false;
                    originalRide.maxBankingAngle = ride.maxBankingAngle;
                    ride.maxBankingAngle = 180;
                    originalRide.maxDeltaHeightDown = ride.maxDeltaHeightDown;
                    ride.maxDeltaHeightDown = float.MaxValue;
                    originalRide.maxDeltaHeightForLift = ride.maxDeltaHeightForLift;
                    ride.maxDeltaHeightForLift = float.MaxValue;
                    originalRide.maxDeltaHeightPerUnit = ride.maxDeltaHeightPerUnit;
                    ride.maxDeltaHeightPerUnit = float.MaxValue;
                    originalRide.maxDeltaHeightUp = ride.maxDeltaHeightUp;
                    ride.maxDeltaHeightUp = float.MaxValue;
                    originalRide.maxLiftSpeed = ride.maxLiftSpeed;
                    ride.maxLiftSpeed = 100f;
                    originalRide.maxSupportHeight = ride.maxSupportHeight;
                    ride.maxSupportHeight = int.MaxValue;
                    if (ride.meshGenerator.lsmFinGO == null)
                    {
                        ride.meshGenerator.lsmFinGO = lsmFin;
                    }
                    originalRide.min90CurveSize = ride.min90CurveSize;
                    ride.min90CurveSize = 1;
                    originalRide.minHalfHelixSize = ride.minHalfHelixSize;
                    ride.minHalfHelixSize = 1;
                    List<SpecialSegmentSettings> originalSpecialSegments = new List<SpecialSegmentSettings>();
                    foreach (SpecialSegmentSettings segment in ScriptableSingleton<AssetManager>.Instance.specialSegments)
                    {
                        Debug.Log(segment.name);
                        if(ride.specialSegments.hasSpecialSegment(segment))
                        {
                            originalSpecialSegments.Add(segment);
                        }
                        if (segment.name != "Splashdown" && segment.name != "HydraulicLaunchSystem")
                        {
                            ride.specialSegments.addSpecialSegment(segment);
                        }
                    }
                    originalRide.carTypes = ride.carTypes;
                    ride.carTypes = ride.carTypes.Union(carTypes).ToArray();
                    originalSettings.Add(ride.getUnlocalizedName(), originalRide);
                    originalSegments.Add(ride.getUnlocalizedName(), originalSpecialSegments);
                }

            }
        }

        public void onDisabled()
        {
            Global.NO_TRACKBUILDER_RESTRICTIONS = false;
            foreach (Attraction current in ScriptableSingleton<AssetManager>.Instance.getAttractionObjects())
            {
                var ride = current as TrackedRide;
                if (ride != null && originalSettings.ContainsKey(ride.getUnlocalizedName()))
                {
                    ride.canAdjustLiftSpeeds = originalSettings[ride.getUnlocalizedName()].canAdjustLiftSpeeds;
                    ride.canHaveLSM = originalSettings[ride.getUnlocalizedName()].canHaveLSM;
                    ride.canChangeLaps = originalSettings[ride.getUnlocalizedName()].canChangeLaps;
                    ride.canCurveLifts = originalSettings[ride.getUnlocalizedName()].canCurveLifts;
                    ride.canCurveSlopes = originalSettings[ride.getUnlocalizedName()].canCurveSlopes;
                    ride.canCurveVertical = originalSettings[ride.getUnlocalizedName()].canCurveVertical;
                    ride.canHaveBrakes = originalSettings[ride.getUnlocalizedName()].canHaveBrakes;
                    ride.canHaveBlockBrakes = originalSettings[ride.getUnlocalizedName()].canHaveBlockBrakes;
                    ride.canHaveHoldingBrakes = originalSettings[ride.getUnlocalizedName()].canHaveHoldingBrakes;
                    ride.canHaveLifts = originalSettings[ride.getUnlocalizedName()].canHaveLifts;
                    ride.canInvertSlopes = originalSettings[ride.getUnlocalizedName()].canInvertSlopes;
                    ride.canChangeCarRotation = originalSettings[ride.getUnlocalizedName()].canChangeCarRotation;
                    ride.everyUpIsLift = originalSettings[ride.getUnlocalizedName()].everyUpIsLift;
                    ride.maxBankingAngle = originalSettings[ride.getUnlocalizedName()].maxBankingAngle;
                    ride.maxDeltaHeightDown = originalSettings[ride.getUnlocalizedName()].maxDeltaHeightDown;
                    ride.maxDeltaHeightForLift = originalSettings[ride.getUnlocalizedName()].maxDeltaHeightForLift;
                    ride.maxDeltaHeightPerUnit = originalSettings[ride.getUnlocalizedName()].maxDeltaHeightPerUnit;
                    ride.maxDeltaHeightUp = originalSettings[ride.getUnlocalizedName()].maxDeltaHeightUp;
                    ride.maxLiftSpeed = originalSettings[ride.getUnlocalizedName()].maxLiftSpeed;
                    ride.maxSupportHeight = originalSettings[ride.getUnlocalizedName()].maxSupportHeight;
                    ride.min90CurveSize = originalSettings[ride.getUnlocalizedName()].min90CurveSize;
                    ride.minHalfHelixSize = originalSettings[ride.getUnlocalizedName()].minHalfHelixSize;
                    ride.carTypes = originalSettings[ride.getUnlocalizedName()].carTypes;
                    ride.canBuildRideCamera = originalSettings[ride.getUnlocalizedName()].canBuildRideCamera;
                    ride.canChangeSpinLock = originalSettings[ride.getUnlocalizedName()].canChangeSpinLock;
                    ride.canBuildMagneticKickers = originalSettings[ride.getUnlocalizedName()].canBuildMagneticKickers;
                    ride.canBuildSlopeTransitionBrakes = originalSettings[ride.getUnlocalizedName()].canBuildSlopeTransitionBrakes;
                    ride.maxSegmentWidth = originalSettings[ride.getUnlocalizedName()].maxSegmentWidth;
                    foreach (SpecialSegmentSettings segment in ScriptableSingleton<AssetManager>.Instance.specialSegments)
                    {
                        ride.specialSegments.removeSpecialSegment(segment);
                    }
                    foreach (SpecialSegmentSettings segment in originalSegments[ride.getUnlocalizedName()])
                    {
                        ride.specialSegments.addSpecialSegment(segment);
                    }
                }
            }
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
