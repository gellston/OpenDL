#pragma once

#pragma unmanaged
#include <ODClassificationFactory.h>

#pragma managed
#include "IODClassificationSharp.h"


using namespace System;

namespace ODL {
	public ref class ODClassificationSharp : public ODL::IODClassificationSharp
	{

	internal:
		odl::IODClassification* __handle;

		ODClassificationSharp(System::String^ _frz, System::String^ _config);
		~ODClassificationSharp();
		!ODClassificationSharp();

	public:

		bool Run(System::IntPtr _input) override;
		int GetLabelCount()override;
		int MaxScoreIndex() override;
		float MaxScore() override;
		List<double>^ Scores() override;

	};
}
