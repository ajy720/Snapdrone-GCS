# SnapDrone GCS

## How To Install

1. git clone https://github.com/dji-sdk/Windows-SDK.git

2. delete Windows-SDK\Sample Code\DJISampleSources\DJISDKInitializing

3. git clone https://github.com/ajy720/Snapdrone-GCS.git DJISDKInitializing(at Windows-SDK\Sample Code\DJISampleSourcesDJISampleSources)

4. Open DJIWindowsSDKSample.sln in Windows-SDK\Sample Code

5. Add package - Find in NuGet Package Manager

   1. SocketIoClientDotNet
   2. Newtonsoft.Json

6. Run(If you have some error when you run, follow solutions below)

   1. Set minimum version  DJIWindowsSDKSample_x64 
      1. right click on DJIWindowsSDKSample_x64
      2. into Properties
      3. Set minimum version to Build 16299 (Fall Creators Update, version 1709)
   2. Change project target DJIVideoParser(Universal Windows)
      1. right click on DJIVideoParser(Universal Windows)
      2. into Retarget Projects
      3. Set Windows SDK min. Version to 10.0.17763.0(or under)
   3. Set startUp project
      1. right click on DJIWindowsSDKSample_x64
      2. click Set StartUp Projects
   4. Start Debug with x64 solution platform

   

## How to set test environment

**MAVIC AIR(Support only Wi-Fi mode)**

1. Connect the aircraft to PC with USB.(At this time, the MAVIC AIR's Wi-Fi hasn't be connected.)
2. Open DJI ASSISTANT2(download link : https://www.dji.com/kr/downloads/softwares/assistant-dji-2)
3. After DJI ASSISTANT 2 connect MAVIC AIR, connect to MAVIC AIR's Wi-Fi.
4. Open simulator in DJI ASSISTANT 2.
5. Open GCS.
6. Start Testing.

**MAVIC 2(Zoom or Pro)**

Coming Soon.. (I think both Zoom & Pro will have same test environment)
