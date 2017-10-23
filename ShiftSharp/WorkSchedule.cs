namespace Point85.ShiftSharp.Schedule
{
	public class WorkSchedule
	{
		// resource manager for exception messages
		internal static PropertyManager MessagesManager;

		/// <summary>
		/// get a particular message by its key
		/// </summary>
		public static string GetMessage(string key)
		{
			return MessagesManager.GetString(key);
		}
	}
}
