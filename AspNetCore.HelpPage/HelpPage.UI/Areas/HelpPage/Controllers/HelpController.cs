using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpPage.Gen;
using HelpPage.UI.Areas.HelpPage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HelpPage.UI.Areas.HelpPage.Controllers
{
    public class HelpController : AreaController
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private IOptions<HelpPageGenOptions> _options;

        public HelpController(IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            IOptions<HelpPageGenOptions> options)
        {
            _options = options;
            _apiDescriptionsProvider = apiDescriptionGroupCollectionProvider;
        }

        [Route("helppage/{groupName?}", Name = "helpPageIndex")]
        // GET: /<controller>/
        public IActionResult Index(string groupName = null)
        {
            OpenApiInfo info = null;
            var docs = _options.Value.HelpPageGeneratorOptions.ApiDocs;
            if (!String.IsNullOrEmpty(groupName))
            {
                docs.TryGetValue(groupName, out info);
            }
            if (info == null)
            {
                var doc = docs.FirstOrDefault();
                info = doc.Value;
                groupName = doc.Key;
            }
            ViewBag.Info = info;
            ViewBag.GroupName = groupName;
            ViewBag.Docs = docs;
            //api描述集
            var items = _apiDescriptionsProvider.ApiDescriptionGroups.Items.SelectMany(group => group.Items)
                .Where(apiDesc => !apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any())
                .Where(apiDesc => apiDesc.GroupName == null || apiDesc.GroupName == info.Version).ToList();

            return View(items);
        }

        [Route("helppage/api/{groupName}/{apiId}", Name ="helpPageApi")]
        public ActionResult Api(string groupName, string apiId)
        {
            if (!String.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = new HelpPageApiModel();
                if (apiModel != null)
                {
                    return View(apiModel);
                }
            }

            return View("Error");
        }
    }
}
