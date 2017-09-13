using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Repositories.Interfaces;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Override the base content repository so we can change the node object type
    /// </summary>
    /// <remarks>
    /// It would be nicer if we could separate most of this down into a smaller version of the ContentRepository class, however to do that
    /// requires quite a lot of work since we'd need to re-organize the interhitance quite a lot or create a helper class to perform a lot of the underlying logic.
    ///
    /// TODO: Create a helper method to contain most of the underlying logic for the ContentRepository
    /// </remarks>
    internal class ContentBlueprintRepository : ContentRepository, IContentBlueprintRepository
    {
        public ContentBlueprintRepository(IScopeUnitOfWork work, CacheHelper cacheHelper, ILogger logger, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, IContentSection settings)
            : base(work, cacheHelper, logger, contentTypeRepository, templateRepository, tagRepository, settings)
        {
            EnsureUniqueNaming = false; // duplicates are allowed
        }

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentBlueprintGuid;
    }
}