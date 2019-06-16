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
        private IHelpPageProvider _helpPageProvider;

        public HelpController( IHelpPageProvider helpPageProvider)
        {
            _helpPageProvider = helpPageProvider; 
        }

        [Route("helppage/{groupName?}", Name = "helpPageIndex")]
        // GET: /<controller>/
        public IActionResult Index(string groupName = null)
        {
            var doc = _helpPageProvider.GetInfo(groupName);
            ViewBag.Info = doc.Value;
            ViewBag.GroupName = doc.Key;
            ViewBag.Docs = _helpPageProvider.GetApiDocs();
            //api描述集
            var items = _helpPageProvider.GetApiDescriptions(doc.Key);

            return View(items);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="apiId"></param>
        /// <returns></returns>
        [Route("helppage/api/{groupName}/{apiId}", Name ="helpPageApi")]
        public ActionResult Api(string groupName, string apiId)
        {
            if (!String.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = _helpPageProvider.GetApiModel(groupName,apiId);
                if (apiModel != null)
                {
                    return View(apiModel);
                }
            }
            return View("Error");
        }
    }
}
