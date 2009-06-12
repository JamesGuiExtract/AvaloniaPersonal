#pragma once

#include <stack>

template <class T>
class stack2 : public std::stack<T>
{
public:
	void clear()
	{
		c.clear();
	} 
};
