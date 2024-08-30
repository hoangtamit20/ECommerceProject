namespace Core.Domain
{
    public class SimulationNoAuthException : Exception
    {
        public SimulationNoAuthException()
            : base("Simulation mode is enabled, and authentication is not allowed.")
        {
        }

        public SimulationNoAuthException(string message)
            : base(message)
        {
        }

        public SimulationNoAuthException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}