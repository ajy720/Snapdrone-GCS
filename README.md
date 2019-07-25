# Take-Drone-Data
## 7/24(화) ~ 7/25(수) 새벽까지 고된 삽질의 결과물 (뿌듯하다)

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



# 마무리

사실 드론 기체 이름 받아오는 DJISDJDEMO의 예제 코드가 더럽게 복잡한거였다.



사실 그렇게 길고 복잡한 코드는 아니다. 



**이번에 짠 코드를 재활용하여 다른 정보도 싹 다 얻어버리자!**
