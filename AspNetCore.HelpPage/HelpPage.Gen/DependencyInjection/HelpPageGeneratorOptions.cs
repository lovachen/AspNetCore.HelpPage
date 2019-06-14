using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelpPage.Gen
{
    public class HelpPageGeneratorOptions
    {
        public HelpPageGeneratorOptions()
        {
            ApiDocs = new Dictionary<string, OpenApiInfo>();

        }

        /// <summary>
        /// 文档
        /// </summary>
        public IDictionary<string, OpenApiInfo> ApiDocs { get; set; }
    }
}
