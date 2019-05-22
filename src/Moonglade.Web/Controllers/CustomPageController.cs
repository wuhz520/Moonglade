﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moonglade.Core;
using Moonglade.Model;
using Moonglade.Model.Settings;
using Moonglade.Web.Models;

namespace Moonglade.Web.Controllers
{
    [Route("page")]
    public class CustomPageController : MoongladeController
    {
        private readonly CustomPageService _customPageService;

        public CustomPageController(
            ILogger<CustomPageController> logger,
            IOptions<AppSettings> settings,
            CustomPageService customPageService) : base(logger, settings)
        {
            _customPageService = customPageService;
        }

        public string[] InvalidPageRouteNames => new[] { "index", "manage" };

        [HttpGet("{routeName}")]
        public async Task<IActionResult> Index(string routeName)
        {
            if (string.IsNullOrWhiteSpace(routeName))
            {
                return BadRequest();
            }

            var response = await _customPageService.GetPageAsync(routeName);
            if (response.IsSuccess)
            {
                if (response.Item == null)
                {
                    return NotFound();
                }

                return View(response.Item);
            }
            return ServerError();
        }

        [Authorize]
        [HttpGet("manage")]
        public async Task<IActionResult> Manage()
        {
            var response = await _customPageService.GetPagesMetaDataListAsync();
            if (response.IsSuccess)
            {
                return View(response.Item);
            }
            return ServerError();
        }

        [Authorize]
        [HttpGet("manage/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await _customPageService.GetPageAsync(id);
            if (response.IsSuccess)
            {
                if (response.Item == null)
                {
                    return NotFound();
                }

                var model = new CustomPageEditViewModel
                {
                    Id = response.Item.Id,
                    Title = response.Item.Title,
                    RouteName = response.Item.RouteName,
                    CssContent = response.Item.CssContent,
                    RawHtmlContent = response.Item.RawHtmlContent,
                    HideSidebar = response.Item.HideSidebar
                };

                return View("CreateOrEdit", model);
            }
            return ServerError();
        }

        [Authorize]
        [HttpPost("manage/edit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomPageEditViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (InvalidPageRouteNames.Contains(model.RouteName.ToLower()))
                    {
                        ModelState.AddModelError(nameof(model.RouteName), "Reserved Route Name.");
                        return View("CreateOrEdit", model);
                    }

                    var req = new CreateEditCustomPageRequest
                    {
                        HtmlContent = model.RawHtmlContent,
                        CssContent = model.CssContent,
                        HideSidebar = model.HideSidebar,
                        RouteName = model.RouteName,
                        Title = model.Title,
                        Id = model.Id
                    };

                    var response = await _customPageService.EditPageAsync(req);
                    if (response.IsSuccess)
                    {
                        return RedirectToAction("Manage");
                    }

                    ModelState.AddModelError(string.Empty, response.Message);
                    return View("CreateOrEdit", model);
                }
                return View("CreateOrEdit", model);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error Editing CustomPage.");
                ModelState.AddModelError(string.Empty, e.Message);
                return View("CreateOrEdit", model);
            }
        }

        [Authorize]
        [HttpGet("manage/create")]
        public IActionResult Create()
        {
            var model = new CustomPageEditViewModel();
            return View("CreateOrEdit", model);
        }

        [Authorize]
        [HttpPost("manage/create"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomPageEditViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (InvalidPageRouteNames.Contains(model.RouteName.ToLower()))
                    {
                        ModelState.AddModelError(nameof(model.RouteName), "Reserved Route Name.");
                        return View("CreateOrEdit", model);
                    }

                    var req = new CreateEditCustomPageRequest
                    {
                        HtmlContent = model.RawHtmlContent,
                        CssContent = model.CssContent,
                        HideSidebar = model.HideSidebar,
                        RouteName = model.RouteName,
                        Title = model.Title
                    };

                    var response = await _customPageService.CreatePageAsync(req);
                    if (response.IsSuccess)
                    {
                        return RedirectToAction("Manage");
                    }

                    ModelState.AddModelError(string.Empty, response.Message);
                    return View("CreateOrEdit", model);
                }
                return View("CreateOrEdit", model);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error Creating CustomPage.");
                ModelState.AddModelError(string.Empty, e.Message);
                return View("CreateOrEdit", model);
            }
        }
    }
}