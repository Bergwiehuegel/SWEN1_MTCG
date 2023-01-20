using MTCG;
using MTCG.Controller;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;

namespace MTCG.Models
{
    public static class Lobby
    {
        private static ConcurrentQueue<UserToken> PlayerQueue = new ConcurrentQueue<UserToken>();

        // semaphore/lock to control access to the join function
        private static SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        // dictionary to store battle logs with users as keys (thanks to simons presentation :D )
        private static Dictionary <UserToken, string> BattleLogs = new Dictionary<UserToken, string> ();

        public static void Join(HttpSvrEventArgs e, UserToken player)
        {
            UserToken playerOne = null, playerTwo = null;
              
            Semaphore.Wait();

            PlayerQueue.Enqueue(player);
            Console.WriteLine(player + "enqueued." + Thread.CurrentThread.ManagedThreadId);
            
            if (PlayerQueue.Count >= 2)
            {
                PlayerQueue.TryDequeue(out playerOne);
                PlayerQueue.TryDequeue(out playerTwo);
                if (playerOne == playerTwo)
                {
                    e.Reply(400, "Cannot battle yourself.");
                    return;
                }
            }
            Semaphore.Release();

            if ((playerOne != null) && (playerTwo != null))
            {
                var battle = new Battle();
                string result = battle.Start(playerOne, playerTwo);
                BattleLogs.Add(playerOne, result);
                e.Reply(200, result);
                return;
            }
                //TODO: Check if deck valid and whatnot

            while (!BattleLogs.ContainsKey(player))
            {
                int i = 0;
                Thread.Sleep(500);
                i++;
                if(i == 20)
                {
                    e.Reply(400, "No Battle found - please queue again.");
                }
            }
            e.Reply(200, BattleLogs[player]);
            BattleLogs.Remove(player);
        }
    }
}