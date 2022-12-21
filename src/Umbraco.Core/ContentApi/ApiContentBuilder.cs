using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiContentBuilder : IApiContentBuilder
{
    private readonly IPropertyMapper _propertyMapper;
    private readonly IContentNameProvider _nameProvider;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public ApiContentBuilder(IPropertyMapper propertyMapper, IContentNameProvider nameProvider, IPublishedUrlProvider publishedUrlProvider)
    {
        _propertyMapper = propertyMapper;
        _nameProvider = nameProvider;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public IApiContent Build(IPublishedContent content) => new ApiContent(
        content.Key,
        _nameProvider.GetName(content),
        content.ContentType.Alias,
        Url(content),
        _propertyMapper.Map(content));

    private string Url(IPublishedContent content)
        => content.ItemType == PublishedItemType.Content
            ? _publishedUrlProvider.GetUrl(content, UrlMode.Relative)
            : _publishedUrlProvider.GetMediaUrl(content, UrlMode.Relative);
}
