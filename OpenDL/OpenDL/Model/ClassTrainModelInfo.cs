using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Model
{
    public class ClassTrainModelInfo
    {

        public ClassTrainModelInfo()
        {

        }

        public string MetaFile { get; set; }
        public string CheckFile { get; set; }
        public string PbTextFile { get; set; }
        public string InputNodeName { get; set; }
        public string OutputNodeName { get; set; }
        public string TargetNodeName { get; set; }
        public string PhaseNodeName { get; set; }
        public string AccuracyNodeName { get; set; }
        public string CostNodeName { get; set; }
        public string GlobalVariableInitializerName { get; set; }
        public string TrainOperationName { get; set; }
        public string LearningRateOperationName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsGray { get; set; }
        public int MaxLabelCount { get; set; }
    }
}
