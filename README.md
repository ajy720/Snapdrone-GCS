# Take-Drone-Data
# 7/24(화) ~ 7/25(수) 새벽까지 고된 삽질의 결과물 (뿌듯하다)

작성일 : 7/25(수) 3:21AM

------


# 드론 정보를 받아오기 위한 기본 준비물

```C#
DJISDKManager.Instance.ComponentManager.
```



------



# 드론의 어디 Component를 이용할 건지?

```C#
Get~~~Handler(parameter).
```

상세 인자는

https://developer.dji.com/api-reference/windows-api/Components/ComponentManager.html

참고



------



# 가져올 상세 정보

```C#
Get~~~Async()
```

상세 메서드 명은 상단에 있는 링크 안에서 각 핸들러 내부에 가면 있다.



------



# 종합해보면, 

```C#
DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync().value
```



란 코드가 처음 나온다, 근데 마지막에 붙은 ~~Async()의 메서드 특성 상, 이 메서드는 비동기로 불러와야 한다.



해서, 맨 앞에 `await`  키워드를  붙인다. (단, 마지막의 .value는 현재 입력 값을 자동으로 받아오기 때문에 괄호 밖으로 빼준다.



```C#
(await DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).GetChargeRemainingInPercentAsync()).value
```



이걸 바로 `WriteLine` 에 때려박든 변수에 알아서 넣든 하자.



------



# 그 외 중요한 사항들

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



------





## 마무리

사실 드론 기체 이름 받아오는 DJISDJDEMO의 예제 코드가 더럽게 복잡한거였다.



사실 그렇게 길고 복잡한 코드는 아니다. 



**이번에 짠 코드를 재활용하여 다른 정보도 싹 다 얻어버리자!**





# 7/29,30,31 드론 정보 비동기로 받아오기



작성일 : 7/31(수)  1:11AM

## '드론 정보를 비동기로 가져온다' ?

DJI 사의 Windows SDK에는 ~~~Changed라는 event가 무지 많다. 이들은 ~~~가 의미하는 데이터가 변경됐을 때의 이벤트를 의미한다. 이 이벤트와 바뀐 정보를 가져오는 메소드를 엮으면 드론의 특정 데이터가 변경되었을 때 자동으로 변경된 데이터를 가져오게 된다.

e.g)

```c#
DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += Get_BatteryRemain;
```

------





## 처음 삽질했던 것들 - 같은 실수를 반복하지 말자

- UWP 프로젝트의 initial 페이지에 바로 이벤트랑 메소드 등록을 때려박는 멍청한 짓은 하지 말자.

1. 아직 정확한 원인은 모름.
2. 추측
3. TestingPage가 로드 됨.
4. 아직 SDKRegistration이 끝나지 않음
5. ComponentManger 클래스를 받아오면 Null이 반환됨.
6. 결론 : NullReferenceException이 발생한다.
7. 해결법
8. 새 메소드 하나를 만들자
9. 그 안에 이벤트 다 넣어버리면 된다
10. e.g)

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



------



## VirtualRemoteController API

- UpdateJoystickValue

```c#
void UpdateJoystickValue(float throttle, float roll, float pitch, float yaw)
```

- 각 파라미터는 -1 ~ 1 의 실수이다.
- throttle - 드론의 상하 제어 / 컨트롤러의 좌측 상하
- roll - 드론의 좌우 이동 제어 / 컨트롤러의 우측 좌우
- pitch - 드론의 전후 제어 / 컨트롤러의 우측 상하
- yaw - 드론의 좌우 방향 제어 / 컨트롤러의 좌측 좌우