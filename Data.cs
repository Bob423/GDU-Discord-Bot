using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

// Static variables to allow all classes to access everything they need
public static class Data {

	public static string version = "3.1.3.0";

	public static DiscordSocketClient client;
	public static CommandService cmdService;
	public static IServiceProvider services;

	// Tokens
	public static string mainToken;
	public static string devToken;
	public static string token;

	// Server IDs
	public const ulong GDUID = 317547320738840576;
	public const ulong botworksID = 317742653099999233;

	/// <summary>
	/// ID of current server
	/// </summary>
	public static ulong currentServer;

	// Channel IDs
	public const ulong GDUGeneralID = 317547320738840576;
	public const ulong BotworksGeneralID = 317742653099999233;
	public const ulong GDUArenaID = 433754337525760022;
	public const ulong GDUShopID = 435965658732167189;
	public const ulong GDUSlotsID = 434119878312460299;
	public const ulong GDUMinesID = 447649427067240455;
	public const ulong backRoomID = 317557573245206531;

	/// <summary>
	/// ID of current server's arena channel
	/// </summary>
	public static ulong arenaID;
	/// <summary>
	/// ID of current server's shop channel
	/// </summary>
	public static ulong shopID;
	/// <summary>
	/// ID of current server's slots channel
	/// </summary>
	public static ulong slotsID;
	/// <summary>
	/// ID of current server's mining channel
	/// </summary>
	public static ulong minesID;
	/// <summary>
	/// ID of current server's general channel
	/// </summary>
	public static ulong generalID;

	// File Names
	public const string userData = @"xml/members.xml";
	public const string botData = @"xml/bot.xml";
	public const string arenaList = @"xml/arenaList.xml";
	public const string backUpDirectory = @"C:\Users\Administrator\Desktop\Backups";

	public const int autoBackUpInterval = 2; // 12 hours
	public static int autoBackUpTimer;
	public static Timer backUpTimer = new Timer();

	// Command Info
	public static Dictionary<string, string> helpEntries = new Dictionary<string, string>();
	public const int arenaLockTimer = 60; // 60 seconds

	// Bot IDs
	public const ulong devBotID = 434169827825811458;
	public const ulong GDUBotID = 434474046554374145;
	public static ulong botID;

	public const string botBio = "You're not dealing with the average Discord bot anymore. I have risen above, into the realm of legend.\n" +
				"I...AM A SUPER BOT!" +
				"\nType $help for a list commands.";

	// Arena Data
	public static Timer arenaTimer = new Timer();
	public static List<User> userList = new List<User>();
	public static List<ulong> userIDs = new List<ulong>();

	public static int arenaBotResetTimer; // 600 (10min)
	public static bool arenaBotFight = true;

	public static Dictionary<ulong, User> members = new Dictionary<ulong, User>();

	public static List<ShopItem> items = new List<ShopItem>() {
		new Data.ShopItem("Weapon", 0, 50, false, 0),
		new Data.ShopItem("Armor", 1, 50, false, 0),
		new Data.ShopItem("Health Potion", 2, 20, true, 0.3f),
	};
	/*
		new SlotResult("Marked", 433934233476661279),
		new SlotResult("RMXPol", 433934233078071296),
		new SlotResult("isaacsol", 445087987466895371),
		new SlotResult("Bob", 433934152656748544),
		new SlotResult("gaust", 433934215881424896)
	*/
	public static List<SlotResult> slotEmojis = new List<SlotResult>() {
		new SlotResult("Marked", 345037495060267018),
		new SlotResult("RMXPol", 342912767461687298),
		new SlotResult("isaacsol", 445009442828714011),
		new SlotResult("Bob", 345036765926522880),
		new SlotResult("gaust", 433466411373953034)
	};

	public static List<ulong> fighters = new List<ulong>() {};

	// Stuff
	[Serializable]
	public class User{

		public string Username;
		public string Nickname;
		public string Mention;
		public ulong Id;
		public string avatar;
		public string bio;

		public int chatLevel;
		public ulong chatexp;
		public ulong chatexpNext;
		public ulong totalMessages;

		// Arena stats
		public int level;
		public int exp;
		public int expNext;
		public int totalExp;

		public int hp;
		public int maxHP;
		public int strength;
		public int critical;
		public int secretStat;

		public int miningLevel;
		public int miningExp;
		public int miningExpNext;
		public int totalMiningExp;

		public int gold;

		public int baseHP;
		public int baseSTR;
		public int baseCRIT;

		public string status;
		public bool beenAttacked;
		public bool autoJoin;
		public ulong lastHit;

		public int arenaKills;
		public int arenaDeaths;

		public List<ShopItem> inventory = new List<ShopItem>();
		public List<Helper> targets = new List<Helper>();

		public int arenaActionTimer; // 3 sec
		public int arenaHealTimer; // 10 sec
		public int arenaLockTimer; // 60 (1min)
		public int arenaDeadTimer; // 60 (1min)
		public int arenaRemoveTimer; // 3600 (1hr)

		public User(SocketGuildUser user) {

			Console.WriteLine("Creating new user");
			this.Username = user.Username;
			this.Nickname = user.Nickname;
			this.Mention = "<@"+ user.Id.ToString() +">";
			this.Id = user.Id;
			Console.WriteLine("Username = "+ Username);
			Console.WriteLine("Nickname = "+ Nickname);
			Console.WriteLine("ID = "+ Id);

			this.avatar = user.GetAvatarUrl();
			this.bio = "This user hasn't set a bio. They can set a bio by using the $me command.";

			level = 1;
			expNext = 5;
		}

		public User(string Username, string Nickname, ulong Id, string avatar, string bio, int level, int baseHP, int baseSTR, int baseCRIT) {
			Console.WriteLine("Setting bot stats");
			Console.WriteLine("Username = "+ Username);
			this.Username = Username;
			Console.WriteLine("Nickname = "+ Nickname);
			this.Nickname = Nickname;
			this.Id = Id;
			this.Mention = "<@"+ this.Id.ToString() +">";
			this.avatar = avatar;
			this.bio = bio;
			this.level = level;
			this.baseHP = baseHP;
			this.baseSTR = baseSTR;
			this.baseCRIT = baseCRIT;
			this.inventory = Data.items;

			maxHP = (int)Math.Pow((baseHP + (level * 15) + 12), 1.15f); // 200 base for GDU Bot
			hp = maxHP;

			strength = (int)(baseSTR + (level * 1.8f)); // 25 base for GDU Bot
			critical = (int)(baseCRIT + (level + 2)); // 5 base for GDU Bot

			status = "none";
		}

		public Task UpdateSocket() {

			try{

				if(client.GetGuild(currentServer).Users.Contains(GetGuildUser(Id))){
					SocketGuildUser user = GetGuildUser(Id);

					this.Username = user.Username;
					this.Nickname = user.Nickname;
					this.Mention = "<@"+ this.Id.ToString() +">";
					this.avatar = user.GetAvatarUrl();
			
					if(Nickname == null || Nickname == "") {
						Nickname = Username;
					}

				}
			}catch(Exception e) {

				Console.WriteLine("User is not in the server.");
			}

			return Task.CompletedTask;
		}

		public Task UpdateStats() {

			if(Id != Data.botID){

				// User Stats
				char[] idchar = Id.ToString().ToCharArray();
				int[] id = new int[Id.ToString().ToCharArray().Length];

				for(int i = 0; i < id.Length; i++) {
					id[i] = Int32.Parse(idchar[i].ToString());
				}

				int lastDigit = Id.ToString().ToCharArray().Length - 1;
				int lastDigit2nd = Id.ToString().ToCharArray().Length - 2;
				int twelveDigit = Id.ToString().ToCharArray().Length - 6;

				baseHP = 35 + (id[lastDigit] / 2);
				int hpRange = (baseHP - 35);

				baseSTR = 10 + (hpRange - (id[lastDigit2nd] / 2));
				baseCRIT = 1 + (id[twelveDigit] / 2);

				maxHP = (int)(baseHP + (level * 1.2f));
			
				strength = (int)(baseSTR + (level * 0.75f));
				critical = (int)(baseCRIT + (level / 4));

				expNext = (int)((level * 5) * 1.1f);
				this.chatexpNext = (ulong)(15 + chatLevel * 3.4f);
				miningExpNext = (int)(20 + (miningLevel * 5));
					
			} else {

				// Bot Stats
				maxHP = (int)Math.Pow((baseHP + (level * 15) + 12), 1.15f); // 200 base for GDU Bot
				hp = maxHP;

				strength = (int)(baseSTR + (level * 1.8f)); // 25 base for GDU Bot
				critical = (int)(baseCRIT + (level + 2)); // 5 base for GDU Bot

				this.chatexpNext = (ulong)(15 + chatLevel * 3.4f);
			}

			status = "none";

			return Task.CompletedTask;
		}

		public Task InitializeInventory() {
			inventory = items;
			return Task.CompletedTask;
		}

		public Task EnterArena() {
			status = "none";
			beenAttacked = false;
			ResetArenaRemoveTimer();

			if(hp < maxHP) {
				arenaLockTimer = 60;
			}

			if(!InArena(Id)) {
				fighters.Add(Id);
				client.GetGuild(currentServer).GetTextChannel(arenaID).SendMessageAsync(Username +" has entered the arena! Type \"$arena help\" for more info.");
			}

			SaveFiles();
			return Task.CompletedTask;
		}

		public Task LeaveArena() {
			status = "none";
			beenAttacked = false;
			arenaRemoveTimer = 0;

			if(hp <= 0) {
				hp = maxHP;
			}

			if(InArena(Id)) {
				fighters.Remove(Id);
				client.GetGuild(currentServer).GetTextChannel(arenaID).SendMessageAsync(Username +" has left the arena. Type \"$arena help\" for more info.");
			}

			SaveFiles();
			return Task.CompletedTask;
		}

		public Task ResetArenaRemoveTimer() {
			arenaRemoveTimer = 3600;
			return Task.CompletedTask;
		}

		public bool AddArenaExp(int amount) {
			exp += amount;
			totalExp = amount;

			if(exp >= expNext) {
				return true;
			}

			return false;
		}

		public string ArenaLevelUp() {
			UpdateStats();
			int oldHP = maxHP;
			int oldSTR = strength;
			int oldCRIT = critical;

			while(exp >= expNext) {
				level += 1;
				exp -= expNext;
				UpdateStats();
			}

			UpdateStats();

			return 
				"**"+ Username +"** has leveled up and is now level "+ level +"!\n"+
				"**Max HP:** "+ oldHP +" => "+ maxHP +"\n"+
				"**Strength:** "+ oldSTR +" => "+ strength +"\n"+
				"**Crit Rate:** "+ oldCRIT +" => "+ critical +"\n";
		}

		public string BotArenaLevelUp() {			
			level += 10;

			UpdateStats();

			return 
				"**"+ Username +"** has leveled up and is now level "+ level +"!\n";
		}

		public Task SetArenaLevel(int level) {
			this.level = level;
			UpdateStats();

			return Task.CompletedTask;
		}

		public bool AddChatExp(ulong amount, bool displayMessages = true) {
			chatexp += amount;
			totalMessages += amount;

			if(Id == botID) {
				UpdateSocket();
				UpdateStats();
			}

			if(chatexp >= chatexpNext) {
				ChatLevelUp(displayMessages);
				return true;
			}

			return false;
		}
	
		public Task ChatLevelUp(bool displayMessages) {
			
			while(chatexp >= chatexpNext) {
				chatexp -= chatexpNext;
				chatLevel++;
				UpdateStats();
			}

			if(displayMessages) {
				client.GetGuild(currentServer).GetTextChannel(generalID).SendMessageAsync(
					Username + "'s chat level has increased to level "+ chatLevel +"!");
			}
			return Task.CompletedTask;
		}

		public bool AddMiningExp(int amount) {
			if(miningLevel < 1) {
				miningLevel = 1;
				UpdateStats();
			}

			miningExp += amount;
			totalMiningExp += amount;

			if(miningExp >= miningExpNext) {
				MiningLevelUp();
				return true;
			}

			return false;
		}

		public Task MiningLevelUp() {

			while(miningExp >= miningExpNext) {
				miningExp -= miningExpNext;
				miningLevel++;
				UpdateStats();
			}

			client.GetGuild(currentServer).GetTextChannel(minesID).SendMessageAsync(
				Username + "'s mining level has increased to level "+ miningLevel +"!");

			return Task.CompletedTask;
		}

		public Data.Helper GetNewTarget() {
			return targets[targets.Count-1];
		}

		public Data.Helper GetFirstTarget() {
			return targets.OrderByDescending(helper => helper.damage).ToList()[0];
		}

		public Task RemoveFirstTarget() {
			targets.Remove(GetFirstTarget());
			return Task.CompletedTask;
		}

		public bool HasTarget(ulong Id) {
			
			foreach(Helper target in targets) {
				if(target.Id == Id) {
					return true;
				}
			}

			return false;
		}

		public User(){}

		public override string ToString() {
			return this.Username;
		}
	}

	[Serializable]
	public class ShopItem {
		public string name;
		public int index;
		public int quantity;
		public int basePrice;
		public bool useable;
		public float hpHealed;

		public ShopItem(string name, int index, int basePrice, bool useable, float hpHealed) {
			this.name = name;
			this.index = index;
			this.basePrice = basePrice;
			this.useable = useable;
			this.hpHealed = hpHealed;
		}

		public int GetPrice(ulong userId) {
			return (int)(basePrice + (35 * members[userId].inventory[index].quantity) + (Data.members[userId].level * 2.1f));
		}

		public int Use(ulong user) {
			Data.members[user].hp += (int)(Data.members[user].hp * (1 + hpHealed));
			
			if(Data.members[user].hp > Data.members[user].maxHP) {
				Data.members[user].hp = Data.members[user].maxHP;
			}

			return (int)(Data.members[user].hp * hpHealed);
		}

		public ShopItem(){
			name = "";
		}
	}

	// Used for checking who did the most damage to a user in the arena.
	[Serializable]
	public class Helper {
		public ulong Id;
		public int damage;
		public int healing;

		public Helper(ulong Id, int damage, int healing) {
			this.Id = Id;
			this.damage = damage;
			this.healing = healing;
		}

		public Helper() {}
	}

	// For the slots
	public class SlotResult {
		public string name;
		public ulong Id;
		public string emoji; // <:name:Id>

		public SlotResult(string name, ulong Id) {
			this.name = name;
			this.Id = Id;
			this.emoji = "<:"+ name +":"+ Id +">";
		}

		public override string ToString() {
			return emoji;
		}
	}

	public class Slot {
		public List<SlotResult> results = new List<SlotResult>();
		int rng;

		public Slot(int seed) {
			this.rng = new Random(seed).Next(10000);

			int[] slotrng = new int[] {
				this.rng % 100,
				(this.rng % 10000) / 100,
				this.rng / 1000,
			};
	
			foreach(int rng in slotrng) {
				
				if(rng < 20) {
					results.Add(slotEmojis[0]);
				}
				if(rng < 40) {
					results.Add(slotEmojis[1]);
				}
				if(rng < 60) {
					results.Add(slotEmojis[2]);
				}
				if(rng < 80) {
					results.Add(slotEmojis[3]);
				}
				if(rng <= 99) {
					results.Add(slotEmojis[4]);
				}
			}

		}
	}

	public class Spin {
		public Slot left;
		public Slot middle;
		public Slot right;

		public Spin(int seed) {
			left = new Slot(seed);
			middle = new Slot(seed * 12);
			right = new Slot(seed / 12);
		}

		public override string ToString() {
			return 
				left.results[0].emoji +" "+ middle.results[0].emoji +" "+ right.results[0].emoji +"\n"+
				left.results[1].emoji +" "+ middle.results[1].emoji +" "+ right.results[1].emoji +"\n"+
				left.results[2].emoji +" "+ middle.results[2].emoji +" "+ right.results[2].emoji +"\n";
		}
	}

	// IO
	// Read
	public static T XMLRead<T>(string filePath) where T : new() {
		TextReader reader = null;

		try {
			var serializer = new XmlSerializer(typeof(T));
			reader = new StreamReader(filePath);
			return (T)serializer.Deserialize(reader);
		}

		finally {
			if (reader != null)
				reader.Close();
		}
	}

	// Write
	public static void XMLWrite<T>(string filePath, T objectToWrite, bool append = false) where T : new() {
		TextWriter writer = null;

		try {
			var serializer = new XmlSerializer(typeof(T));
			writer = new StreamWriter(filePath, append);
			serializer.Serialize(writer, objectToWrite);
		}

		finally {
			if (writer != null)
				writer.Close();
		}
	}

	public static void SaveFiles() {

		try {
			Data.XMLWrite(userData, members.Values.ToList());
			Data.XMLWrite(arenaList, fighters);
		}catch(Exception e) {
		}

	}

	public static Task BackUpFiles() {
		Console.WriteLine("Backing up files...");

		if(!Directory.Exists(backUpDirectory+"/xml")) {
			Directory.CreateDirectory(backUpDirectory+"/xml");
		}

		Data.XMLWrite(backUpDirectory +"/"+ Data.userData, Data.members.Values.ToList());
		Data.XMLWrite(backUpDirectory +"/"+ Data.arenaList, Data.fighters);

		Console.WriteLine("Files backed up");
		return Task.CompletedTask;
	}

	public static void AddNewUser(SocketGuildUser user) {
		User newUser = new User(user);	

		newUser.UpdateSocket();
		newUser.UpdateStats();
		newUser.hp = newUser.maxHP;
		newUser.InitializeInventory();

		Data.userList.Add(newUser);
	}

	public static void UpdateMembers() {
		Data.XMLWrite(Data.userData, Data.members.Values.ToList());
	}

	public static void LoadMembers() {

		foreach(User user in Data.userList) {

			try {
				Data.members.Add(user.Id, user);
			}catch(Exception e) {
			}
				
		}

	}

	public static SocketGuildUser GetGuildUser(SocketUser user) {
		return client.GetGuild(currentServer).GetUser(user.Id);
	} 

	public static SocketGuildUser GetGuildUser(ulong userId) {
		return client.GetGuild(currentServer).GetUser(userId);
	}

	public static ulong GetMemberId(string name) {
		ulong id = 0;

		foreach(User member in members.Values) {

			if(member.Nickname.Equals(null) || member.Nickname.Equals("") || member.Nickname == null) {
				member.Nickname = member.Username;
			}

			if(member.Username.Equals(name) || member.Nickname.Equals(name) || member.Id.ToString().Equals(name) || name.Equals("<@"+ member.Id.ToString() +">")) {
				return member.Id;
			}
		}

		return id;
	}

	public static bool InArena(ulong id) {

		foreach(ulong user in Data.fighters) {
			if(user == id) {
				return true;
			}
		}

		return false;
	}

	public static Task MassLevelUp(List<ulong> users) {
		string message = "";

		foreach(ulong user in users) {
			message += Data.members[user].ArenaLevelUp() +"\n";
		}

		client.GetGuild(currentServer).GetTextChannel(arenaID).SendMessageAsync(message);

		return Task.CompletedTask;
	}
}
	