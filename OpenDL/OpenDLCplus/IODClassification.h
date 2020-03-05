#pragma once

#include <string>
#include <vector>


namespace odl {
	class IODClassification {
	public:

		IODClassification(std::string _frzPath, std::string _configFile) {};
		IODClassification() {};

		virtual ~IODClassification() = default;

		virtual bool Run(const unsigned char* _input) = 0;
		virtual int GetLabelCount() = 0;
		virtual int MaxScoreIndex() = 0;
		virtual float MaxScore() = 0;
		virtual std::vector<double> Scores() = 0;


	};
};