using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basic.Models
{
    /// <summary>
    /// 接口结果对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResModel<T>
    {
        /// <summary>
        /// 0：成功，
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 结果对象
        /// </summary>
        public T data { get; set; }
    }
}
