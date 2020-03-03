#pragma once




#pragma managed

using namespace System;



namespace ODL {

	public interface  class IODSegmentationSharp {

		//IODSegmentation(System::String^ _frz, System::String^ _config);

		bool Run(System::IntPtr _input, System::IntPtr _segmentMap);
		int GetLabelCount();

	};


}