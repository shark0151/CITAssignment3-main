namespace ClientServer.Protocol
{
    /// <summary>
    /// Provides constants representing status codes for CJTP (Custom JSON Transfer Protocol) responses.
    /// </summary>
    public static class StatusCodes
    {
        /// <summary>
        /// Represents a successful response code (1).
        /// </summary>
        public const string Ok = "1 OK";

        /// <summary>
        /// Represents a response code indicating a resource was created (2).
        /// </summary>
        public const string Created = "2 Created";

        /// <summary>
        /// Represents a response code indicating a resource was updated (3).
        /// </summary>
        public const string Updated = "3 Updated";

        /// <summary>
        /// Represents a response code indicating a missing method (4).
        /// </summary>
        public const string BadRequest = "4 BadRequest";

        /// <summary>
        /// Represents a response code indicating a missing path (5).
        /// </summary>
        public const string NotFound = "5 NotFound";

        /// <summary>
        /// Represents a response code indicating a missing date (6).
        /// </summary>
        public const string Error = "6 Error";

        /// <summary>
        /// Represents a response code indicating an illegal method (7).
        /// </summary>
        public const string IllegalMethod = "7 IllegalMethod";
    }
}