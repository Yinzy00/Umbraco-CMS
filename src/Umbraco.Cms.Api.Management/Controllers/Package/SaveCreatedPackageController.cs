using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

public class SaveCreatedPackageController : PackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IPackageDefinitionFactory _packageDefinitionFactory;
    private readonly IUmbracoMapper _umbracoMapper;

    public SaveCreatedPackageController(
        IPackagingService packagingService,
        IPackageDefinitionFactory packageDefinitionFactory,
        IUmbracoMapper umbracoMapper)
    {
        _packagingService = packagingService;
        _packageDefinitionFactory = packageDefinitionFactory;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Creates or updates a package.
    /// </summary>
    /// <param name="model">The PackageDefinition model containing the data for a package.</param>
    /// <returns>The created or updated package.</returns>
    [HttpPost("created/save")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PackageDefinitionViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PackageDefinitionViewModel>> Save(PackageDefinitionViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return await Task.FromResult(ValidationProblem(ModelState));
        }

        PackageDefinition packageDefinition = _packageDefinitionFactory.CreatePackageDefinition(model);

        // Save it
        if (!_packagingService.SaveCreatedPackage(packageDefinition))
        {
            return await Task.FromResult(ValidationProblem(
                packageDefinition.Id == default
                    ? $"A package with the name {packageDefinition.Name} already exists"
                    : $"The package with id {packageDefinition.Id} was not found"));
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<PackageDefinitionViewModel>(packageDefinition)));
    }
}
