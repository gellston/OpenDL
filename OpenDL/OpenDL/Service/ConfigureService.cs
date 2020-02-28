using Newtonsoft.Json;
using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace OpenDL.Service
{
    public class ConfigureService
    {

        public ConfigureService()
        {
            string checkPath = SegmentationPureModelContainerPath;
            checkPath = SegmentationTrainedModelContainerPath;
            checkPath = SegmentationTrainedModelUnzipPath;
        }

        public string LabelInfoFileName
        {
            get
            {
                return "__LabelInfo.json";
            }
        }

        public string ModelInfoFileName
        {
            get
            {
                return "__ModelInfo.json";
            }
        }

        public string SecurityPassword
        {
            get
            {
                return "Qhdkso!88";
            }
        }

        public string BaseDirectory
        {
            get
            {
                return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
        }

        public string SegmentationPureModelContainerPath
        {
            get
            {
                string segmentationModelContainerPath = this.BaseDirectory + Path.DirectorySeparatorChar + "SegmentationPureModelContainer";
                if (!Directory.Exists(segmentationModelContainerPath))
                    Directory.CreateDirectory(segmentationModelContainerPath);
;
                return segmentationModelContainerPath;
            }
        }

        public string SegmentationTrainedModelContainerPath
        {
            get
            {
                string segmentationModelContainerPath = this.BaseDirectory + Path.DirectorySeparatorChar + "SegmentationTrainedModelContainer";
                if (!Directory.Exists(segmentationModelContainerPath))
                    Directory.CreateDirectory(segmentationModelContainerPath);
                ;
                return segmentationModelContainerPath;
            }
        }

        public string SegmentationTrainedModelUnzipPath
        {
            get
            {
                string segmentationTrainedModelUnzipPath = this.BaseDirectory + Path.DirectorySeparatorChar + "SegmentationTrainedModelUnzipPath";
                if (!Directory.Exists(segmentationTrainedModelUnzipPath))
                    Directory.CreateDirectory(segmentationTrainedModelUnzipPath);
                
                return segmentationTrainedModelUnzipPath;
            }
        }
    }
}
