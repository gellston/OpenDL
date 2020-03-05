


#pragma unmanaged

#include <ODClassificationFactory.h>
#include <string>


#pragma managed
#include "IODClassificationSharp.h"
#include "ODClassificationSharp.h"

using namespace System;


ODL::ODClassificationSharp::ODClassificationSharp(System::String^ _frz, System::String^ _config) {
	char* _frzPath = static_cast<char*>(Runtime::InteropServices::Marshal::StringToHGlobalAnsi(_frz).ToPointer());
	std::string _frzString(_frzPath);


	char* _configPath = static_cast<char*>(Runtime::InteropServices::Marshal::StringToHGlobalAnsi(_config).ToPointer());
	std::string _configString(_configPath);

	try {
		this->__handle = (odl::IODClassification*) ClassificationLoader(_frzString, _configString);
	}
	catch (std::exception & e) {
		throw gcnew System::Exception(gcnew System::String(e.what()));
	}



	Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)_frzPath));
	Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)_configPath));
}


ODL::ODClassificationSharp::~ODClassificationSharp() {
	this->!ODClassificationSharp();
}
ODL::ODClassificationSharp::!ODClassificationSharp() {
	if (this->__handle != nullptr)
		delete this->__handle;
}

bool ODL::ODClassificationSharp::Run(IntPtr _input) {
	try {
		const unsigned char* _inputImage = static_cast<const unsigned char*>(_input.ToPointer());

		return this->__handle->Run(_inputImage);
	}
	catch (std::exception & e) {
		throw gcnew System::Exception(gcnew System::String(e.what()));
	}
	return false;
}


int ODL::ODClassificationSharp::GetLabelCount() {
	return this->__handle->GetLabelCount();
}

int ODL::ODClassificationSharp::MaxScoreIndex() {
	return this->__handle->MaxScoreIndex();
}
float ODL::ODClassificationSharp::MaxScore() {
	return this->__handle->MaxScore();
}
List<double>^ ODL::ODClassificationSharp::Scores() {
	List<double>^ returnList = gcnew List<double>();
	std::vector<double> result = this->__handle->Scores();
	for (int index = 0; index < result.size(); index++)
		returnList->Add(result[index]);

	return returnList;
}