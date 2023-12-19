namespace DeepgramSharp.AsyncEvents
{
    /// <summary>
    /// Represents a non-generic base for async events.
    /// </summary>
    public abstract class AsyncEvent
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncEvent"/> class.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        protected internal AsyncEvent(string name) => Name = name;
    }
}
