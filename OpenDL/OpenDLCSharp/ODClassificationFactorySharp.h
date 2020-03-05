#pragma once


#pragma managed
#include "IODClassificationSharp.h"

using namespace System;

namespace ODL {
	public ref class ODClassificationFactorySharp
	{


	public:
		static IODClassificationSharp^ ClassificationLoader(String^ _frzPath, String^ _configPath);
	};
}
