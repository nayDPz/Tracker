using Tracker;
using RoR2;

public class TrackerContent
{
	public static void Initialize()
	{
		Buffs.Initialize();
	}

	public static class Buffs
	{

		public static BuffDef chargeCannon;
		public static void Initialize()
		{
			chargeCannon = Assets.mainAssetBundle.LoadAsset<BuffDef>("bdChargeCannon");

		}
	}

	
}
