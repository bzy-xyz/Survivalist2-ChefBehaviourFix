Chef Behaviour Fix
===================

A mod for Survivalist: Invisible Strain.

By default, when a survivor with Cook role finishes cooking a recipe
at a campfire, they become stuck staring at the campfire instead of
clearing out the finished food. (They also forget what they were cooking
in the first place.)

We change this behaviour here to make this role work much more
effectively, by preventing these survivors from forgetting their recipe
in this case, and encouraging them to put away the newly cooked food.

Note that chefs will repeat the same recipe until your community runs
out of ingredients or hits the crafting limit for the product, or the
chef gives up for some other reason. You may want to set up limits
beforehand (via scheduling and then canceling recurrent craft orders)
to make it less likely that you'll cook _all_ of your corn / pumpkins
without noticing, especially in winter when you can't plant anything.

There are still rare cases where chefs will stare into space without a
particular reason. As a potential workaround, manually queue a recurring
recipe and then have the chef resume their role.

Requirements
------------

* Unity Mod Manager installed into Survivalist: Invisible Strain.

Build
-----

1. Checkout to `dev/ChefBehaviourFix`.
2. From the `dev/ChefBehaviourFix/src` directory, run `dotnet build`.
3. Copy `dev/ChefBehaviourFix/src/bin/Debug/net40/ChefBehaviourFix.dll` 
   and `dev/ChefBehaviourFix/Info.json` to `Mods/ChefBehaviourFix`.

License
-------

This mod contains code from Survivalist: Invisible Strain. Bob the P.R. Bot
has confirmed that use of small amounts of such code in non-commercial mods
for Survivalist: Invisible Strain is permissible.

Feel free to use other code from this mod in non-commercial mods for 
Survivalist: Invisible Strain if doing so meets your needs.
