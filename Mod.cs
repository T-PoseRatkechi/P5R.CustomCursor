using P5R.CustomCursor.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Drawing;
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

	private readonly List<string> cursorFiles = new();

	public Mod(ModContext context)
	{
		this.modLoader = context.ModLoader;
		this.hooks = context.Hooks;
		this.log = context.Logger;
		this.owner = context.Owner;
		this.modConfig = context.ModConfig;

		Log.Initialize("CustomCursor", this.log, Color.White);

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

		this.modLoader.ModLoaded += this.OnModLoaded;
		this.modLoader.OnModLoaderInitialized += this.OnModLoaderInitialized;
	}

	private void OnModLoaderInitialized()
    {
		if (this.cursorFiles.Count > 0)
        {
            var cursorFile = this.cursorFiles[Random.Shared.Next(this.cursorFiles.Count)];
            this.LoadCursor(cursorFile);
        }
    }

	private void OnModLoaded(IModV1 mod, IModConfigV1 config)
	{
		if (!config.ModDependencies.Contains(this.modConfig.ModId)
			|| *this.cursorPtr != IntPtr.Zero)
		{
			return;
		}

		var modDir = this.modLoader.GetDirectoryForModId(config.ModId);

		var cursorFile = Path.Join(modDir, "cursor.dds");
		if (File.Exists(cursorFile))
		{
			this.cursorFiles.Add(cursorFile);
		}

		var cursorDir = Path.Join(modDir, "cursor");
		if (Directory.Exists(cursorDir))
		{
			foreach (var file in Directory.EnumerateFiles(cursorDir, "*.dds"))
			{
				this.cursorFiles.Add(file);
			}
		}
	}

	private void LoadCursor(string cursorFile)
	{
		var data = File.ReadAllBytes(cursorFile);
		*this.cursorPtr = Marshal.AllocHGlobal(data.Length);
		Marshal.Copy(data, 0, *this.cursorPtr, data.Length);
		Log.Information($"Using custom cursor.");
	}

	#region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public Mod() { }
#pragma warning restore CS8618
	#endregion
}