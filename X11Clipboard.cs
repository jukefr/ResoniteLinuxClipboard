using Elements.Assets;
using Renderite.Host;
using System.Diagnostics;

using ResoniteModLoader;
using HarmonyLib;

#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace X11Clipboard;

public class X11Clipboard : ResoniteMod
{
	public override string Name => "X11Clipboard";
	public override string Author => "yosh";
	public override string Version => typeof(X11Clipboard).Assembly.GetName().Version?.ToString() ?? "0.0.0";
	public override string Link => "https://git.unix.dog/yosh/ResoniteX11Clipboard/";

	private static Harmony harmony = new Harmony("org.yosh.X11Clipboard");

	//// CONFIG ////

/*
	internal static ModConfiguration? config;

	[AutoRegisterConfigKey]
	internal static readonly ModConfigurationKey<long> KExampleKey = new(
		"ExampleKey",
		"Example configuration key",
		computeDefault: () => 4,
		valueValidator: (v) => 1 <= v && v <= 9
	);
	internal static long ExampleKey => config!.GetValue(KExampleKey);
	*/

	//// INIT ////

	public override void OnEngineInit()
	{
#if DEBUG
		HotReloader.RegisterForHotReload(this);
#endif
		// config = GetConfiguration();
		InitMod();
	}

	public static void InitMod()
	{
		harmony.PatchAll();
	}

	//// RELOAD ////

#if DEBUG
	static void BeforeHotReload()
	{
		harmony.UnpatchAll(harmony.Id);
	}

	static void OnHotReload(ResoniteMod modInstance)
	{
		// config = modInstance.GetConfiguration();
		InitMod();
	}
#endif

	//// PATCHES ////

	public static class Patch_LinuxClipboardInterface
	{
		static ProcessStartInfo GetPSI()
		{
			var psi = new ProcessStartInfo("xclip");
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardInput = true;
			return psi;
		}

		static CommonClipboard.ImageFormat? MyGetImageMime()
		{
			var mimes = GetClipboardMimes();
			return CommonClipboard.ImageFormats
				.Where(f => mimes.Contains(f.OLE))
				.Select(f => new Nullable<CommonClipboard.ImageFormat>(f))
				.FirstOrDefault();
		}

		static string[] GetClipboardMimes()
		{
			var psi = GetPSI();
			psi.Arguments = "-sel clipboard -t TARGETS -o";
			using var p = Process.Start(psi)!;
			var ret = p.StandardOutput.ReadToEnd()
				.Split('\n', StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s == "UTF8_STRING" ? "text/plain;charset=utf-8" : s)
				.ToArray();
			return ret;
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.GetText))]
		public static class Patch_GetText
		{
			static bool Prefix(ref Task<string> __result)
			{
				var psi = GetPSI();
				psi.Arguments = "-sel clipboard -o";
				using var p = Process.Start(psi)!;
				__result = p.StandardOutput.ReadToEndAsync();
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), "GetImageMime")]
		public static class Patch_GetImageMime
		{
			static bool Prefix(ref CommonClipboard.ImageFormat? __result)
			{
				__result = MyGetImageMime();
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), "HasMime")]
		public static class Patch_HasMime
		{
			static bool Prefix(ref bool __result, string mime_type)
			{
				var mimes = GetClipboardMimes();
				__result = mimes
					.Where(f => mimes.Contains(mime_type))
					.Any();
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.GetImage))]
		public static class Patch_GetImage
		{
			static bool Prefix(ref Task<Bitmap2D> __result)
			{
				var psi = GetPSI();
				var mime = MyGetImageMime()!.Value;
				psi.Arguments = $"-sel clipboard -t {mime} -o";
				using var p = Process.Start(psi)!;
				var memstr = new MemoryStream();
				p.StandardOutput.BaseStream.CopyTo(memstr);

				__result = Task.Run(delegate {
					try {
						return Bitmap2D.Load(memstr, mime.Extension, true);
					} finally {
						memstr.Dispose();
					}
				});
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.SetText))]
		public static class Patch_SetText
		{
			static bool Prefix(ref Task<bool> __result, string text)
			{
				var psi = GetPSI();
				psi.Arguments = $"-sel clipboard";
				using var p = Process.Start(psi)!;
				p.StandardInput.Write(text);

				__result =  Task.FromResult(result: true);
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.SetBitmap))]
		public static class Patch_SetBitmap
		{
			static bool Prefix(ref Task<bool> __result, Bitmap2D bitmap)
			{
				var psi = GetPSI();
				psi.Arguments = $"-sel clipboard -t image/png -i";
				using var p = Process.Start(psi)!;

				using MemoryStream ms = new MemoryStream();
				bitmap.Save(ms, "png");
				p.StandardInput.BaseStream.Write(new ReadOnlySpan<byte>(ms.ToArray()));

				__result = Task.FromResult(result: true);
				return false;
			}
		}
	}
}
