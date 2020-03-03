


#pragma unmanaged

#include <ODSegmentationFactory.h>
#include <string>


#pragma managed
#include "IODSegmentationSharp.h"
#include "ODSegmentationSharp.h"

using namespace System;


ODL::ODSegmentationSharp::ODSegmentationSharp(System::String^ _frz, System::String^ _config){
	char* _frzPath = static_cast<char*>(Runtime::InteropServices::Marshal::StringToHGlobalAnsi(_frz).ToPointer());
	std::string _frzString(_frzPath);


	char* _configPath = static_cast<char*>(Runtime::InteropServices::Marshal::StringToHGlobalAnsi(_config).ToPointer());
	std::string _configString(_configPath);

	try {
		this->__handle = (odl::IODSegmentation*) SegmentatorLoader(_frzString, _configString);
	}
	catch (std::exception & e) {
		throw gcnew System::Exception(gcnew System::String(e.what()));
	}

	

	Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)_frzPath));
	Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)_configPath));
}


ODL::ODSegmentationSharp::~ODSegmentationSharp() {
	this->!ODSegmentationSharp();
}
ODL::ODSegmentationSharp::!ODSegmentationSharp() {
	if (this->__handle != nullptr)
		delete this->__handle;
}

bool ODL::ODSegmentationSharp::Run(IntPtr _input, IntPtr _segmentMap) {
	try {
		const unsigned char * _inputImage = static_cast<const unsigned char*>(_input.ToPointer());
		const float* _segmentPtr = static_cast<const float*>(_segmentMap.ToPointer());

		return this->__handle->Run(_inputImage, _segmentPtr);
	}
	catch (std::exception & e) {
		throw gcnew System::Exception(gcnew System::String(e.what()));
	}
	return false;
}


int ODL::ODSegmentationSharp::GetLabelCount() {
	return this->__handle->GetLabelCount();
}