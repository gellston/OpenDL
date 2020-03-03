using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace OpenDLCSharpConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Cv2.NamedWindow("original");
            Cv2.NamedWindow("result");


            Mat image = new Mat("C://Github//OpenDL//OpenDL//x64//Release//00036.jpg");
            Mat result = new Mat(new Size(256, 256), MatType.CV_32FC1);
            Mat convertedDepth = new Mat();

            ODL.IODSegmentationSharp segmentation = ODL.ODSegmentationFactorySharp.SegmentatorLoader("C://Github//OpenDL//OpenDL//x64//Release//FaceDetector.frz",
                                                                                                    "C://Github//OpenDL//OpenDL//x64//Release//__FreezeModelInfo.xml");

            System.Console.WriteLine("label count :" + segmentation.GetLabelCount());
            segmentation.Run(image.Data, result.Data);

            Mat threshold = result.Threshold(0.8, 255, ThresholdTypes.Binary);
            threshold.ConvertTo(convertedDepth, MatType.CV_8UC1);

            Cv2.ImShow("original", image);
            Cv2.ImShow("result", convertedDepth);
            Cv2.WaitKey();
        }
    }
}
