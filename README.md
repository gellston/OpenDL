
# OpenDL 1.1

#### 실행환경
* Window10, .Net Core 3.1, Cuda10

#### 라이브러리 사용법

* C++  


```cpp

#include <iostream>
#include <opencv2/opencv.hpp>

#include "ODSegmentationFactory.h"


int main()
{
    cv::namedWindow("original");
    cv::namedWindow("result");

    cv::Mat image = cv::imread("C://Github//OpenDL//OpenDL//x64//Release//00036.jpg");
    cv::Mat result = cv::Mat(cv::Size(256, 256), CV_32FC1); // 32bit 배열 필요.

    odl::IODSegmentation* segmentation = SegmentatorLoader("C://Github//OpenDL//OpenDL//x64//Release//FaceDetector.frz",      // 프리징 모델 경로
                                                           "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml"); // 모델 정보파일 경로

    std::cout << "label count : " << segmentation->GetLabelCount() << std::endl; // 라벨 갯수 출력
    
    segmentation->Run(image.data, (float *)result.data); // 세그먼테이션 수행

    cv::threshold(result, result, 0.8, 255, cv::THRESH_BINARY); // 시각화를 위한 이진화

    cv::imshow("original", image);
    cv::imshow("result", result);
    cv::waitKey();

    delete segmentation;
}

```

* C#


```cpp

// System.Console.WriteLine("준비중입니다..");

```

#### 업데이트 예정 사항

 1. C++, C# 라이브러리 추가 예정.
 2. Classification 학습 기능 추가.

---
#### 1.1 Release Note

* 업데이트 리스트  

```
1. Segmentation C++ 라이브러리 추가
```
---

#### 1.0 Release Note

* 업데이트 리스트  

```
1. Segmentation 학습 기능 추가

2. Segmentation 모델 추가 (HySegNetV1)
   - 입력 이미지 사이즈 : 256x256x3
   - 아웃풋 라벨 갯수 : 1
```
---