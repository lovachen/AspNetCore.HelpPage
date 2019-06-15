using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelpPage.Gen
{
    public class HelpPageGenOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public HelpPageGeneratorOptions HelpPageGeneratorOptions { get; set; } = new HelpPageGeneratorOptions();
         
        /// <summary>
        /// 定义配置文档，可配置多个
        /// </summary>
        /// <param name="name">一个url友好标识的唯一名称</param>
        /// <param name="info">文档描述</param>
        public void ApiDoc(string name,
            OpenApiInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info) + "不能为空");
            if (String.IsNullOrEmpty(info.Version))
                throw new ArgumentNullException(nameof(info) + " version 不能为空");

            HelpPageGeneratorOptions.ApiDocs.Add(name, info);
        }

        /// <summary>
        /// 导入XML注释文件
        /// </summary>
        public void IncludeXmlComments()
        {


        }

    }
}
