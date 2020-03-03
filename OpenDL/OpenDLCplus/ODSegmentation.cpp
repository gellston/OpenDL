
#include "ODSegmentation.h"


#include <fstream>
#include <iostream>
#include <stdexcept>
#include <numeric>
#include <vector>


#include "tinyxml/tinyxml.h"
#include "tinyxml/tinystr.h"


static void deallocator(void* data, size_t length, void* arg) {

}

static void free_buffer(void* data, size_t length) {
	free(data);
}

static TF_Buffer* load_pb_file(const std::string& filename) {
	std::streampos fsize = 0;
	std::ifstream file(filename, std::ios::binary);
	// get data size
	fsize = file.tellg();
	file.seekg(0, std::ios::end);
	fsize = file.tellg() - fsize;
	// reset stream
	file.seekg(0, std::ios::beg);
	char* data = new char[fsize];
	file.read(data, fsize);
	file.close();

	TF_Buffer* graph_def = TF_NewBuffer();
	graph_def->data = data;
	graph_def->length = fsize;
	graph_def->data_deallocator = free_buffer;
	return graph_def;
}

static std::string ReplaceAll(std::string& str, const std::string& from, const std::string& to) {
	size_t start_pos = 0;
	while ((start_pos = str.find(from, start_pos)) != std::string::npos)
	{
		str.replace(start_pos, from.length(), to);
		start_pos += to.length();
	}
	return str;
}

odl::ODSegmentation::~ODSegmentation() {
	if (this->image_buffer == nullptr)
		free(this->image_buffer);

	std::cout << "test" << std::endl;

	TF_CloseSession(this->_session, this->_status);
	if (TF_GetCode(this->_status) != TF_OK) {
		TF_DeleteStatus(this->_status);
	}

	std::cout << "test" << std::endl;

	TF_DeleteSession(this->_session, this->_status);
	if (TF_GetCode(this->_status) != TF_OK) {
		TF_DeleteStatus(this->_status);
	}

	std::cout << "test" << std::endl;
	TF_DeleteGraph(this->_graph);
	TF_DeleteStatus(this->_status);

	std::cout << "test" << std::endl;
}

odl::ODSegmentation::ODSegmentation(std::string _frzPath, std::string __xml) : imageSize(0),
																					 imageWidth(0),
																					 imageHeight(0),
																					 isGray(false),
																					 maxLabelCount(0),
																					 image_buffer(nullptr),
																				     imageChannel(1)
																					 {
	TiXmlDocument doc(__xml.c_str());
	if (doc.LoadFile())
	{
		TiXmlHandle hDoc(&doc);
		TiXmlElement* pRoot, * pParm;
		pRoot = doc.FirstChildElement("FreezedSegmentModelInfo");
		if (pRoot)
		{
			memset(&this->inputNodeName[0], 0, sizeof(char) * 50);
			memset(&this->outputNodeName[0], 0, sizeof(char) * 50);
			memset(&this->phaseNodeName[0], 0, sizeof(char) * 50);

			std::string inputNodeStr = std::string(pRoot->FirstChildElement("FreezeInputNodeName")->GetText());
			std::string outputNodeStr = std::string(pRoot->FirstChildElement("FreezeOutNodeName")->GetText());
			std::string phaseNodeNameStr = std::string(pRoot->FirstChildElement("FreezePhaseNodeName")->GetText());

			ReplaceAll(inputNodeStr, std::string(":0"), std::string(""));
			ReplaceAll(outputNodeStr, std::string(":0"), std::string(""));
			ReplaceAll(phaseNodeNameStr, std::string(":0"), std::string(""));

			memcpy(&this->inputNodeName[0], inputNodeStr.c_str(), 50);
			memcpy(&this->outputNodeName[0], outputNodeStr.c_str(), 50);
			memcpy(&this->phaseNodeName[0], phaseNodeNameStr.c_str(), 50);

			const char* imageWidth = pRoot->FirstChildElement("Width")->GetText();
			const char* imageHeight = pRoot->FirstChildElement("Height")->GetText();
			const char* isGrayFlag = pRoot->FirstChildElement("IsGray")->GetText();
			const char* maxLabelCount = pRoot->FirstChildElement("LabelCount")->GetText();

			this->imageWidth = std::stoi(imageWidth);
			this->imageHeight = std::stoi(imageHeight);
			this->isGray = std::strcmp(isGrayFlag, "false") == 0 ? false : true;
			this->maxLabelCount = std::stoi(maxLabelCount);

			this->imageChannel = isGray == true ? 1 : 3;
			this->imageSize = this->imageChannel * this->imageWidth * this->imageHeight * sizeof(float);
			this->imagePixelSize = this->imageChannel * this->imageWidth * this->imageHeight;
			this->segmentPixelSize = this->imageWidth * this->imageHeight * sizeof(float) * this->maxLabelCount;
			this->image_buffer = (float*)malloc(imageSize);
			//this->segmentedMap = (float*)malloc(this->segmentPixelSize);

			TF_Buffer* graph_def = load_pb_file(_frzPath.c_str());
			_status = TF_NewStatus();
			_graph = TF_NewGraph();

			TF_ImportGraphDefOptions* opts = TF_NewImportGraphDefOptions();
			TF_GraphImportGraphDef(_graph, graph_def, opts, _status);
			TF_DeleteImportGraphDefOptions(opts);
			if (TF_GetCode(_status) != TF_OK) {
				std::cerr << "ERROR: Unable to import graph " << TF_Message(_status) << "\n";
				throw std::exception("Unable to import graph");
			}
			TF_DeleteBuffer(graph_def);

			// creating new session variables
			_sess_opts = TF_NewSessionOptions();
			_session = TF_NewSession(_graph, _sess_opts, _status);
			if (TF_GetCode(_status) != TF_OK) {
				throw std::exception("Failed to load session");
			}
		}
		else {
			throw std::exception("invalid xml file.");
		}
	}
	else {
		throw std::exception("invalid xml file.");
	}

}

bool odl::ODSegmentation::Run(const unsigned char * _input, const float * _segmentMap) {


	if (_input == nullptr)
		throw std::exception("Null ptr exception.");

	std::vector<TF_Output> input_vec;
	std::vector<TF_Tensor*> input_tensors;
	std::vector<TF_Output> output_vec;
	std::vector<TF_Tensor*> output_tensors(1);

	input_vec.push_back({ TF_GraphOperationByName(this->_graph, this->inputNodeName), 0 });
	input_vec.push_back({ TF_GraphOperationByName(this->_graph, this->phaseNodeName), 0 });
	output_vec.push_back({ TF_GraphOperationByName(this->_graph, this->outputNodeName), 0 });

	memset(this->image_buffer, 0.0F, this->imageSize);
	//memset(this->segmentedMap, 0.0F, this->segmentPixelSize);


	for (int index = 0; index < this->imagePixelSize; index++) {
		this->image_buffer[index] = (float)_input[index];
	}

	int64_t in_dims[] = { 1, this->imageHeight, this->imageWidth, this->imageChannel };
	bool _phase = false;
	input_tensors.push_back(TF_NewTensor(TF_FLOAT, in_dims, 4, this->image_buffer, this->imageSize, &deallocator, 0));
	input_tensors.push_back(TF_NewTensor(TF_BOOL, nullptr, 0, &_phase, sizeof(bool), &deallocator, nullptr));

	try {
		TF_SessionRun(_session, nullptr,
			input_vec.data(), input_tensors.data(), input_vec.size(),
			output_vec.data(), output_tensors.data(), output_vec.size(),
			nullptr, 0, nullptr, _status);
	}
	catch (std::exception e) {
		std::cout << e.what() << std::endl;
		throw std::exception("run model failed.");
	}

	const auto data = static_cast<float*>(TF_TensorData(output_tensors[0]));


	memcpy((void*)_segmentMap, data, this->segmentPixelSize);



	if (TF_GetCode(this->_status) != TF_OK) {
		throw std::exception("invalid status");
	}

	for (TF_Tensor* tensor : input_tensors) {
		TF_DeleteTensor(tensor);
	}

	for (TF_Tensor* tensor : output_tensors) {
		TF_DeleteTensor(tensor);
	}


	return true;
}

int odl::ODSegmentation::GetLabelCount() {

	return this->maxLabelCount;
}

