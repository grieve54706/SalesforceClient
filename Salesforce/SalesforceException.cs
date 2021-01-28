using System;
using System.Collections.Generic;

namespace sf_demo.Salesforce
{
    public class SalesforceException : Exception
    {
        public new Exception InnerException { get; private set; }
        public new string Message { get; private set; }
        public SalesforceError SalesforceError { get; private set; }
        public List<ApiError> ApiErrors { get; set; }

        public SalesforceException(
            SalesforceError salesforceError,
            string message,
            Exception innerException,
            List<ApiError> apiErrors
            )
        {
            SalesforceError = salesforceError;
            Message = message;
            InnerException = innerException;
            ApiErrors = apiErrors;
        }

        public SalesforceException(
            SalesforceError SalesforceError,
            string message,
            Exception innerException
            ) : this(SalesforceError, message, innerException, null)
        { }

        public SalesforceException(
            SalesforceError SalesforceError,
            Exception innerException
            ) : this(SalesforceError, SalesforceError.ToString(), innerException, null)
        { }

        public SalesforceException(
            SalesforceError SalesforceError,
            List<ApiError> apiErrors
            ) : this(SalesforceError, SalesforceError.ToString(), null, apiErrors)
        { }

        public SalesforceException(
            SalesforceError SalesforceError,
            string message
            ) : this(SalesforceError, message, null, null)
        { }

        public SalesforceException(
            SalesforceError SalesforceError
            ) : this(SalesforceError, SalesforceError.ToString(), null, null)
        { }

        public override string ToString()
        {
            var apiErrors = string.Empty;

            if (ApiErrors != null && ApiErrors.Count > 0)
                foreach (var apiError in ApiErrors)
                    apiErrors = $"{apiErrors}; {apiError.ToString()}";

            return $@"Salesforce exception =>
                        Type: {SalesforceError},
                        Message: {Message},
                        ApiErrors: {apiErrors},
                        InnerException: {InnerException}";
        }
    }

    public enum SalesforceError
    {
        /// <summary>
        /// Thrown when Salesforce settings are missing (call AddSalesforce method).
        /// </summary>
        SettingsException,

        /// <summary>
        /// Thrown when salesforce response is BadRequest.
        /// </summary>
        AuthorizationBadRequest,

        /// <summary>
        /// Thrown when authorization throws not known error.
        /// </summary>
        AuthorizationError,

        /// <summary>
        /// Error when json deserialization fails.
        /// </summary>
        JsonDeserializationError,

        CommandIsEmpty,
        SalesforceObjectNotFound,
        ProcessingError,
        NoInsertResponse,
        InsertError,
        DeleteError,
        NoDeleteResponse,
        UpdateError,
        NoUpdateResponse,
        SalesforceObjectWithoutId,
        SalesforceObjectIdIsNull
    }
}