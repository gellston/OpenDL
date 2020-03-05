/* Segmentation 예제
// Segmentation 예제
// Segmentation 예제
// Segmentation 예제
// Segmentation 예제

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

    segmentation->Run(image.data, (float *)result.data);

    std::cout << "label count : " << segmentation->GetLabelCount() << std::endl;

    cv::threshold(result, threshold, 0.8, 255, cv::THRESH_BINARY);

    cv::imshow("original", image);
    cv::imshow("result", threshold);
    cv::waitKey();

    delete segmentation;
}
*/


// Classification 예제
// Classification 예제
// Classification 예제
// Classification 예제

#include <iostream>
#include <opencv2/opencv.hpp>

#include "ODClassificationFactory.h"


int main()
{
    cv::namedWindow("original");

    cv::Mat image = cv::imread("C://Github//OpenDL//sampleImage//classificationSample.jpg");

    odl::IODClassification* classification = ClassificationLoader("C://Github//OpenDL//OpenDL//x64//Release//ObjectDetector.frz",    // 학습된 모델파일
                                                                  "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml");//학습된 모델 설정파일. 

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