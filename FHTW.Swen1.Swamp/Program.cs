using FHTW.Swen1.Swamp.Database;


namespace FHTW.Swen1.Swamp
{
    internal class Program
    {
        private static Router router = new Router();

        static void Main(string[] args)
        {
            //DatabaseHelper.ResetDatabase();
            //DatabaseHelper.CreateDatabase();
            //DatabaseHelper.CreateTables();
            HttpSvr svr = new();
            svr.Incoming += _ProcessMesage;

            svr.Run();
            
        }

        private static void _ProcessMesage(object sender, HttpSvrEventArgs e)
        {
            router.RouteRequest(e);
        }
    }

}
