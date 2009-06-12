#ifndef GENERIC_OBJECT_CLOSER
#define GENERIC_OBJECT_CLOSER

template <class T>
class GenericObjectCloser
{
public:
	GenericObjectCloser(T& t)
	:rObject(t)
	{
		bClosed = false;
	}

	~GenericObjectCloser()
	{
		if (!bClosed)
		{
			close();
		}
	}

	void close()
	{
		rObject.close();
		bClosed = true;
	}

private:
	T& rObject;
	bool bClosed;
};

#endif // GENERIC_OBJECT_CLOSER