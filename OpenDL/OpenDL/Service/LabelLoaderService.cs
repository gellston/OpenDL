using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace OpenDL.Service
{
    public class LabelLoaderService
    {

        public LabelLoaderService()
        {

        }


        public ObservableCollection<SegmentLabelUnit> LoadSegmentedLabel(string [] _files)
        {
            ObservableCollection<SegmentLabelUnit> labelCollection = new ObservableCollection<SegmentLabelUnit>();


            foreach(var file in _files)
            {
                SegmentLabelUnit unit = new SegmentLabelUnit();
                labelCollection.Add(unit);

                unit.FilePath = file;

                string filePath = Path.GetDirectoryName(file);
                string pureFileName = Path.GetFileNameWithoutExtension(file);
                string xmlFileName = filePath + Path.DirectorySeparatorChar + pureFileName + ".xml";

                unit.FileName = pureFileName;
            }


            return labelCollection;
        }
    }
}
