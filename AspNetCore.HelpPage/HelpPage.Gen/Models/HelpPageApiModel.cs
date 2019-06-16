using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.ObjectModel;

namespace HelpPage.Gen
{
    public class HelpPageApiModel
    {
        public ApiDescription ApiDescription { get; set; }

        /// <summary>
        /// 接口描述说明
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Body 的参数模型
        /// </summary>
        public ModelDescription RequestModelDescription { get; set; }

        /// <summary>
        /// 请求说明
        /// </summary>
        public string RequestDocumentation { get; set; }

        /// <summary>
        /// Url参数
        /// </summary>
        public Collection<ApiParameterDescription> UriParameters { get; private set; }

    }
}
