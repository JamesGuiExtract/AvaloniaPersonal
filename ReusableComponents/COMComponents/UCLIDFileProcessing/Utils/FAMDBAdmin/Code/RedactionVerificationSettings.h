#pragma once
namespace Extract {
	namespace FAMDBAdmin {

		using namespace System;
		using namespace System::Collections::Generic;
		using namespace System::Runtime::Serialization;

		// Represents settings for a web-based redaction verification application
		[DataContract]
		public ref class RedactionVerificationSettings
		{
		public:

			RedactionVerificationSettings();
			~RedactionVerificationSettings();

			void InitializeDefaults();

			[OnDeserializing]
			void OnDeserializing(StreamingContext context);

			// A enumeration of strings representing the types that should be available to apply to
			// redactions.
			[DataMember]
			property System::Collections::Generic::IEnumerable<String ^> ^RedactionTypes;

			// This is a filename that contains the valid document types, not the document types themselves.
			[DataMember]
			property String^ DocumentTypes;
		};
	}
};

