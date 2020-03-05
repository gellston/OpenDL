#pragma once



#ifdef CALLEE_EXPORTS
#define CALLEE_API extern "C" __declspec(dllexport)
#else
#define CALLEE_API extern "C" __declspec(dllimport)
#endif


#include <string>

#include "IODClassification.h"


CALLEE_API odl::IODClassification* ClassificationLoader(std::string _frzPath, std::string _configFile);


