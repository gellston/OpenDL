#pragma once



#include <tensorflow/c/c_api.h>
#include "IODClassification.h"

namespace odl {
	class ODClassification : public IODClassification {
	private:

		char inputNodeName[50];
		char outputNodeName[50];
		char phaseNodeName[50];
		int imageSize;
		int imageChannel;
		int imagePixelSize = 0;
		int classificationPixelSize = 0;
		int imageWidth;
		int imageHeight;
		bool isGray;
		int maxLabelCount;

		float* labelMap;
		float* image_buffer;

		TF_Graph* _graph;
		TF_Status* _status; // used to hold all statuses about TF operations
		TF_SessionOptions* _sess_opts;
		TF_Session* _session;

		TF_Output input_op;
		TF_Output phase_op;
		TF_Output output_op;

	public:
		ODClassification() = delete;
		ODClassification(std::string _frzPath, std::string _configFile);

		~ODClassification();

		bool Run(const unsigned char* _input) override;
		int GetLabelCount() override;
		int MaxScoreIndex() override;
		float MaxScore() override;
		std::vector<double> Scores() override;

	};

};










