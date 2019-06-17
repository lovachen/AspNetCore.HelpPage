using System;
using System.Collections.Generic;
using System.Text;

namespace HelpPage.Gen
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyValuePairModelDescription : ModelDescription
    {
        /// <summary>
        /// 
        /// </summary>
        public ModelDescription KeyModelDescription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ModelDescription ValueModelDescription { get; set; }
    }
}
