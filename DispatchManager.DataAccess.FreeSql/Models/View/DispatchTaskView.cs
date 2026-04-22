using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using Longbow.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.DataAccess.FreeSql.Models.View
{
    public class DispatchTaskView
    {
        public DispatchTask dispatchTask;

        public DispatchTaskView(DispatchTask _dispatchTask)
        {
            dispatchTask = _dispatchTask;
        }

        /// <summary>
        /// Y9密钥
        /// </summary>
        [Display(Name = "Y9密钥")]
        public string? Y9Key { get; set; } 
    }
}
