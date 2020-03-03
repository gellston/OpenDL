#include <iostream>
#include <opencv2/opencv.hpp>

#include "ODSegmentationFactory.h"


int main()
{
    cv::namedWindow("original");
    cv::namedWindow("result");

    cv::Mat image = cv::imread("C://Github//OpenDL//OpenDL//x64//Release//00036.jpg");
    cv::Mat result = cv::Mat(cv::Size(256, 256), CV_32FC1);
    cv::Mat threshold = cv::Mat(cv::Size(256, 256), CV_32FC1);

    odl::IODSegmentation* segmentation = SegmentatorLoader("C://Github//OpenDL//OpenDL//x64//Release//FaceDetector.frz", 
                                                           "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml");

    std::cout << "label count : " << segmentation->GetLabelCount() << std::endl;
    
    segmentation->Run(image.data, (float *)result.data);

    cv::threshold(result, threshold, 0.8, 255, cv::THRESH_BINARY);

    cv::imshow("original", image);
    cv::imshow("result", threshold);
    cv::waitKey();

    delete segmentation;
}

