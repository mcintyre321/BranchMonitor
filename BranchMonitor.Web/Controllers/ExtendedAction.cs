using System;
using Action = SirenSharp.Action;

namespace BranchMonitor.Web.Controllers
{
    public class ExtendedAction : Action
    {
        public bool Enabled { get; set; }

        public ExtendedAction(string name, Uri href) : base(name, href)
        {
        }

        public ExtendedAction(string name, string href) : base(name, href)
        {
        }
    }
}