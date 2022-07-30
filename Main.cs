using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrackedRiderUtility;
using UnityEngine;
using MiniJSON;
using System.Timers;

namespace CoasterAnarchy
{
    public class Main : AbstractMod, IModSettings
    {
        Dictionary<string, TrackedRide> originalSettings = new Dictionary<string, TrackedRide>();
        Dictionary<string, List<SpecialSegmentSettings>> originalSegments = new Dictionary<string, List<SpecialSegmentSettings>>();
        Dictionary<string, Dictionary<string, GameObject>> originalObjects = new Dictionary<string, Dictionary<string, GameObject>>();
        Dictionary<string, Color[]> originalColors = new Dictionary<string, Color[]>();
        private StreamWriter sw;
        private double settingsVersion = 1.3;
        private double dictionaryVersion = 1.4;
        public Dictionary<string, object> anarchy_settings;
        public Dictionary<string, object> anarchy_strings;
        public Dictionary<string, string> settings_string = new Dictionary<string, string>();
        public Dictionary<string, bool> settings_bool = new Dictionary<string, bool>();
        private Type type;
        private int result;
        private bool isenabled = false;
        private bool ismultiplayer = false;
        public CoasterCarInstantiator[] carTypes = { };
        GameObject lsmFin;
        private List<string> syncedsettings = new List<string> {
            "allowDeltaHeight",
            "allowTightHelix",
            "allowSteepLifts",
            "allowSteepHills",
            "allowSteepDrops",
            "allowTightTurns"
        };
        public Main()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                System.IO.Directory.CreateDirectory(Path);
            }
            catch
            {
                Debug.LogError("Creating path failed: " + Path);
                return;
            }
            if (File.Exists(Path + @"/coasteranarchy.settings.json"))
            {
                anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/coasteranarchy.settings.json")) as Dictionary<string, object>;
            }
            else if (File.Exists(LegacyPath + @"/settings.json"))
            {
                anarchy_settings = Json.Deserialize(File.ReadAllText(LegacyPath + @"/settings.json")) as Dictionary<string, object>;
            }
            else
            {
                anarchy_settings = new Dictionary<string, object>();
                generateSettingsFile();
                anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/coasteranarchy.settings.json")) as Dictionary<string, object>;
            }
            if (File.Exists(Path + @"/coasteranarchy.dictionary.json"))
            {
                anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/coasteranarchy.dictionary.json")) as Dictionary<string, object>;
            }
            else if (File.Exists(LegacyPath + @"/dictionary.json"))
            {
                anarchy_strings = Json.Deserialize(File.ReadAllText(LegacyPath + @"/dictionary.json")) as Dictionary<string, object>;
            }
            else
            {
                anarchy_strings = new Dictionary<string, object>();
                generateDictionaryFile();
                anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/coasteranarchy.dictionary.json")) as Dictionary<string, object>;
            }
            if (anarchy_settings == null || string.IsNullOrEmpty(anarchy_settings["version"].ToString()) || Double.Parse(anarchy_settings["version"].ToString()) < settingsVersion)
            {
                generateSettingsFile();
                anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/coasteranarchy.settings.json")) as Dictionary<string, object>;
            }
            if (anarchy_strings == null || string.IsNullOrEmpty(anarchy_strings["version"].ToString()) || Double.Parse(anarchy_strings["version"].ToString()) < dictionaryVersion)
            {
                generateDictionaryFile();
                anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/coasteranarchy.dictionary.json")) as Dictionary<string, object>;
            }
            if (anarchy_settings.Count > 0)
            {
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    type = S.Value.GetType();
                    if (type == typeof(bool))
                    {
                        settings_bool[S.Key] = bool.Parse(S.Value.ToString());
                    }
                    else
                    {
                        settings_string[S.Key] = S.Value.ToString();
                    }
                }
            }
        }

        public override void onEnabled()
        {
            ismultiplayer = CommandController.Instance.isInMultiplayerMode();
            enable(null, null);
        }

        public void enable(object source, ElapsedEventArgs e)
        {
            isenabled = true;
            createRevertSettings();
            applyChangedSettings();
        }

        public override void onDisabled()
        {
            isenabled = false;
            revertAllSettings();
        }

        public void createRevertSettings()
        {
            originalSettings = new Dictionary<string, TrackedRide>();
            originalSegments = new Dictionary<string, List<SpecialSegmentSettings>>();
            originalObjects = new Dictionary<string, Dictionary<string, GameObject>>();
            originalColors = new Dictionary<string, Color[]>();
            lsmFin = TrackRideHelper.GetTrackedRide("Floorless Coaster").meshGenerator.lsmFinGO;
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
                    originalObjects[ride.getUnlocalizedName()] = new Dictionary<string, GameObject>();
                    originalObjects[ride.getUnlocalizedName()]["stationHandRailGO"] = ride.meshGenerator.stationHandRailGO;
                    originalObjects[ride.getUnlocalizedName()]["lsmFinGO"] = ride.meshGenerator.lsmFinGO;
                    originalRide.meshGenerator = ride.meshGenerator;
                    originalRide.canBuildRideCamera = ride.canBuildRideCamera;
                    originalRide.canChangeSpinLock = ride.canChangeSpinLock;
                    originalRide.canBuildMagneticKickers = ride.canBuildMagneticKickers;
                    originalRide.canBuildSlopeTransitionBrakes = ride.canBuildSlopeTransitionBrakes;
                    originalRide.maxSegmentWidth = ride.maxSegmentWidth;
                    originalRide.canAdjustLiftSpeeds = ride.canAdjustLiftSpeeds;
                    originalRide.canHaveLSM = ride.canHaveLSM;
                    originalRide.canChangeLaps = ride.canChangeLaps;
                    originalRide.canCurveLifts = ride.canCurveLifts;
                    originalRide.canCurveSlopes = ride.canCurveSlopes;
                    originalRide.canCurveVertical = ride.canCurveVertical;
                    originalRide.canHaveBrakes = ride.canHaveBrakes;
                    originalRide.canHaveBlockBrakes = ride.canHaveBlockBrakes;
                    originalRide.canHaveHoldingBrakes = ride.canHaveHoldingBrakes;
                    originalRide.canHaveLifts = ride.canHaveLifts;
                    originalRide.canInvertSlopes = ride.canInvertSlopes;
                    originalRide.canChangeCarRotation = ride.canChangeCarRotation;
                    originalRide.everyUpIsLift = ride.everyUpIsLift;
                    originalRide.canRunInShuttleMode = ride.canRunInShuttleMode;
                    originalRide.canOnlyPlaceOnWater = ride.canOnlyPlaceOnWater;
                    originalRide.maxBankingAngle = ride.maxBankingAngle;
                    originalRide.maxDeltaHeightDown = ride.maxDeltaHeightDown;
                    originalRide.maxDeltaHeightForLift = ride.maxDeltaHeightForLift;
                    originalRide.maxDeltaHeightPerUnit = ride.maxDeltaHeightPerUnit;
                    originalRide.maxDeltaHeightUp = ride.maxDeltaHeightUp;
                    originalRide.maxLiftSpeed = ride.maxLiftSpeed;
                    originalRide.maxSupportHeight = ride.maxSupportHeight;
                    originalRide.min90CurveSize = ride.min90CurveSize;
                    originalRide.minHalfHelixSize = ride.minHalfHelixSize;
                    originalRide.canChangeDirectionAngle = ride.canChangeDirectionAngle;
                    List<SpecialSegmentSettings> originalSpecialSegments = new List<SpecialSegmentSettings>();
                    foreach (SpecialSegmentSettings segment in ScriptableSingleton<AssetManager>.Instance.specialSegments)
                    {
                        if (ride.specialSegments.hasSpecialSegment(segment))
                        {
                            originalSpecialSegments.Add(segment);
                        }
                    }
                    originalRide.carTypes = ride.carTypes;
                    originalSettings.Add(ride.getUnlocalizedName(), originalRide);
                    originalSegments.Add(ride.getUnlocalizedName(), originalSpecialSegments);
                    originalColors.Add(ride.getUnlocalizedName(), ride.meshGenerator.customColors);
                }
            }
        }

        public bool settingEnabled(string name)
        {
            if (syncedsettings.Contains(name))
            {
                return true;
            }
            else {
                if (settings_bool.ContainsKey(name))
                {
                    return settings_bool[name];
                }
                else
                {
                    return false;
                }
            }
        }

        public void applyChangedSettings()
        {
            if (settingEnabled("noBuildRestrictions"))
            {
                Global.NO_TRACKBUILDER_RESTRICTIONS = true;
            }
            if(settingEnabled("allowBrokenStuff_NO_WARRANTY") && !ismultiplayer)
            {
                Global.ALLOW_IMPOSSIBLE_TRACKBUILDER_SEGMENTS = true;
            }
            foreach (Attraction current in ScriptableSingleton<AssetManager>.Instance.getAttractionObjects())
            {
                var ride = current as TrackedRide;
                if (ride != null)
                {

                    if(settingEnabled("allowVerticalDirectionSwap"))
                    {
                        ride.canChangeDirectionAngle = true;
                    }
                    if(settingEnabled("allowVerticalCurve"))
                    {
                        ride.canCurveVertical = true;
                    }
                    if(settingEnabled("allowDeltaHeight"))
                    {
                        ride.maxDeltaHeightPerUnit = float.MaxValue;
                    }
                    if(settingEnabled("allowTightHelix"))
                    {
                        ride.minHalfHelixSize = 1;
                    }
                    if(settingEnabled("allowSteepLifts"))
                    {
                        ride.maxDeltaHeightForLift = float.MaxValue;
                    }
                    if(settingEnabled("allowSteepHills"))
                    {
                        ride.maxDeltaHeightUp = float.MaxValue;
                    }
                    if(settingEnabled("allowSteepDrops"))
                    {
                        ride.maxDeltaHeightDown = float.MaxValue;
                    }
                    if(settingEnabled("allowTrackCrests"))
                    {
                        ride.canInvertSlopes = true;
                    }
                    if(settingEnabled("allowCurvedSlopes"))
                    {
                        ride.canCurveSlopes = true;
                    }
                    if(settingEnabled("allowBrakes"))
                    {
                        if (ride.getUnlocalizedName().Contains("Coaster"))
                        {
                            ride.canHaveBrakes = true;
                            ride.canHaveBlockBrakes = true;
                        }
                    }
                    if(settingEnabled("changeLiftSpeedLimit"))
                    {
                       ride.canAdjustLiftSpeeds = true;
                       ride.maxLiftSpeed = float.Parse(settings_string["liftSpeedLimit"]);
                    }
                    if(settingEnabled("changeSegmentWidth"))
                    {
                        ride.maxSegmentWidth = float.Parse(settings_string["segmentWidth"]); ;
                    }
                    if(settingEnabled("allowHoldingBrakes"))
                    {
                        ride.canHaveHoldingBrakes = true;
                    }
                    if(settingEnabled("allowCarRotation"))
                    {
                        ride.canChangeCarRotation = true;
                    }
                    if(settingEnabled("allowTightTurns"))
                    {
                        ride.min90CurveSize = 1;
                    }
                    if(settingEnabled("unlimitedHeight"))
                    {
                        ride.maxSupportHeight = int.MaxValue;
                    }
                    if(settingEnabled("allowCurvedLifts"))
                    {
                        ride.canCurveLifts = true;
                    }
                    if(settingEnabled("allowLapsChange"))
                    {
                        ride.canChangeLaps = true;
                    }
                    if(settingEnabled("allowRideCamera"))
                    {
                        ride.canBuildRideCamera = true;
                    }
                    if(settingEnabled("allowSpinLock"))
                    {
                        ride.canChangeSpinLock = true;
                    }
                    if(settingEnabled("allowMagneticKickers"))
                    {
                        ride.canBuildMagneticKickers = true;
                    }
                    if(settingEnabled("allowSlopeTransitionBrakes"))
                    {
                        ride.canBuildSlopeTransitionBrakes = true;
                    }
                    if(settingEnabled("allowBanking"))
                    {
                        ride.maxBankingAngle = 180;
                    }
                    if(settingEnabled("allowLiftHills"))
                    {
                        if (ride.getUnlocalizedName().ToLower().Contains("coaster") || ride.everyUpIsLift)
                        {
                            ride.canHaveLifts = true;
                        }
                        ride.everyUpIsLift = false;
                    }
                    if(settingEnabled("allowLSM"))
                    {
                        ride.canHaveLSM = true;
                        if (ride.meshGenerator.lsmFinGO == null && originalSettings[ride.getUnlocalizedName()].canHaveLSM == false)
                        {
                            ride.meshGenerator.lsmFinGO = lsmFin;
                        }
                        
                    }
                    if(settingEnabled("allowAllSpecialSegments"))
                    {
                        ride.meshGenerator.elevatorTunnelMeshGenerator = TrackRideHelper.GetTrackedRide("Spinning Coaster").meshGenerator.elevatorTunnelMeshGenerator;
                        if(TrackRideHelper.GetTrackedRide("Tilt Coaster") != null)
                        {
                            ride.meshGenerator.customColors = new[] {
                                ride.meshGenerator.customColors.Length > 0?ride.meshGenerator.customColors[0]:new Color(0, 0, 0),
                                ride.meshGenerator.customColors.Length > 1?ride.meshGenerator.customColors[1]:new Color(0, 0, 0),
                                ride.meshGenerator.customColors.Length > 2?ride.meshGenerator.customColors[2]:new Color(0, 0, 0),
                                ride.meshGenerator.customColors.Length > 3?ride.meshGenerator.customColors[3]:new Color(0, 0, 0)
                            };
                        }
                        foreach (SpecialSegmentSettings segment in ScriptableSingleton<AssetManager>.Instance.specialSegments)
                        {
                            if (segment.name != "Splashdown" && segment.name != "HydraulicLaunchSystem")
                            {
                                ride.specialSegments.addSpecialSegment(segment);
                            }
                        }
                    }
                    if(settingEnabled("allowAllTrains")) {
                        ride.carTypes = ride.carTypes.Union(carTypes).ToArray();
                    }
                    if(settingEnabled("allowLongTrains"))
                    {
                        foreach (CoasterCarInstantiator carType in ride.carTypes)
                        {
                            carType.minTrainLength = 1;
                            carType.maxTrainLength = 255;
                        }
                    }
                    if(settingEnabled("removeStationHandrails"))
                    {
                        ride.meshGenerator.stationHandRailGO = null;
                    }
                    if (settingEnabled("allowShuttleMode"))
                    {
                        ride.canRunInShuttleMode = true;
                    }
                    if (settingEnabled("allowWaterRidesAnywhere"))
                    {
                        ride.canOnlyPlaceOnWater = false;
                    }
                }
            }
        }

        public void revertAllSettings()
        {
            Global.NO_TRACKBUILDER_RESTRICTIONS = false;
            Global.ALLOW_IMPOSSIBLE_TRACKBUILDER_SEGMENTS = false;
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
                    ride.canChangeDirectionAngle = originalSettings[ride.getUnlocalizedName()].canChangeDirectionAngle;
                    ride.canRunInShuttleMode = originalSettings[ride.getUnlocalizedName()].canRunInShuttleMode;
                    ride.canOnlyPlaceOnWater = originalSettings[ride.getUnlocalizedName()].canOnlyPlaceOnWater;
                    ride.meshGenerator.lsmFinGO = originalObjects[ride.getUnlocalizedName()]["lsmFinGO"];
                    ride.meshGenerator.stationHandRailGO = originalObjects[ride.getUnlocalizedName()]["stationHandRailGO"];
                    ride.meshGenerator.customColors = originalColors[ride.getUnlocalizedName()];
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

        public void onDrawSettingsUI()
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.margin = new RectOffset(15, 0, 10, 0);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle displayStyle = new GUIStyle(GUI.skin.label);
            displayStyle.margin = new RectOffset(0, 10, 10, 0);
            displayStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.margin = new RectOffset(0, 10, 19, 16);
            toggleStyle.alignment = TextAnchor.MiddleLeft;
            GUIStyle textfieldStyle = new GUIStyle(GUI.skin.textField);
            textfieldStyle.margin = new RectOffset(0, 10, 10, 0);
            textfieldStyle.alignment = TextAnchor.MiddleCenter;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(0, 10, 10, 0);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Version", labelStyle, GUILayout.Height(30));
            GUILayout.Label("Changed settings only apply to newly build rides\nUnless the game is saved and reloaded.", labelStyle, GUILayout.Height(30));
            if (ismultiplayer)
            {
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    if (S.Key != "version" && !syncedsettings.Contains(S.Key))
                    {
                        GUILayout.Label(anarchy_strings.ContainsKey(S.Key) ? anarchy_strings[S.Key].ToString() : S.Key, labelStyle, GUILayout.Height(30));
                    }
                }
                GUILayout.Label("The following settings cannot be changed and are always enabled in multiplayer.", labelStyle, GUILayout.Height(30));
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    if (syncedsettings.Contains(S.Key))
                    {
                        GUILayout.Label(anarchy_strings.ContainsKey(S.Key) ? anarchy_strings[S.Key].ToString() : S.Key, labelStyle, GUILayout.Height(30));
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    if (S.Key != "version")
                    {
                        GUILayout.Label(anarchy_strings.ContainsKey(S.Key) ? anarchy_strings[S.Key].ToString() : S.Key, labelStyle, GUILayout.Height(30));
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Label(getVersionNumber(), displayStyle, GUILayout.Height(30));
            GUILayout.Label(" \n ", displayStyle, GUILayout.Height(30));
            if(ismultiplayer)
            {
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    type = S.Value.GetType();
                    if (S.Key != "version" && !syncedsettings.Contains(S.Key))
                    {
                        if (type == typeof(bool))
                        {
                            settings_bool[S.Key] = GUILayout.Toggle(settings_bool[S.Key], "", toggleStyle, GUILayout.Width(16), GUILayout.Height(20.5f));
                        }
                        else
                        {
                            settings_string[S.Key] = GUILayout.TextField(settings_string[S.Key], textfieldStyle, GUILayout.Width(130), GUILayout.Height(30));
                        }
                    }
                }
                GUILayout.Label(" \n ", displayStyle, GUILayout.Height(30));
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    type = S.Value.GetType();
                    if (S.Key != "version" && syncedsettings.Contains(S.Key))
                    {
                        if (type == typeof(bool))
                        {
                            GUILayout.Label((settings_bool[S.Key] ? "Enabled" : "Disabled"), displayStyle, GUILayout.Height(30));
                        }
                        else
                        {
                            GUILayout.Label(settings_string[S.Key], displayStyle, GUILayout.Height(30));
                        }
                    }
                }

            }
            else
            {
                foreach (KeyValuePair<string, object> S in anarchy_settings)
                {
                    type = S.Value.GetType();
                    if (S.Key != "version")
                    {
                        if (type == typeof(bool))
                        {
                            settings_bool[S.Key] = GUILayout.Toggle(settings_bool[S.Key], "", toggleStyle, GUILayout.Width(16), GUILayout.Height(20.5f));
                        }
                        else
                        {
                            settings_string[S.Key] = GUILayout.TextField(settings_string[S.Key], textfieldStyle, GUILayout.Width(130), GUILayout.Height(30));
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public void onSettingsOpened()
        {
            LoadSettings();
        }

        public void onSettingsClosed()
        {
            if (settings_bool.Count > 0)
            {
                foreach (KeyValuePair<string, bool> S in settings_bool)
                {
                    anarchy_settings[S.Key] = S.Value;
                }
            }
            if (settings_string.Count > 0)
            {
                foreach (KeyValuePair<string, string> S in settings_string)
                {
                    anarchy_settings[S.Key] = S.Value;
                }
            }
            generateSettingsFile();
            if (isenabled)
            {
                revertAllSettings();
                applyChangedSettings();
            }
        }

        public void generateSettingsFile()
        {
            try
            {
                File.Delete(Path + @"/coasteranarchy.settings.json");
                File.Delete(LegacyPath + @"/settings.json");
            }
            catch
            {

            }
            try
            {
                sw = File.CreateText(Path + @"/coasteranarchy.settings.json");
                sw.WriteLine("{");
                sw.WriteLine("	\"version\": " + settingsVersion.ToString().Replace(",", ".") + (int.TryParse(settingsVersion.ToString(), out result) ? ".0" : "") + ",");
                writeSettingLine(sw, "allowAllSpecialSegments", typeof(bool), true);
                writeSettingLine(sw, "allowAllTrains", typeof(bool), true);
                writeSettingLine(sw, "allowLongTrains", typeof(bool), true);
                writeSettingLine(sw, "noBuildRestrictions", typeof(bool), true);
                writeSettingLine(sw, "allowLSM", typeof(bool), true);
                writeSettingLine(sw, "allowBanking", typeof(bool), true);
                writeSettingLine(sw, "allowSteepDrops", typeof(bool), true);
                writeSettingLine(sw, "allowSteepHills", typeof(bool), true);
                writeSettingLine(sw, "allowCurvedSlopes", typeof(bool), true);
                writeSettingLine(sw, "allowVerticalCurve", typeof(bool), true);
                writeSettingLine(sw, "allowSteepLifts", typeof(bool), true);
                writeSettingLine(sw, "allowCurvedLifts", typeof(bool), true);
                writeSettingLine(sw, "changeLiftSpeedLimit", typeof(bool), true);
                writeSettingLine(sw, "liftSpeedLimit", typeof(string), "100");
                writeSettingLine(sw, "allowVerticalDirectionSwap", typeof(bool), true);
                writeSettingLine(sw, "allowLapsChange", typeof(bool), true);
                writeSettingLine(sw, "allowTrackCrests", typeof(bool), true);
                writeSettingLine(sw, "unlimitedHeight", typeof(bool), true);
                writeSettingLine(sw, "allowTightTurns", typeof(bool), true);
                writeSettingLine(sw, "allowDeltaHeight", typeof(bool), true);
                writeSettingLine(sw, "allowSlopeTransitionBrakes", typeof(bool), true);
                writeSettingLine(sw, "allowHoldingBrakes", typeof(bool), true);
                writeSettingLine(sw, "allowBrakes", typeof(bool), true);
                writeSettingLine(sw, "allowTightHelix", typeof(bool), true);
                writeSettingLine(sw, "allowRideCamera", typeof(bool), true);
                writeSettingLine(sw, "allowMagneticKickers", typeof(bool), true);
                writeSettingLine(sw, "allowSpinLock", typeof(bool), true);
                writeSettingLine(sw, "allowCarRotation", typeof(bool), true);
                writeSettingLine(sw, "allowShuttleMode", typeof(bool), false);
                writeSettingLine(sw, "allowLiftHills", typeof(bool), true);
                // writeSettingLine(sw, "allowWaterRidesAnywhere", typeof(bool), false);
                // Isn't being used
                writeSettingLine(sw, "changeSegmentWidth", typeof(bool), true);
                writeSettingLine(sw, "segmentWidth", typeof(string), "10");
                writeSettingLine(sw, "removeStationHandrails", typeof(bool), false);
                if (anarchy_settings.ContainsKey("allowBrokenStuff_NO_WARRANTY"))
                {
                    writeSettingLine(sw, "allowBrokenStuff_NO_WARRANTY", typeof(bool), false);
                }
                sw.WriteLine("}");
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/coasteranarchy.settings.json");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public void writeSettingLine(StreamWriter sw, string setting, Type type, object defaultValue)
        {
            try
            {
                if (type == typeof(string))
                {
                    sw.WriteLine("	\"" + setting + "\": \"" + (anarchy_settings.ContainsKey(setting) ? anarchy_settings[setting] : defaultValue) + "\",");
                }
                if (type == typeof(bool))
                {
                    sw.WriteLine("	\"" + setting + "\": " + (anarchy_settings.ContainsKey(setting) ? anarchy_settings[setting].ToString().ToLower() : defaultValue.ToString().ToLower()) + ",");
                }
            }
            catch
            {
                Debug.LogError("Failed writing setting: " + setting);
            }
        }

        public void generateDictionaryFile()
        {
            try
            {
                File.Delete(Path + @"/coasteranarchy.dictionary.json");
                File.Delete(LegacyPath + @"/dictionary.json");
            }
            catch
            {

            }
            try
            {
                sw = File.CreateText(Path + @"/coasteranarchy.dictionary.json");
                sw.WriteLine("{");
                sw.WriteLine("	\"version\": " + dictionaryVersion.ToString().Replace(",", ".") + (int.TryParse(dictionaryVersion.ToString(), out result) ? ".0" : "") + ",");
                writeDictionaryLine(sw, "allowAllTrains", "Allow all vehicles");
                writeDictionaryLine(sw, "allowLongTrains", "Allow trains of any length");
                writeDictionaryLine(sw, "allowAllSpecialSegments", "Allow all special track pieces");
                writeDictionaryLine(sw, "allowLSM", "Allow LSM launches");
                writeDictionaryLine(sw, "allowBanking", "Allow full banking");
                writeDictionaryLine(sw, "allowRideCamera", "Allow camera");
                writeDictionaryLine(sw, "allowSpinLock", "Allow spin toggle");
                writeDictionaryLine(sw, "allowMagneticKickers", "Allow magnetic kickers");
                writeDictionaryLine(sw, "allowSlopeTransitionBrakes", "Allow building brakes on slope transitions");
                writeDictionaryLine(sw, "allowLapsChange", "Allow changing lap count");
                writeDictionaryLine(sw, "allowCurvedLifts", "Allow curved lift hills");
                writeDictionaryLine(sw, "unlimitedHeight", "Allow infinite height");
                writeDictionaryLine(sw, "allowTightTurns", "Allow tight turns");
                writeDictionaryLine(sw, "allowCarRotation", "Allow car rotation changer");
                writeDictionaryLine(sw, "allowHoldingBrakes", "Allow holding brakes");
                writeDictionaryLine(sw, "allowLiftHills", "Allow custom lifthill placement on water rides");
                writeDictionaryLine(sw, "changeLiftSpeedLimit", "Overwrite lift speed limit");
                writeDictionaryLine(sw, "liftSpeedLimit", "Lift speed limit");
                writeDictionaryLine(sw, "changeSegmentWidth", "Overwrite maximum track width");
                writeDictionaryLine(sw, "segmentWidth", "Maximum track width");
                writeDictionaryLine(sw, "allowCurvedSlopes", "Allow curved slopes");
                writeDictionaryLine(sw, "allowBrakes", "Allow friction and block brakes");
                writeDictionaryLine(sw, "allowTrackCrests", "Allow building drops directly from hills");
                writeDictionaryLine(sw, "allowSteepDrops", "Allow steep drops");
                writeDictionaryLine(sw, "allowSteepHills", "Allow steep hills");
                writeDictionaryLine(sw, "allowSteepLifts", "Allow lifts on steep track");
                writeDictionaryLine(sw, "allowTightHelix", "Allow tight helixes");
                writeDictionaryLine(sw, "noBuildRestrictions", "Allow building \\\"invalid\\\" pieces");
                writeDictionaryLine(sw, "allowDeltaHeight", "Allow tight slope transitions");
                writeDictionaryLine(sw, "allowVerticalCurve", "Allow turns on vertical track");
                writeDictionaryLine(sw, "allowVerticalDirectionSwap", "Allow vertical direction toggle");
                writeDictionaryLine(sw, "allowShuttleMode", "Allow shuttle mode operations");
                writeDictionaryLine(sw, "allowWaterRidesAnywhere", "Allow water rides to be built anywhere");
                writeDictionaryLine(sw, "removeStationHandrails", "Remove handrails from station platforms");
                writeDictionaryLine(sw, "allowBrokenStuff_NO_WARRANTY", "Allow horribly broken elements,\ndisable this before reporting bugs.");
                sw.WriteLine("}");
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/coasteranarchy.dictionary.json");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public void writeDictionaryLine(StreamWriter sw, string setting, string defaultValue)
        {
            try
            {
                sw.WriteLine("	\"" + setting + "\": \"" + (anarchy_strings.ContainsKey(setting) ? anarchy_strings[setting] : defaultValue) + "\",");
            }
            catch
            {
                Debug.LogError("Failed writing dictionary entry: " + setting);
            }
        }

        public override string getName() { return "Coaster Anarchy"; }

        public override string getDescription() { return "Allows more track pieces on more coasters..."; }

        public override string getIdentifier() { return "Marnit@ParkitectCoasterAnarchy"; }

        public override string getVersionNumber() { return "2.5.1"; }

        public override bool isMultiplayerModeCompatible() { return true; }

        public override bool isRequiredByAllPlayersInMultiplayerMode() { return true; }

        public override int getOrderPriority() { return 9999; }

        public string LegacyPath { get { return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect/Mods/CoasterAnarchySettings"; } }

        public string Path { get { return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect"; } }
    }
}
