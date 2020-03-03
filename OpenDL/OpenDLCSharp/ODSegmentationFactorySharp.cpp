
#pragma managed
#include "ODSegmentationFactorySharp.h"
#include "ODSegmentationSharp.h"


ODL::IODSegmentationSharp^ ODL::ODSegmentationFactorySharp::SegmentatorLoader(String^ _frzPath, String^ _configPath) {

	ODL::IODSegmentationSharp^ instance = gcnew ODL::ODSegmentationSharp(_frzPath, _configPath);

	return instance;
}