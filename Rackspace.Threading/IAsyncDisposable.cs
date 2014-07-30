namespace Rackspace.Threading
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a mechanism for asynchronously releasing unmanaged resources.
    /// </summary>
    public interface IAsyncDisposable : IDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// This method should perform the same operation as <see cref="IDisposable.Dispose"/>, with the following
        /// key differences.
        /// <list type="bullet">
        /// <item>The <see cref="DisposeAsync"/> method should never be called from a finalizer.</item>
        /// <item>The <see cref="DisposeAsync"/> method will not be called automatically from a <c>using</c> block,
        /// even when that block is located within an <see langword="async"/> method. The
        /// <see cref="O:Rackspace.Threading.TaskBlocks.Using"/> methods provide support for the
        /// <see cref="IAsyncDisposable"/> interface in a manner resembling the behavior proposed in
        /// <see href="http://roslyn.codeplex.com/discussions/546377">IAsyncDisposable, using statements, and async/await</see>.</item>
        /// </list>
        /// <note type="implement">
        /// To prevent finalization of the object while an asynchronous dispose operation is ongoing,
        /// the <see cref="Task"/> returned by this method should retain a reference to the object
        /// until the operation is complete.
        /// </note>
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DisposeAsync();
    }
}
