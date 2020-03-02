

#include "ODSegmentationFactory.h"

#include "ODSegmentation.h"


CALLEE_API odl::IODSegmentation* SegmentatorLoader(std::string _frzPath, std::string _configFile) {

	odl::IODSegmentation* instance = new odl::ODSegmentation(_frzPath, _configFile);

	return instance;
}