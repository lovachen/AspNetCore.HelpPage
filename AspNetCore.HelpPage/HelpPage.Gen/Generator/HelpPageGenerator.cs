using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelpPage.Gen
{
    public class HelpPageGenerator : IHelpPageProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private HelpPageGenOptions _options;

        public HelpPageGenerator(IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            IOptions<HelpPageGenOptions> options)
        {
            _options = options.Value;
            _apiDescriptionsProvider = apiDescriptionsProvider;

        }

        /// <summary>
        /// 文档描述字典
        /// </summary>
        public IDictionary<string, OpenApiInfo> GetApiDocs()
        {
            return _options.ApiDocs;

        }

        /// <summary>
        /// 获取文档描述
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public KeyValuePair<string, OpenApiInfo> GetInfo(string groupName)
        {
            var docs = _options.ApiDocs;
            if (!String.IsNullOrEmpty(groupName))
            {
                if (docs.ContainsKey(groupName))
                {
                    return docs.FirstOrDefault(o => o.Key == groupName);
                }
            }
            return docs.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="info"></param>
        /// <returns></returns>

        public IList<ApiDescription> GetApiDescriptions(string groupName)
        {
            var items = _apiDescriptionsProvider.ApiDescriptionGroups.Items.SelectMany(group => group.Items)
               .Where(apiDesc => !apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any())
               .Where(apiDesc => apiDesc.GroupName == null || apiDesc.GroupName == groupName).ToList();



            return items;
        }

        /// <summary>
        /// 获取接口详情的Model
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="apiDescriptionId"></param>
        /// <returns></returns>
        public HelpPageApiModel GetApiModel(string groupName, string apiDescriptionId)
        {
            HelpPageApiModel model = new HelpPageApiModel();

            var apiDescriptions = GetApiDescriptions(groupName);
            ApiDescription apiDescription = apiDescriptions.FirstOrDefault(api => (api.GroupName == null || api.GroupName == groupName) 
                && String.Equals(api.GetFriendlyId(), apiDescriptionId, StringComparison.OrdinalIgnoreCase));

            if(apiDescription!=null)
            {
                model.ApiDescription = apiDescription;

            }
            return model;
        }



    }
}
