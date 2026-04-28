// PiQApi.Abstractions/Enums/AuthenticationType.cs
namespace PiQApi.Abstractions.Enums
{
    /// <summary>
    /// Defines authentication types for service operations
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// No authentication
        /// </summary>
        None = 0,

        /// <summary>
        /// OAuth 2.0 authentication
        /// </summary>
        OAuth = 1,

        /// <summary>
        /// Basic authentication
        /// </summary>
        Basic = 2,

        /// <summary>
        /// Windows integrated authentication
        /// </summary>
        Windows = 3,

        /// <summary>
        /// Certificate-based authentication
        /// </summary>
        Certificate = 4
    }
}