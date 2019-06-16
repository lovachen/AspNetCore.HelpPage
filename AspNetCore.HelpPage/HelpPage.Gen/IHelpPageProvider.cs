using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelpPage.Gen
{
    public interface IHelpPageProvider
    {
        /// <summary>
        /// 文档描述字典
        /// </summary>
        IDictionary<string, OpenApiInfo> GetApiDocs();

        /// <summary>
        /// 获取文档描述
        /// </summary>
        /// <param name="groupName">文档名称</param>
        /// <returns></returns>
        KeyValuePair<string, OpenApiInfo> GetInfo(string groupName);

        /// <summary>
        /// 获取文档的apidesc 列表数据
        /// </summary>
        /// <param name="groupName">文档</param>
        /// <returns></returns>
        IList<ApiDescription> GetApiDescriptions(string groupName );


        /// <summary>
        /// 获取接口详情的Model
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="apiDescriptionId"></param>
        /// <returns></returns>
        HelpPageApiModel GetApiModel(string groupName,string apiDescriptionId);
    }
}
