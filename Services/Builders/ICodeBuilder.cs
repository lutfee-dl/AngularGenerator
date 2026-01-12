namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Base interface for all code builders
    /// </summary>
    public interface ICodeBuilder<T> where T : ICodeBuilder<T>
    {
        /// <summary>
        /// Build and return the final code string
        /// </summary>
        string Build();
        
        /// <summary>
        /// Reset builder to initial state
        /// </summary>
        T Reset();
    }
}