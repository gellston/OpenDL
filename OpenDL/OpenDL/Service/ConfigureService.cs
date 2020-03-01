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
            checkPath = FreezeUnzipPath;
            checkPath = TensorflowPackagePath;
            checkPath = PackageZipPath;
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

        public string FreezeModelInfoFileName
        {
            get
            {
                return "__FreezeModelInfo.xml";
            }
        }

        public string SecurityPassword
        {
            get
            {
                return "Qhdkso!88";
            }
        }


        public string TensorflowGpuDllName
        {
            get
            {
                return "tensorflow_gpu.dll";
            }
        }

        public string TensorflowCpuDllName
        {
            get
            {
                return "tensorflow_cpu.dll";
            }
        }

        public string TensorflowDllName
        {
            get
            {
                return "tensorflow.dll";
            }
        }

        public string BaseDirectory
        {
            get
            {
                return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
        }

        public string SpecialCharacter
        {
            get
            {
                string specialCharacter = @"[~!@\#$%^&*\()\=+|\\/:;?""<>']";
                return specialCharacter;
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

        public string FreezeUnzipPath
        {
            get
            {
                string freezeUnzipPath = this.BaseDirectory + Path.DirectorySeparatorChar + "FreezeUnzip";
                if (!Directory.Exists(freezeUnzipPath))
                    Directory.CreateDirectory(freezeUnzipPath);

                return freezeUnzipPath;
            }
        }

        public string PackageZipPath
        {
            get
            {
                string packageZipPath = this.BaseDirectory + Path.DirectorySeparatorChar + "PackageZip";
                if (!Directory.Exists(packageZipPath))
                    Directory.CreateDirectory(packageZipPath);

                return packageZipPath;
            }
        }

        public string TensorflowPackagePath
        {
            get
            {
                string tensorflowPackagePath = this.BaseDirectory + Path.DirectorySeparatorChar + "TensorflowPackage";
                if (!Directory.Exists(tensorflowPackagePath))
                    Directory.CreateDirectory(tensorflowPackagePath);

                return tensorflowPackagePath;
            }
        }
    }
}
