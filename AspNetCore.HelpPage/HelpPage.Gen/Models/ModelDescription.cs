using System;
using System.Collections.Generic;
using System.Text;

namespace HelpPage.Gen
{
    /// <summary>
    /// 模型文档
    /// </summary>
    public class ModelDescription
    {
        public string Documentation { get; set; }

        public Type ModelType { get; set; }

        public string Name { get; set; }
    }
}
