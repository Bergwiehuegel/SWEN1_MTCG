using MTCG.Server;
using Npgsql;
using System;
using System.Data;
using System.Text;
using System.Threading;



namespace MTCG
{
    /// <summary>This is the program class for the application.</summary>
    public static partial class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // main entry point                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            HttpSvr svr = new HttpSvr();
            svr.Incoming += _Svr_Incoming;

            svr.Run();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // event handlers                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Processes an incoming HTTP request.</summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        public static void _Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(RequestRoutes._Svr_Incoming, e);
        }
    }
}
