using Elements.Assets;
using Renderite.Host;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using ResoniteModLoader;
using HarmonyLib;

#if DEBUG && RML_HOTRELOAD
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("LinuxClipboard.Tests")]

namespace LinuxClipboard;

public class LinuxClipboard : ResoniteMod
{
	public override string Name => "LinuxClipboard";
	public override string Author => "jukefr";
	public override string Version => typeof(LinuxClipboard).Assembly.GetName().Version?.ToString() ?? "0.0.0";
	public override string Link => "https://github.com/jukefr/ResoniteLinuxClipboard/";

	private static readonly Harmony harmony = new Harmony("com.github.jukefr.ResoniteLinuxClipboard");
	private static BackendDetector? backendDetector;

	//// CONFIG ////

	internal static ModConfiguration? config;

	[AutoRegisterConfigKey]
	internal static readonly ModConfigurationKey<int> KClipboardTimeoutMs = new(
		"ClipboardTimeoutMs",
		"Timeout in milliseconds for clipboard operations (0 = no timeout)",
		computeDefault: () => 5000,
		valueValidator: (v) => v >= 0
	);

	internal static int ClipboardTimeoutMs => config?.GetValue(KClipboardTimeoutMs) ?? 5000;

	//// INIT ////

	public override void OnEngineInit()
	{
#if DEBUG && RML_HOTRELOAD
		HotReloader.RegisterForHotReload(this);
#endif
		config = GetConfiguration();
		InitMod();
	}

	public static void InitMod()
	{
		backendDetector = new BackendDetector();
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
		instance = (LinuxClipboard)modInstance;
		// config = modInstance.GetConfiguration();
		InitMod();
	}
#endif

	private static void Info(string message) => Msg($"[{nameof(LinuxClipboard)}] {message}");
	private static void Warn(string message) => Msg($"[{nameof(LinuxClipboard)}] {message}");
	private static void ErrorMsg(string message) => Error($"[{nameof(LinuxClipboard)}] {message}");

	public static class Patch_LinuxClipboardInterface
	{
		private static BackendDetector.ClipboardBackend Backend => LinuxClipboard.backendDetector?.DetectBackend() ?? BackendDetector.ClipboardBackend.None;

		static ProcessStartInfo GetReadPSI(string mimeType = "")
		{
			ProcessStartInfo psi;
			if (Backend == BackendDetector.ClipboardBackend.Linux)
			{
				var args = string.IsNullOrEmpty(mimeType) ? "-n" : $"--type {mimeType} -n";
				psi = new ProcessStartInfo("wl-paste", args);
			}
			else if (Backend == BackendDetector.ClipboardBackend.X11)
			{
				var args = string.IsNullOrEmpty(mimeType) ? "-sel clipboard -o" : $"-sel clipboard -t {mimeType} -o";
				psi = new ProcessStartInfo("xclip", args);
			}
			else
			{
				throw new InvalidOperationException("No clipboard backend available.");
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
			if (Backend == BackendDetector.ClipboardBackend.Linux)
			{
				var args = string.IsNullOrEmpty(mimeType) ? "" : $"--type {mimeType}";
				psi = new ProcessStartInfo("wl-copy", args);
			}
			else if (Backend == BackendDetector.ClipboardBackend.X11)
			{
				var args = string.IsNullOrEmpty(mimeType) ? "-sel clipboard" : $"-sel clipboard -t {mimeType} -i";
				psi = new ProcessStartInfo("xclip", args);
			}
			else
			{
				throw new InvalidOperationException("No clipboard backend available.");
			}

			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardInput = true;
			psi.UseShellExecute = false;
			return psi;
		}

		static Renderite.Host.ImageFormat? MyGetImageMime()
		{
			var mimes = GetClipboardMimes();
			return CommonClipboard.ImageFormats
				.Where(f => mimes.Contains(f.OLE))
				.Select(f => new Nullable<Renderite.Host.ImageFormat>(f))
				.FirstOrDefault();
		}

		static string[] GetClipboardMimes()
		{
			try
			{
				ProcessStartInfo psi;
				if (Backend == BackendDetector.ClipboardBackend.Linux)
					psi = new ProcessStartInfo("wl-paste", "-l");
				else if (Backend == BackendDetector.ClipboardBackend.X11)
					psi = new ProcessStartInfo("xclip", "-sel clipboard -t TARGETS -o");
				else
					return Array.Empty<string>();

				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardInput = true;
				psi.UseShellExecute = false;

				using var p = Process.Start(psi);
				if (p == null)
					return Array.Empty<string>();

				if (ClipboardTimeoutMs > 0)
					p.WaitForExit(ClipboardTimeoutMs);
				else
					p.WaitForExit();

				var output = p.StandardOutput.ReadToEnd();
				var mimes = output
					.Split('\n', StringSplitOptions.RemoveEmptyEntries)
					.Select(s => s.Trim())
					.Where(s => !string.IsNullOrEmpty(s))
					.Select(s => Backend == BackendDetector.ClipboardBackend.X11 && s == "UTF8_STRING" ? "text/plain;charset=utf-8" : s)
					.ToArray();
				return mimes;
			}
			catch (Exception ex)
			{
				ErrorMsg($"Failed to get clipboard MIME types: {ex.Message}");
				return Array.Empty<string>();
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.GetText))]
		public static class Patch_GetText
		{
			static bool Prefix(ref Task<string> __result)
			{
				try
				{
					if (Backend == BackendDetector.ClipboardBackend.None)
					{
						__result = Task.FromException<string>(new InvalidOperationException("No clipboard backend available."));
						return false;
					}

					var psi = GetReadPSI();
					var p = Process.Start(psi);
					if (p == null)
					{
						__result = Task.FromException<string>(new InvalidOperationException("Failed to start clipboard process."));
						return false;
					}

					using (p)
					{
						var task = p.StandardOutput.ReadToEndAsync();
						if (ClipboardTimeoutMs > 0)
							p.WaitForExit(ClipboardTimeoutMs);
						__result = task;
					}
				}
				catch (Exception ex)
				{
					ErrorMsg($"Failed to get clipboard text: {ex.Message}");
					__result = Task.FromException<string>(ex);
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), "GetImageMime")]
		public static class Patch_GetImageMime
		{
			static bool Prefix(ref Renderite.Host.ImageFormat? __result)
			{
				try
				{
					__result = MyGetImageMime();
				}
				catch (Exception ex)
				{
					ErrorMsg($"Failed to get image MIME: {ex.Message}");
					__result = null;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), "HasMime")]
		public static class Patch_HasMime
		{
			static bool Prefix(ref bool __result, string mime_type)
			{
				try
				{
					if (Backend == BackendDetector.ClipboardBackend.None)
					{
						__result = false;
						return false;
					}

					var mimes = GetClipboardMimes();
					__result = mimes.Contains(mime_type);
				}
				catch (Exception ex)
				{
					ErrorMsg($"Failed to check MIME type {mime_type}: {ex.Message}");
					__result = false;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.GetImage))]
		public static class Patch_GetImage
		{
			static bool Prefix(ref Task<Bitmap2D> __result)
			{
				try
				{
					if (Backend == BackendDetector.ClipboardBackend.None)
					{
						__result = Task.FromException<Bitmap2D>(new InvalidOperationException("No clipboard backend available."));
						return false;
					}

					var imageMime = MyGetImageMime();
					if (!imageMime.HasValue)
					{
						__result = Task.FromException<Bitmap2D>(new InvalidOperationException("No image format available on clipboard"));
						return false;
					}

					var mime = imageMime.Value;
					var psi = GetReadPSI(mime.OLE);
					var p = Process.Start(psi);
					if (p == null)
					{
						__result = Task.FromException<Bitmap2D>(new InvalidOperationException("Failed to start clipboard process."));
						return false;
					}

					using (p)
					{
						var memstr = new MemoryStream();
						p.StandardOutput.BaseStream.CopyTo(memstr);

						if (ClipboardTimeoutMs > 0)
							p.WaitForExit(ClipboardTimeoutMs);

						__result = Task.Run(delegate {
							try {
								return Bitmap2D.Load(memstr, mime.Extension, true);
							} finally {
								memstr.Dispose();
							}
						});
					}
				}
				catch (Exception ex)
				{
					ErrorMsg($"Failed to get clipboard image: {ex.Message}");
					__result = Task.FromException<Bitmap2D>(ex);
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.SetText))]
		public static class Patch_SetText
		{
			static bool Prefix(ref Task<bool> __result, string text)
			{
				try
				{
					if (Backend == BackendDetector.ClipboardBackend.None)
					{
						__result = Task.FromResult(false);
						return false;
					}

					if (text == null)
						text = string.Empty;

					var psi = GetWritePSI();
					var p = Process.Start(psi);
					if (p == null)
					{
						ErrorMsg("Failed to start clipboard process for SetText.");
						__result = Task.FromResult(false);
						return false;
					}

					using (p)
					{
						p.StandardInput.Write(text);
						p.StandardInput.Close();

						if (ClipboardTimeoutMs > 0)
							p.WaitForExit(ClipboardTimeoutMs);
						else
							p.WaitForExit();
					}

					__result = Task.FromResult(true);
				}
				catch (Exception ex)
				{
					ErrorMsg($"Failed to set clipboard text: {ex.Message}");
					__result = Task.FromResult(false);
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(LinuxClipboardInterface), nameof(LinuxClipboardInterface.SetBitmap))]
		public static class Patch_SetBitmap
		{
			static bool Prefix(ref Task<bool> __result, Bitmap2D bitmap)
			{
				try
				{
					if (Backend == BackendDetector.ClipboardBackend.None)
					{
						__result = Task.FromResult(false);
						return false;
					}

					if (bitmap == null)
					{
						__result = Task.FromResult(false);
						return false;
					}

					var psi = GetWritePSI("image/png");
					var p = Process.Start(psi);
					if (p == null)
					{
						ErrorMsg("Failed to start clipboard process for SetBitmap.");
						__result = Task.FromResult(false);
						return false;
					}

					using (p)
					{
						using MemoryStream ms = new MemoryStream();
						bitmap.Save(ms, "png");
						var bytes = ms.ToArray();
						p.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
						p.StandardInput.Close();

						if (ClipboardTimeoutMs > 0)
							p.WaitForExit(ClipboardTimeoutMs);
						else
							p.WaitForExit();
					}

					__result = Task.FromResult(true);
				}
				catch (Exception ex)
				{
					ErrorMsg($"Failed to set clipboard image: {ex.Message}");
					__result = Task.FromResult(false);
				}
				return false;
			}
		}
	}
}
