# ReactiveUI - Interaction Sample
Sample to show to to deal with interactions in RxUI

The scenario that covers is very simple:

We work in a cool factory that produces toys using a Magic-Toy-Machine® to be delivered to children all over the world. 

Each toy is produced every 5 seconds and is placed on a conveyor belt, but **our toy producing machine isn't perfect**: From time to time it produces a defective toy. Fortunately, we have a Magic Hammer (like  Fix It Felix). With a slight touch of it, a toy will be fixed immediately. We occupy a place in front of the belt, watching each passing toy. *Our goal is TO FIX EVERY BROKEN TOY! So everytime as broken toy passes in front of us, we fix it. While we fix do it, the conveyor band stops, so we can aim better with the hammer! 

All the toys, broken or not, go through the belt **in order**. This order has to be kept because otherwise, the toy will be received by the wrong child and there will be a big mess. The guy loading the truck uses this order to know which child is the toy delivered to :)
