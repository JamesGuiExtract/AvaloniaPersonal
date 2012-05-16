#include "stdafx.h"
#include "TextEntity.h"

#include <UCLIDException.hpp>

using namespace std;

TextEntity::TextEntity(const string& strText)
:m_strText(strText), m_strSourceFileName("")
{
}

TextEntity::~TextEntity()
{
}

void TextEntity::setColor(const COLORREF& newColor)
{
	// this call is ignored in the default implementation
}

COLORREF TextEntity::getColor()
{
	return RGB(0, 0, 0);
}

bool TextEntity::isEqualTo(const TextEntity *pTextEntity)
{
	return m_strText == pTextEntity->m_strText;
}

void TextEntity::setText(const string& strNewText)
{
	m_strText = strNewText;
}

const string& TextEntity::getText()
{
	return m_strText;
}
