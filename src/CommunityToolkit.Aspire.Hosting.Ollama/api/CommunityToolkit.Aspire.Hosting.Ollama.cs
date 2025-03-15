//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Aspire.Hosting
{
    public enum OllamaGpuVendor
    {
        Nvidia = 0,
        AMD = 1
    }

    public static partial class OllamaResourceBuilderExtensions
    {
        public static ApplicationModel.IResourceBuilder<ApplicationModel.OllamaModelResource> AddHuggingFaceModel(this ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> builder, string name, string modelName) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.OllamaModelResource> AddModel(this ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> builder, string name, string modelName) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.OllamaModelResource> AddModel(this ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> builder, string modelName) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> AddOllama(this IDistributedApplicationBuilder builder, string name, int? port = null) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> WithDataVolume(this ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> builder, string? name = null, bool isReadOnly = false) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.OpenWebUIResource> WithDataVolume(this ApplicationModel.IResourceBuilder<ApplicationModel.OpenWebUIResource> builder, string? name = null, bool isReadOnly = false) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> WithGPUSupport(this ApplicationModel.IResourceBuilder<ApplicationModel.OllamaResource> builder, OllamaGpuVendor vendor = OllamaGpuVendor.Nvidia) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.OpenWebUIResource> WithHostPort(this ApplicationModel.IResourceBuilder<ApplicationModel.OpenWebUIResource> builder, int? port) { throw null; }

        public static ApplicationModel.IResourceBuilder<T> WithOpenWebUI<T>(this ApplicationModel.IResourceBuilder<T> builder, System.Action<ApplicationModel.IResourceBuilder<ApplicationModel.OpenWebUIResource>>? configureContainer = null, string? containerName = null)
            where T : ApplicationModel.OllamaResource { throw null; }
    }
}

namespace Aspire.Hosting.ApplicationModel
{
    public partial class OllamaModelResource : Resource, IResourceWithParent<OllamaResource>, IResourceWithParent, IResource, IResourceWithConnectionString, IManifestExpressionProvider, IValueProvider, IValueWithReferences
    {
        public OllamaModelResource(string name, string modelName, OllamaResource parent) : base(default!) { }

        public ReferenceExpression ConnectionStringExpression { get { throw null; } }

        public string ModelName { get { throw null; } }

        public OllamaResource Parent { get { throw null; } }
    }

    public partial class OllamaResource : ContainerResource, IResourceWithConnectionString, IResource, IManifestExpressionProvider, IValueProvider, IValueWithReferences
    {
        public OllamaResource(string name) : base(default!, default) { }

        public ReferenceExpression ConnectionStringExpression { get { throw null; } }

        public System.Collections.Generic.IReadOnlyList<string> Models { get { throw null; } }

        public EndpointReference PrimaryEndpoint { get { throw null; } }

        public void AddModel(string modelName) { }
    }

    public partial class OpenWebUIResource : ContainerResource, IResourceWithConnectionString, IResource, IManifestExpressionProvider, IValueProvider, IValueWithReferences
    {
        public OpenWebUIResource(string name) : base(default!, default) { }

        public ReferenceExpression ConnectionStringExpression { get { throw null; } }

        public System.Collections.Generic.IReadOnlyList<OllamaResource> OllamaResources { get { throw null; } }

        public EndpointReference PrimaryEndpoint { get { throw null; } }
    }
}