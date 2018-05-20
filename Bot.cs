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

public class Bot {
	public static void Main(string[] args)
		=> new Bot().MainAsync().GetAwaiter().GetResult();

	// Main
	public async Task MainAsync() {
		// Get Token Files
		FileInfo tokenFile = new FileInfo(@"txt/tokens.txt");
		string[] tokens = new string[2];

		for(int i = 0; i < tokens.Length; i++) {
			tokens[i] = tokenFile.OpenText().ReadLine();
		}
	
		Data.mainToken = tokens[0];
		Data.devToken = tokens[1];
		Data.token = Data.mainToken; // Change this to switch servers. Everything else (channel and server IDs) should change accordingly.

		Data.client = new DiscordSocketClient();
		Data.cmdService = new CommandService();
		Data.services = new ServiceCollection().BuildServiceProvider();

		Data.client.Log += Log;
		Data.client.MessageReceived += MessageReceived;
		Data.client.UserJoined += UserJoined;
		Data.client.UserLeft += UserLeft;
		Data.client.Ready += Ready;
		await BotLog("GDU Bot version "+ Data.version);

		string token = Data.token;

		await GetServerID();
		await InstallCommands();

		await Data.client.LoginAsync(TokenType.Bot, token);
		await Data.client.StartAsync();

		await Task.Delay(-1);
	}

	// Ready (On Startup)
	public async Task Ready() {

		// Load and create member files and stats
		await CheckDirectory();
		await LoadArenaFile();
		Data.arenaBotFight = false;

		Data.arenaTimer.Elapsed += new ElapsedEventHandler(ArenaTimerHandler);
		Data.arenaTimer.Interval = 1000; // 1 second
		Data.arenaTimer.Enabled = true;

		Data.backUpTimer.Elapsed += new ElapsedEventHandler(BackUpTimerHandler);
		Data.backUpTimer.Interval = 3600000; // 1 hour : 3600000
		Data.backUpTimer.Enabled = true;

		await LoadUserFile();

		await BotLog("Checking Bot Stats...");
		if(!Data.members.ContainsKey(Data.botID)) {
			SocketGuildUser GDUBot = Data.client.GetGuild(Data.currentServer).GetUser(Data.botID);
			Data.members.Add(Data.botID, new Data.User(GDUBot.Username, "GDU Bot", Data.botID, GDUBot.GetAvatarUrl(), Data.botBio, 10, 200, 25, 5));
		}

		await BotLog("Checking Users...");
		foreach(SocketGuildUser user in Data.client.GetGuild(Data.currentServer).Users) {
			try{
				if(!Data.members.ContainsKey(user.Id) && !user.IsBot){
					Data.AddNewUser(user);
					Data.members[user.Id].hp = Data.members[user.Id].maxHP;
				}
			} catch(Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		Data.LoadMembers();
		Data.SaveFiles();

		if(!Data.InArena(Data.botID)) {
			Data.fighters.Add(Data.botID);
			Data.XMLWrite(Data.arenaList, Data.fighters);
		}
		
		Data.SaveFiles();

		Data.autoBackUpTimer = Data.autoBackUpInterval;
	}

	// On new user join
	public Task UserJoined(SocketUser user) {
		BotLog("New user joined.");

		if(Data.members[user.Id] == null) {

			BotLog("A new user! I must greet them.");
			Data.members.Add(user.Id, new Data.User(Data.GetGuildUser(user)));
			Data.members[user.Id].UpdateSocket();
			Data.members[user.Id].UpdateStats();
			Data.client.GetGuild(Data.currentServer).GetTextChannel(Data.GDUGeneralID).SendMessageAsync(
				"Welcome to the official Game Dev Unlimited Discord server, "+ user.Mention +"!");
		} else {

			BotLog("A returning user! I must greet them.");
			Data.members[user.Id].UpdateSocket();
			Data.members[user.Id].UpdateStats();
			Data.client.GetGuild(Data.currentServer).GetTextChannel(Data.generalID).SendMessageAsync(
				"Welcome back to the official Game Dev Unlimited Discord server, "+ user.Mention +"!");
		}

		Data.XMLWrite(Data.userData, Data.members.Values.ToList());

		return Task.CompletedTask;
	}

	public Task UserLeft(SocketUser user) {

		Data.client.GetGuild(Data.currentServer).GetTextChannel(Data.generalID).SendMessageAsync(user.Username +" has left the server.");
		return Task.CompletedTask;
	}

	// Debug Log
	private Task Log(LogMessage msg) {

		Console.WriteLine(msg.ToString());
		return Task.CompletedTask;
	}
	private Task BotLog(string msg) {

		Console.WriteLine(new LogMessage(LogSeverity.Info, "Bot", msg));
		return Task.CompletedTask;
	}

	// Get Server ID
	public async Task GetServerID() {

		if(Data.token == Data.mainToken) {
			await BotLog("Connecting to GDU Server");
			Data.currentServer = Data.GDUID;
			Data.generalID = Data.GDUGeneralID;
			Data.arenaID = Data.GDUArenaID;
			Data.slotsID = Data.GDUSlotsID;
			Data.shopID = Data.GDUShopID;
			Data.botID = Data.GDUBotID;
		}else

		if(Data.token == Data.devToken) {
			await BotLog("Connecting to Botworks");
			Data.currentServer = Data.botworksID;
			Data.generalID = Data.BotworksGeneralID;
			Data.arenaID = Data.BotworksGeneralID;
			Data.slotsID = Data.BotworksGeneralID;
			Data.shopID = Data.BotworksGeneralID;
			Data.botID = Data.devBotID;
		}

	}

	// Install commands
	public async Task InstallCommands() {

		Data.client.MessageReceived += HandleCommand;
		await Data.cmdService.AddModulesAsync(Assembly.GetEntryAssembly());

		// Build help entries string
		foreach(CommandInfo cmd in Data.cmdService.Commands) {
			string aliases = "";
			string parameters = "";

			// Build alias list
			foreach(string alias in cmd.Aliases) {
				aliases += "`$"+ alias +"`  ";
			}

			// Build parameters list
			foreach(Discord.Commands.ParameterInfo param in cmd.Parameters) {
				parameters += "`<"+ param.Type +" "+ param.Name +">`  ";
			}

			Data.helpEntries.Add("$"+ cmd.Name +" help",
				"Aliases: "+ aliases +"\n"+
				"Parameters: "+ parameters +"\n"+
				cmd.Summary
				);

		}

	}
	public async Task HandleCommand(SocketMessage msg) {

		// Ignore bot commands
		//if(!msg.Author.IsBot) {

			if(Data.members[msg.Author.Id].hp > Data.members[msg.Author.Id].maxHP) {
				Data.members[msg.Author.Id].hp = Data.members[msg.Author.Id].maxHP;
			}

			// Don't process the command if it was a System Message
			var message = msg as SocketUserMessage;
			if (message == null) return;
			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message is a command, based on if it starts with '!' or a mention prefix
			if (!(message.HasCharPrefix('$', ref argPos) || message.HasMentionPrefix(Data.client.CurrentUser, ref argPos))) return;
			// Create a Command Context
			var context = new CommandContext(Data.client, message);

			// Get help information if user asks for help
			if(context.Message.Content.EndsWith(" help")) {
				await context.Channel.SendMessageAsync(Data.helpEntries[context.Message.Content]);

			} else {

				// Execute the command. (result does not indicate a return value, 
				// rather an object stating if the command executed successfully)
				var result = await Data.cmdService.ExecuteAsync(context, argPos, Data.services);

				if (!result.IsSuccess) {
					await context.Channel.SendMessageAsync(result.ErrorReason +"\nTry adding \'help\' to the command for more info.");
				}
			}

		//}

	}

	// Load Files
	public Task CheckDirectory() {
		BotLog("Reading XML Files...");

		if(!Directory.Exists(@"xml")) {
			Directory.CreateDirectory(@"xml");
		}

		return Task.CompletedTask;
	}

	public Task LoadUserFile() {

		// Create or load member file
		if(File.Exists(Data.userData)) {

			BotLog("Loading Member File: "+ Data.userData);
			
			Data.userList = Data.XMLRead<List<Data.User>>(Data.userData);
			Data.LoadMembers();
		} else {

			BotLog(Data.userData +" not found. Creating new file.");
			Data.XMLWrite(Data.userData, Data.members.Values.ToList());
		}

		return Task.CompletedTask;
	}

	public Task LoadArenaFile() {

		// Create or load arena list
		if(File.Exists(Data.arenaList)) {

			BotLog("Loading Arena List File: "+ Data.arenaList);
			Data.fighters = Data.XMLRead<List<ulong>>(Data.arenaList);
		} else {

			BotLog(Data.arenaList +" not found. Creating new file.");
			Data.XMLWrite(Data.arenaList, Data.fighters);
		}

		return Task.CompletedTask;
	}

	// Messages
	public async Task MessageReceived(SocketMessage msg) {

		if(Data.members[msg.Author.Id].hp > Data.members[msg.Author.Id].maxHP) {
			Data.members[msg.Author.Id].hp = Data.members[msg.Author.Id].maxHP;
		}

		if(!msg.Author.IsBot){

			if(!msg.Content.StartsWith("$")) {

				bool thingsChanged = false;

				if(Data.members[msg.Author.Id] == null) {
					await BotLog("New user!");
					Data.members.Add(msg.Author.Id, new Data.User(Data.GetGuildUser(msg.Author)));
					thingsChanged = true;
				}

				if(msg.Channel.Id != Data.backRoomID && msg.Channel.Id != Data.arenaID && msg.Channel.Id != Data.slotsID && msg.Channel.Id != Data.shopID) {
					Data.members[msg.Author.Id].chatexp += 1;
					Data.members[msg.Author.Id].totalMessages += 1;
					Data.members[Data.botID].totalMessages += 1;

					Data.members[msg.Author.Id].AddChatExp(1);
					Data.members[Data.botID].AddChatExp(1);
					thingsChanged = true;
				}

				// Auto join arena
				if(msg.Channel.Id == Data.arenaID) {

					if(Data.members[msg.Author.Id].autoJoin) {

						if(!Data.InArena(msg.Author.Id) && Data.members[msg.Author.Id].arenaLockTimer == 0) {

							await Data.members[msg.Author.Id].EnterArena();
							Data.XMLWrite(Data.arenaList, Data.fighters);

							await msg.Channel.SendMessageAsync(Data.members[msg.Author.Id].Username +" joined the arena!\n"+
							"Type \"$arena\" help to learn how to play."
							);
						}
					}
				}

				if(thingsChanged) {
					Data.XMLWrite(Data.userData, Data.members.Values.ToList());
				}
			}
		}
	}
	
	// Arena timer
	private static void ArenaTimerHandler(object source, ElapsedEventArgs e) {

		foreach(ulong user in Data.fighters) {

			if(user > 200) {
			
				if(Data.members[user].arenaActionTimer > 0) {
					Data.members[user].arenaActionTimer -= 1;
				}

				if(Data.members[user].arenaHealTimer > 0) {
					Data.members[user].arenaHealTimer -= 1;
				}

				if(Data.members[user].arenaLockTimer > 0){
					Data.members[user].arenaLockTimer -= 1;
				}

				if(Data.members[user].arenaRemoveTimer > 0) {
					Data.members[user].arenaRemoveTimer -= 1;
				}

				if(Data.members[user].arenaDeadTimer > 0){
					Data.members[user].arenaDeadTimer -= 1;
				}

				if(Data.members[user].arenaRemoveTimer <= 0 && !Data.client.GetUser(user).IsBot) {
					Data.members[user].LeaveArena();
				}

				if(Data.arenaBotResetTimer > 0) {
					Data.arenaBotResetTimer -= 1;
				}

				if(Data.arenaBotResetTimer == 0 && Data.arenaBotFight) {
					Data.arenaBotFight = false;
					Data.client.GetGuild(Data.currentServer).GetTextChannel(Data.arenaID).SendMessageAsync(
						"Your 10 minutes to defeat me are up.");
					Data.members[Data.botID].hp = Data.members[Data.botID].maxHP;
					Data.members[Data.botID].targets.Clear();
					Data.SaveFiles();
				}
			}

		}
	}

	private static void BackUpTimerHandler(object source, ElapsedEventArgs e) {

		if(Data.autoBackUpTimer > 0) {
			Data.autoBackUpTimer -= 1;
		}

		if(Data.autoBackUpTimer <= 0) {
			Data.autoBackUpTimer = Data.autoBackUpInterval;
			Data.SaveFiles();
			Data.BackUpFiles();
		}
	}
}