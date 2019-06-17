using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace HelpPage.Gen
{
    public class HelpPageGenOptions
    {
        public HelpPageGenOptions()
        {
            this.ApiDocs = new Dictionary<string, OpenApiInfo>();
        }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, OpenApiInfo> ApiDocs { get; }

        /// <summary>
        /// 注释文档提供者
        /// </summary>
        public XmlDocumentationProvider XmlProvider { get; private set; }

        /// <summary>
        /// 定义配置文档，可配置多个
        /// </summary>
        /// <param name="groupName">文档组名</param>
        /// <param name="info">文档描述</param>
        public void ApiDoc(string groupName,
            OpenApiInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info) + "不能为空");
            if (String.IsNullOrEmpty(info.Version))
                throw new ArgumentNullException(nameof(info) + " version 不能为空");

            this.ApiDocs.Add(groupName, info);
        }

        /// <summary>
        /// 导入XML注释文件
        /// </summary>
        public void IncludeXmlComments(string filePath)
        {
            XmlProvider =  new XmlDocumentationProvider(filePath);
        }

    }
}
