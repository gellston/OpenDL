#pragma once


#ifdef CALLEE_EXPORTS
#define CALLEE_API extern "C" __declspec(dllexport)
#else
#define CALLEE_API extern "C" __declspec(dllimport)
#endif


#include <string>

#include "IODSegmentation.h"


CALLEE_API odl::IODSegmentation* SegmentatorLoader(std::string _frzPath, std::string _configFile);