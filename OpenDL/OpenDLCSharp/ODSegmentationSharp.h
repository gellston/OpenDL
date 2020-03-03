#pragma once



#pragma unmanaged
#include <ODSegmentationFactory.h>



#pragma managed
#include "IODSegmentationSharp.h"


using namespace System;

namespace ODL {
	public ref class ODSegmentationSharp : public ODL::IODSegmentationSharp
	{



	internal:
		odl::IODSegmentation* __handle;

		ODSegmentationSharp(System::String^ _frz, System::String^ _config);
		~ODSegmentationSharp();
		!ODSegmentationSharp();

	public:
		



		bool Run(System::IntPtr _input, System::IntPtr _segmentMap) override;
		int GetLabelCount()override;

	};
}
