using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpPage.Gen;
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

        [Route("helppage/{groupNmae?}", Name = "helpIndex")]
        // GET: /<controller>/
        public IActionResult Index(string groupNmae = null)
        {
            OpenApiInfo info = null;
            var docs = _options.Value.HelpPageGeneratorOptions;
            if (!String.IsNullOrEmpty(groupNmae))
            {
                groupNmae = groupNmae.ToLower();
                docs.ApiDocs.TryGetValue(groupNmae, out info);


            }
            if (info == null)
            {
                var doc = docs.ApiDocs.FirstOrDefault();
                info = doc.Value;
                groupNmae = doc.Key;
            }
            ViewBag.GroupName = groupNmae;
            ViewBag.Docs = docs;
            //api描述集
            var items = _apiDescriptionsProvider.ApiDescriptionGroups.Items.SelectMany(group => group.Items)
                .Where(apiDesc => apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any())
                .Where(apiDesc => apiDesc.GroupName == null || apiDesc.GroupName == info.Version);

            return View(items);
        }
    }
}
