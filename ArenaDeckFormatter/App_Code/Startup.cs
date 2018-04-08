using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ArenaDeckFormatter.Startup))]
namespace ArenaDeckFormatter
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
