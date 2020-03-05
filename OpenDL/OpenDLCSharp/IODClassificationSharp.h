#pragma once


#pragma managed

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;


namespace ODL {

	public interface  class IODClassificationSharp {

		//IODSegmentation(System::String^ _frz, System::String^ _config);

		bool Run(System::IntPtr _input);
		int GetLabelCount();
		int MaxScoreIndex();
		float MaxScore();
		List<double>^ Scores();
	};


}