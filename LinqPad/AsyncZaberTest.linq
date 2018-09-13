<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

public class Image
{
	public string Name { get; set; }
}

public class CameraController
{
	private object _lock = new object();
	private Image _current = null;

	public async void StartCamera()
	{
		int iteration = 0;
		while (true)
		{
			await Task.Delay(10);
			lock (_lock)
			{
				_current = new Image { Name = $"Img_{iteration}" };
			}
			iteration++;
		}
	}

	public async Task<Image> GetNextImage()
	{
		Image image = null;
		
		lock (_lock)
		{
			// destroy current
			_current = null;
		}

		await Task.Run(() =>
		{
			while (image == null)  // I would also put a timeout on this loop
			{
				lock (_lock)
				{
					image = _current;
				}
			}
		});
		
		return image;
	}
}

public class Program
{
	private CameraController _cameraController = new CameraController();

	public static void Main()
	{
		new Program();
	}
	
	public Program()
	{
		Task cameraTask = new Task(_cameraController.StartCamera);
		cameraTask.Start();

		Task task = new Task(StartMainLoop);
		task.Start();
	}

	private async void StartMainLoop()
	{
		Image image = null;
		
		for (int step = 0; step < 5; ++step)
		{
			Console.WriteLine($"Loop: Starting step {step}.");

			await RotateStage(step);
			await DoImageProcessing(image);
			image = await AquireNextImage();
		}
		await DoImageProcessing(image);

		Console.WriteLine($"All done.");
	}

	private async Task RotateStage(double angle)
	{
		await Task.Run(async () =>
		{
			await Task.Delay(200);
			Console.WriteLine($"                                 Stage: Rotated to angle {angle}.");
			return;
		});
	}

	private async Task<Image> AquireNextImage()
	{
		Image result = null;
		await Task.Run(async () =>
		{
			result = await _cameraController.GetNextImage();
			Console.WriteLine($"                                                                         Camera: Got image {result.Name}.");
		});

		return result;
	}

	private async Task<Image> DoImageProcessing(Image input)
	{
		Image result = null;
		await Task.Run(async () =>
		{
			if (input == null)
			{
				Console.WriteLine($"                                                                                                                    Processor: image null, skipped.");
			}
			else
			{
				await Task.Delay(500);
				Console.WriteLine($"                                                                                                                    Processor: Completed image {input.Name}.");
				result = new Image { Name = $"Processed_{input.Name}" };
			}
		});

		return result;
	}
}
