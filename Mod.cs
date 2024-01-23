using P5R.CustomCursor.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Runtime.InteropServices;

namespace P5R.CustomCursor;

public unsafe class Mod : ModBase
{
    private readonly IModLoader modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger log;
    private readonly IMod owner;
    private readonly IModConfig modConfig;

    private readonly nint* cursorPtr;
    private IAsmHook? cursorHook;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks;
        this.log = context.Logger;
        this.owner = context.Owner;
        this.modConfig = context.ModConfig;

        Log.Initialize("CustomCursor", this.log);

        this.cursorPtr = (nint*)Marshal.AllocHGlobal(sizeof(nint));
        *this.cursorPtr = IntPtr.Zero;

        this.modLoader.GetController<IStartupScanner>().TryGetTarget(out var scanner);
        scanner!.Scan("Custom Cursor Hook", "E8 ?? ?? ?? ?? BF 01 00 00 00 48 89 43", result =>
        {
            var patch = new string[]
            {
                "use64",
                $"mov rax, {(nint)this.cursorPtr}",
                "mov rax, [rax]",
                "test rax, rax",
                "jz original",
                "mov rcx, rax",
                "original:"
            };

            this.cursorHook = hooks!.CreateAsmHook(patch, result).Activate();
        });

        this.modLoader.ModLoaded += this.OnModLoading;
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 config)
    {
        if (!config.ModDependencies.Contains(this.modConfig.ModId)
            || *this.cursorPtr != IntPtr.Zero)
        {
            return;
        }

        var cursorFile = Path.Join(this.modLoader.GetDirectoryForModId(config.ModId), "cursor.dds");
        if (File.Exists(cursorFile))
        {
            var data = File.ReadAllBytes(cursorFile);
            *this.cursorPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, *this.cursorPtr, data.Length);
            Log.Information($"Using custom cursor from: {config.ModName}");
        }
    }

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}