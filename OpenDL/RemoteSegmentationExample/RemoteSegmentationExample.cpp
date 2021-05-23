// RemoteSegmentationExample.cpp : 이 파일에는 'main' 함수가 포함됩니다. 거기서 프로그램 실행이 시작되고 종료됩니다.
//

#include <iostream>
#include <SpiderIPC.h>
#include <ODSegmentationFactory.h>
#include <IODSegmentation.h>
#include <memory>

int main(int argc, char* argv[])
{


    odl::IODSegmentation* segmentation = SegmentatorLoader(argv[1], argv[2]);
    
    std::shared_ptr< odl::IODSegmentation> segmentation_ptr(segmentation);
    float output[4900];
    unsigned char input[4900];
    double output_double[4900];

    spider::function function("segmentation", [&](spider::function* instance) {
        memset(output, 0, sizeof(float) * 4900);
        memset(input, 0, sizeof(unsigned char) * 4900);
        memset(output_double, 0, sizeof(double) * 4900);

        instance->args()
            .get<unsigned char>("input", input, 4900);

        segmentation_ptr->Run(input, output);

        for (int index = 0; index < 4900; index++)
            output_double[index] = output[index];

        instance->returns()
            .push<double>("output", output_double, 4900);

    });
    function.args()
        .arg<unsigned char>("input", 70 * 70)
        .returns()
        .arg<double>("output", 70 * 70)
        .complete();


    while (true) {
        char key = getchar();
        if (key == 'q') {
            break;
        }
        else if(key == 'h'){
            std::cout << "============== how to use =============" << std::endl;
            std::cout << "RemoteSegmentation (FreezedModel file path) (FreezedModel setting file path)" << std::endl;
        }
    }
}
