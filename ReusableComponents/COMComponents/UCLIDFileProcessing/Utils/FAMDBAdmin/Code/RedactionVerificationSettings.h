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

			// The name of the action in which redaction verification is to occur.
			[DataMember]
			property String ^ VerifyAction;

			// The name of the action to set to pending following verification.
			[DataMember]
			property String ^ PostVerifyAction;

			// A enumeration of strings representing the types that should be available to apply to
			// redactions.
			[DataMember]
			property System::Collections::Generic::IEnumerable<String ^> ^RedactionTypes;
		};
	}
};

