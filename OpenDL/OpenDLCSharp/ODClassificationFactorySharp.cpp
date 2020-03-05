
#pragma managed
#include "ODClassificationFactorySharp.h"
#include "ODClassificationSharp.h"


ODL::IODClassificationSharp^ ODL::ODClassificationFactorySharp::ClassificationLoader(String^ _frzPath, String^ _configPath) {

	ODL::IODClassificationSharp^ instance = gcnew ODL::ODClassificationSharp(_frzPath, _configPath);

	return instance;
}