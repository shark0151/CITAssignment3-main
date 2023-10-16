namespace ClientServer.Protocol
{
    /// <summary>
    /// Represents a response in the Custom JSON Transfer Protocol (CJTP).
    /// </summary>
    public class CJTPResponse
    {
        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the body of the response.
        /// </summary>
        public string Body { get; set; }
    }
}