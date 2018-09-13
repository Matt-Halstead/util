<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

class ThreadingExample  
{  
    static AutoResetEvent autoEvent;
	static ManualResetEvent manualEvent;

	static void DoWork()
	{
		Console.WriteLine("                                                            worker: waiting on event...");
		autoEvent.WaitOne();
		Console.WriteLine("                                                            worker: signalled. Done.");
	}

    static void Main()  
    {
		//autoEvent = new AutoResetEvent(false);

		//		Console.WriteLine("main: starting worker thread...");  
		//        Thread t = new Thread(DoWork);  
		//        t.Start();  
		//
		//        Console.WriteLine("main: sleeping for 1 second...");  
		//        Thread.Sleep(1000);
		//
		//        Console.WriteLine("main: signaling worker thread...");
		//		autoEvent.Set();
		//
		//		Console.WriteLine("main: waiting for worker to finish...");
		//		t.Join();
		//		Console.WriteLine("main: Done.");

		manualEvent = new ManualResetEvent(false);

		var t = Task.Run(() =>
		{
//			Console.WriteLine("waiting to start ...");
//			manualEvent.WaitOne();
//			Console.WriteLine("started 1.");
			
			while (true)
			{
				if (!manualEvent.WaitOne())
				{
					Console.WriteLine($"stopped.");
				}
				Console.WriteLine($"started again.");
				Thread.Sleep(500);
			}
		});

		Thread.Sleep(1000);
		manualEvent.Set();

		Thread.Sleep(1000);
		manualEvent.Reset();

		Thread.Sleep(1000);
		Console.WriteLine("Finished.");

	}
}