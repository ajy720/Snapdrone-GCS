using DJI.WindowsSDK;
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

namespace DJIWindowsSDKSample.DJISDKInitializing
{
    public sealed partial class ActivatingPage : Page
    {
        public ActivatingPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
            DJISDKManager.Instance.RegisterApp("c9e68649b0d02247fc1410eb");
        }
        private async void Get_BatteryRemain(object sender, RoutedEventArgs e)
        {
            var bat = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value;
            System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat?.value);
        }
        private async void Get_AircraftObject(object sender, RoutedEventArgs e)
        {
            var craft = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value;
            System.Diagnostics.Debug.WriteLine("Current Aircraft : " + craft?.value);
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (state == SDKRegistrationState.Succeeded) System.Diagnostics.Debug.WriteLine("Activated.");
                else System.Diagnostics.Debug.WriteLine("Not Activated.");
                if(resultCode == SDKError.NO_ERROR) System.Diagnostics.Debug.WriteLine("Register success.");
                else System.Diagnostics.Debug.WriteLine(resultCode.ToString());
            });
        }

    }
}
