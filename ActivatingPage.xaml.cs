using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using DJI.WindowsSDK;
using DJI.WindowsSDK.Mission.Waypoint;
using Newtonsoft.Json;
using Quobject.SocketIoClientDotNet.Client;

using Windows.UI.Popups;

namespace DJIWindowsSDKSample.DJISDKInitializing
{
    public class Location
    {
        [JsonProperty("latitude")]
        private String _latitude;//위도 (-90~90)
        [JsonProperty("longitude")]
        private String _longitude; //경도 (-180~180)

        public String Longitude() => this._longitude;
        public void Longitude(String longitude) => this._longitude = longitude;
        //public String Longitude
        //{
        //    get => _longitude;
        //    set => _longitude = value;
        //}

        public String Latitude() => this._latitude;
        public void Latitude(String latitude) => this._latitude = latitude;
        //public String Latitude
        //{
        //    get => _latitude;
        //    set => _latitude = value;
        //}

        // Constructor
        public Location(String latitude, String longitude)
        {
            Longitude(longitude);
            Latitude(latitude);
        }
    }

    public class DroneData
    {
        [JsonProperty("battery")]
        private int _battery;
        [JsonProperty("location")]
        private Location _location;
        [JsonProperty("altitude")]
        private String _altitude;

        public DroneData()
        {
            // Default Constructor
        }

        public DroneData(int battery, String latitude, String longitude, String altitude)
        {
            // Constructor
            _battery = battery;
            _location = new Location(latitude, longitude);
            _altitude = altitude;
        }

        public int Battery() => this._battery;
        public void Battery(int bat) => this._battery = bat;
        //public int Battery
        //{
        //    get => _battery;
        //    set => _battery = value;
        //}

        public Location Location() => this._location;
        public void Location(Location location) => this._location= location;
        //public Location Location
        //{
        //    get => _location;
        //    set => _location = value;
        //}

        public String Altitude() => this._altitude;
        public void Altitude(String altitude) => this._altitude = altitude;

        public void SetLocation(String latitude, String longitude)
        {
            _location.Longitude(longitude); // = longitude;
            _location.Latitude(latitude); // = latitude;
        }
    }

    public class UserData
    {
        [JsonProperty("location")]
        private Location _location;

        public UserData()
        {
            //Default Constructor
        }

        public UserData(String longitude, String latitude) => _location = new Location(latitude, longitude);

        public Location Location() => this._location;
        public void Location(Location location) => this._location = location;

        public void SetLocation(String latitude, String longitude)
        {
            _location.Longitude(longitude); // = longitude;
            _location.Latitude(latitude); // = latitude;
        }
    }

    public sealed partial class TestingPage : Page
    {
        Socket socket = IO.Socket("https://api.teamhapco.com/");
        DroneData DD = new DroneData(80, "37", "127", "1.0");
        UserData UD = new UserData("37", "127");
        WaypointMission WaypointMission;
        
        public TestingPage()
        {
            this.InitializeComponent();

            System.Diagnostics.Debug.WriteLine("socket connecting start");
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                String JsonString = JsonConvert.SerializeObject(DD);
                System.Diagnostics.Debug.WriteLine("connect success!!");
                socket.Emit("init_gcs", JsonString);
                System.Diagnostics.Debug.WriteLine("Data to 'init_gcs' : " + JsonString);
            });
            
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
            DJISDKManager.Instance.RegisterApp("c9e68649b0d02247fc1410eb");
        }

        //Handmade Mission Flight
        private async void Run_MissionFlight(string _latitude, string _longitude, string _altitude)
        {
            var lat = Convert.ToDouble(_latitude);
            var lng = Convert.ToDouble(_longitude);
            var alt = Convert.ToDouble(_altitude);

            double nowalt;

            var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
            System.Diagnostics.Debug.WriteLine("Start send takeoff command: " + err.ToString());
            takeOffError.Text = "Start send takeoff command: " + err.ToString();

            DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(1, 0, 0, 0);
            while (true)
            {
                //nowalt = Convert.ToDouble(DD.Altitude());
                nowalt =(double)(await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync()).value?.value;
                if (nowalt >= alt)
                {
                    DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, 0);
                    break;
                }
            }
        }

        private async void TEMP_Start(object sender, RoutedEventArgs value)
        {
            Run_MissionFlight("37", "127", "8");
        }



        private async void Init_Mission(object sender, RoutedEventArgs value)
        {
            double nowLat = Convert.ToDouble(DD.Location().Latitude());
            double nowLng = Convert.ToDouble(DD.Location().Longitude());

            WaypointMission mission = new WaypointMission()
            {
                waypointCount = 0,
                maxFlightSpeed = 15,
                autoFlightSpeed = 10,
                finishedAction = WaypointMissionFinishedAction.NO_ACTION,
                headingMode = WaypointMissionHeadingMode.AUTO,
                flightPathMode = WaypointMissionFlightPathMode.NORMAL,
                gotoFirstWaypointMode = WaypointMissionGotoFirstWaypointMode.SAFELY,
                exitMissionOnRCSignalLostEnabled = false,
                pointOfInterest = new LocationCoordinate2D()
                {
                    latitude = 0,
                    longitude = 0
                },
                gimbalPitchRotationEnabled = true,
                repeatTimes = 0,
                missionID = 0,
                waypoints = new List<Waypoint>()
                            {
                                InitDumpWaypoint(nowLat+0.001, nowLng+0.0015),
                                InitDumpWaypoint(nowLat+0.001, nowLng-0.0015),
                                InitDumpWaypoint(nowLat-0.001, nowLng-0.0015),
                                InitDumpWaypoint(nowLat-0.001, nowLng+0.0015),
                            }
            };
            WaypointMission = mission;
        }

        private async void Load_Mission(object sender, RoutedEventArgs value)
        {
            var handler = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0);
            var state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();
            WaypointMissionState.Text = state.ToString();
            SDKError err = new SDKError();

            //var WaypointMission2 = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLoadedMission();

            err = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(WaypointMission);
            //else err = SDKError.MISSION_WAYPOINT_NULL_MISSION;
            
            

            System.Diagnostics.Debug.WriteLine("SDK load mission : " + err.ToString());
            LoadMissionError.Text = "SDK load mission : " + err.ToString();
        }

        private async void Get_Loaded_Mission(object sender, RoutedEventArgs value)
        {
            var WaypointMission2 = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLoadedMission();
        }

        private async void Upload_Mission(object sender, RoutedEventArgs value)
        {
            //DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLoadedMission();
            SDKError err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();
            System.Diagnostics.Debug.WriteLine("Upload mission to aircraft : " + err.ToString());
            UploadMissionError.Text = "Upload mission to aircraft : " + err.ToString();
        }

        private async void Execute_Mission(object sender, RoutedEventArgs value)
        {
            var err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();
            System.Diagnostics.Debug.WriteLine("Start mission : " + err.ToString());
            ExecuteMissionError.Text = "Start mission : " + err.ToString();
        }

        private Waypoint InitDumpWaypoint(double latitude, double longitude)
        {
            Waypoint waypoint = new Waypoint()
            {
                location = new LocationCoordinate2D() { latitude = latitude, longitude = longitude },
                altitude = 20,
                gimbalPitch = -30,
                turnMode = WaypointTurnMode.CLOCKWISE,
                heading = 0,
                actionRepeatTimes = 1,
                actionTimeoutInSeconds = 60,
                cornerRadiusInMeters = 0.2,
                speed = 0,
                shootPhotoTimeInterval = -1,
                shootPhotoDistanceInterval = -1,
                waypointActions = new List<WaypointAction>()
                {
                    InitDumpWaypointAction(1000, WaypointActionType.STAY),
                }
            };
            return waypoint;
        }

        private WaypointAction InitDumpWaypointAction(int Param, WaypointActionType actiontype)
        {
            WaypointAction action = new WaypointAction()
            {
                actionType = actiontype,
                actionParam = Param
            };
            return action;
        }

        private async void Start_Take_Off(object sender, RoutedEventArgs value)
        {
            var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
            System.Diagnostics.Debug.WriteLine("Start send takeoff command: " + res.ToString());
            takeOffError.Text = "Start send takeoff command: " + res.ToString();
            
        }

        private async void Start_Landing(object sender, RoutedEventArgs value)
        {
            var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartAutoLandingAsync();
            System.Diagnostics.Debug.WriteLine("Start send landing command: " + res.ToString());
            LandingError.Text = "Start send landing command: " + res.ToString();
        }

        private async void Confirm_Landing(object sender, RoutedEventArgs value)
        {
            var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).ConfirmLandingAsync();
            System.Diagnostics.Debug.WriteLine("Start send confirm landing command: " + res.ToString());
            ConfirmLandingError.Text = "Start send confirm landing command: " + res.ToString();
        }

        private async void Start_ReturnToHome(object sender, RoutedEventArgs value)
        {
            var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartGoHomeAsync();
            System.Diagnostics.Debug.WriteLine("Start send RTH command: " + res.ToString());
            RthError.Text = "Start send RTH command: " + res.ToString();
        }

        private async void Socket_Send(object sender, RoutedEventArgs e)
        {
            var JsonString = JsonConvert.SerializeObject(DD);
            socket.Emit("drone_data", JsonString);

            System.Diagnostics.Debug.WriteLine("Data to 'drone_data' => " + JsonString);
            SocketSend.Text = "Data to 'drone_data' => " + JsonString;

            socket.On("dront_data", (data) => 
            {
                System.Diagnostics.Debug.WriteLine("Data from 'drone_data' => " + data);
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //{
                //    SocketSend.Text = "Data from 'drone_data' => " + data;
                //});
                
            });
        }

        private async void Socket_Get(object sender, RoutedEventArgs e)
        {
            socket.On("client_gps", (data) =>
            {
                System.Diagnostics.Debug.WriteLine("Data from 'client_gps' => " + data);

                JsonConvert.DeserializeObject<UserData>(data.ToString());
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //{
                //    SocketGet.Text = "Data from 'client_gps' => " + data;
                //});
            });
        }

        private async void Get_DroneData_Master(object sender, RoutedEventArgs value)
        {
            DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += Get_AircraftObject;
            DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += Get_BatteryRemain;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += Get_Location;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged += Get_Altitude;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GPSSignalLevelChanged += Get_GpsSignalLevel;
            DJISDKManager.Instance.ComponentManager.GetProductHandler(0).SerialNumberChanged += Get_SerialNumber;
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StateChanged += Get_WaypointMissionState;
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadStateChanged += Get_UploadState;
            //DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).DownloadStateChanged += Get_DownloadState;
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).ExecutionStateChanged += Get_ExecutionState;
        }
            
        private async void Get_DroneData_DeMaster(object sender, RoutedEventArgs value)
        {
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged -= Get_Location;
        }

        private async void Select_Photo_Mode(object sender, RoutedEventArgs value)
        {
            RadioButton rb = sender as RadioButton;
            var setmode = new CameraShootPhotoModeMsg();

            switch (rb.Tag.ToString())
            {
                case "N":
                    setmode.value = CameraShootPhotoMode.NORMAL;
                    break;
                case "H":
                    setmode.value = CameraShootPhotoMode.HDR;
                    break;
                case "B":
                    setmode.value = CameraShootPhotoMode.BURST;
                    break;
                case "A":
                    setmode.value = CameraShootPhotoMode.AEB;
                    break;
                case "R":
                    setmode.value = CameraShootPhotoMode.RAW_BURST;
                    break;
                case "E":
                    setmode.value = CameraShootPhotoMode.EHDR;
                    break;
            }DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetShootPhotoModeAsync(setmode);

            var getmode = (await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetShootPhotoModeAsync()).value?.value;
            System.Diagnostics.Debug.WriteLine("Current Shoot Mode : " + getmode);
            //take photo mode

            

        }

        private async void Select_FocusMode(object sender, RoutedEventArgs value)
        {
            RadioButton rb = sender as RadioButton;
            var setFM = new CameraFocusModeMsg();

            switch (rb.Tag.ToString())
            {
                case "M":
                    setFM.value = CameraFocusMode.MANUAL;
                    break;
                case "AF":
                    setFM.value = CameraFocusMode.AF;
                    break;
                case "AFC":
                    setFM.value = CameraFocusMode.AFC;
                    break;
            } DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetCameraFocusModeAsync(setFM);

            var getFM = (await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetCameraFocusModeAsync()).value?.value;

            System.Diagnostics.Debug.WriteLine("Set Focus mode ==> " + getFM);
        }

        private async void Select_Format(object sender, RoutedEventArgs value)
        {
            RadioButton rb = sender as RadioButton;
            String format = rb.Tag.ToString();

            var setformat = new PhotoStorageFormatMsg();
            switch (format)
            {
                case "RAW":
                    setformat.value = PhotoStorageFormat.RAW;
                    break;
                case "JPEG":
                    setformat.value = PhotoStorageFormat.JPEG;
                    break;
                case "RAW_JPEG":
                    setformat.value = PhotoStorageFormat.RAW_JPEG;
                    break;
            }

            DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetPhotoStorageFormatAsync(setformat);

            var getformat = (await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetPhotoStorageFormatAsync()).value?.value;
            System.Diagnostics.Debug.WriteLine("Set Format ==> " + getformat);
        }

        private async void Get_Photo(object sender, RoutedEventArgs value)
        {
            RemainSpace.Text = (await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetSDCardRemainSpaceAsync()).value?.value.ToString() + " MB";

            var error = (await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).StartShootPhotoAsync());
            ErrorinPhoto.Text = error.ToString();
        }

        private async void Get_WaypointMissionState(object sender, RoutedEventArgs vlaue)
        {
            var state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();
            WaypointMissionState.Text = state.ToString();
        }

        private async void Get_WaypointMissionState(object sender, WaypointMissionStateTransition? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                WaypointMissionState.Text = value.HasValue ? value.Value.current.ToString() : DJI.WindowsSDK.WaypointMissionState.UNKNOWN.ToString();
            });
        }
        /*
        private async void Get_UploadState(object sender, RoutedEventArgs vlaue)
        {
            var state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();
            UploadState.Text = state.ToString();
        }
        */
        private async void Get_UploadState(WaypointMissionHandler sender, WaypointMissionUploadState? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                String data = value.HasValue ? JsonConvert.SerializeObject(value) : "Invalid data";
                UploadState.Text = data;
                System.Diagnostics.Debug.WriteLine("Upload State => " + data);
            });
        }
        /*
        private async void Get_DownloadState(object sender, RoutedEventArgs vlaue)
        {
            var state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();
            WaypointMissionState.Text = state.ToString();
        }
        //*/
        //private async void Get_DownloadState(WaypointMissionHandler sender, WaypointMissionDownloadState? value)
        //{
        //    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        String data = value.HasValue ? JsonConvert.SerializeObject(value) : "Invalid data";
        //        DownloadState.Text = data;
        //        System.Diagnostics.Debug.WriteLine("Download State => " + data);
        //    });
        //}
        /*
        private async void Get_ExecutionState(object sender, RoutedEventArgs vlaue)
        {
            var state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();
            WaypointMissionState.Text = state.ToString();
        }
        */
        private async void Get_ExecutionState(WaypointMissionHandler sender, WaypointMissionExecutionState? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            { 
                String data = value.HasValue ? JsonConvert.SerializeObject(value) : "Invalid data";
                ExecutionState.Text = data;
                System.Diagnostics.Debug.WriteLine("Execution State => " + data);
            });
        }

        private async void Get_AircraftObject(object sender, RoutedEventArgs value)
        {
            var craft = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value?.value;
            
            System.Diagnostics.Debug.WriteLine("Current Aircraft : " + craft);
            CurrentAircaft.Text = "Current Aircraft : " + craft;
        }

        private async void Get_AircraftObject(object sender, ProductTypeMsg? craft)
        {
            //var craft = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value?.value;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                System.Diagnostics.Debug.WriteLine("Current Aircraft : " + craft?.value);
                CurrentAircaft.Text = "Current Aircraft : " + craft?.value;
            });
        }

        private async void Get_BatteryRemain(object sender, RoutedEventArgs value)
        {
            var bat = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value?.value;
            if (bat != null)
            {
                DD.Battery((int)bat); // = (int)bat;
            }
            System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat);
            CurrentBattery.Text = "Remaining Battery : " + bat;
        }

        private async void Get_BatteryRemain(object sender, IntMsg? bat)
        {
            //var bat = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value?.value;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (bat?.value != null)
                {
                    DD.Battery((int)bat?.value);
                }//= (int)bat?.value;
                var JsonString = JsonConvert.SerializeObject(DD);
                socket.Emit("drone_data", JsonString);

                System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat?.value);
                CurrentBattery.Text = "Remaining Battery : " + bat?.value;
            });

        }

        private async void Get_Location(object sender, RoutedEventArgs value)
        {
            var location = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync()).value;

            String latitude = location?.latitude.ToString();
            String longitude = location?.longitude.ToString();
            DD.SetLocation(longitude, latitude);
            var altitude = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync()).value?.value;

            System.Diagnostics.Debug.WriteLine("Longitude : " + longitude);
            System.Diagnostics.Debug.WriteLine("Latitude : " + latitude);
            System.Diagnostics.Debug.WriteLine("Altitude : " + altitude);
            CurrentLongitude.Text = "Longitude : " + longitude;
            CurrentLatitude.Text = "Latitude : " + latitude;
            CurrentAltitude.Text = "Altitude : " + altitude;

        }

        private async void Get_Location(object sender, LocationCoordinate2D? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                String longitude = value?.longitude.ToString();
                String latitude = value?.latitude.ToString();
                DD.SetLocation(longitude, latitude);

                var JsonString = JsonConvert.SerializeObject(DD);
                socket.Emit("drone_data", JsonString);

                System.Diagnostics.Debug.WriteLine("Longitude : " + longitude);
                System.Diagnostics.Debug.WriteLine("Latitude : " + latitude);

                CurrentLongitude.Text = "Longitude : " + longitude;
                CurrentLatitude.Text = "Latitude : " + latitude;
            });
        }

        private async void Get_Altitude(object sender, DoubleMsg? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DD.Altitude(value?.value.ToString());

                var JsonString = JsonConvert.SerializeObject(DD);
                socket.Emit("drone_data", JsonString);

                System.Diagnostics.Debug.WriteLine("Altitude : " + value?.value);
                CurrentAltitude.Text = "Altitude : " + value?.value;
            });
        }

        private async void Get_GpsSignalLevel(object sender, RoutedEventArgs value)
        {
            var level = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetGPSSignalLevelAsync()).value;

            System.Diagnostics.Debug.WriteLine("GPS Signal Level : " + level?.value);
            //CurrentGpsSignalLevel.Text = "GPS Signal Level : " + level?.value;
        }

        private async void Get_GpsSignalLevel(object sender, FCGPSSignalLevelMsg? value)
        {
            var level = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetGPSSignalLevelAsync()).value;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                System.Diagnostics.Debug.WriteLine("GPS Signal Level : " + level?.value);
                //CurrentGpsSignalLevel.Text = "GPS Signal Level : " + level?.value;
            });
        }

        private async void Get_SerialNumber(object sender, RoutedEventArgs value)
        {
            var serialNumber_Drone = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetSerialNumberAsync()).value;
            //var serialNumber_Battery = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetSerialNumberAsync()).value;
            //var serialNumber_Camera= (await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetSerialNumberAsync()).value;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                System.Diagnostics.Debug.WriteLine("Serial Number : " + serialNumber_Drone?.value);
                //SerialNumber.Text = "Serial Number : " + serialNumber_Drone?.value;
            });
        }

        private async void Get_SerialNumber(object sender, StringMsg? value)
        {
            var serialNumber_Drone = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetSerialNumberAsync()).value;
            //var serialNumber_Battery = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetSerialNumberAsync()).value;
            //var serialNumber_Camera = (await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetSerialNumberAsync()).value;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                System.Diagnostics.Debug.WriteLine("Serial Number : " + serialNumber_Drone?.value);
                //SerialNumber.Text = "Serial Number : " + serialNumber_Drone?.value;
            });
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (state == SDKRegistrationState.Succeeded) System.Diagnostics.Debug.WriteLine("Activated.");
                else System.Diagnostics.Debug.WriteLine("Not Activated.");
                if (resultCode == SDKError.NO_ERROR)
                {
                    System.Diagnostics.Debug.WriteLine("Register success.");
                    activateStateTextBlock.Text = "Register Success";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(resultCode.ToString());
                    activateStateTextBlock.Text = resultCode.ToString();
                }
            });
        }

        //private async void Throttle_change(object sender, RangeBaseValueChangedEventArgs e) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue((float)e.NewValue, 0, 0, 0);
        //private async void Roll_change(object sender, RangeBaseValueChangedEventArgs e) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, (float)e.NewValue, 0, 0);
        //private async void Pitch_change(object sender, RangeBaseValueChangedEventArgs e) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, (float)e.NewValue, 0);
        //private async void Yaw_change(object sender, RangeBaseValueChangedEventArgs e) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, (float)e.NewValue);
        //private async void Throttle_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(1, 0, 0, 0);
        //private async void Throttle_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(-1, 0, 0, 0);
        //private async void Pitch_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 1, 0);
        //private async void Pitch_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, -1, 0);
        //private async void Yaw_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, 1);
        //private async void Yaw_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, -1);
        //private async void Roll_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 1, 0, 0);
        //private async void Roll_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, -1, 0, 0);
        private async void Emergency(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, 0);

        private void ExecutionState_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
