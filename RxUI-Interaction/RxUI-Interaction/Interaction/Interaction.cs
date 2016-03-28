using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace ReactiveUI
{
    /// <summary>
    /// Represents an interaction between collaborating parties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Interactions allow collaborating components to asynchronously ask questions of each other. Typically,
    /// a view model wants to ask the user a question before proceeding with some operation, and it's the view
    /// that provides the interface via which users can answer the question.
    /// </para>
    /// <para>
    /// Interactions have both an input and output, both of which are strongly-typed via generic type parameters.
    /// The input is passed into the interaction so that handlers have the information they require. The output
    /// is provided by a handler.
    /// </para>
    /// <para>
    /// By default, handlers are invoked in reverse order of registration. That is, handlers registered later
    /// are given the opportunity to handle interactions before handlers that were registered earlier. This
    /// chaining mechanism enables handlers to be registered temporarily in a specific context, such that
    /// interactions can be handled in a different manner. Subclasses may modify this behavior by overriding
    /// the <see cref="Handle"/> method.
    /// </para>
    /// <para>
    /// Note that handlers are not required to handle an interaction. They can choose to ignore it, leaving it
    /// for some other handler to handle. If no handler handles the interaction, the <see cref="Handle"/> method
    /// will throw an <see cref="UnhandledInteractionException"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TInput">
    /// The interaction's input type.
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// The interaction's output type.
    /// </typeparam>
    public class Interaction<TInput, TOutput>
    {
        private readonly IList<Func<InteractionContext<TInput, TOutput>, IObservable<Unit>>> handlers;
        private readonly object sync;

        public Interaction()
        {
            this.handlers = new List<Func<InteractionContext<TInput, TOutput>, IObservable<Unit>>>();
            this.sync = new object();
        }

        /// <summary>
        /// Registers a synchronous interaction handler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This overload of <c>RegisterHandler</c> is only useful if the handler can handle the interaction
        /// immediately. That is, it does not need to wait for a user or some other collaborating component.
        /// </para>
        /// </remarks>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <returns>
        /// A disposable which, when disposed, will unregister the handler.
        /// </returns>
        public IDisposable RegisterHandler(Action<InteractionContext<TInput, TOutput>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return RegisterHandler(interaction => {
                handler(interaction);
                return Observable.Return(Unit.Default);
            });
        }

        /// <summary>
        /// Registers a task-based asynchronous interaction handler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This overload of <c>RegisterHandler</c> is useful if the handler needs to perform some asynchronous
        /// operation, such as displaying a dialog and waiting for the user's response.
        /// </para>
        /// </remarks>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <returns>
        /// A disposable which, when disposed, will unregister the handler.
        /// </returns>
        public IDisposable RegisterHandler(Func<InteractionContext<TInput, TOutput>, Task> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return RegisterHandler(interaction => handler(interaction).ToObservable());
        }

        /// <summary>
        /// Registers an observable-based asynchronous interaction handler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This overload of <c>RegisterHandler</c> is useful if the handler needs to perform some asynchronous
        /// operation, such as displaying a dialog and waiting for the user's response.
        /// </para>
        /// </remarks>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <returns>
        /// A disposable which, when disposed, will unregister the handler.
        /// </returns>
        public IDisposable RegisterHandler(Func<InteractionContext<TInput, TOutput>, IObservable<Unit>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            this.AddHandler(handler);
            return Disposable.Create(() => this.RemoveHandler(handler));
        }

        /// <summary>
        /// Handles an interaction and asynchronously returns the result.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method passes the interaction through to relevant handlers in reverse order of registration,
        /// ceasing once any handler handles the interaction. If the interaction remains unhandled after all
        /// relevant handlers have executed, an <see cref="UnhandledInteractionException"/> is thrown.
        /// </para>
        /// </remarks>
        /// <param name="input">
        /// The input for the interaction.
        /// </param>
        /// <returns>
        /// An observable that ticks when the interaction completes, or throws an
        /// <see cref="UnhandledInteractionException"/> if no handler handles the interaction.
        /// </returns>
        public virtual IObservable<TOutput> Handle(TInput input)
        {
            var context = new InteractionContext<TInput, TOutput>(input);

            return this
                .GetHandlers()
                .Reverse()
                .ToObservable()
                .Select(handler => Observable.Defer(() => handler(context)))
                .Concat()
                .TakeWhile(_ => !context.IsHandled)
                .IgnoreElements()
                .Select(_ => default(TOutput))
                .Concat(
                    Observable.Defer(
                        () => context.IsHandled
                            ? Observable.Return(context.GetOutput())
                            : Observable.Throw<TOutput>(new UnhandledInteractionException<TInput, TOutput>(this, input))));
        }

        /// <summary>
        /// Gets all registered handlers in order of their registration.
        /// </summary>
        /// <returns>
        /// All registered handlers.
        /// </returns>
        protected Func<InteractionContext<TInput, TOutput>, IObservable<Unit>>[] GetHandlers()
        {
            lock (this.sync)
            {
                return this.handlers.ToArray();
            }
        }

        private void AddHandler(Func<InteractionContext<TInput, TOutput>, IObservable<Unit>> handler)
        {
            lock (this.sync)
            {
                this.handlers.Add(handler);
            }
        }

        private void RemoveHandler(Func<InteractionContext<TInput, TOutput>, IObservable<Unit>> handler)
        {
            lock (this.sync)
            {
                this.handlers.Remove(handler);
            }
        }
    }
}
