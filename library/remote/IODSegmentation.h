#pragma once

#include <string>

namespace odl {
	class IODSegmentation {
	public:

		IODSegmentation(std::string _frzPath, std::string _configFile) {};
		IODSegmentation() {};

		virtual ~IODSegmentation() = default;
		
		virtual bool Run(const unsigned char* _input, const float* _segmentMap) = 0;
		virtual int GetLabelCount() = 0;
		
	};
};