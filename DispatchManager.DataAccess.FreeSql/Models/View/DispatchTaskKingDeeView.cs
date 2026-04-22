using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using Longbow.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.DataAccess.FreeSql.Models.View
{
    public class DispatchTaskKingDeeView
    {
        public DispatchTaskKingDee dispatchTaskKingDee;

        public DispatchTaskKingDeeView(DispatchTaskKingDee _dispatchTaskKingDee)
        {
            dispatchTaskKingDee = _dispatchTaskKingDee;
        }

        /// <summary>
        /// Y9密钥
        /// </summary>
        [Display(Name = "Y9密钥")]
        public string? Y9Key { get; set; }

        /// <summary>
        /// 金蝶账套ID
        /// </summary>
        [Display(Name = "账套ID")]
        public string? KingDeeAccountID { get; set; }

        /// <summary>
        /// 金蝶系统授权的用户
        /// </summary>
        [Display(Name = "授权用户")]
        public string? KingDeeUserName { get; set; }

        /// <summary>
        /// 金蝶系统授权的应用ID
        /// </summary>
        [Display(Name = "应用ID")]
        public string? KingDeeAppID { get; set; }

        /// <summary>
        /// 金蝶系统授权的应用密钥
        /// </summary>
        [Display(Name = "应用密钥")]
        public string? KingDeeAppSec { get; set; }

        /// <summary>
        /// 金蝶系统账套语系
        /// </summary>
        [Display(Name = "账套语系")]
        [DefaultValue("2052")]
        public string? KingDeeLCID { get; set; } = "2052";

        /// <summary>
        /// 金蝶系统服务器地址
        /// </summary>
        [Display(Name = "服务器地址")]
        public string? KingDeeServerUrl { get; set; }
    }
}
