using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

public class Commander : ModuleBase<CommandContext> {

	// help
	[Command("help"), Alias("commands"), Summary("Displays a list of commands and their aliases.")]
	public async Task Help(string help = "commands") {
		string message = "";

		if(help.Equals("commands") || help.Equals("2")) {
			int pageStart;
			int pageEnd;

			if(help.Equals("2")) {
				pageStart = 11;
				pageEnd = Data.cmdService.Commands.ToList().Count() - hiddenCommands;
				message += "**Commands (Page 2/2):** \n";
			} else {
				pageStart = 0;
				pageEnd = 10;
				message += "**Commands (Page 1/2):** \n";
			}

			for(int i = pageStart; i < pageEnd; i++) {
				message += "`$"+ Data.cmdService.Commands.ToList()[i].Aliases[0];
				
				if(Data.cmdService.Commands.ToList()[i].Aliases.Count() > 1) {

					foreach(string alias in Data.cmdService.Commands.ToList()[i].Aliases) {

						if(!alias.Equals(Data.cmdService.Commands.ToList()[i].Name)) {
							message += " $"+ alias;
						}
					}
				}

				message += "`\n";
			}

			message += "\nAdd \"help\" to the end of one of these for more info."+
		"\n**NOTE: Please don't spam commands. The bot can get backed up easily and there is currently no easy fix to this, so chill.\n"+
		"You may even need to wait a few seconds for a command to register.**\n";
		}

		if(help.Equals("help") || help.Equals("me") || help.ToLower().Equals("i need somebody")) {

			message = "Help is on the way! :ambulance: \n https://www.youtube.com/watch?v=ZNahS3OHPwA";
		}

		await ReplyAsync(message);
	}

	// Math
	[Command("math"), Alias("solve"), Summary("Solves one simple mathmatical expression, like 4 + 4 for example\n"+
		"Certain words and phrases (in quotes) also work, like plus, minus, \"divided by\", etc.\n"+
		"Can add, subtract, divide, multiply, find remainders, and do exponents.")]
	public async Task Math(float operand1 = 1, string @operator ="+", float operand2 = 1) {
		float sollution = 0;

		// Add
		if(@operator == "+" || @operator.ToLower() == "plus" || @operator.ToLower() == "add") {
			sollution = operand1 + operand2;
			await ReplyAsync(operand1 +" + "+ operand2 +" = "+ sollution);
		}else

		// Subtract
		if(@operator == "-" || @operator.ToLower() == "minus" || @operator.ToLower() == "subtract") {
			sollution = operand1 - operand2;
			await ReplyAsync(operand1 +" - "+ operand2 +" = "+ sollution);
		}else

		// Multiply
		if(@operator == "x" || @operator == "*" || @operator.ToLower() == "times" || @operator.ToLower() == "multiply") {
			sollution = operand1 * operand2;
			await ReplyAsync(operand1 +" x "+ operand2 +" = "+ sollution);
		}else

		// Divide
		if(@operator == "/" || @operator.ToLower() == "divided by" || @operator.ToLower() == "divide") {
			sollution = operand1 / operand2;
			await ReplyAsync(operand1 +" / "+ operand2 +" = "+ sollution);
		}else

		// Remainder
		if(@operator == "%" || @operator.ToLower() == "mod" || @operator.ToLower() == "remainder") {
			sollution = operand1 % operand2;
			await ReplyAsync("The remainder of "+ operand1 +" / "+ operand2 +" is "+ sollution);
		}else
		
		// Exponent
		if(@operator == "^" || @operator.ToLower() == "to the power of" || @operator.ToLower() == "exponent") {
			sollution = (float)System.Math.Pow(operand1, operand2);
			await ReplyAsync(operand1 +" ^"+ operand2 +" = "+ sollution);
		}

	}

	// Tobinary
	[Command("tobinary"), Alias("binary"), Summary("Converts the specified integer to binary"), Remarks("")]
	public async Task ToBinary(int number = 8) {
		string result = "";
		int index = 0;

		int dividen = number;

		// Convert to backwards binary
		while(dividen > 0) {
			result += dividen % 2;
			dividen /= 2;

			// Add a space every 8th bit
			index += 1;
			if(index == 8){
				index = 0;
				result += " ";
			}
		}
		
		// Reverse the string.
		char[] charArray = result.ToCharArray();
		Array.Reverse(charArray);
		result = new string(charArray);

		await ReplyAsync(result);
	}

	// rng
	[Command("rng"), Alias("random"), Summary("Generate a random number between two specified integers.\n"+
		"Decimals don't work, but the order of the numbers doesn't matter.")]
	public async Task RNG(int num1 = 0, int num2 = 100) {
		Random rng = new Random();
		string output;

		if(num1 >= num2){
			output = rng.Next(num2, num1+1) +"";
		} else {	
			output = rng.Next(num1, num2+1) +"";
		}

		await ReplyAsync(output);
	}

	// roll
	[Command("roll"), Summary("Generate a random number between 1 and the specified integer.\n"+
		"Something might happen if you get a certain number.")]
	public async Task Roll(int number = 20) {

		Random rng = new Random();
		int output = rng.Next(1, number+1);

		await ReplyAsync(Context.User.Username +" rolled a **d"+ number +"**. It landed on **"+ output +"**");

		if(output == 20 && number == 20) {
			await ReplyAsync("**Critical!**");
		}
	
	}

	[Command("me"), Alias("moi"), Summary("Display your info. All parameters are optional")]
	public async Task Me(string action = "info") {
		string message = "";

		if(action.Equals("info")) {
			message = UserInfo(Context.User.Id);
		}

		await ReplyAsync(message);
	}

	[Command("profile"), Alias("user", "find", "whois", "member"), Summary("Display a user's info. Can use their username, nickname, or user ID.\n" +
		"Leave the parameter blank to display your own stats")]
	public async Task UserProfile([Remainder] string user = "me") {
		string message = "User not found.";

		ulong userId;

		if(!user.Equals("me")) {
			userId = Data.GetMemberId(user);
		} else {
			userId = Context.User.Id;
		}

		if(userId > 200) {
			await Data.members[userId].UpdateSocket();
			message = UserInfo(userId);
		}

		await ReplyAsync(message);
	}

	// top (10)
	[Command("top"), Alias("chatLevels"), Summary("Displays a list of users and their total messages")]
	public async Task UserMessages(int number = 10) {
		List<Data.User> users = Data.members.Values.OrderByDescending(user => user.totalMessages).ToList();
		users.RemoveAt(0);
		List<Data.User> UsernameCut = users.OrderByDescending(user => user.Username.Length).ToList();
		List<Data.User> NicknameCut = users.OrderByDescending(user => user.Nickname.Length).ToList();
		int numOfUsers = number;

		if(number > users.Count()) {
			numOfUsers = users.Count();
		}

		string message = "[Top "+ number +" Talkers]\n"+
			"Total Messages: "+ Data.members[Data.botID].totalMessages+"\n\n";

		for(int i = 0; i < numOfUsers; i++) {

			message += "[#"+ (i+1).ToString("0#") +"] "+ users[i].Username +
				AddSpace(UsernameCut[0].Username.Length, users[i].Username)+
				" / "+ users[i].Nickname +
				AddSpace(NicknameCut[0].Nickname.Length, users[i].Nickname)+
				" | Lv."+ string.Format("{0,2}", users[i].chatLevel) +" | "+ string.Format("{0,2}", users[i].totalMessages) +"\n";
			
		}

		await ReplyAsync("```"+ message +"```");
	}

	[Command("stats"), Alias("ArenaStats"), Summary("Display a user's arena stats. Can use their username, nickname, or user ID.\n" +
		"Leave the parameter blank to display your own stats")]
	public async Task Stats([Remainder] string user = "me") {
		string message = "User not found.";

		ulong userId;

		if(!user.Equals("me")) {
			userId = Data.GetMemberId(user);
		} else {
			userId = Context.User.Id;
		}

		if(userId > 200) {
			await Data.members[userId].UpdateStats();
			await Data.members[userId].UpdateSocket();
			message = ArenaStats(userId);
		}

		await ReplyAsync(message);
	}

	// levels
	[Command("levels"), Summary("Displays a list of users and their levels.")]
	public async Task userLevels(int number = 10) {
		List<Data.User> users = Data.members.Values.OrderByDescending(user => user.level).ToList();
		List<Data.User> UsernameCut = Data.members.Values.OrderByDescending(user => user.Username.Length).ToList();
		List<Data.User> NicknameCut = Data.members.Values.OrderByDescending(user => user.Nickname.Length).ToList();
		int numOfUsers = number;

		if(number > users.Count()) {
			numOfUsers = users.Count();
		}
		string message = "[Top "+ number +" user levels]\n\n";

		for(int i = 0; i < numOfUsers; i++) {
			message += "[#"+ (i + 1).ToString("0#") +"] "+ users[i].Username +
				AddSpace(UsernameCut[0].Username.Length, users[i].Username)+
				" / "+ users[i].Nickname +
				AddSpace(NicknameCut[0].Nickname.Length, users[i].Nickname)+
				" | "+ string.Format("Lvl {0,2}", users[i].level) +"\n";
		}

		await ReplyAsync("```"+ message +"```");
	}

	// arena
	[Command("arena"), Summary("Join or leave the arena.\n"+
		"You can also type \"view\" (or nothing) to view the users currently in the arena.\n"+
		"If you want to automatically join the arena when you type in there, use $arena autojoin on\n" +
		"Users will automatically be removed from the arena if they don't use any arena commands for 1 hour. ($arena does not count)\n\n" +
		"**Arena Commands:**\n"+
		"**$attack:** Attack the specified user if they are in the arena.\n" +
		"**$defend:** Cuts the damage you take from the next attack in half.\n" +
		"**$charge:** Prepares an attack, increasing its damage, but leaving you open for another hit.\n" +
		"**$potion:** Use a health potion to restore HP to yourself or another user. (30% of max HP. 15% if reviving)\n"+
		"\n**NOTE: Please don't spam commands. The bot can get backed up easily and there is currently no easy fix to this, so chill.\n"+
		"You may even need to wait a few seconds for a command to register. Especially the $attack command.**\n")]
	public async Task Arena(string option = "view", string param = "none") {
		// Spectate
		if(option.Equals("view")) {

			string message = "**Users currently in the arena**```\n\n";

			foreach(ulong fighter in Data.fighters) {

				message += Data.members[fighter].Username +" / "+ Data.members[fighter].Nickname +":\n"+
					"     Lvl. "+ Data.members[fighter].level +"\n"+
					"     HP: "+ Data.members[fighter].hp +"/"+ Data.members[fighter].maxHP+"\n\n";
				
			}
			
			await ReplyAsync(message +"```");

		}else

		// Join
		if(option.Equals("join")) {

			if(!Data.InArena(Context.User.Id)) {

				await Data.members[Context.User.Id].EnterArena();

			} else {
				await ReplyAsync("You're already in the arena.");
			}

		}else

		// Leave
		if(option.Equals("leave")) {

			if(Data.members[Context.User.Id].arenaLockTimer == 0) {

				if(Data.members[Context.User.Id].arenaDeadTimer == 0) {

					if(Data.InArena(Context.User.Id)) {

						await Data.members[Context.User.Id].LeaveArena();

					} else {
						await ReplyAsync("You are not in the arena.");
					}
				} else {
					await ReplyAsync("Please wait "+ Data.members[Context.User.Id].arenaDeadTimer +" more seconds before leaving the arena.");
				}
			} else {
				await ReplyAsync("Please wait "+ Data.members[Context.User.Id].arenaLockTimer +" more seconds before leaving the arena.");
			}
		}else

		// Auto-Join
		if(option.Equals("autojoin")) {
				
			if(param.Equals("on") || param.Equals("true")) {

				Data.members[Context.User.Id].autoJoin = true;
				await ReplyAsync("You will now automatically join the arena when you type in the arena channel.");
			}else

			if(param.Equals("off") || param.Equals("false")) {

				Data.members[Context.User.Id].autoJoin = false;
				await ReplyAsync("You will no longer automatically join the arena when you type in the arena channel.");
			}
		}
	}
	
	[Command("attack"), Alias("atk", "hit", "fight"), Summary("Arena only.\nAttack someone")]
	public async Task ArenaAttack([Remainder] string target = "none") {
		await Data.members[Context.User.Id].ResetArenaRemoveTimer();
		int seed = GenerateSeed();

		// If action is useable.
		if(IsValidArenaAction(Context.Message)) {
			
			// Get target Id
			ulong targetId;

			if(target.Equals("none")) {
				targetId = Data.members[Context.User.Id].lastHit;
			} else {
				targetId = Data.GetMemberId(target);
			}

			// If target is valid
			if(IsValidArenaTarget(Context.Message, targetId)) {

				Data.members[targetId].lastHit = Context.User.Id;
				
				string message = "";
				
				// Get Damage
				int damage = GetDamage(Context.User.Id, targetId, 0.1f);
				
				// Get Crit
				if(GetCrit(Data.members[Context.User.Id].critical + (Data.members[Context.User.Id].secretStat * 10))) {
					damage *= 2;
					message += "**Critical Hit!**\n";
				}

				// Apply and Display damage
				message += "**"+ Data.members[Context.User.Id].Username +"** attacked **"+ Data.members[targetId].Username +"**";

				Data.members[targetId].hp -= damage;
				message += " for **"+ damage +" damage!**"+
					"\nRemaining HP: "+ Data.members[targetId].hp +" / "+ Data.members[targetId].maxHP;

				// Add user to target's target list
				if(!Data.members[targetId].HasTarget(Context.User.Id)) {
					Data.members[targetId].targets.Add(new Data.Helper(Context.User.Id, damage, 0));
				} else {
					Data.members[targetId].GetNewTarget().damage += damage;
				}

				bool botDefeated = false;

				// If they ded
				if(Data.members[targetId].hp <= 0) {
					Data.members[targetId].hp = 0;

					// Get highest aggro target
					ulong firstTargetId = Data.members[targetId].GetFirstTarget().Id;

					message += "\n\n**"+ Data.members[targetId].Username +"** has been defeated!\n";
					int moneyDrop = (int)((Data.members[targetId].level * 5) + (Data.members[targetId].gold * 0.1f));

					// Give exp
					List<ulong> leveledHelpers = new List<ulong>();
					foreach(Data.Helper helper in Data.members[targetId].targets) {

						if(helper.Id != Data.botID) {
							int expDrop = Data.members[targetId].level;

							if(targetId == Data.botID) {
								expDrop *= (int)2.5f;
							}

							if(Data.members[helper.Id].AddArenaExp(expDrop)) {
								leveledHelpers.Add(helper.Id);
							}

							Data.members[helper.Id].gold += moneyDrop;

							message += Data.members[helper.Id].Username +" gained "+ expDrop +
								" EXP and "+ moneyDrop +" gold!\n";
						}
					}

					await Data.MassLevelUp(leveledHelpers);

					Data.members[targetId].beenAttacked = false;
					Data.members[targetId].status = "none";
					Data.members[targetId].arenaDeaths += 1;
					Data.members[targetId].arenaDeadTimer = 60;
					Data.members[Context.User.Id].arenaKills += 1;

					if(targetId == Data.botID) {
						message += "\n\n"+ Data.members[targetId].BotArenaLevelUp();
						Data.members[targetId].hp = Data.members[targetId].maxHP;
						botDefeated = true;
					}

					Data.members[targetId].targets.Clear();
					
					// Remove dead targets from target list.
					foreach(Data.Helper helpers in Data.members[Context.User.Id].targets) {

						if(helpers.Id == targetId) {
							Data.members[Context.User.Id].targets.Remove(helpers);
						}
					}
				}
		
				ResetTurn(Context.User.Id, targetId);

				Data.SaveFiles();
				await ReplyAsync(message);
				
				// Bot reacts if attacked 
				if(targetId == Data.botID && !botDefeated) {
					BotAction(Context.Message, seed);
				}
			}
		}
	}

	[Command("defend"), Alias("def", "guard"), Summary("Arena only.\nDefend against the next attack (half damage)")]
	public async Task ArenaDefend() {
		await Data.members[Context.User.Id].ResetArenaRemoveTimer();

		// If action is useable
		if(IsValidArenaAction(Context.Message)) {

			ResetTurn(Context.User.Id);
			Data.members[Context.User.Id].status = "defending";
			Data.SaveFiles();

			await ReplyAsync(Data.members[Context.User.Id].Username +" is defending.");

			// Bot reacts if you're a target.
			bool userIsTarget = false;

			foreach(Data.Helper helper in Data.members[Data.botID].targets) {
				if(Context.User.Id == helper.Id) {
					userIsTarget = true;
				}
			}

			if(userIsTarget) {
				BotAction(Context.Message, GenerateSeed());
			}
		}
	}

	[Command("charge"), Alias("chrg"), Summary("Arena only.\nInreases the damage your next attack will do, but opens you up for another hit.")]
	public async Task ArenaCharge() {
		await Data.members[Context.User.Id].ResetArenaRemoveTimer();

		// If action is useable
		if(IsValidArenaAction(Context.Message)) {

			ResetTurn(Context.User.Id);
			Data.members[Context.User.Id].status = "charging";
			Data.SaveFiles();

			await ReplyAsync(Data.members[Context.User.Id].Username +" is preparing an attack.");

			// Bot reacts if you're a target.
			bool userIsTarget = false;

			foreach(Data.Helper helper in Data.members[Data.botID].targets) {
				if(Context.User.Id == helper.Id) {
					userIsTarget = true;
				}
			}

			if(userIsTarget) {
				BotAction(Context.Message, GenerateSeed());
			}
		}
	}

	[Command("potion"), Alias("usepotion"), Summary("Arena only.\nuse a health potion on yourself or another user and restore 30% of your/their max HP\n" +
		"15% if reviving.")]
	public async Task ArenaPotion([Remainder] string target = "self") {
		await Data.members[Context.User.Id].ResetArenaRemoveTimer();

		// If action is useable
		if(IsValidArenaAction(Context.Message, true)) {

			if(Data.members[Context.User.Id].inventory[2].quantity > 0) {

					// Get target Id
					ulong targetId;

					if(target.Equals("self")) {
						targetId = Context.User.Id;
					} else {
						targetId = Data.GetMemberId(target);
					}

				if(IsValidArenaTarget(Context.Message, targetId, true)) {
					int healing = 0;
					int healed = 0;

					Data.members[Context.User.Id].inventory[2].quantity -= 1;
					Data.members[targetId].arenaDeadTimer = 0;

					if(Data.members[targetId].hp <= 0) {
						Data.members[targetId].hp = 0;
						healing = (int)(Data.members[targetId].maxHP * 0.15f);
					} else {
						healing = (int)(Data.members[targetId].maxHP * 0.30f);
					}

					if(healing + Data.members[targetId].hp >= Data.members[targetId].maxHP) {
						healing = Data.members[targetId].maxHP - Data.members[targetId].hp;
					}

					if(Data.members[targetId].hp >= Data.members[targetId].maxHP) {
						healing = 0;
					}

					Data.members[targetId].hp += healing;
					healed = healing;

					ResetTurn(Context.User.Id, targetId, true);
					Data.SaveFiles();

					await ReplyAsync("**"+ Data.members[Context.User.Id].Username +"** uses a healing potion on **"+ Data.members[targetId].Username +"**\n" +
						"**"+ Data.members[targetId].Username +"** is healed for **"+ healed +" HP**");

					// Bot reacts if you're a target.
					bool userIsTarget = false;

					foreach(Data.Helper helper in Data.members[Data.botID].targets) {
						if(Context.User.Id == helper.Id) {
							userIsTarget = true;
						}
					}

					if(userIsTarget) {
						BotAction(Context.Message, GenerateSeed());
					}
				}

			} else {
				await ReplyAsync("You're out of potions.");
			}

		}
	}

	// shop (buy, sell, view)
	[Command("shop"), Summary("buy or view arena equipment. Can only be used in the shop channel"+
		"\n**NOTE: Please don't spam commands. The bot can get backed up easily and there is currently no easy fix to this, so chill.\n"+
		"You may even need to wait a few seconds for a command to register.**\n")]
	public async Task Shop(string action = "view", int option = 0, int quantity = 1) {
		int singlePrice = Data.items[option].GetPrice(Context.User.Id);
		int totalPrice = singlePrice * quantity;

		if(Context.Channel.Id == Data.shopID) {

			if(!Data.InArena(Context.User.Id)) {

				// Look at the shop
				if(action.Equals("view")) {

					// Get user's inventory. If blank, initialize it.
					if(Data.members[Context.User.Id].inventory.Count() == 0) {
						await Data.members[Context.User.Id].InitializeInventory();
					}

					// Display shop
					string shop = "**Arena Shop**```\n"+
						Context.User.Username + " --- Type \"$shop buy x n\" to buy n of item x\n"+
						"Your Gold: "+ Data.members[Context.User.Id].gold +"G\n\n";

					for(int i = 0; i < Data.items.Count(); i++) {

						shop += "["+ i +"] "+ Data.items[i].name +" : "+ Data.items[i].GetPrice(Context.User.Id) +"G : " +
							Data.members[Context.User.Id].inventory[i].quantity +" in inventory\n";
					}

					await ReplyAsync(shop+"```");
				}

				// Buy stuff
				if(action.Equals("buy")) {

					// Check if user has money
					if(Data.members[Context.User.Id].gold > 0) {

						// Check if user has enough for what they want to buy
						if(Data.members[Context.User.Id].gold >= totalPrice) {

							// Subtract money
							Data.members[Context.User.Id].gold -= totalPrice;
							Data.members[Context.User.Id].inventory[option].quantity += quantity;

							Data.members[Data.botID].gold += totalPrice;

							// Save Bot and Users
							Data.XMLWrite(Data.userData, Data.members.Values.ToList());

							await ReplyAsync(Context.User.Username +" bought "+ quantity +" "+ Data.items[option].name +"(s) for "+
								 totalPrice +" Gold.");

						} else {
							await ReplyAsync("You don't have enough money for that.");
						}

					} else {
						await ReplyAsync("I don't accept \"please and thank you\" as currency.");
					}
				}
			
			} else {

				await ReplyAsync("You can't shop while in the arena. Type \"$arena leave\" to leave");
			}
		} else {

			await ReplyAsync("That command can only be used in the shop channel");
		}

	}
	
	// bet
	[Command("slots"), Alias("bet", "spin"), Summary("Bet gold in the slots channel. You can earn gold by fighting in the arena as well."+
		"\n**NOTE: Please don't spam commands. The bot can get backed up easily and there is currently no easy fix to this, so chill.\n"+
		"You may even need to wait a few seconds for a command to register.**\n")]
	public async Task Bet(int gold = 1) {
		Data.User user = Data.members[Context.User.Id];

		if(Context.Message.Channel.Id == Data.slotsID) {
			if(gold > 0) {
				if(gold <= Data.members[Context.User.Id].gold) {
					
					Data.Spin spin = new Data.Spin(new Random().Next(-100000000, 99999999));

					string message = user.Username +" bets **"+ gold +"G**\n\n"+
						spin +"\n";

					int winnings = 0;

					user.gold -= gold;
					Data.members[Data.botID].gold += gold;

					// If tops match
					if(spin.left.results[0].emoji.Equals(spin.middle.results[0].emoji) && (spin.left.results[0].emoji.Equals(spin.right.results[0].emoji))){
						winnings += 2;
					}

					// If left diagonals match
					if(spin.left.results[0].emoji.Equals(spin.middle.results[1].emoji) && (spin.left.results[0].emoji.Equals(spin.right.results[2].emoji))){
						winnings += 2;
					}

					// If left verticals match
					if(spin.left.results[0].emoji.Equals(spin.left.results[1].emoji) && (spin.left.results[0].emoji.Equals(spin.left.results[2].emoji))){
						winnings += 2;
					}

					// If middles match
					if(spin.left.results[1].emoji.Equals(spin.middle.results[1].emoji) && (spin.left.results[1].emoji.Equals(spin.right.results[1].emoji))){
						winnings += 2;
					}

					// If middle verticals match
					if(spin.middle.results[0].emoji.Equals(spin.middle.results[1].emoji) && (spin.middle.results[0].emoji.Equals(spin.middle.results[2].emoji))){
						winnings += 2;
					}

					// If right diagonals match
					if(spin.left.results[2].emoji.Equals(spin.middle.results[1].emoji) && (spin.left.results[2].emoji.Equals(spin.right.results[0].emoji))){
						winnings += 2;
					}

					// If right verticals match
					if(spin.right.results[0].emoji.Equals(spin.right.results[1].emoji) && (spin.right.results[0].emoji.Equals(spin.right.results[2].emoji))){
						winnings += 2;
					}

					// If bottoms match
					if(spin.left.results[2].emoji.Equals(spin.middle.results[2].emoji) && (spin.left.results[2].emoji.Equals(spin.right.results[2].emoji))){
						winnings += 2;
					}
					
					winnings *= gold;
					message += user +"'s winnings: **"+ winnings +"G** ("+ (winnings - gold) +"G)";
					user.gold += winnings;

					await ReplyAsync(message);

				} else {
					await ReplyAsync("You don't have that much money.");
				}
			} else {
				await ReplyAsync("No money, no slots");
			}
		} else {
			await ReplyAsync("That's only useable in the slots channel");
		}
	}

	[Command("mine"), Summary("Used for mining in the mines.")]
	public async Task Mine() {

		if(Context.Channel.Id == Data.minesID) {
			if(!Data.InArena(Context.User.Id)) {

				int rng = new Random().Next(0, 100);
				
				string message = "*ting ting...*\n"+
					"You found...\n";

				if(rng < 22 +(Data.members[Context.User.Id].secretStat * 4)) { // 25%
					int minGold = Data.members[Context.User.Id].miningLevel + Data.members[Context.User.Id].level;
					int maxGold = 25 + Data.members[Context.User.Id].miningLevel + Data.members[Context.User.Id].level;

					int goldFound = new Random(GenerateSeed() + (int)(Context.User.Id / 1000)).Next(minGold + Data.members[Context.User.Id].secretStat, maxGold);

					message += goldFound +" gold!";
					Data.members[Context.User.Id].gold += goldFound;
					Data.members[Context.User.Id].AddMiningExp(1);
				} else {

					message += "Nothing :frowning:";
				}
				Data.members[Context.User.Id].AddMiningExp(1);

				await ReplyAsync(message);
				Data.SaveFiles();

			} else {
				await ReplyAsync("You can't mine while in the arena. Type \"$arena leave\" to leave");
			}
		} else {
			await ReplyAsync("You need to be in the mines channel to use that.");
		}
	}

	// Stop the fight!
	[Command("stopFight"), Summary("Cancel the fight with GDU Bot and reset his HP and target list.")]
	public async Task StopFight() {
		if(Data.arenaBotFight) {
			Data.arenaBotFight = false;
			Data.members[Data.botID].hp = Data.members[Data.botID].maxHP;
			Data.members[Data.botID].targets.Clear();
			Data.arenaBotResetTimer = 0;

			await ReplyAsync("Ok.");
		} else {

			await ReplyAsync("I'm not fighting anyone at the moment. You can change that by typing \"$attack GDU Bot\" while in the arena channel.");
		}	
	}

	[Command("version"), Summary("A link to GDU Bot 3.0's github page")]
	public async Task Version() {
		await ReplyAsync(Data.version);
	}

	[Command("github"), Alias("source", "sourceCode", "sauce"), Summary("A link to GDU Bot 3.0's github page")]
	public async Task GitHub() {
		await ReplyAsync("https://github.com/Bob423/GDU-Discord-Bot");
	}

	int hiddenCommands = 1;

	// say
	[Command("say"), RequireUserPermission(GuildPermission.Administrator)]
	public async Task Say([Remainder] string message) {
		await Context.Message.DeleteAsync();
		await ReplyAsync(message);
	}
	
	// Calculates base damage of an attack
	public int GetDamage(ulong attackerID, ulong targetID, float variance) {
		
		// Calculate base damage
		int damage = 0;
		int attack = Data.members[attackerID].strength + Data.members[attackerID].inventory[0].quantity;
		int defend = 0;

		// Prevent armor from mitigating more than 70% of incoming damage
		if(Data.members[targetID].inventory[1].quantity > (attack * 0.7f)) {
			defend = (int)(attack * 0.7f);
		} else {
			defend = Data.members[targetID].inventory[1].quantity;
		}

		damage = attack - defend;

		// Damage + 50% if attacker is charging
		if(Data.members[attackerID].status.Equals("charging")) {
			damage = (int)(damage * 1.5f);
		}

		// Damage halved if target is defending
		if(Data.members[targetID].status.Equals("defending")) {
			damage /= 2;
		}

		// Damage cannot be 0 or negative
		if(damage <= 0) {
			damage = 1;
		}

		damage += Data.members[attackerID].secretStat;
		damage -= Data.members[targetID].secretStat;

		// Damage variance
		int min = (int)(damage - (damage * variance));
		int max = (int)(damage + (damage * variance));
		
		if(min < 1) {
			min = 1;
		}

		return new Random().Next(min, max + 1);
	}

	// Decides if user attack is a crit
	public bool GetCrit(int userCrit) {
		float critRate = 0;
		
		// Calculate crit rate
		if(userCrit < 100) {
			critRate = 5 + (userCrit / 10.0f);
		} else {
			critRate = 15 + (userCrit / 50.0f);
		}

		int rng = new Random().Next(0, 100);

		return (rng <= critRate);
	}

	// Bot AI Reaction
	public void BotAction(IMessage msg, int seed) {
		ulong targetId = msg.Author.Id;

		// Start bot reset timer
		if(!Data.arenaBotFight) {
			Data.arenaBotFight = true;
			Data.arenaBotResetTimer = 600;
			Data.client.GetGuild(Data.currentServer).GetTextChannel(msg.Channel.Id).SendMessageAsync(
				"You have 10 minutes to defeat me! If you don't want to fight me, type $stopFight. Otherwise, I'll react to any arena commands used by " +
				"anyone in my target list.");
		}

		// Pick an action
		int botAction = 0;

		if(Data.members[Data.botID].status.Equals("charging")) {
			int botActiona = new Random(seed + 4435).Next(0, 2);
						
			if(botActiona == 0){
				botAction = 0;

			}else
			if(botAction == 1) {
				botAction = 3;
			}

		} else {
			botAction = new Random(seed - 36).Next(0, 4);
		}

		switch(botAction) {

			case 0: // Attack
				int botsTarget = new Random(seed * 12).Next(0, Data.members[Data.botID].targets.Count() - 1); // Pick a target

				Data.client.GetGuild(Data.currentServer).GetTextChannel(msg.Channel.Id).SendMessageAsync(
					"$attack "+ Data.members[Data.members[Data.botID].targets[botsTarget].Id].Username);
				break;

			case 1: // Defend
				Data.client.GetGuild(Data.currentServer).GetTextChannel(msg.Channel.Id).SendMessageAsync(
					"$defend");
				break;

			case 2: // Charge
				Data.client.GetGuild(Data.currentServer).GetTextChannel(msg.Channel.Id).SendMessageAsync(
					"$charge");
				break;

			case 3: // Multi-Attack
						
				List<Data.Helper> targetsToHit = new List<Data.Helper>();
							
				foreach(Data.Helper botTarget in Data.members[Data.botID].targets){

					if(new Random(seed + (int)(botTarget.Id / 5)).Next(0, 2) == 0 || Data.members[botTarget.Id].status.Equals("charging")) {

						if(!Data.members[botTarget.Id].beenAttacked) {

							targetsToHit.Add(botTarget);
						}
					}
				}

				foreach(Data.Helper botTarget in targetsToHit) {

					Data.client.GetGuild(Data.currentServer).GetTextChannel(msg.Channel.Id).SendMessageAsync(
						"$attack "+ Data.members[botTarget.Id].Username);
				}

				break;
		}
	}

	// Is arena action valid?
	public bool IsValidArenaAction(IMessage msg, bool healing = false) {
		SocketGuildUser user = Data.client.GetGuild(Data.currentServer).GetUser(msg.Author.Id);
		bool error = false;
		string reply = "**Invalid Arena Action.**\n";

		if(msg.Channel.Id != Data.arenaID) {
			reply += "```You must be in the arena channel to do that. ```\n";
			error = true;
		}

		if(!Data.InArena(user.Id)) {
			reply += "```You are not a fighter in the arena. Type \"$arena help\" for more info.```\n";
			error = true;
		}

		if(Data.members[user.Id].arenaLockTimer > 0) {
			reply += "```You must wait "+ Data.members[user.Id].arenaLockTimer +" more seconds before you can do that.```\n";
			error = true;
		}

		if(Data.members[user.Id].arenaDeadTimer > 0) {
			reply += "```You must wait "+ Data.members[user.Id].arenaDeadTimer +" more seconds before you can do that.```\n";
			error = true;
		}

		if(Data.members[user.Id].hp <= 0) {
			reply += "```You are already dead.``` https://i.imgur.com/UHAtlbQ.jpg \n";
			error = true;
		}

		if(!healing && Data.members[user.Id].arenaActionTimer > 0) {
			reply += "```You must wait "+ Data.members[user.Id].arenaActionTimer +" more seconds before you can take another action.```\n";
			error = true;
		}

		if(healing && Data.members[user.Id].arenaHealTimer > 0) {
			reply += "```You must wait "+ Data.members[user.Id].arenaHealTimer +" more seconds before you can do that again.```\n";
			error = true;
		}

		if(error) {

			if(!msg.Author.IsBot) {
				Data.client.GetGuild(Data.currentServer).GetTextChannel(msg.Channel.Id).SendMessageAsync(reply);
			} else {
				return true;
			}

			return false;
		}

		return true;
	}

	// Is arena target valid?
	public bool IsValidArenaTarget(IMessage msg, ulong targetId, bool healing = false) {
		SocketGuildUser user = Data.client.GetGuild(Data.currentServer).GetUser(msg.Author.Id);
		
		bool error = false;
		string reply = "**Invalid Arena Target.**\n";

		if(targetId != 0) {
			SocketGuildUser target = Data.client.GetGuild(Data.currentServer).GetUser(targetId);

			if(!Data.InArena(target.Id)) {
				reply += "```"+ target.Username +" is not a fighter in the arena. Type \"$arena help\" for more info.```\n";
				error = true;
			}
			
			if(user.Id == target.Id && healing == false) {
				reply += "```You can't attack yourself, silly.```\n";
				error = true;
			}

			if(Data.members[targetId].beenAttacked && healing == false) {
				reply += "```"+ target.Username +" must take an action before they can be attacked again.```";
				error = true;
			}

			if(Data.members[target.Id].hp <= 0 && healing == false) {
				reply += "```Stop! Stop! They're already dead!``` https://i.imgur.com/9K4GqXL.gif \n";
				error = true;
			}

		} else {
			reply += "User not found.";
			error = true;
		}

		if(error) {

			if(!msg.Author.IsBot) {
				Data.client.GetGuild(Data.currentServer).GetTextChannel(msg.Channel.Id).SendMessageAsync(reply);
			} else {
				return true;
			}

			return false;
		}

		return true;
	}

	// Reset fighter's turn
	public void ResetTurn(ulong userId, ulong targetId = 0, bool healing = false) {

		if(!healing) {
			if(Data.members[userId].status.Equals("charging")) {
				Data.members[userId].status = "none";
			}

			if(targetId != 0){

				if(Data.members[targetId].status.Equals("defending")) {
					Data.members[targetId].status = "none";
				}

				if(Data.members[targetId].hp > 0) {
					Data.members[targetId].beenAttacked = true;
				}
			}

			Data.members[userId].arenaActionTimer = 3;
		}

		if(healing){
			Data.members[userId].arenaHealTimer = 10;
		}

		Data.members[userId].beenAttacked = false;

		if(targetId == Data.botID) {
			Data.members[targetId].beenAttacked = false;
		}

		Data.SaveFiles();
	}

	// Display user info
	public string UserInfo(ulong id) {
		Data.members[id].UpdateStats();
		Data.members[id].UpdateSocket();

		if(Data.members[Context.User.Id].inventory.Count() == 0) {
			Data.members[Context.User.Id].InitializeInventory();
		}

		// For bot
		if(id == Data.botID) {
			
			return
				"**Username:** " + Data.members[id].Username + "\n" +
				"**Nickname:** " + Data.members[id].Nickname + "\n" +
				"**Chat Level:** "+ Data.members[id].chatLevel +"\n"+
				"**Chat EXP:** " + Data.members[id].chatexp + "\n" +
				"**Messages To Next Level:** " + Data.members[id].chatexpNext +" ("+ (Data.members[id].chatexpNext - Data.members[id].chatexp) +" more)\n"+
				"**Total Messages:** " + Data.members[id].totalMessages +"\n\n"+

				"**Bio:** " + "\n" +
				Data.members[id].bio + "\n" +
				Data.members[id].avatar;
			
		// For normal users
		} else {

			return
				"**Username:** " + Data.members[id].Username + "\n" +
				"**Nickname:** " + Data.members[id].Nickname + "\n" +
				"**Chat Level:** "+ Data.members[id].chatLevel +"\n"+
				"**Chat EXP:** " + Data.members[id].chatexp + "\n" +
				"**Messages To Next Level:** " + Data.members[id].chatexpNext +" ("+ (Data.members[id].chatexpNext - Data.members[id].chatexp) +" more)\n"+
				"**Messages sent:** " + Data.members[id].totalMessages +"\n\n"+

				"**Bio:** " + "\n" +
				Data.members[id].bio + "\n" +
				Data.members[id].avatar;
		}
	}

	public string ArenaStats(ulong id) {
		Data.members[id].UpdateStats();
		Data.members[id].UpdateSocket();

		if(Data.members[Context.User.Id].inventory.Count() == 0) {
			Data.members[Context.User.Id].InitializeInventory();
		}

		// For bot
		if(id == Data.botID) {
			
			return
				"**Username:** " + Data.members[id].Username + "\n" +
				"**Nickname:** " + Data.members[id].Nickname + "\n" +
				"**Level:** " + Data.members[id].level + "\n" +
				"**Arena kills:** " + Data.members[id].arenaKills + "\n" +
				"**Arena deaths:** " + Data.members[id].arenaDeaths + "\n\n" +

				"**HP: **" + Data.members[id].hp + " / " + Data.members[id].maxHP + "\n" +
				"**Strength:** " + Data.members[id].strength + "\n" +
				"**Crit Rate:** " + Data.members[id].critical + "\n" +
				"**Gold:** " + Data.members[id].gold + "\n\n";

		// For normal users
		} else {

			return
				"**Username:** " + Data.members[id].Username + "\n" +
				"**Nickname:** " + Data.members[id].Nickname + "\n" +

				"\n:crossed_swords: \n"+
				"**Level:** " + Data.members[id].level + "\n" +
				"**EXP:** " + Data.members[id].exp + "\n" +
				"**EXP To Next Level:** " + Data.members[id].expNext +" ("+ (Data.members[id].expNext - Data.members[id].exp) +" more)\n"+
				"**Total EXP:** " + Data.members[id].totalExp + "\n" +
				"**Arena kills:** " + Data.members[id].arenaKills + "\n" +
				"**Arena deaths:** " + Data.members[id].arenaDeaths + "\n\n" +

				"**HP: **" + Data.members[id].hp + " / " + Data.members[id].maxHP + "\n" +
				"**Strength:** " + Data.members[id].strength + "\n" +
				"**Crit Rate:** " + Data.members[id].critical + "\n" +
				"**Weapon:** " + Data.members[id].inventory[0].quantity + "\n" +
				"**Armor:** " + Data.members[id].inventory[1].quantity + "\n" +
				"**Health Potions:** "+ Data.members[id].inventory[2].quantity + "\n" +

				"\n:pick:\n"+
				"**Mining Level:** " + Data.members[id].miningLevel + "\n" +
				"**Mining EXP:** " + Data.members[id].miningExp + "\n" +
				"**Mining EXP To Next Level:** " + Data.members[id].miningExpNext +" ("+ (Data.members[id].miningExpNext - Data.members[id].miningExp) +" more)\n"+
				"**Total Mining EXP:** " + Data.members[id].totalMiningExp + "\n" +

				"**Gold:** " + Data.members[id].gold + "\n\n";
		}

	}

	public string AddSpace(int space, string str) {
		string spaces = "";

		for(int i = 0; i < (space - str.Length); i++){
				spaces += " ";
		}

		return spaces;
	}

	public int GetCommandIndex(string name) {

		for(int i = 0; i < Data.cmdService.Commands.Count(); i++) {
			if(Data.cmdService.Commands.ToList()[i].Name.Equals(name)) {
				return i;
			}
		}

		return 0;
	}

	public int GenerateSeed() {
		return new Random().Next(-100000000, 99999999);
	}
}