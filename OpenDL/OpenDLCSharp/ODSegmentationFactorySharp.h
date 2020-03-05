#pragma once


#pragma managed
#include "IODSegmentationSharp.h"


using namespace System;

namespace ODL {
	public ref class ODSegmentationFactorySharp
	{


	public:
		static IODSegmentationSharp^ SegmentatorLoader(String^ _frzPath, String^ _configPath);
	};
}
