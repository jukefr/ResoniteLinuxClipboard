using Elements.Assets;
using Renderite.Host;
using System.Diagnostics;
using System.Reflection;

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

		private static readonly Harmony harmony = new Harmony("org.yosh.WaylandClipboard");
		private static bool discoveryStarted;

	//// CONFIG ////

	internal static ModConfiguration? config;

	[AutoRegisterConfigKey]
	internal static readonly ModConfigurationKey<bool> KEnableDiscovery = new(
		"EnableDiscovery",
		"Enable discovery mode for debugging (logs inspector/font candidates)",
		computeDefault: () => false
	);

	[AutoRegisterConfigKey]
	internal static readonly ModConfigurationKey<int> KClipboardTimeoutMs = new(
		"ClipboardTimeoutMs",
		"Timeout in milliseconds for clipboard operations (0 = no timeout)",
		computeDefault: () => 5000,
		valueValidator: (v) => v >= 0
	);

	internal static bool EnableDiscovery => config?.GetValue(KEnableDiscovery) ?? false;
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
		harmony.PatchAll();
		if (EnableDiscovery)
			StartDiscoveryMode();
	}

	private static void StartDiscoveryMode()
	{
		if (discoveryStarted)
			return;

		discoveryStarted = true;
		Info($"[{nameof(WaylandClipboard)}] Discovery mode enabled: scanning for inspector/font candidates.");
		DiscoveryTools.DumpCandidates();
		DiscoveryTools.PatchRuntimeProbes(harmony);
	}

	//// RELOAD ////

#if DEBUG && RML_HOTRELOAD
	static void BeforeHotReload()
	{
		harmony.UnpatchAll(harmony.Id);
	}

	static void OnHotReload(ResoniteMod modInstance)
	{
		instance = (WaylandClipboard)modInstance;
		// config = modInstance.GetConfiguration();
		InitMod();
	}
#endif

	private static void Info(string message) => Msg($"[{nameof(WaylandClipboard)}] {message}");
	private static void Warn(string message) => Msg($"[{nameof(WaylandClipboard)}] {message}");
	private static void ErrorMsg(string message) => Error($"[{nameof(WaylandClipboard)}] {message}");

	public static class DiscoveryTools
	{
		private static readonly string[] AssemblyPrefixes =
		{
			"FrooxEngine",
			"Elements."
		};

		private static readonly string[] Keywords =
		{
			"Inspector",
			"Font",
			"FontChain",
			"Text",
			"Style",
			"Theme",
			"Label"
		};

		private static readonly HashSet<string> LoggedRuntimeMethods = new HashSet<string>();

		public static void DumpCandidates()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(IsInterestingAssembly))
			{
				Type[] types;
				try
				{
					types = asm.GetTypes();
				}
				catch (ReflectionTypeLoadException rtl)
				{
					types = rtl.Types.Where(t => t != null).Cast<Type>().ToArray();
				}
				catch (Exception ex)
				{
					Warn($"[{nameof(WaylandClipboard)}] Discovery scan failed for assembly '{asm.GetName().Name}': {ex.Message}");
					continue;
				}

				foreach (var type in types)
				{
					var score = ScoreType(type);
					if (score <= 0)
						continue;

					var methods = type
						.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
						.Where(m => ScoreMethod(m) > 0)
						.Take(8)
						.Select(m => m.Name)
						.Distinct()
						.ToArray();

					Info($"[{nameof(WaylandClipboard)}] Candidate[{score}] {type.FullName} :: {string.Join(", ", methods)}");
				}
			}
		}

		public static void PatchRuntimeProbes(Harmony patcher)
		{
			var targetMethods = EnumerateRuntimeProbeMethods().ToArray();
			var postfix = new HarmonyMethod(typeof(DiscoveryTools), nameof(RuntimeProbePostfix));
			var patchedCount = 0;

			foreach (var method in targetMethods)
			{
				try
				{
					patcher.Patch(method, postfix: postfix);
					patchedCount++;
				}
				catch (Exception ex)
				{
					Warn($"[{nameof(WaylandClipboard)}] Failed to patch probe method {method.DeclaringType?.FullName}.{method.Name}: {ex.Message}");
				}
			}

			Info($"[{nameof(WaylandClipboard)}] Runtime probes attached: {patchedCount} methods.");
		}

		public static void RuntimeProbePostfix(MethodBase __originalMethod)
		{
			if (__originalMethod == null)
				return;

			var key = $"{__originalMethod.DeclaringType?.FullName}.{__originalMethod.Name}";
			if (!LoggedRuntimeMethods.Add(key))
				return;

			Info($"[{nameof(WaylandClipboard)}] Runtime hit: {key}");
		}

		private static IEnumerable<MethodBase> EnumerateRuntimeProbeMethods()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(IsInterestingAssembly))
			{
				Type[] types;
				try
				{
					types = asm.GetTypes();
				}
				catch (ReflectionTypeLoadException rtl)
				{
					types = rtl.Types.Where(t => t != null).Cast<Type>().ToArray();
				}
				catch
				{
					continue;
				}

				foreach (var type in types)
				{
					if (ScoreType(type) <= 0)
						continue;

					MethodInfo[] methods;
					try
					{
						methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
					}
					catch
					{
						continue;
					}

					foreach (var method in methods)
					{
						if (method.IsAbstract || method.ContainsGenericParameters)
							continue;
						if (method.IsSpecialName)
							continue;
						if (method.GetMethodBody() == null)
							continue;
						if (ScoreMethod(method) <= 0)
							continue;

						yield return method;
					}
				}
			}
		}

		private static bool IsInterestingAssembly(Assembly asm)
		{
			var name = asm.GetName().Name;
			if (string.IsNullOrEmpty(name))
				return false;

			return AssemblyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		}

		private static int ScoreType(Type type)
		{
			var fullName = type.FullName ?? type.Name;
			var score = CountKeywordMatches(fullName);
			return score;
		}

		private static int ScoreMethod(MethodInfo method)
		{
			var score = CountKeywordMatches(method.Name);
			if (method.GetParameters().Any(p => CountKeywordMatches(p.ParameterType.Name) > 0))
				score += 1;
			if (method.ReturnType != null && CountKeywordMatches(method.ReturnType.Name) > 0)
				score += 1;
			return score;
		}

		private static int CountKeywordMatches(string? value)
		{
			if (string.IsNullOrEmpty(value))
				return 0;

			var count = 0;
			foreach (var keyword in Keywords)
			{
				if (value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
					count++;
			}
			return count;
		}
	}

	public static class Patch_LinuxClipboardInterface
	{
		private enum ClipboardBackend
		{
			Wayland,
			X11,
			None
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
					Info("Using Wayland clipboard backend (wl-copy/wl-paste).");
				}
				else if (CommandExists("xclip"))
				{
					backend = ClipboardBackend.X11;
					Info("Using X11 clipboard backend (xclip).");
				}
				else
				{
					backend = ClipboardBackend.None;
					ErrorMsg("No clipboard backend available. Install wl-clipboard or xclip.");
				}

				return backend.Value;
			}
		}

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
			else if (Backend == ClipboardBackend.X11)
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
			if (Backend == ClipboardBackend.Wayland)
			{
				var args = string.IsNullOrEmpty(mimeType) ? "" : $"--type {mimeType}";
				psi = new ProcessStartInfo("wl-copy", args);
			}
			else if (Backend == ClipboardBackend.X11)
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
			try
			{
				ProcessStartInfo psi;
				if (Backend == ClipboardBackend.Wayland)
					psi = new ProcessStartInfo("wl-paste", "-l");
				else if (Backend == ClipboardBackend.X11)
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
					.Select(s => Backend == ClipboardBackend.X11 && s == "UTF8_STRING" ? "text/plain;charset=utf-8" : s)
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
					if (Backend == ClipboardBackend.None)
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
			static bool Prefix(ref CommonClipboard.ImageFormat? __result)
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
					if (Backend == ClipboardBackend.None)
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
					if (Backend == ClipboardBackend.None)
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
					if (Backend == ClipboardBackend.None)
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
					if (Backend == ClipboardBackend.None)
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
