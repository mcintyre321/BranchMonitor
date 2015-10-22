using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Octokit;
using Octokit.Internal;
using SirenSharp;
using Action = SirenSharp.Action;
using HttpVerbs = SirenSharp.HttpVerbs;

namespace BranchMonitor.Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public async Task<ViewResult> Index()
        {
            ViewBag.SirenJson = await JsonData();
            return View("Index");
        }

        private async Task<string> JsonData()
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("BranchMonitor"),
                new InMemoryCredentialStore(new Credentials(ConfigurationManager.AppSettings["gitHubOwner"])));

            var branchesOnGithub = await gitHubClient.Repository.GetAllBranches(ConfigurationManager.AppSettings["gitHubOwner"], ConfigurationManager.AppSettings["repository"]);
            var bes = new BranchEnvironmentsService();
            var environments = bes.ListEnvironments();


            var body = new Entity<object>()
            {
                Class = new[] {"repo"},
                Properties = new {Title = "Platform"},
                Entities = environments.Select(env => new ExtendedSubEntity(null, "branch")
                {
                    Properties = new Dictionary<string, string>()
                    {
                        {"Title", env}
                    },
                    Actions = new Action[]
                    {
                        new ExtendedAction("DeleteEnvironment",
                            Url.Action("DeleteEnvironment", new {branchFriendlyName = env}))
                        {
                            Title = "Delete Environment",
                            Method = HttpVerbs.Post,
                            Enabled = branchesOnGithub.All(branch => branch.Name != env)
                        },
                    }
                }).ToList()
            };
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore

            };
            return JsonConvert.SerializeObject(body, settings);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult DeleteEnvironment(string branchfriendlyname)
        {
            new BranchEnvironmentsService().DeleteBranchEnvironment(branchfriendlyname);
            return new RedirectResult("/");
        }
    }
}