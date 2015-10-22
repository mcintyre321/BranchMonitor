using System;
using System.Collections.Generic;
using SirenSharp;
using Action = SirenSharp.Action;

namespace BranchMonitor.Web.Controllers
{
    class ExtendedSubEntity : SubEntity
    {
        public IEnumerable<Action> Actions { get; set; }

        public ExtendedSubEntity(Uri href, params string[] rel) : base(href, rel)
        {
        }
    }
}