#pragma once

#include "NuanceUtils.h"

#include <KernelAPI.h>

#include <string>

using namespace std;

NUANCEUTILS_API void loadPageFromImageHandle(const string& strImage, HIMGFILE hImage, int iPageIndex, HPAGE* phPage);