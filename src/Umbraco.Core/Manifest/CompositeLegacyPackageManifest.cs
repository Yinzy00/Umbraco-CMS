using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     A package manifest made up of all combined manifests
/// </summary>
public class CompositeLegacyPackageManifest
{
    public CompositeLegacyPackageManifest(
        IReadOnlyList<IDataEditor> propertyEditors,
        IReadOnlyList<IDataEditor> parameterEditors,
        IReadOnlyList<LegacyManifestContentAppDefinition> contentApps,
        IReadOnlyList<LegacyManifestDashboard> dashboards,
        IReadOnlyList<LegacyManifestSection> sections,
        IReadOnlyDictionary<BundleOptions, IReadOnlyList<LegacyManifestAssets>> scripts,
        IReadOnlyDictionary<BundleOptions, IReadOnlyList<LegacyManifestAssets>> stylesheets)
    {
        PropertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
        ParameterEditors = parameterEditors ?? throw new ArgumentNullException(nameof(parameterEditors));
        ContentApps = contentApps ?? throw new ArgumentNullException(nameof(contentApps));
        Dashboards = dashboards ?? throw new ArgumentNullException(nameof(dashboards));
        Sections = sections ?? throw new ArgumentNullException(nameof(sections));
        Scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
        Stylesheets = stylesheets ?? throw new ArgumentNullException(nameof(stylesheets));
    }

    /// <summary>
    ///     Gets or sets the property editors listed in the manifest.
    /// </summary>
    public IReadOnlyList<IDataEditor> PropertyEditors { get; }

    /// <summary>
    ///     Gets or sets the parameter editors listed in the manifest.
    /// </summary>
    public IReadOnlyList<IDataEditor> ParameterEditors { get; }

    /// <summary>
    ///     Gets or sets the content apps listed in the manifest.
    /// </summary>
    public IReadOnlyList<LegacyManifestContentAppDefinition> ContentApps { get; }

    /// <summary>
    ///     Gets or sets the dashboards listed in the manifest.
    /// </summary>
    public IReadOnlyList<LegacyManifestDashboard> Dashboards { get; }

    /// <summary>
    ///     Gets or sets the sections listed in the manifest.
    /// </summary>
    public IReadOnlyCollection<LegacyManifestSection> Sections { get; }

    public IReadOnlyDictionary<BundleOptions, IReadOnlyList<LegacyManifestAssets>> Scripts { get; }

    public IReadOnlyDictionary<BundleOptions, IReadOnlyList<LegacyManifestAssets>> Stylesheets { get; }
}