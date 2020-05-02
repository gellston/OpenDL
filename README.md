
# OpenDL 1.4


#### 데모영상
<center>

<a href="https://youtu.be/HkKbtf9pzhU" target="_blank"><img src="https://raw.githubusercontent.com/gellston/OpenDL/master/preview/preview1.2.png" 
alt="데모영상"/></a>

<a href="https://1drv.ms/u/s!AiHCv3RUThwnhhYJ8UNEaeQWTZsM?e=sM9ia5" target="_blank"> 데모 이미지 샘플 다운로드 </a>

</center>


#### 실행환경
* Window 10, .Net Core 3.1, Cuda10


#### Segmentation tutorial
* 결과 이미지

<center>

![Segmentation 예제](https://raw.githubusercontent.com/gellston/opendl/master/preview/segmentation.png)

</center>

* C++  


```cpp

#include <iostream>
#include <opencv2/opencv.hpp>

#include "ODSegmentationFactory.h"


int main()
{
    cv::namedWindow("original");
    cv::namedWindow("result");

    cv::Mat image = cv::imread("C://Github//OpenDL//sampleImage//segmentationSample.jpg");
    cv::Mat result = cv::Mat(cv::Size(256, 256), CV_32FC1);
    cv::Mat threshold = cv::Mat(cv::Size(256, 256), CV_32FC1);

    odl::IODSegmentation* segmentation = SegmentatorLoader("C://Github//OpenDL//OpenDL//x64//Release//FaceDetector.frz", 
                                                           "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml");
    // Segmentation 클래스 생성 

    segmentation->Run(image.data, (float *)result.data); 
    // Segmentation 수행

    std::cout << "label count : " << segmentation->GetLabelCount() << std::endl;
    // Segmentation Label 갯수 리턴

    cv::threshold(result, threshold, 0.8, 255, cv::THRESH_BINARY);
    // Segmentation Map 이진화

    cv::imshow("original", image);
    cv::imshow("result", threshold);
    cv::waitKey();

    delete segmentation;
}

```

* C#


```csharp

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace OpenDLCSharpConsoleExample
{
    class Example
    {
        static void Main(string[] args)
        {
            Cv2.NamedWindow("original");
            Cv2.NamedWindow("result");


            Mat image = new Mat("C://Github//OpenDL//sampleImage//segmentationSample.jpg");
            Mat result = new Mat(new Size(256, 256), MatType.CV_32FC1);
            Mat convertedDepth = new Mat();

            ODL.IODSegmentationSharp segmentation = ODL.ODSegmentationFactorySharp.SegmentatorLoader("C://Github//OpenDL//OpenDL//x64//Release//FaceDetector.frz",
                                                                                                     "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml");
            // 세그먼테이션 클래스 생성 

            segmentation.Run(image.Data, result.Data);
            // Segmentation 수행

            System.Console.WriteLine("label count :" + segmentation.GetLabelCount());
            // Segmentation Label 갯수 리턴

            Mat threshold = result.Threshold(0.8, 255, ThresholdTypes.Binary);
            threshold.ConvertTo(convertedDepth, MatType.CV_8UC1);
            // Segmentation Map 이진화

            Cv2.ImShow("original", image);
            Cv2.ImShow("result", convertedDepth);
            Cv2.WaitKey();
        }
    }
}


```


#### Classification tutorial
* 결과 이미지

<center>

![Segmentation 예제](https://raw.githubusercontent.com/gellston/OpenDL/master/preview/classification.png)

</center>

* C++  


```cpp

#include <iostream>
#include <opencv2/opencv.hpp>

#include "ODClassificationFactory.h"


int main()
{
    cv::namedWindow("original");

    cv::Mat image = cv::imread("C://Github//OpenDL//sampleImage//classificationSample.jpg");

    odl::IODClassification* classification = ClassificationLoader("C://Github//OpenDL//OpenDL//x64//Release//ObjectDetector.frz",
                                                                  "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml");

    // Classification 클래스 생성

    classification->Run(image.data);
    // Classification 수행 

    std::cout << "Index 0 : Lotion" << std::endl;
    std::cout << "Index 1 : Bottle" << std::endl;
    std::cout << "Index 2 : Roll tissue" << std::endl;
    std::cout << "Index 3 : Empty" << std::endl;
    // Classification 인덱스 정보. 
    // 학습 툴에서 만든 라벨 순서에 따름. 

    std::cout << "label count : " << classification->GetLabelCount() << std::endl;     // 라벨 갯수 리턴
    std::cout << "max score : " << classification->MaxScore() << std::endl;            // 최대 스코어 리턴
    std::cout << "max score index : " << classification->MaxScoreIndex() << std::endl; // 최대 스코어 인덱스

    std::cout << " == score list ==" << std::endl;                                     
    std::vector<double> scores = classification->Scores();
    for (int index = 0; index < 4; index++) {
        std::cout << "score : " << scores[index] << std::endl;
    }
    // 스코어 리스트 출력 

    cv::imshow("original", image);
    cv::waitKey();

    delete classification;
}

```

* C#


```csharp

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace OpenDLCSharpConsoleExample
{
    class Example
    {
        static void Main(string[] args)
        {
            Cv2.NamedWindow("original");

            Mat image = new Mat("C://Github//OpenDL//sampleImage//classificationSample.jpg");

            ODL.IODClassificationSharp classification = ODL.ODClassificationFactorySharp.ClassificationLoader("C://Github//OpenDL//OpenDL//x64//Release//ObjectDetector.frz",
                                                                                                            "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml");
            // Classification 클래스 생성 

            classification.Run(image.Data);
            // Classification 수행 

            System.Console.WriteLine("Index 0 : Lotion");
            System.Console.WriteLine("Index 1 : Bottle");
            System.Console.WriteLine("Index 2 : Roll tissue");
            System.Console.WriteLine("Index 3 : Empty");
            // Classification 인덱스 정보. 
            // 학습 툴에서 만든 라벨 순서에 따름. 

            System.Console.WriteLine("label count :" + classification.GetLabelCount());
            System.Console.WriteLine("max score : " + classification.MaxScore());
            System.Console.WriteLine("max score index : " + classification.MaxScoreIndex());

            System.Console.WriteLine(" == score list ==");
            List<double> scores = classification.Scores();
            for (int index = 0; index < 4; index++)
            {
                System.Console.WriteLine("score : " + scores[index]);
            }

            Cv2.ImShow("original", image);
            Cv2.WaitKey();
        }
    }
}


```



#### 업데이트 예정 사항

 1. ~~Segmentation C++, C# 라이브러리 추가 예정.~~
 2. ~~Classification 학습 기능 추가.~~
 3. ~~Classification C++, C# 라이브러리 추가 예정.~~
 4. 메뉴얼 업데이트 및 예제 업데이트
 
---
#### 1.4 Release Note

* 업데이트 리스트  

```
1. Tensorflow 1.15 업데이트
   
2. HySegNetV2 모델 추가
   - 입력 이미지 사이즈 : 512x512x1
   - 아웃풋 라벨 갯수 : 1

```
#### 1.3 Release Note

* 업데이트 리스트  

```
1. Classification 학습 기능 추가.

2. MobileNetV2 모델 추가
   - 입력 이미지 사이즈 : 224x224x3
   - 아웃풋 라벨 갯수 : 40
   
3. VTNetColor 모델 추가
   - 입력 이미지 사이즈 : 100x100x3
   - 아웃풋 라벨 갯수 : 2

```
---

#### 1.2 Release Note

* 업데이트 리스트  

```
1. Segmentation CSharp 라이브러리 추가
```
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