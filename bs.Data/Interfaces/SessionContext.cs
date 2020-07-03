namespace bs.Data.Interfaces
{
    /// <summary>
    /// The session context for nhibernate session ... for examble web in case or session per web request
    /// </summary>
    public enum SessionContext
    {
        /// <summary>
        /// This is the right context for appòlications or unit test
        /// </summary>
        call = 10,

        /// <summary>
        /// This is good for web site or web application cause the context of the session will be the web request
        /// </summary>
        web = 20
    }
}