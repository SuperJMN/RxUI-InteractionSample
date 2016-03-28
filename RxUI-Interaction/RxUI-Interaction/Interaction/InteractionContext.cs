namespace ReactiveUI
{
    using System;
    using System.Threading;

    /// <summary>
    /// Contains contextual information for an interaction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instances of this class are passed into interaction handlers. The <see cref="Input"/> property exposes
    /// the input to the interaction, whilst the <see cref="SetOutput"/> method allows a handler to provide the
    /// output.
    /// </para>
    /// </remarks>
    /// <typeparam name="TInput">
    /// The type of the interaction's input.
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// The type of the interaction's output.
    /// </typeparam>
    public sealed class InteractionContext<TInput, TOutput>
    {
        private readonly TInput input;
        private TOutput output;
        private int outputSet;

        internal InteractionContext(TInput input)
        {
            this.input = input;
        }

        /// <summary>
        /// Gets the input for the interaction.
        /// </summary>
        public TInput Input
        {
            get { return this.input; }
        }

        /// <summary>
        /// Gets a value indicating whether the interaction is handled. That is, whether the output has been set.
        /// </summary>
        public bool IsHandled
        {
            get { return this.outputSet == 1; }
        }

        /// <summary>
        /// Sets the output for the interaction.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the output has already been set.
        /// </exception>
        public void SetOutput(TOutput output)
        {
            if (Interlocked.CompareExchange(ref this.outputSet, 1, 0) != 0)
            {
                throw new InvalidOperationException("Output has already been set.");
            }

            this.output = output;
        }

        /// <summary>
        /// Gets the output of the interaction.
        /// </summary>
        /// <returns>
        /// The output.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the output has not been set.
        /// </exception>
        public TOutput GetOutput()
        {
            if (this.outputSet == 0)
            {
                throw new InvalidOperationException("Output has not been set.");
            }

            return this.output;
        }
    }
}