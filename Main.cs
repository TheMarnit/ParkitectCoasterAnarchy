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
        private StreamWriter sw;
        private int i;
        private double settingsVersion = 1.2;
        private double dictionaryVersion = 1.2;
        public Dictionary<string, object> anarchy_settings;
        public Dictionary<string, object> anarchy_strings;
        public Dictionary<string, string> settings_string = new Dictionary<string, string>();
        public Dictionary<string, bool> settings_bool = new Dictionary<string, bool>();
        private string output;
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
            if (!File.Exists(Path + @"/settings.json"))
            {
                generateSettingsFile();
            }
            if (!File.Exists(Path + @"/dictionary.json"))
            {
                generateDictionaryFile();
            }
            anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/settings.json")) as Dictionary<string, object>;
            if (anarchy_settings == null || string.IsNullOrEmpty(anarchy_settings["version"].ToString()) || Double.Parse(anarchy_settings["version"].ToString()) < settingsVersion)
            {
                generateSettingsFile();
                anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/settings.json")) as Dictionary<string, object>;
            }
            anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/dictionary.json")) as Dictionary<string, object>;
            if (anarchy_strings == null || string.IsNullOrEmpty(anarchy_strings["version"].ToString()) || Double.Parse(anarchy_strings["version"].ToString()) < dictionaryVersion)
            {
                generateDictionaryFile();
                anarchy_strings = Json.Deserialize(File.ReadAllText(Path + @"/dictionary.json")) as Dictionary<string, object>;
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
            if (CommandController.Instance.isInMultiplayerMode())
            {
                ismultiplayer = true;
            }
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
                return settings_bool[name];
            }
        }

        public void applyChangedSettings()
        {
            if (settingEnabled("noBuildRestrictions"))
            {
                Global.NO_TRACKBUILDER_RESTRICTIONS = true;
            }
            foreach (Attraction current in ScriptableSingleton<AssetManager>.Instance.getAttractionObjects())
            {
                var ride = current as TrackedRide;
                TrackedRide originalRide = new TrackedRide();
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
                    if(settingEnabled("allowMagnetickKickers"))
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
                        Debug.Log(ride.getUnlocalizedName());
                        if (ride.getUnlocalizedName().ToLower().Contains("coaster") || ride.everyUpIsLift)
                        {
                            ride.canHaveLifts = true;
                        }
                        ride.everyUpIsLift = false;
                    }
                    if(settingEnabled("allowLSM"))
                    {
                        ride.canHaveLSM = true;
                        if (ride.meshGenerator.lsmFinGO == null)
                        {
                            ride.meshGenerator.lsmFinGO = lsmFin;
                        }

                    }
                    if(settingEnabled("allowAllSpecialSegments"))
                    {
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
                }
            }
        }

        public void revertAllSettings()
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
                    ride.canChangeDirectionAngle = originalSettings[ride.getUnlocalizedName()].canChangeDirectionAngle;
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
            anarchy_settings = Json.Deserialize(File.ReadAllText(Path + @"/settings.json")) as Dictionary<string, object>;
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
            writeSettingsFile();
            if (isenabled)
            {
                revertAllSettings();
                applyChangedSettings();
            }
        }

        public void writeSettingsFile()
        {
            sw = File.CreateText(Path + @"/settings.json");
            sw.WriteLine("{");
            i = 0;
            foreach (KeyValuePair<string, object> S in anarchy_settings)
            {
                type = S.Value.GetType();
                i++;
                output = "	\"" + S.Key + "\": ";
                if (type == typeof(bool))
                {
                    output += settings_bool[S.Key].ToString().ToLower();
                }
                else if (type == typeof(double))
                {
                    output += settings_string[S.Key].Replace(",", ".");
                    if (int.TryParse(settings_string[S.Key], out result))
                    {
                        output += ".0";
                    }
                }
                else if (type == typeof(string))
                {
                    output += "\"" + settings_string[S.Key] + "\"";
                }
                if (i != anarchy_settings.Count)
                {
                    output += ",";
                }
                sw.WriteLine(output);
            }
            sw.WriteLine("}");
            sw.Flush();
            sw.Close();
        }

        public void generateSettingsFile()
        {
            try
            {
                sw = File.CreateText(Path + @"/settings.json");
                sw.WriteLine("{");
                sw.WriteLine("	\"version\": " + settingsVersion.ToString().Replace(",", ".") + (int.TryParse(settingsVersion.ToString(), out result) ? ".0" : "") + ",");
                sw.WriteLine("	\"allowAllSpecialSegments\": true,");
                sw.WriteLine("	\"allowAllTrains\": true,");
                sw.WriteLine("	\"allowLongTrains\": true,");
                sw.WriteLine("	\"noBuildRestrictions\": true,");
                sw.WriteLine("	\"allowLSM\": true,");
                sw.WriteLine("	\"allowBanking\": true,");
                sw.WriteLine("	\"allowSteepDrops\": true,");
                sw.WriteLine("	\"allowSteepHills\": true,");
                sw.WriteLine("	\"allowCurvedSlopes\": true,");
                sw.WriteLine("	\"allowVerticalCurve\": true,");
                sw.WriteLine("	\"allowSteepLifts\": true,");
                sw.WriteLine("	\"allowCurvedLifts\": true,");
                sw.WriteLine("	\"changeLiftSpeedLimit\": true,");
                sw.WriteLine("	\"liftSpeedLimit\": \"100\",");
                sw.WriteLine("	\"allowVerticalDirectionSwap\": true,");
                sw.WriteLine("	\"allowLapsChange\": true,");
                sw.WriteLine("	\"allowTrackCrests\": true,");
                sw.WriteLine("	\"unlimitedHeight\": true,");
                sw.WriteLine("	\"allowTightTurns\": true,");
                sw.WriteLine("	\"allowDeltaHeight\": true,");
                sw.WriteLine("	\"allowSlopeTransitionBrakes\": true,");
                sw.WriteLine("	\"allowHoldingBrakes\": true,");
                sw.WriteLine("	\"allowBrakes\": true,");
                sw.WriteLine("	\"allowTightHelix\": true,");
                sw.WriteLine("	\"allowRideCamera\": true,");
                sw.WriteLine("	\"allowSpinLock\": true,");
                sw.WriteLine("	\"allowMagnetickKickers\": true,");
                sw.WriteLine("	\"allowCarRotation\": true,");
                sw.WriteLine("	\"allowLiftHills\": true,");
                sw.WriteLine("	\"changeSegmentWidth\": true,");
                sw.WriteLine("	\"segmentWidth\": \"10\",");
                sw.WriteLine("}");
                sw.Flush();
                sw.Close();
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/settings.json");
            }
        }

        public void generateDictionaryFile()
        {
            try
            {
                sw = File.CreateText(Path + @"/dictionary.json");
                sw.WriteLine("{");
                sw.WriteLine("	\"version\": " + dictionaryVersion.ToString().Replace(",", ".") + (int.TryParse(dictionaryVersion.ToString(), out result) ? ".0" : "") + ",");
                sw.WriteLine("	\"allowAllTrains\": \"Allow all vehicles\",");
                sw.WriteLine("	\"allowLongTrains\": \"Allow trains of any length\",");
                sw.WriteLine("	\"allowAllSpecialSegments\": \"Allow all special track pieces\",");
                sw.WriteLine("	\"allowLSM\": \"Allow LSM launches\",");
                sw.WriteLine("	\"allowBanking\": \"Allow full banking\",");
                sw.WriteLine("	\"allowRideCamera\": \"Allow camera\",");
                sw.WriteLine("	\"allowSpinLock\": \"Allow spin toggle\",");
                sw.WriteLine("	\"allowMagnetickKickers\": \"Allow magnetic kickers\",");
                sw.WriteLine("	\"allowSlopeTransitionBrakes\": \"Allow building brakes on slope transitions\",");
                sw.WriteLine("	\"allowLapsChange\": \"Allow changing lap count\",");
                sw.WriteLine("	\"allowCurvedLifts\": \"Allow curved lift hills\",");
                sw.WriteLine("	\"unlimitedHeight\": \"Allow infinite height\",");
                sw.WriteLine("	\"allowTightTurns\": \"Allow tight turns\",");
                sw.WriteLine("	\"allowCarRotation\": \"Allow car rotation changer\",");
                sw.WriteLine("	\"allowHoldingBrakes\": \"Allow holding brakes\",");
                sw.WriteLine("	\"allowLiftHills\": \"Allow custom lifthill placement on water rides\",");
                sw.WriteLine("	\"changeLiftSpeedLimit\": \"Overwrite lift speed limit\",");
                sw.WriteLine("	\"liftSpeedLimit\": \"Lift speed limit\",");
                sw.WriteLine("	\"changeSegmentWidth\": \"Overwrite maximum track width\",");
                sw.WriteLine("	\"segmentWidth\": \"Maximum track width\",");
                sw.WriteLine("	\"allowCurvedSlopes\": \"Allow curved slopes\",");
                sw.WriteLine("	\"allowBrakes\": \"Allow friction and block brakes\",");
                sw.WriteLine("	\"allowTrackCrests\": \"Allow building drops directly from hills\",");
                sw.WriteLine("	\"allowSteepDrops\": \"Allow steep drops\",");
                sw.WriteLine("	\"allowSteepHills\": \"Allow steep hills\",");
                sw.WriteLine("	\"allowSteepLifts\": \"Allow lifts on steep track\",");
                sw.WriteLine("	\"allowTightHelix\": \"Allow tight helixes\",");
                sw.WriteLine("	\"noBuildRestrictions\": \"Allow building \\\"invalid\\\" pieces\",");
                sw.WriteLine("	\"allowDeltaHeight\": \"Allow tight slope transitions\",");
                sw.WriteLine("	\"allowVerticalCurve\": \"Allow turns on vertical track\",");
                sw.WriteLine("	\"allowVerticalDirectionSwap\": \"Allow vertical direction toggle\",");
                sw.WriteLine("}");
                sw.Flush();
                sw.Close();
            }
            catch
            {
                Debug.LogError("Failed writing to: " + Path + @"/dictionary.json");
            }
        }

        public override string getName() { return "Coaster Anarchy"; }

        public override string getDescription() { return "Allows more track pieces on more coasters..."; }

        public override string getIdentifier() { return "Marnit@ParkitectCoasterAnarchy"; }

        public override string getVersionNumber() { return "2.4.0"; }

        public override bool isMultiplayerModeCompatible() { return true; }

        public override bool isRequiredByAllPlayersInMultiplayerMode() { return true; }

        public override int getOrderPriority() { return 9999; }

        public string Path { get { return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Parkitect/Mods/CoasterAnarchySettings"; } }
    }
}
