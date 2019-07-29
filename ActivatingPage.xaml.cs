using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace DJIWindowsSDKSample.DJISDKInitializing
{
    public sealed partial class TestingPage : Page
    {
        private DJIVideoParser.Parser videoParser;

        public TestingPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
            //DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).CurrentChanged += Instance_BatteryChangeEvent;
            DJISDKManager.Instance.RegisterApp("c9e68649b0d02247fc1410eb");


        }
        private async void Get_Photo(object sender, RoutedEventArgs value)
        {
            var setmode = new CameraShootPhotoModeMsg();
            setmode.value = CameraShootPhotoMode.NORMAL;
            DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetShootPhotoModeAsync(setmode);

            var getmode =(await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetShootPhotoModeAsync()).value?.value;
            System.Diagnostics.Debug.WriteLine("Current Shoot Mode : " + getmode);
            PhotoMode.Text = "Current Shoot Mode : " + getmode;
        }

        private async void Get_AircraftObject(object sender, RoutedEventArgs value)
        {
            var craft = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value?.value;
            if (craft == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                CurrentAircaft.Text = "Airdraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Current Aircraft : " + craft);
                CurrentAircaft.Text = "Current Aircraft : " + craft;
            }
        }

        private async void Get_BatteryRemain(object sender, IntMsg? value)
        {
            var bat = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value?.value;
            if (bat == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                CurrentBattery.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat);
                CurrentBattery.Text = "Remaining Battery : " + bat;
            }

        }

        private async void Get_BatteryRemain(object sender, RoutedEventArgs value)
        {
            var bat = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value?.value;
            if (bat == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                CurrentBattery.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat);
                CurrentBattery.Text = "Remaining Battery : " + bat;
            }

        }

        private async void Get_Location(object sender, LocationCoordinate2D? value)
        {
            //var location = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync()).value;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                String latitude = value?.latitude.ToString();
                String longitude = value?.longitude.ToString();

                if (longitude == null && latitude == null)
                {
                    System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                    CurrentLongitude.Text = "Aircraft isn't connected";
                    CurrentLatitude.Text = "Aircraft isn't connected";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Longitude : " + longitude);
                    System.Diagnostics.Debug.WriteLine("Latitude : " + latitude);
                    CurrentLongitude.Text = "Longitude : " + longitude;
                    CurrentLatitude.Text = "Latitude : " + latitude;
                }
            });
        }

        private async void Get_Altitude(object sender, DoubleMsg? value)
        {
            //var altitude = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync()).value?.value;
            if (value == null)
            {
                CurrentAltitude.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Altitude : " + value);
                CurrentAltitude.Text = "Altitude : " + value;
            }
        }

        private async void Get_Location(object sender, RoutedEventArgs value)
        {
            var location = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync()).value;
            
            String latitude = location?.latitude.ToString();
            String longitude = location?.longitude.ToString();
            var altitude = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync()).value?.value;

            if (longitude == null&& latitude == null&& altitude == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                CurrentLongitude.Text = "Aircraft isn't connected";
                CurrentLatitude.Text = "Aircraft isn't connected";
                CurrentAltitude.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Longitude : " + longitude);
                System.Diagnostics.Debug.WriteLine("Latitude : " + latitude);
                System.Diagnostics.Debug.WriteLine("Altitude : " + altitude);
                CurrentLongitude.Text = "Longitude : " + longitude; 
                CurrentLatitude.Text = "Latitude : " + latitude;
                CurrentAltitude.Text = "Altitude : " + altitude;
            }
            
        }

        private async void Get_GpsSignalLevel(object sender, FCGPSSignalLevelMsg? value)
        {
            var level = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetGPSSignalLevelAsync()).value;

            if (level == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                CurrentGpsSignalLevel.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GPS Signal Level : " + level?.value);
                CurrentGpsSignalLevel.Text = "GPS Signal Level : " + level?.value;
            }
        }

        private async void Get_GpsSignalLevel(object sender, RoutedEventArgs value)
        {
            var level = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetGPSSignalLevelAsync()).value;

            if (level == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                CurrentGpsSignalLevel.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GPS Signal Level : " + level?.value);
                CurrentGpsSignalLevel.Text = "GPS Signal Level : " + level?.value;
            }
        }

        private async void Get_SerialNumber(object sender, StringMsg? value)
        {
            var serialNumber = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetSerialNumberAsync()).value;

            if (serialNumber == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                SerialNumber.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Serial Number : " + serialNumber?.value);
                SerialNumber.Text = "Serial Number : " + serialNumber?.value;
            }
        }

        private async void Get_SerialNumber(object sender, RoutedEventArgs value)
        {
            var serialNumber = (await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetSerialNumberAsync()).value;

            if (serialNumber == null)
            {
                System.Diagnostics.Debug.WriteLine("Aircraft isn't connected");
                SerialNumber.Text = "Aircraft isn't connected";
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Serial Number : " + serialNumber?.value);
                SerialNumber.Text = "Serial Number : " + serialNumber?.value;
            }
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (state == SDKRegistrationState.Succeeded) System.Diagnostics.Debug.WriteLine("Activated.");
                else System.Diagnostics.Debug.WriteLine("Not Activated.");
                if(resultCode == SDKError.NO_ERROR)
                {
                    System.Diagnostics.Debug.WriteLine("Register success.");
                    activateStateTextBlock.Text = "Connection Success.";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(resultCode.ToString());
                    activateStateTextBlock.Text = resultCode.ToString();
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        //private async void Instance_BatteryChangeEvent(object sender, IntMsg? value)
        //{
        //    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        //System.Diagnostics.Debug.WriteLine("Remaining Battery Changed : " + value?.value);
        //        //CurrentBattery.Text = value?.value.ToString();
        //    });
        //}
    }
}
