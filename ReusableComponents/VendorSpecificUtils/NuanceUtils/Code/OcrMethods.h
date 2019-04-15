#pragma once

#include <KernelAPI.h>

#include <string>

using namespace std;

void loadPageFromImageHandle(const string& strImage, HIMGFILE hImage, int iPageIndex, HPAGE* phPage);