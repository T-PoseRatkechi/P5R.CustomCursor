using P5R.CustomCursor.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace P5R.CustomCursor
{
    public class Mod : ModBase
    {
        private readonly IModLoader modLoader;
        private readonly IReloadedHooks? hooks;
        private readonly ILogger log;
        private readonly IMod owner;

        private readonly IModConfig modConfig;

        public Mod(ModContext context)
        {
            this.modLoader = context.ModLoader;
            this.hooks = context.Hooks;
            this.log = context.Logger;
            this.owner = context.Owner;
            this.modConfig = context.ModConfig;

            this.modLoader.GetController<IStartupScanner>().TryGetTarget(out var scanner);
        }


        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}