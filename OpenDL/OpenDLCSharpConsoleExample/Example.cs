

 //* Segmentation 예제
 //* Segmentation 예제
 //* Segmentation 예제
 //* 
 //* 
 //* 
using System;
using System.Collections.Generic;
using System.IO;
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


            ODL.IODSegmentationSharp segmentation = ODL.ODSegmentationFactorySharp.SegmentatorLoader("C://Users//gellston//Desktop//prezed_model//prezed_model.frz",
                                                                                         "C://Users//gellston//Desktop//prezed_model//__FreezeModelInfo.xml");


            Cv2.NamedWindow("original");
            Cv2.NamedWindow("result");
        

            Mat image8bit = new Mat("C://Users//gellston//Desktop//source.jpg", ImreadModes.Color);



            Mat result = new Mat(new Size(1000, 200), MatType.CV_32FC1);
            Mat convertedDepth = new Mat();

            

            segmentation.Run(image8bit.Data, result.Data);

            //////System.Console.WriteLine("label count :" + segmentation.GetLabelCount());

            Mat threshold = result * 255;
            threshold.ConvertTo(convertedDepth, MatType.CV_8UC1);


            Cv2.ImShow("original", image8bit);
            Cv2.ImShow("result", convertedDepth);
            Cv2.WaitKey();

        }
    }
}



/*
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
}*/