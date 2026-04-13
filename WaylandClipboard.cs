using Elements.Assets;
using Renderite.Host;
using System.Diagnostics;

using ResoniteModLoader;
using HarmonyLib;

#if DEBUG && RML_HOTRELOAD
using ResoniteHotReloadLib;
#endif

namespace WaylandClipboard;

public class WaylandClipboard : ResoniteMod
{
	public override string Name => "WaylandClipboard";
	public override string Author => "yosh";
	public override string Version => typeof(WaylandClipboard).Assembly.GetName().Version?.ToString() ?? "0.0.0";
	public override string Link => "https://git.unix.dog/yosh/ResoniteWaylandClipboard/";

	private static Harmony harmony = new Harmony("org.yosh.WaylandClipboard");

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
#if DEBUG && RML_HOTRELOAD
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

#if DEBUG && RML_HOTRELOAD
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
		private enum ClipboardBackend
		{
			Wayland,
			X11
		}

		private static ClipboardBackend? backend;

		private static ClipboardBackend Backend
		{
			get
			{
				if (backend.HasValue)
					return backend.Value;

				if (CommandExists("wl-copy") && CommandExists("wl-paste"))
				{
					backend = ClipboardBackend.Wayland;
					Msg("Using Wayland clipboard backend (wl-copy/wl-paste).");
				}
				else
				{
					backend = ClipboardBackend.X11;
					Msg("Using X11 clipboard backend (xclip).");
				}

				return backend.Value;
			}
		}

		private static void Msg(string message) => Error($"[{nameof(WaylandClipboard)}] {message}");

		private static bool CommandExists(string command)
		{
			try
			{
				var psi = new ProcessStartInfo(command, "--version");
				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;
				psi.UseShellExecute = false;
				using var p = Process.Start(psi);
				if (p == null)
					return false;

				p.WaitForExit(1000);
				return true;
			}
			catch
			{
				return false;
			}
		}

		static ProcessStartInfo GetReadPSI(string mimeType = "")
		{
			ProcessStartInfo psi;
			if (Backend == ClipboardBackend.Wayland)
			{
				var args = string.IsNullOrEmpty(mimeType) ? "-n" : $"--type {mimeType} -n";
				psi = new ProcessStartInfo("wl-paste", args);
			}
			else
			{
				var args = string.IsNullOrEmpty(mimeType) ? "-sel clipboard -o" : $"-sel clipboard -t {mimeType} -o";
				psi = new ProcessStartInfo("xclip", args);
			}

			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardInput = true;
			psi.UseShellExecute = false;
			return psi;
		}

		static ProcessStartInfo GetWritePSI(string mimeType = "")
		{
			ProcessStartInfo psi;
			if (Backend == ClipboardBackend.Wayland)
			{
				var args = string.IsNullOrEmpty(mimeType) ? "" : $"--type {mimeType}";
				psi = new ProcessStartInfo("wl-copy", args);
			}
			else
			{
				var args = string.IsNullOrEmpty(mimeType) ? "-sel clipboard" : $"-sel clipboard -t {mimeType} -i";
				psi = new ProcessStartInfo("xclip", args);
			}

			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardInput = true;
			psi.UseShellExecute = false;
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
			ProcessStartInfo psi;
			if (Backend == ClipboardBackend.Wayland)
				psi = new ProcessStartInfo("wl-paste", "-l");
			else
				psi = new ProcessStartInfo("xclip", "-sel clipboard -t TARGETS -o");

			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardInput = true;
			psi.UseShellExecute = false;

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
				var psi = GetReadPSI();
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
				__result = mimes.Contains(mime_type);
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.GetImage))]
		public static class Patch_GetImage
		{
			static bool Prefix(ref Task<Bitmap2D> __result)
			{
				var imageMime = MyGetImageMime();
				if (!imageMime.HasValue)
				{
					__result = Task.FromException<Bitmap2D>(new InvalidOperationException("No image format available on clipboard"));
					return false;
				}

				var mime = imageMime.Value;
				var psi = GetReadPSI(mime.OLE);
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
				var psi = GetWritePSI();
				using var p = Process.Start(psi)!;
				p.StandardInput.Write(text);
				p.StandardInput.Close();

				__result =  Task.FromResult(result: true);
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.SetBitmap))]
		public static class Patch_SetBitmap
		{
			static bool Prefix(ref Task<bool> __result, Bitmap2D bitmap)
			{
				var psi = GetWritePSI("image/png");
				using var p = Process.Start(psi)!;

				using MemoryStream ms = new MemoryStream();
				bitmap.Save(ms, "png");
				var bytes = ms.ToArray();
				p.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
				p.StandardInput.Close();

				__result = Task.FromResult(result: true);
				return false;
			}
		}
	}
}
