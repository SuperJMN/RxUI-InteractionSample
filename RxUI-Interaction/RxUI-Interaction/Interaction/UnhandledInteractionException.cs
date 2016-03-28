namespace ReactiveUI
{
    using System;

    /// <summary>
    /// Indicates that an interaction has gone unhandled.
    /// </summary>
    /// <typeparam name="TInput">
    /// The type of the interaction's input.
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// The type of the interaction's output.
    /// </typeparam>
    public class UnhandledInteractionException<TInput, TOutput> : Exception
    {
        private readonly Interaction<TInput, TOutput> interaction;
        private readonly TInput input;

        public UnhandledInteractionException(Interaction<TInput, TOutput> interaction, TInput input)
        {
            this.interaction = interaction;
            this.input = input;
        }

        /// <summary>
        /// Gets the interaction that was not handled.
        /// </summary>
        public Interaction<TInput, TOutput> Interaction
        {
            get { return this.interaction; }
        }

        /// <summary>
        /// Gets the input for the interaction that was not handled.
        /// </summary>
        public TInput Input
        {
            get { return this.input; }
        }
    }
}