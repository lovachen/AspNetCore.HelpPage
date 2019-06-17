using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace HelpPage.Gen
{
    /// <summary>
    /// 枚举模型对象描述
    /// </summary>
    public class EnumTypeModelDescription : ModelDescription
    {
        /// <summary>
        /// 
        /// </summary>
        public EnumTypeModelDescription()
        {
            Values = new Collection<EnumValueDescription>();
        }

        /// <summary>
        /// 枚举值集合
        /// </summary>
        public Collection<EnumValueDescription> Values { get; private set; }
    }
}
