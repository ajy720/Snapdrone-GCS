
---

# 7/24,25 드론 정보 받아오기

작성일 : 7/25(수) 3:21AM

## 드론 정보를 받아오기 위한 기본 준비물

```C#
DJISDKManager.Instance.ComponentManager.
```



## 드론의 어디 Component를 이용할 건지?

```C#
Get~~~Handler(parameter).
```

상세 인자는

https://developer.dji.com/api-reference/windows-api/Components/ComponentManager.html

참고



## 가져올 상세 정보

```C#
Get~~~Async()
```

상세 메서드 명은 상단에 있는 링크 안에서 각 핸들러 내부에 가면 있다.



## 종합해보면, 

```C#
DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync().value
```



란 코드가 처음 나온다, 근데 마지막에 붙은 ~~Async()의 메서드 특성 상, 이 메서드는 비동기로 불러와야 한다.



해서, 맨 앞에 `await`  키워드를  붙인다. (단, 마지막의 .value는 현재 입력 값을 자동으로 받아오기 때문에 괄호 밖으로 빼준다.



```C#
(await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value
```



이걸 바로 `WriteLine` 에 때려박든 변수에 알아서 넣든 하자.



## 그 외 중요한 사항들

메인 페이지에서 버튼을 입력 받았을 때 출력하고 싶으면, 프론트 .xaml에

```C#
<Button Margin="0,5,0,0" Click="Get_BatteryRemain"> GETBATTERY </Button>
```



버튼을 추가해주고 `Click` 이라는 이벤트를 호출하면 불러오게 하자. 호출당하는 메서드는 .xaml.cs 안에다 넣자.

```C#
private async void Get_BatteryRemain(object sender, RoutedEventArgs e)
    {
      var bat = (await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0,  0).GetChargeRemainingInPercentAsync()).value;
      System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat?.value);
    }
```

이런 식이다. 



버튼의 이벤트명, 호출받는 첫번째 메서드 이름을 같게 하자



버튼과 이벤트 메서드는 같은 폴더 안에 있어야 한다! 

> Exam.xaml , Exam,xaml.cs



async와 await를 사용하여 비동기화를 꼭 시키자.



버튼 호출 이벤트의 인자는 그냥 맘편히 `(object sender, RoutedEventArgs e)` 로 고정하자. delegate error 예방.



불러온 값을 변수에 넣고, 다시 쓸 때  `(변수명)?.value` 하는 거 잊지 말자.



## 마무리

사실 드론 기체 이름 받아오는 DJISDJDEMO의 예제 코드가 더럽게 복잡한거였다.



사실 그렇게 길고 복잡한 코드는 아니다. 



**이번에 짠 코드를 재활용하여 다른 정보도 싹 다 얻어버리자!**



---

# 7/29,30,31 드론 정보 비동기로 받아오기

작성일 : 7/31(수)  1:11AM

## '드론 정보를 비동기로 가져온다' ?

DJI 사의 Windows SDK에는 ~~~Changed라는 event가 무지 많다. 이들은 ~~~가 의미하는 데이터가 변경됐을 때의 이벤트를 의미한다. 이 이벤트와 바뀐 정보를 가져오는 메소드를 엮으면 드론의 특정 데이터가 변경되었을 때 자동으로 변경된 데이터를 가져오게 된다.

e.g)

```c#
DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += Get_BatteryRemain;
```



## 처음 삽질했던 것들 - 같은 실수를 반복하지 말자

- UWP 프로젝트의 initial 페이지에 바로 이벤트랑 메소드 등록을 때려박는 멍청한 짓은 하지 말자.

1. 아직 정확한 원인은 모름.
   1. 추측
      1. TestingPage가 로드 됨.
      2. 아직 SDKRegistration이 끝나지 않음
      3. ComponentManger 클래스를 받아오면 Null이 반환됨.
   2. 결론 : NullReferenceException이 발생한다.
   3. 해결법
      1. 새 메소드 하나를 만들자
      2. 그 안에 이벤트 다 넣어버리면 된다
      3. e.g)

```c#
private async void Get_DroneData_Master(object sender, RoutedEventArgs value)
    {
      DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeCha nged += Get_AircraftObject;
      DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += Get_BatteryRemain;
      DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += Get_Location;
      DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged += Get_Altitude;
      DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GPSSignalLevelChanged += Get_GpsSignalLevel;
      DJISDKManager.Instance.ComponentManager.GetProductHandler(0).SerialNumberChanged += Get_SerialNumber;
    }
```



- 프론트(xaml)의 Textblock에 바로 박는 멍청한 짓은 하지 말자.

1. 비동기로 받아올 때는, 실행되는 스레드가 달라 스레드 처리를 해주어야 한다.
2. e.g)

```c#
await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
      {
        System.Diagnostics.Debug.WriteLine("Remaining Battery : " + bat?.value);
        CurrentBattery.Text = "Remaining Battery : " + bat?.value;
      });
```





---

# 7/31 mavic air 테스트 환경 설정 + 가상 컨트롤러 API

작성일 : 7/31(수) 3:08AM

## Wi-Fi 모드만 지원하는 MAVIC AIR를 조종기 없기 테스트 하는 법!

다만 좀 귀찮게 가상 컨트롤러를 UWP 내에 직접 만들어주어야 한다.

1. MAVIC AIR를 USB로 PC와 연결한다. - 이 때 MAVIC AIR와 Wi-Fi 연결이 돼있으면 있으면 안 된다.
2. DJI ASSISTANT 2 를 실행한다.
3. MAVIC AIR와 PC를 Wi-Fi로 연결하고, GCS를 실행한다.
4. ASSISTANT 내의 시뮬레이터를 켠다.
5. GCS로 자유조작

- 시뮬레이터는, 드론의 조종 정보를 받는다. 
- 이 때 드론 조종 정보란, Window GCS와 연결된 가상의 컨트롤러에서 나온다
- 해서, 시뮬레이터는 GCS의 조종대로 이동한다.
- GCS는 시뮬레이터 내의 정보를 실제 값으로 인식하고, 정보를 받아온다.



## VirtualRemoteController API

- UpdateJoystickValue

```c#
void UpdateJoystickValue(float throttle, float roll, float pitch, float yaw)
```

- 각 파라미터는 -1 ~ 1 의 실수이다.
- throttle - 드론의 상하 제어 / 컨트롤러의 좌측 상하
- roll - 드론의 좌우 방향 제어 / 컨트롤러의 좌측 좌우
- pitch - 드론의 전후 제어 / 컨트롤러의 우측 상하
- yaw - 드론의 좌우 이동 제어 / 컨트롤러의 우측 좌우

---

# 8/2 WaypointMission에 대하여(미션 플라이트)

작성일 : 8/2(금) AM 1:20 ~

## Waypoint Mission이란?

- Location 정보를 주면, 그 순서에 따라 좌표를 향해 비행하는 모드.



## Waypoint Mission의 속성들(중요한 것만)

- ### WaypointMissionState - enum

  | property         | description                                                                  |
  | ---------------- | ---------------------------------------------------------------------------- |
  | DISCONNECTED     | 모바일 기기와, 컨트롤러와, 기체의 연결이 끊김                                                   |
  | RECOVERING       | 각 기기 사이의 연결을 구성하는 중. operator는 기체의 상태와 동기화한다.                                |
  | READY_TO_UPROAD  | 기체에 미션을 올릴 준비가 되었다.                                                          |
  | UPLOADING        | 미션 업로드가 성공적으로 시작되었음.                                                         |
  | READY_TO_EXECUTE | Waypoint Mission 업로드가 완료되었고 기체가 실행할 준비가 된 상태.                                |
  | EXECUTING        | 실행이 성공적으로 시작됨.                                                               |
  | EXECUTE_PAUSED   | Waypoint Mission이 성공적으로 중단되었음. 사용자는 실행을 계속하기 위해 resume mission을 call 할 수 있다. |

  

- ### WaypointMissionStateTransition - struct

  | type                 | variable name | description                     |
  | -------------------- | ------------- | ------------------------------- |
  | WaypointMissionState | previous      | 이전 Waypoint Mission handler의 상태 |
  | WaypointMissionState | current       | 현재 Waypoint Mission handler의 상태 |

  

- ### WaypointMissionGotoFirstWaypointMode - enum

  | property       | description                                                                                                                          |
  | -------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
  | SAFELY         | Waypoint로 안전하게 간다. 기체의 현재 고도가 목적지의 고도보다 낮다면 목적지의 고도와 같게 상승한다. 그리고 기체는 목적지의 좌표로 현재 고도에서 이동하고, 목적지의 고도로 계속 진행한다(목적지의 고도와 계속 맞춘다는 말인듯?) |
  | POINT_TO_POINT | 직선거리로 바로 간다.                                                                                                                         |

  

- ### WaypointMissionExecutionState - struct

  | type                              | variable name       | description                                                                                    |
  | --------------------------------- | ------------------- | ---------------------------------------------------------------------------------------------- |
  | int                               | targetWaypointIndex | 기체가 진행 할 다음 Waypoint의 인덱스.                                                                     |
  | int                               | totalWaypointCount  | Waypoint Mission의 총 Waypoint 갯수.                                                               |
  | bool                              | isWaypointReached   | 기체가 Waypoint에 도착하면 true가 되고, Waypoint action과 heading(목적)이 완전히 변경되면, 이 값은 증가하고 속성이 false로 변한다. |
  | bool                              | isExecutionFinish   | Waypoint Mission이 끝났는지 여부                                                                      |
  | WaypointMission<br />ExecuteState | state               | 현재 Waypoint Mission handler의 상태                                                                |

  

- ### WaypointMissionExecuteState - enum

  | property              | description                                                                                                                    |
  | --------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
  | INITIALIZING          | Waypoint Mission이 초기화되는중이다. 미션이 시작되었고 기체가 첫번째 목적지를 향해 가는 중이라는 의미.                                                              |
  | MOVING                | 기체가 현재 미션의 다음 목적지를 향해 이동하는 중이다.  WaypointMissionFlightPathMode가 NORMAL로 set 되었을 때 발생한다.                                        |
  | CURVE_MODE_MOVING     | 기체가 현재 움직이는 중이다. WaypointMissionFlightPathMode가 CURVED로 set 되었을 때 발생한다.                                                        |
  | CURVE_MODE_TURNING    | 기체가 현재 움직이는 중이다. WaypointMissionFlightPathMode가 CURVED로 set 되었을 때 발생한다.                                                        |
  | BEGIN_ACTION          | 기체가 목적지에 도착했고, 새 방향을 향해 회전했고, 지금은 Action을 진행하는 중이다. 이 상태는 waypoint actions가 실행을 시작하기 전에 호출될 것이고, 각각의 waypoint action에 발생할 것이다. |
  | DOING_ACTION          | 기체가 waypoint에 있고, action을 실행하는 중이다.                                                                                            |
  | FINISHED_ACTION       | 기체가 waypoint에 있고, 현재 waypoint action 실행을 끝냈다. 이 상태는 각 waypoint action마다 한 번 발생한다.                                              |
  | RETURN_TO_FIRST_POINT | 기체가 첫 번째 waypoint로 돌아왔다.WaypointMissionFinishedAction이 GO_FIRST_WAYPOINT로 set 되었을 때 발생한다.                                      |
  | PAUSED                | 미션이 현재 사용자에 의해 중지되었다.                                                                                                          |



- ### WaypointMission - struct

  | type                                             | variable name                          | description                                                                                                                                                                                                                          |
  | ------------------------------------------------ | -------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
  | int                                              | waypointCount                          | WM의 waypoint 갯수.                                                                                                                                                                                                                     |
  | double                                           | maxFlightSpeed                         | 기체가 목적지로 이동할 때 최대 속도를 조정한다. 컨트롤러의 throttle 스틱을 이용해 속도를 높여도 막혀버림. 이 변수는 2~15의 범위를 가진다. 기체의 속도는 [0, maxFlightSpeed] 사이에서 1000단계로 나뉜다.                                                                                                  |
  | double                                           | autoFlightSpeed                        | 기체가 이동할 때 기본 속도를 [-15, 15]m/s로 지정한다. <br />autoFlightSpeed이<br /> > 0 : 이 변수값 + 조종기 값의 속도로 비행(maxFlightSpeed 이하로)<br /><br />= 0 : 오로지 컨트롤러의 조작에 의해 속도가 조절됨.<br />< 0 && (첫번째 waypoint에 위치함) : 컨트롤러로 속도를 positive로 만들기 전까지공중에 떠 있는다. |
  | WaypointMission<br />FinishedAction              | finishedAction                         | 기체가 WM을 끝냈을 때 가져오는 Action                                                                                                                                                                                                            |
  | WaypointMission<br />HeadingMode                 | headingMode                            | 목적지 사이를 이동할 때 같은 기체의 방향. 기본은 AUTO                                                                                                                                                                                                    |
  | WaypointMission<br />FlightPathMode              | flightPathMode                         | 기체가 따라가는 목적지 사이의 길. 기체는 목적지 사이를 직선으로 바로 비행하거나, 목적지 위치가 curve의 일부로 정의되었을 때,목적지 가깝게 곡선을 그리며 돈다                                                                                                                                         |
  | WaypointMission<br />GotoFirst<br />WaypointMode | gotoFirst<br />WaypointMode            | 어떻게 기체가 현재 위치에서 첫번째 목적지로 올 것인지 정의한다. 기본은 SAFELY(WaypointMissionGotoFirstWaypointMode)                                                                                                                                                |
  | bool                                             | exitMission<br />OnRCSignalLostEnabled | 기체와 컨트롤러 사이 연결을 잃어버렸을 때 미션을 멈출 것인지 아닌지 결정한다. 기본은 FALSE(bool)                                                                                                                                                                         |
  | Location<br />Coordinate2D                       | pointOfInterest                        | WaypointMission중 기체의 방향이 관심지점(POI)으로 고정된다. WaypointMissionHeadingMode headingMode가 TOWARD_POINT_OF_INTEREST일 때 사용한다                                                                                                                  |
  | bool                                             | gimbalPitch<br />RotationEnabled       | 참이면 짐벌 pitch rotation이 WaypointMission 중에 제어될 수 있다. TRUE 일 때, Waypoint 안의 double gimbalPitch가 짐벌 피치를 조정할 때 사용된다.                                                                                                                     |
  | int                                              | repeatTimes                            | 미션 실행은 1번 이상 반복될 수 있다. 0은 미션이 한 번 실행되는 것을 의미하고, 반복되지 않는다. 1의 의미는 총 두 번 실행한다. (기본 1회 + value = 총 execute)                                                                                                                             |
  | int                                              | missionID                              | mission ID가 WaypointMission에 할당된다.                                                                                                                                                                                                   |



---

# 8/3 Upload 

작성일 : 8/3(토)

Mission Flight 기능 코드를 넣었지만, 아직 INVALID_REQUEST_IN_CURRENT_STATE 에러로 실행 불가. 그 외 RTH, Take off, Landing 기능은 정상 작동. 추가해야 할 것은 Landing을 눌러도 완전 착륙은 x, 고도를 낮출 뿐



---

# 8/4~5 Upload

작성일 : 8/5(월) AM 12:35

WaypointMission load가 되지 않아서 모든 상태를 불러와서 확인할 수 있도록 코드 추가. 아직 드론이 없어서 테스트는 안 해봄.



---

# 8/12 이건우 멘토님 멘토링

작성일 : 8/12(월) 

### 각 API의 역할

LoadMission은 SDK에 WaypointMission을 올려놓는다.(일종의 버퍼)

UploadMission은 일종의 버퍼에 저장된 WaypointMission을 드론에 업로드 한다.

StartMission은 UploadMission이 끝난 후, 드론에게 미션 시작을 명령한다. 



---

# 8/13 새벽 문득 든 생각

작성일 : 8/13(화)

WaypointMission을 직접 만들어도 되는데, 그러지 않았던 이유는 드론의 방향을 알 수 있는 방법을 몰랐기 때문이다. 하지만 IMU 센서가 그 역할을 한다는 것을 깨닫고 가능성을 생각하게 되었다.



---

# 8/13 수동 고도 조절

작성일 : 8/14(수) AM 1:59

## 서론

Waypoint Mission의 API가 도저히 먹통임을 깨닫고, 직접 만들기로 결심.

대략 순서를 정리해보면 

> 1. 드론을 이륙(take off) 시킨다.
> 2. 드론을 목표 고도(altitude)까지 상승시킨다.
> 3. 드론의 IMU 센서를 이용하여 어느 방향을 바라보고 있는지 파악한다.
> 4. 드론의 방향을 목적지를 향해 설정한다.
> 5. 드론을 이동시킨다.

현재 단계는 1, 2단계는 성공했다.



## 본론

일단 내가 직접 만든 Mission Flight의 Method 이름은 `Run_MissionFlight`이다.

인자로 목표 위도, 경도, 고도를 받아 목표 좌표로 이동하는 코드를 짜고 있다.

### Zero Step

​	받은 인자를 현재 드론의 위치와 비교할 수 있도록 `double` 형으로 바꿔야 한다.

​	`Convert.ToDouble()`를 사용하자.

​	현재 드론의 위치를 저장한 지역 변수를 하나 선언하자. `double nowlat`

### First Step

​	드론 이륙부터 시키자. 저번에 이미 했던 것처럼  `StartTakeoffAsync()` 메서드를 	사용하자.

### Second Step

​	드론 고도를 상승시켜야 하니 [UpdateJoystickValue](##VirtualRemoteController API)를 사용하자.

​	`	DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(1, 0, 0, 0);`

### Third Step

​	드론이 고도를 올리다가 목표 고도에 도달하면 그만 고도를 유지하도록 하자.

```C#
while(true){
    nowalt = (double)(await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync()).value?.value;
    if(nowalt >= alt){
		DJISDKManager.Instance.VirtualRemoteController.UpdateJoystickValue(0, 0, 0, 0);
        break;
    }
    
}
```



*(Actually, I don't know how to do better this feature. I think it must have better way.)*

더 좋은 방법이 분명이 존재할 것이라고 믿지만, 일단은 이 방식으로 구현했다.

~~작동하긴 하더라...~~



## 결론

작동하기는 하는데,,,, Third Step 부분 코드가 아쉽다. 비동기에 대해서 공부를 많이 해야할 것 같다. 앞으로 IMU 센서 값 받아서 방향 직접 계산하려니 막막하다. 화이팅!



#### 그 외 그냥 하고싶은 잡담

깃헙 페이지를 파서 여기다가 이렇게 적지 말고 블로그 해보려고 하는데 중간평가랑 겹치는 바람에 시간이 부족해서(핑계는 참) 아직도 여기다가 쓴다. 하루 빨리 옮겨 가기를! 그래도 아직 준비가 덜 됐긴 하지만 페이지는 파놨다. [여기!](ajy720.github.io) 



README.md를 작성할 때 마다 스스로가 성장함을 느낀다. 그 성취감에 이 README를 계속 이어 쓰고 있지 않을까 생각한다. 특히 md파일 작성하는 것은 한 달이 채 안되는 시간에 엄청 늘었다. 아직 많이 부족하지만 한 달 전의 나를 되돌아 보니 아직 앞으로도 한참 남았구나 싶다.



오늘도 1 commit ㅎ



---

# 8/15 Waypoint Mission

작성일 : 8/15(목) PM 11:13

드론의 IMU 센서를 통해 Waypoint Mission을 직접 만들어보려 했는데, DJI SDK가 IMU 센서의 값을 받아오는 API는 만들어 놓지 않았다(안전 때문이라고 믿는다.) .

그리하여 직접 만드는 것은 포기하고, 기존에 하던 대로 Waypoint Mission API를 삽질해서 뚫어보려 시도했다. 

하지만 곧 깨달았다.

MAVIC AIR는 DJI Windows SDK 에서 지원하는 Waypoint Mission을 사용하지 못한다.

![1565878594722](./images/1565878594722.png)

보면 알 수 있듯 Flight controller firmware 3.2.10.0 혹은 그 이상만 지원한다고 나와있는데, MAVIC AIR는 애초에 FC를 사용하지 않는다.(Wi -Fi 모드만 지원)

그러니 빨리 MAVIC 2를 사버리자,, 