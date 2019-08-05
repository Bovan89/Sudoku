using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Sudoku.Startup))]
namespace Sudoku
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
