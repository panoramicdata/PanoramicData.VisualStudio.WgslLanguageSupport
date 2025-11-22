using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace PanoramicData.VisualStudio.WgslLanguageSupport;

/// <summary>
/// VS Package that registers the WGSL language and TextMate grammar.
/// </summary>
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideLanguageExtension(typeof(WgslLanguagePackage), ".wgsl")]
public sealed class WgslLanguagePackage : AsyncPackage
{
	public const string PackageGuidString = "ad1d7d39-de8b-49b5-ba08-acd06597d11e";

	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
	}
}
