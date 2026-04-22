using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.DataAccess.FreeSql.Models
{
    public class DispatchClass
    {

        /// <summary>
        /// 自增主键
        /// </summary>
        [NotNull]
        [Column(IsIdentity = true, IsPrimary = true)]
        public long? ID { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "系统名称")]
        [NotNull]
        [Required(ErrorMessage = "系统名称都空着,你想干嘛?")]
        [PlaceHolder("系统名称不能为空,最大50个字")]
        [MaxLength(50)]
        public string? ClassName { get;set; }=Color.Primary.ToString();

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "系统颜色标识")]
        [NotNull]
        [Required(ErrorMessage = "系统颜色标识必选哦")]
        public Color FColor { get;set; }

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
