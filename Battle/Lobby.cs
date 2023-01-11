using MTCG;
using MTCG.Battle;
using MTCG.Server;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;

public static class Lobby
{
    // A list to hold the players in the lobby
    private static ConcurrentQueue<UserToken> players = new ConcurrentQueue<UserToken>();

    // A semaphore to control access to the players list
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    // A method for players to join the lobby
    public static void Join(HttpSvrEventArgs e, UserToken player)
    {
        // Acquire the semaphore lock
        semaphore.Wait();

        // Add the player to the list
        players.Enqueue(player);
        Console.WriteLine(player + "enqueued." + Thread.CurrentThread.ManagedThreadId);
        // If there are now two players in the lobby, start a battle
        if (players.Count >= 2)
        {
            UserToken playerOne, playerTwo;
            players.TryDequeue(out playerOne);
            players.TryDequeue(out playerTwo);
            if(playerOne == playerTwo)
            {
                e.Reply(400, "Cannot battle yourself.");
                return;
            }
            // Create a new battle object
            var battle = new Battle();

            // Start the battle
            string battleLogs = battle.Start(playerOne, playerTwo);
            Console.WriteLine("waiting for thread... to finnish:" + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine(battleLogs);
            e.Reply(200, battleLogs);
        }

        Wait(e, player);
        // Release the semaphore lock
        semaphore.Release();
    }

    public static void Wait(HttpSvrEventArgs e, UserToken player)
    {

    }
}