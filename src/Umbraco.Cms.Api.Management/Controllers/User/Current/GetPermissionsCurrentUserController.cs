﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class GetPermissionsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;

    public GetPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _mapper = mapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(IEnumerable<UserPermissionsResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> permissionsAttempt = await _userService.GetPermissionsAsync(CurrentUserKey(_backOfficeSecurityAccessor), ids.ToArray());

        if (permissionsAttempt.Success is false)
        {
            return UserOperationStatusResult(permissionsAttempt.Status);
        }

        List<UserPermissionViewModel> viewmodels = _mapper.MapEnumerable<NodePermissions, UserPermissionViewModel>(permissionsAttempt.Result);

        return Ok(new UserPermissionsResponseModel { Permissions = viewmodels });
    }
}
