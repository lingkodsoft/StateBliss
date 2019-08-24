namespace StateBliss
{
    public class StateChangeInfo
    {
        internal object Data { get; set; }
        internal int FromState { get; set; }
        internal int ToState { get; set; }
        
        /// <summary>
        /// When Continue is set to false on a handler, setting ThrowExceptionWhenDiscontinued to true will throw StateEnterGuardDiscontinuedException.
        /// Only applies to Changing, OnEntering and OnExiting transitions
        /// </summary>
        public bool ThrowExceptionWhenDiscontinued { get; set; }

        /// <summary>
        /// Only applies to Changing, OnEntering and OnExiting transitions
        /// </summary>
        public bool Continue { get; set; } = true;
        
    }
}