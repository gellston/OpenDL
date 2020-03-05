

#include "ODClassificationFactory.h"

#include "ODClassification.h"


CALLEE_API odl::IODClassification* ClassificationLoader(std::string _frzPath, std::string _configFile) {

	odl::IODClassification* instance = new odl::ODClassification(_frzPath, _configFile);

	return instance;
}