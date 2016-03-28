namespace RxUI_Interaction
{
    using System;
    using System.Reactive.Linq;
    using ReactiveUI;

    public class MainViewModel
    {
        private readonly Interaction<Toy, Toy> fixToyInteraction = new Interaction<Toy, Toy>();
        private static readonly Random Random = new Random();


        public MainViewModel()
        {
            fixToyInteraction.RegisterHandler((Action<InteractionContext<Toy, Toy>>)FixToy);

            var toyNames = new[]
            {
                "Train",
                "Plush Doll",
                "Dinosaur",
                "Plastic Chicken",
                "Magic Wand",
                "Wig",
                "Mr. Potato",
                "Car",
                "Doll",
                "Kitchen",
                "Coloured Pencils",
            };

            var toyGenerator = Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Zip(toyNames, (i, name) => new Toy()
                {
                    Name = name,
                    ShippingOrdinal = i + 1,
                    IsBroken = GetIsBroken(),
                });

            var fixedToys = toyGenerator
                .SelectMany(
                    t =>
                    {
                        if (t.IsBroken)
                        {

                            return fixToyInteraction.Handle(t);
                        }

                        return Observable.Return(t);
                    });

            fixedToys
                .ObserveOnDispatcher()
                .Subscribe(t => FixedToys.Add(t));
        }

        private static bool GetIsBroken()
        {
            return Random.Next(0, 2) > 0;
        }

        public ReactiveList<Toy> FixedToys { get; set; } = new ReactiveList<Toy>();

        private void FixToy(InteractionContext<Toy, Toy> context)
        {
            var inputToy = context.Input;
            inputToy.IsBroken = false;
            context.SetOutput(inputToy);
        }
    }
}