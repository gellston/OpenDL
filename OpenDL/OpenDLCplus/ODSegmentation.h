#pragma once


#include <tensorflow/c/c_api.h>

#include "IODSegmentation.h"

namespace odl {
	class ODSegmentation : public IODSegmentation{
	private:

		char inputNodeName[50];
		char outputNodeName[50];
		char phaseNodeName[50];
		int imageSize;
		int imageChannel;
		int imagePixelSize = 0;
		int segmentPixelSize = 0;
		int imageWidth;
		int imageHeight;
		bool isGray;
		int maxLabelCount;

		//float* segmentedMap;
		float* image_buffer;



		TF_Graph* _graph;
		TF_Status* _status; // used to hold all statuses about TF operations
		TF_SessionOptions* _sess_opts;
		TF_Session* _session;

		TF_Output input_op;
		TF_Output phase_op;
		TF_Output output_op;

	public:
		ODSegmentation() = delete;
		ODSegmentation(std::string _frzPath, std::string _configFile);

		~ODSegmentation();

		bool Run(const unsigned char* _input, const float* _segmentMap) override;
		int GetLabelCount() override;

	};

};










