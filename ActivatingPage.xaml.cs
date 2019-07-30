using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
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
using DJIWindowsSDKSample.FPV;
using DJIVideoParser;
using Quobject.SocketIoClientDotNet.Client;

namespace DJIWindowsSDKSample.DJISDKInitializing
{
    public sealed partial class TestingPage : Page
    {
        private DJIVideoParser.Parser videoParser;

        public TestingPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
            DJISDKManager.Instance.RegisterApp("c9e68649b0d02247fc1410eb");
        }

        private async void Get_DroneData_Master(object sender, RoutedEventArgs value)
        {
            DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += Get_AircraftObject;
            DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += Get_BatteryRemain;
            //DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += Get_Location;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged += Get_Altitude;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GPSSignalLevelChanged += Get_GpsSignalLevel;
            DJISDKManager.Instance.ComponentManager.GetProductHandler(0).SerialNumberChanged += Get_SerialNumber;
        }
        private async void Get_DroneData_DeMaster(object sender, RoutedEventArgs value)
        {
            DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged -= Get_AircraftObject;
            DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).CurrentChanged -= Get_BatteryRemain;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged -= Get_Location;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged -= Get_Altitude;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GPSSignalLevelChanged -= Get_GpsSignalLevel;
            DJISDKManager.Instance.ComponentManager.GetProductHandler(0).SerialNumberChanged -= Get_SerialNumber;
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
            
            System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat);
            CurrentBattery.Text = "Remaining Battery : " + bat;
        }

        private async void Get_BatteryRemain(object sender, IntMsg? bat)
        {
            //var bat = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value?.value;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat?.value);
                CurrentBattery.Text = "Remaining Battery : " + bat?.value;
            });

        }

        private async void Get_Location(object sender, RoutedEventArgs value)
        {
            var location = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync()).value;

            String latitude = location?.latitude.ToString();
            String longitude = location?.longitude.ToString();

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
                System.Diagnostics.Debug.WriteLine("Altitude : " + value?.value);
                CurrentAltitude.Text = "Altitude : " + value?.value;
            });
        }

        private async void Get_GpsSignalLevel(object sender, RoutedEventArgs value)
        {
            var level = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetGPSSignalLevelAsync()).value;

            System.Diagnostics.Debug.WriteLine("GPS Signal Level : " + level?.value);
            CurrentGpsSignalLevel.Text = "GPS Signal Level : " + level?.value;
        }

        private async void Get_GpsSignalLevel(object sender, FCGPSSignalLevelMsg? value)
        {
            var level = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetGPSSignalLevelAsync()).value;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                System.Diagnostics.Debug.WriteLine("GPS Signal Level : " + level?.value);
                CurrentGpsSignalLevel.Text = "GPS Signal Level : " + level?.value;
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
                SerialNumber.Text = "Serial Number : " + serialNumber_Drone?.value;
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
                SerialNumber.Text = "Serial Number : " + serialNumber_Drone?.value;
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


        private async void Throttle_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(1, 0, 0, 0);
        private async void Throttle_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(-1, 0, 0, 0);
        private async void Pitch_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 1, 0);
        private async void Pitch_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, -1, 0);
        private async void Yaw_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, 1);
        private async void Yaw_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, -1);
        private async void Roll_Up(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 1, 0, 0);
        private async void Roll_Down(object sender, RoutedEventArgs value) => DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, -1, 0, 0);
    }
}
