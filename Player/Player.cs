﻿/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
http://www.opensource.org/licenses/ecl2.php
http://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MCGalaxy.Drawing;
using MCGalaxy.SQL;
using MCGalaxy.Util;

namespace MCGalaxy {
    public sealed partial class Player : IDisposable {
        
        /// <summary>
        /// Key - Name
        /// Value - IP
        /// All players who have left this restart.
        /// </summary>
        public Dictionary<string, object> ExtraData = new Dictionary<string, object>();

        public void ClearChat() { OnChat = null; }
        public static Dictionary<string, string> left = new Dictionary<string, string>();
        
        static List<string> pendingNames = new List<string>();
        static object pendingLock = new object();
        
        public static List<Player> connections = new List<Player>(Server.players);
        System.Timers.Timer muteTimer = new System.Timers.Timer(1000);
        public static List<string> emoteList = new List<string>();
        public List<string> listignored = new List<string>();
        public List<string> mapgroups = new List<string>();
        public static List<string> globalignores = new List<string>();
        public static int totalMySQLFailed = 0;
        public static byte number { get { return (byte)PlayerInfo.players.Count; } }
        static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        public static string lastMSG = "";
        
        //TpA
        public bool Request = false;
        public string senderName = "";
        public string currentTpa = "";

        public static bool storeHelp = false;
        public static string storedHelp = "";
        private string truename;
        internal bool dontmindme = false;
        public Socket socket;
        System.Timers.Timer timespent = new System.Timers.Timer(1000);
        System.Timers.Timer loginTimer = new System.Timers.Timer(1000);
        public System.Timers.Timer pingTimer = new System.Timers.Timer(2000);
        System.Timers.Timer extraTimer = new System.Timers.Timer(22000);
        public System.Timers.Timer afkTimer = new System.Timers.Timer(2000);
        public int afkCount = 0;
        public DateTime afkStart;
        public string WoMVersion = "";
        public bool cmdTimer = false;
        public bool UsingWom = false;

        byte[] buffer = new byte[0];
        byte[] tempbuffer = new byte[0xFF];
        public bool disconnected = false;
        public string time;
        public string name;
        public string DisplayName;
        public string SkinName;
        public string realName;
        public int warn = 0;
        public byte id;
        public int userID = -1;
        public string ip;
        public string color;
        public Group group;
        public bool hidden = false;
        public bool painting = false;
        public bool muted = false;
        public bool jailed = false;
        public bool agreed = true;
        public bool invincible = false;
        public string prefix = "";
        public string title = "";
        public string titlecolor;
        public int TotalMessagesSent = 0;
        public int passtries = 0;
        public int ponycount = 0;
        public int rdcount = 0;
        public bool hasreadrules = false;
        public bool canusereview = true;
        public float ReachDistance = 5;
        
        public string FullName { get { return color + prefix + DisplayName; } }

        //Gc checks
        public string lastmsg = "";
        public int spamcount = 0, capscount = 0, floodcount = 0, multi = 0;
        public DateTime lastmsgtime = DateTime.MinValue;
        /// <summary>
        /// Console only please
        /// </summary>
        public bool canusegc = true;

        public bool deleteMode = false;
        public bool ignorePermission = false;
        public bool ignoreGrief = false;
        public bool parseSmiley = true;
        public bool smileySaved = true;
        public bool opchat = false;
        public bool adminchat = false;
        public bool onWhitelist = false;
        public bool whisper = false;
        public string whisperTo = "";
        public bool ignoreglobal = false;

        public string storedMessage = "";

        public bool trainGrab = false;
        public bool onTrain = false;
        public bool allowTnt = true;

        public bool frozen = false;
        public string following = "";
        public string possess = "";

        // Only used for possession.
        //Using for anything else can cause unintended effects!
        public bool canBuild = true;

        public int money = 0;
        public long overallBlocks = 0;

        public int loginBlocks = 0;

        public DateTime timeLogged;
        public DateTime firstLogin;
        public int totalLogins = 0;
        public int totalKicked = 0;
        public int overallDeath = 0;

        public string savedcolor = "";

        public bool staticCommands = false;

        public DateTime ZoneSpam;
        public bool ZoneCheck = false;
        public bool zoneDel = false;

        public bool aiming;
        public bool isFlying = false;

        public bool joker = false;
        public bool adminpen = false;

        public bool voice = false;
        public string voicestring = "";

        public bool useCheckpointSpawn = false;
        public ushort checkpointX, checkpointY, checkpointZ;

        //CTF
        public Team team;
        public Team hasflag;

        //Countdown
        public bool playerofcountdown = false;
        public bool incountdown = false;
        public ushort countdowntempx;
        public ushort countdowntempz;
        public bool countdownsettemps = false;

        //Zombie
        public bool referee = false;
        public int blockCount = 50;
        public bool voted = false;
        public int blocksStacked = 0;
        public int infectThisRound = 0;
        public int lastYblock = 0;
        public int lastXblock = 0;
        public int lastZblock = 0;
        public bool infected = false;
        public bool aka = false;
        public bool flipHead = true;
        public int playersInfected = 0;
        public int NoClipcount = 0;


        //Tnt Wars
        public bool PlayingTntWars = false;
        public int CurrentAmountOfTnt = 0;
        public int CurrentTntGameNumber; //For keeping track of which game is which
        public int TntWarsHealth = 2;
        public int TntWarsKillStreak = 0;
        public float TntWarsScoreMultiplier = 1f;
        public int TNTWarsLastKillStreakAnnounced = 0;
        public bool inTNTwarsMap = false;
        public Player HarmedBy = null; //For Assists

        //Copy
        public CopyState CopyBuffer;
        public bool copyAir = false;
        public int[] copyoffset = new int[3] { 0, 0, 0 };
        public ushort[] copystart = new ushort[3] { 0, 0, 0 };
        
        //Center
        public int[] centerstart = new int[3] { 0, 0, 0 };
        public int[] centerend = new int[3] { 0, 0, 0 };
        
        // GlobalBlock
        internal int gbStep = 0, gbTargetId = 0;
        internal BlockDefinition gbBlock;
        internal int lbStep = 0, lbTargetId = 0;
        internal BlockDefinition lbBlock;
        
        public string model = "humanoid";
        public bool spawned = false;

        public bool Mojangaccount {
            get { return truename.Contains('@'); }
        }

        //Undo
        public struct UndoPos { public ushort x, y, z; public byte type, extType, newtype, newExtType; public string mapName; public DateTime timePlaced; }
        public List<UndoPos> UndoBuffer = new List<UndoPos>();
        public List<UndoPos> RedoBuffer = new List<UndoPos>();


        public bool showPortals = false;
        public bool showMBs = false;

        public string prevMsg = "";

        //Block Change variable holding
        public int[] BcVar;

        //Movement
        public int oldIndex = -1, oldFallY = 10000;
        public int fallCount = 0, drownCount = 0;

        //Games
        public DateTime lastDeath = DateTime.Now;

        public byte BlockAction;
        public byte modeType;
        public byte[] bindings = new byte[128];
        public string[] cmdBind = new string[10];
        public string[] messageBind = new string[10];
        public string lastCMD = "";
        public sbyte c4circuitNumber = -1;

        public Level level = Server.mainLevel;
        public bool Loading = true; //True if player is loading a map.
        internal bool usingGoto = false;
        public ushort[] lastClick = new ushort[] { 0, 0, 0 };
        public ushort[] beforeTeleportPos = new ushort[] { 0, 0, 0 };
        public string beforeTeleportMap = "";
        public ushort[] pos = new ushort[] { 0, 0, 0 };
        ushort[] oldpos = new ushort[] { 0, 0, 0 };
        ushort[] basepos = new ushort[] { 0, 0, 0 };
        public byte[] rot = new byte[] { 0, 0 };
        byte[] oldrot = new byte[] { 0, 0 };

        //ushort[] clippos = new ushort[3] { 0, 0, 0 };
        //byte[] cliprot = new byte[2] { 0, 0 };

        // grief/spam detection
        public static int spamBlockCount = 200;
        public static int spamBlockTimer = 5;
        Queue<DateTime> spamBlockLog = new Queue<DateTime>(spamBlockCount);

        public int consecutivemessages;
        private System.Timers.Timer resetSpamCount = new System.Timers.Timer(Server.spamcountreset * 1000);
        //public static int spamChatCount = 3;
        //public static int spamChatTimer = 4;
        //Queue<DateTime> spamChatLog = new Queue<DateTime>(spamChatCount);

        // CmdVoteKick
        public VoteKickChoice voteKickChoice = VoteKickChoice.HasntVoted;

        // Extra storage for custom commands
        public ExtrasCollection Extras = new ExtrasCollection();

        //Chatrooms
        public string Chatroom;
        public List<string> spyChatRooms = new List<string>();
        public DateTime lastchatroomglobal;

        public List<Waypoint> Waypoints = new List<Waypoint>();

        public Random random = new Random();

        //Global Chat
        public bool muteGlobal;

        public bool loggedIn;
        public bool InGlobalChat { get; set; }
        public Dictionary<string, string> sounds = new Dictionary<string, string>();

        public bool isDev, isMod, isGCMod; //is this player a dev/mod/gcmod?
        public bool isStaff;
        public bool verifiedName;

        public static string CheckPlayerStatus(Player p) {
            if ( p.hidden ) return "hidden";
            if ( Server.afkset.Contains(p.name) ) return "afk";
            return "active";
        }
        
        public bool Readgcrules = false;
        public DateTime Timereadgcrules = DateTime.MinValue;
        public bool CheckIfInsideBlock() {
            ushort x = (ushort)(pos[0] / 32), y = (ushort)(pos[1] / 32), z = (ushort)(pos[2] / 32);
            byte head = level.GetTile(x, y, z);
            byte feet = level.GetTile(x, (ushort)(y - 1), z);

            if (Block.Walkthrough(Block.Convert(head)) && Block.Walkthrough(Block.Convert(feet)))
                return false;
            return Block.Convert(head) != Block.Zero && Block.Convert(head) != Block.op_air;
        }

        //This is so that plugin devs can declare a player without needing a socket..
        //They would still have to do p.Dispose()..
        public Player(string playername) { name = playername; if (playername == "IRC") { group = Group.Find("nobody"); color = Colors.lime; } }

        public Player(Socket s) {
            try {
                socket = s;
                ip = socket.RemoteEndPoint.ToString().Split(':')[0];

                /*if (IPInPrivateRange(ip))
                    exIP = ResolveExternalIP(ip);
                else
                    exIP = ip;*/

                Server.s.Log(ip + " connected to the server.");

                for ( byte i = 0; i < 128; ++i ) bindings[i] = i;

                socket.BeginReceive(tempbuffer, 0, tempbuffer.Length, SocketFlags.None, new AsyncCallback(Receive), this);
                InitTimers();
                connections.Add(this);
            }
            catch ( Exception e ) { Kick("Login failed!"); Server.ErrorLog(e); }
        }


        public void save() {
            //safe against SQL injects because no user input is provided
            string commandString =
                "UPDATE Players SET IP='" + ip + "'" +
                ", LastLogin='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                ", totalLogin=" + totalLogins +
                ", totalDeaths=" + overallDeath +
                ", Money=" + money +
                ", totalBlocks=" + overallBlocks +
                ", totalKicked=" + totalKicked +
                ", TimeSpent='" + time +
                "' WHERE Name='" + name + "'";
            if ( MySQLSave != null )
                MySQLSave(this, commandString);
            OnMySQLSaveEvent.Call(this, commandString);
            if ( cancelmysql ) {
                cancelmysql = false;
                return;
            }
            Database.executeQuery(commandString);

            try {
                if ( !smileySaved ) {
                    if ( parseSmiley )
                        emoteList.RemoveAll(s => s == name);
                    else
                        emoteList.Add(name);

                    File.WriteAllLines("text/emotelist.txt", emoteList.ToArray());
                    smileySaved = true;
                }
            }
            catch ( Exception e ) {
                Server.ErrorLog(e);
            }
            try {
                SaveUndo();
            } catch (Exception e) {
                Server.s.Log("Error saving undo data.");
                Server.ErrorLog(e);
            }
        }

        #region == INCOMING ==
        byte[] HandleMessage(byte[] buffer) {
            try {
                int length = 0; byte msg = buffer[0];
                // Get the length of the message by checking the first byte
                switch (msg) {
                    //For wom
                    case (byte)'G':
                        return new byte[1];
                    case Opcode.Handshake:
                        length = 130;
                        break;
                    case Opcode.SetBlockClient:
                        if (!loggedIn)
                            goto default;
                        length = 8;
                        break;
                    case Opcode.EntityTeleport:
                        if (!loggedIn)
                            goto default;
                        length = 9;
                        break;
                    case Opcode.Message:
                        if (!loggedIn)
                            goto default;
                        length = 65;
                        break;
                    case Opcode.CpeExtInfo:
                        length = 66;
                        break;
                    case Opcode.CpeExtEntry:
                        length = 68;
                        break;
                    case Opcode.CpeCustomBlockSupportLevel:
                        length = 1;
                        break;
                    default:
                        if (!dontmindme)
                            Kick("Unhandled message id \"" + msg + "\"!");
                        else
                            Server.s.Log(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                        return new byte[0];
                }
                if (buffer.Length > length) {
                    byte[] message = new byte[length];
                    Buffer.BlockCopy(buffer, 1, message, 0, length);

                    byte[] tempbuffer = new byte[buffer.Length - length - 1];
                    Buffer.BlockCopy(buffer, length + 1, tempbuffer, 0, buffer.Length - length - 1);

                    buffer = tempbuffer;

                    switch (msg) {
                    	case Opcode.Handshake:
                            HandleLogin(message);
                            lock (pendingLock)
                                pendingNames.Remove(truename);
                            break;
                    	case Opcode.SetBlockClient:
                            if (!loggedIn)
                                break;
                            HandleBlockchange(message);
                            break;
                    	case Opcode.EntityTeleport:
                            if (!loggedIn)
                                break;
                            HandleMovement(message);
                            break;
                    	case Opcode.Message:
                            if (!loggedIn)
                                break;
                            HandleChat(message);
                            break;
                    	case Opcode.CpeExtInfo:
                            HandleExtInfo( message );
                            break;
                    	case Opcode.CpeExtEntry:
                            HandleExtEntry( message );
                            break;
                    	case Opcode.CpeCustomBlockSupportLevel:
                            HandleCustomBlockSupportLevel( message );
                            break;
                    }
                    //thread.Start((object)message);
                    if (buffer.Length > 0)
                        buffer = HandleMessage(buffer);
                    else
                        return new byte[0];
                }
            } catch (Exception e) {
                Server.ErrorLog(e);
            }
            return buffer;
        }
        
        #region Login
        
        void HandleLogin(byte[] message)
        {
            try
            {
                if (loggedIn) return;

                byte version = message[0];
                name = enc.GetString(message, 1, 64).Trim();
                truename = name;
                
                lock (pendingLock) {
                    pendingNames.Add(name);
                    int altsCount = 0;
                    foreach (string other in pendingNames) {
                        if (other == truename) altsCount++;
                    }
                    
                    if (altsCount > 1) {
                        Kick("Already logged in!", true); return;
                    }
                }

                string verify = enc.GetString(message, 65, 32).Trim();
                if (Server.verify)
                {
                    if (verify == "--" || verify !=
                        BitConverter.ToString(md5.ComputeHash(enc.GetBytes(Server.salt + truename)))
                        .Replace("-", "").ToLower())
                    {
                        if (!IPInPrivateRange(ip))
                        {
                            Kick("Login failed! Try again.", true); return;
                        }
                    }
                }
                DisplayName = name;
                SkinName = name;
                name += "+";
                byte type = message[129];

                isDev = Server.Devs.Contains(name.ToLower());
                isMod = Server.Mods.Contains(name.ToLower());
                isGCMod = Server.GCmods.Contains(name.ToLower());
                verifiedName = Server.verify;

                try {
                    Server.TempBan tBan = Server.tempBans.Find(tB => tB.name.ToLower() == name.ToLower());
                    if (tBan.allowedJoin < DateTime.Now) {
                        Server.tempBans.Remove(tBan);
                    } else {
                        Kick("You're still banned (temporary ban)!", true);
                    }
                } catch { }

                if (!CheckWhitelist())
                    return;
                LoadIgnores();
                // ban check
                if (Server.bannedIP.Contains(ip) && (!Server.useWhitelist || !onWhitelist)) {
                    Kick(Server.customBanMessage, true);
                    return;
                }
                
                if (Server.omniban.CheckPlayer(this)) { Kick(Server.omniban.kickMsg); return; } //deprecated
                if (Group.findPlayerGroup(name) == Group.findPerm(LevelPermission.Banned)) {
                    if (!Server.useWhitelist || !onWhitelist) {
                        if (Ban.IsBanned(name)) {
                            string[] data = Ban.GetBanData(name);
                            Kick("Banned for \"" + data[1] + "\" by " + data[0], true);
                    	} else {
                            Kick(Server.customBanMessage, true);
                    	}
                        return;
                    }
                }

                //server maxplayer check
                if (!VIP.Find(this))
                {
                    // Check to see how many guests we have
                    if (PlayerInfo.players.Count >= Server.players && !IPInPrivateRange(ip)) { Kick("Server full!"); return; }
                    // Code for limiting no. of guests
                    if (Group.findPlayerGroup(name) == Group.findPerm(LevelPermission.Guest))
                    {
                        // Check to see how many guests we have
                        int currentNumOfGuests = PlayerInfo.players.Count(pl => pl.group.Permission <= LevelPermission.Guest);
                        if (currentNumOfGuests >= Server.maxGuests)
                        {
                            if (Server.guestLimitNotify) Chat.GlobalMessageOps("Guest " + this.DisplayName + " couldn't log in - too many guests.");
                            Server.s.Log("Guest " + this.name + " couldn't log in - too many guests.");
                            Kick("Server has reached max number of guests", true);
                            return;
                        }
                    }
                }

                if (version != Server.version) { Kick("Wrong version!", true); return; }
                
                foreach (Player p in PlayerInfo.players) {
                    if (p.name == name)  {
                        if (Server.verify) {
                            p.Kick("Someone logged in as you!"); break;
                        } else { 
                            Kick("Already logged in!", true); return;
                        }
                    }
                }
                
                if (type == 0x42) {
                    hasCpe = true;

                    SendExtInfo(16);
                    SendExtEntry(CpeExt.ClickDistance, 1);
                    SendExtEntry(CpeExt.CustomBlocks, 1);
                    SendExtEntry(CpeExt.HeldBlock, 1);
                    
                    SendExtEntry(CpeExt.TextHotkey, 1);
                    SendExtEntry(CpeExt.EnvColors, 1);
                    SendExtEntry(CpeExt.SelectionCuboid, 1);
                    
                    SendExtEntry(CpeExt.BlockPermissions, 1);
                    SendExtEntry(CpeExt.ChangeModel, 1);
                    SendExtEntry(CpeExt.EnvMapAppearance, 2);
                    
                    SendExtEntry(CpeExt.EnvWeatherType, 1);
                    SendExtEntry(CpeExt.HackControl, 1);
                    SendExtEntry(CpeExt.EmoteFix, 1);
                    
                    SendExtEntry(CpeExt.FullCP437, 1);
                    SendExtEntry(CpeExt.LongerMessages, 1);
                    SendExtEntry(CpeExt.BlockDefinitions, 1);
                    
                    SendExtEntry(CpeExt.BlockDefinitionsExt, 2);
                }
                
                try { left.Remove(name.ToLower()); }
                catch { }

                group = Group.findPlayerGroup(name);
                Loading = true;
                if (disconnected) return;             
                id = FreeId();
                
                if (type != 0x42)
                     CompleteLoginProcess();
            } catch (Exception e) {
                Server.ErrorLog(e);
                Player.GlobalMessage("An error occurred: " + e.Message);
            }
        }
        
        bool CheckWhitelist() {
            if (!Server.useWhitelist)
                return true;
            
            if (Server.verify) {
                if (Server.whiteList.Contains(name))
                    onWhitelist = true;
            } else {
                // Verify Names is off. Gotta check the hard way.
                Database.AddParams("@IP", ip);
                DataTable ipQuery = Database.fillData("SELECT Name FROM Players WHERE IP = @IP");

                if (ipQuery.Rows.Count > 0) {
                    if (ipQuery.Rows.Contains(name) && Server.whiteList.Contains(name)) {
                        onWhitelist = true;
                    }
                }
                ipQuery.Dispose();
            }
            if (!onWhitelist) 
                Kick("This is a private server!"); //i think someone forgot this?
            return onWhitelist;
        }
        
        void LoadIgnores() {
            if (File.Exists("ranks/ignore/" + name + ".txt")) {
                try {
                    string[] lines = File.ReadAllLines("ranks/ignore/" + name + ".txt");
                    foreach (string line in lines)
                        listignored.Add(line);
                    File.Delete("ranks/ignore/" + name + ".txt");
                } catch {
                    Server.s.Log("Failed to load ignore list for: " + name);
                }
            }

            if (File.Exists("ranks/ignore/GlobalIgnore.xml")) {
                try {
                    string[] searchxmls = File.ReadAllLines("ranks/ignore/GlobalIgnore.xml");
                    foreach (string searchxml in searchxmls)
                        globalignores.Add(searchxml);
                    
                    foreach (string ignorer in globalignores) {
                        Player foundignore = PlayerInfo.Find(ignorer);
                        foundignore.ignoreglobal = true;
                    }
                    File.Delete("ranks/ignore/GlobalIgnore.xml");
                } catch {
                    Server.s.Log("Failed to load global ignore list!");
                }
            }
        }
        
        void CompleteLoginProcess() {
            try {
                SendMotd();
                SendMap(null);
                if (disconnected) return;
                loggedIn = true;

                lock (PlayerInfo.players)
                    PlayerInfo.players.Add(this);

                connections.Remove(this);

                Server.s.PlayerListUpdate();

                //Test code to show when people come back with different accounts on the same IP
                string alts = name + " is lately known as:";
                bool found = false;
                if (!ip.StartsWith("127.0.0.")) {
                    foreach (KeyValuePair<string, string> prev in left)  {
                        if (prev.Value == ip)
                        {
                            found = true;
                            alts += " " + prev.Key;
                        }
                    }
                    if (found) {
                        if (group.Permission < Server.adminchatperm || !Server.adminsjoinsilent) {
                            Chat.GlobalMessageOps(alts);
                            //IRCBot.Say(temp, true); //Tells people in op channel on IRC
                        }
                        Server.s.Log(alts);
                    }
                }
            } catch (Exception e) {
                Server.ErrorLog(e);
                Player.GlobalMessage("An error occurred: " + e.Message);
            }
            
            //OpenClassic Client Check
            SendBlockchange(0, 0, 0, 0);
            Database.AddParams("@Name", name);
            DataTable playerDb = Database.fillData("SELECT * FROM Players WHERE Name=@Name");

            if (playerDb.Rows.Count == 0)
                InitPlayerStats(playerDb);
            else
                LoadPlayerStats(playerDb);
            
            if (!Directory.Exists("players"))
                Directory.CreateDirectory("players");
            PlayerDB.Load(this);
            SetPrefix();
            playerDb.Dispose();

            if (PlayerConnect != null)
                PlayerConnect(this);
            OnPlayerConnectEvent.Call(this);

            if (Server.server_owner != "" && Server.server_owner.ToLower().Equals(name.ToLower())) {
                if (color == Group.standard.color)
                    color = "&c";
                if (title == "")
                    title = "Owner";
                SetPrefix();
            }

            if (Server.verifyadmins && group.Permission >= Server.verifyadminsrank)
                adminpen = true;
            if (emoteList.Contains(name)) parseSmiley = false;
            if (!Directory.Exists("text/login"))
                Directory.CreateDirectory("text/login");
            if (!File.Exists("text/login/" + this.name + ".txt"))
                CP437Writer.WriteAllText("text/login/" + this.name + ".txt", "joined the server.");

            CheckLoginJailed();

            if (Server.agreetorulesonentry) {
                if (!File.Exists("ranks/agreed.txt"))
                    File.WriteAllText("ranks/agreed.txt", "");
                var agreedFile = File.ReadAllText("ranks/agreed.txt");
                if (group.Permission == LevelPermission.Guest && !agreedFile.Contains(this.name.ToLower())) {
                    SendMessage("&9You must read the &c/rules&9 and &c/agree&9 to them before you can build and use commands!");
                    agreed = false;
                }
            }

            string joinm = "&a+ " + this.FullName + Server.DefaultColor + " " + File.ReadAllText("text/login/" + this.name + ".txt");
            if (group.Permission < Server.adminchatperm || Server.adminsjoinsilent == false)
            {
                if ((Server.guestJoinNotify && group.Permission <= LevelPermission.Guest) || group.Permission > LevelPermission.Guest)
                {
                	PlayerInfo.players.ForEach(p1 => Player.SendMessage(p1, joinm));
                }
            }
            if (group.Permission >= Server.adminchatperm && Server.adminsjoinsilent) {
                hidden = true;
                adminchat = true;
            }

            if (Server.verifyadmins && group.Permission >= Server.verifyadminsrank) {
                if (!Directory.Exists("extra/passwords") || !File.Exists("extra/passwords/" + this.name + ".dat"))
                    SendMessage("&cPlease set your admin verification password with &a/setpass [Password]!");
                else
                    SendMessage("&cPlease complete admin verification with &a/pass [Password]!");
            }
            try
            {
                WaypointList.Load(this);
                //if (Waypoints.Count > 0) { this.SendMessage("Loaded " + Waypoints.Count + " waypoints!"); }
            }
            catch (Exception ex)
            {
                SendMessage("Error loading waypoints!");
                Server.ErrorLog(ex);
            }
            try
            {
                if (File.Exists("ranks/muted.txt"))
                {
                    using (StreamReader read = new StreamReader("ranks/muted.txt"))
                    {
                        string line;
                        while ((line = read.ReadLine()) != null)
                        {
                            if (line.ToLower() == this.name.ToLower())
                            {
                                this.muted = true;
                                Player.SendMessage(this, "!%cYou are still %8muted%c since your last login.");
                                break;
                            }
                        }
                    }
                }
                else { File.Create("ranks/muted.txt").Close(); }
            }
            catch { muted = false; }

            Server.s.Log(name + " [" + ip + "] has joined the server.");

            Server.zombie.PlayerJoinedServer(this);
            try {
                ushort x = (ushort)((0.5 + level.spawnx) * 32);
                ushort y = (ushort)((1 + level.spawny) * 32);
                ushort z = (ushort)((0.5 + level.spawnz) * 32);
                pos = new ushort[3] { x, y, z }; rot = new byte[2] { level.rotx, level.roty };

                GlobalSpawn(this, x, y, z, rot[0], rot[1], true);
                foreach (Player p in PlayerInfo.players) {
                    if (p.level == level && p != this && !p.hidden)
                        SendSpawn(p.id, p.color + p.name, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1]);
                    if (HasCpeExt(CpeExt.ChangeModel))
                        SendChangeModel(p.id, p.model);
                }
                
                foreach (PlayerBot pB in PlayerBot.playerbots) {
                    if (pB.level == level)
                        SendSpawn(pB.id, pB.color + pB.name, pB.pos[0], pB.pos[1], pB.pos[2], pB.rot[0], pB.rot[1]);
                }
            } catch (Exception e) {
                Server.ErrorLog(e);
                Server.s.Log("Error spawning player \"" + name + "\"");
            }
            Loading = false;
        }
        
        void InitPlayerStats(DataTable playerDb) {
            SendMessage("Welcome " + DisplayName + "! This is your first visit.");
            PlayerInfo.CreateInfo(this);
        }
        
        void LoadPlayerStats(DataTable playerDb) {
        	PlayerInfo.LoadInfo(playerDb, this);
            SendMessage("Welcome back " + color + prefix + DisplayName + "%S! " +
        	            "You've been here " + totalLogins + " times!");
            
            if (Server.muted.Contains(name)) {
                muted = true;
                GlobalMessage(DisplayName + " is still muted from the last time they went offline.");
            }
        }
        
        void CheckLoginJailed() {
            //very very sloppy, yes I know.. but works for the time
            bool gotoJail = false;
            string gotoJailMap = "", gotoJailName = "";
            try  {
                if (File.Exists("ranks/jailed.txt"))
                {
                    using (StreamReader read = new StreamReader("ranks/jailed.txt"))
                    {
                        string line;
                        while ((line = read.ReadLine()) != null)
                        {
                            string[] parts = line.Split();
                            if (parts[0].ToLower() == this.name.ToLower())
                            {
                                gotoJail = true;
                                gotoJailName = parts[0];
                                gotoJailMap = parts[1];
                                break;
                            }
                        }
                    }
                } else { 
                    File.Create("ranks/jailed.txt").Close(); 
                }
            } catch {
                gotoJail = false;
            }
            
            if (gotoJail) {
                try {
                    Command.all.Find("goto").Use(this, gotoJailMap);
                    Command.all.Find("jail").Use(null, gotoJailName);
                } catch (Exception e) {
                    Kick(e.ToString());
                }
            }
        }

        #endregion
        
        public void SetPrefix() { 
            string viptitle = isDev ? string.Format("{1}[{0}Dev{1}] ", Colors.blue, color) : 
        	    isMod ? string.Format("{1}[{0}Mod{1}] ", Colors.lime, color) 
        	    : isGCMod ? string.Format("{1}[{0}GCMod{1}] ", Colors.gold, color) : "";
            prefix = (title == "") ? "" : color + "[" + titlecolor + title + color + "] ";
            prefix = viptitle + prefix;
        }

        void HandleBlockchange(byte[] message) {
            try {
                if ( !loggedIn ) return;
                if ( CheckBlockSpam() ) return;
                
                ushort x = NetUtils.ReadU16(message, 0);
                ushort y = NetUtils.ReadU16(message, 2);
                ushort z = NetUtils.ReadU16(message, 4);
                byte action = message[6];
                byte type = message[7];
                byte extType = type;
                
                if ((action == 0 || type == 0) && !level.Deletable) {
                    SendMessage("You cannot currently delete blocks in this level.");
                    RevertBlock(x, y, z); return;
                } else if (action == 1 && !level.Buildable) {
                    SendMessage("You cannot currently place blocks in this level.");
                    RevertBlock(x, y, z); return;
                }
                
                if (type >= Block.CpeCount) {
                    if (!HasCpeExt(CpeExt.BlockDefinitions) || level.CustomBlockDefs[type] == null) {
                        SendMessage("Invalid block type: " + type); 
                        RevertBlock(x, y, z); return;
                    }
                    extType = type;
                    type = Block.custom_block;
                }
                ManualChange(x, y, z, action, type, extType);
            } catch ( Exception e ) {
                // Don't ya just love it when the server tattles?
                Chat.GlobalMessageOps(DisplayName + " has triggered a block change error");
                Chat.GlobalMessageOps(e.GetType().ToString() + ": " + e.Message);
                Server.ErrorLog(e);
            }
        }
        
        public void ManualChange(ushort x, ushort y, ushort z, byte action, byte type, byte extType = 0) {
            byte b = level.GetTile(x, y, z);
            if ( b == Block.Zero ) { return; }
            if ( jailed || !agreed ) { RevertBlock(x, y, z); return; }
            if ( level.name.Contains("Museum " + Server.DefaultColor) && Blockchange == null ) {
                return;
            }

            if ( !deleteMode ) {
                string info = level.foundInfo(x, y, z);
                if ( info.Contains("wait") ) return;
            }

            if ( !canBuild ) {
                RevertBlock(x, y, z); return;
            }

            if ( Server.verifyadmins && adminpen ) {
                SendMessage("&cYou must use &a/pass [Password]&c to verify!");
                RevertBlock(x, y, z); return;
            }

            if (Server.ZombieModeOn && Server.zombie.HandlesManualChange(this, x, y, z, action, type, b)) 
                return;

            if ( Server.lava.active && Server.lava.HasPlayer(this) && Server.lava.IsPlayerDead(this) ) {
                SendMessage("You are out of the round, and cannot build.");
                RevertBlock(x, y, z); return;
            }

            Level.BlockPos bP;
            bP.name = name;
            bP.TimePerformed = DateTime.Now;
            bP.index = level.PosToInt(x, y, z);
            bP.type = type;
            bP.extType = extType;

            lastClick[0] = x; lastClick[1] = y; lastClick[2] = z;
            if ( Blockchange != null ) {
                if ( Blockchange.Method.ToString().IndexOf("AboutBlockchange") == -1 && !level.name.Contains("Museum " + Server.DefaultColor) ) {
                    bP.deleted = true;
                    level.blockCache.Add(bP);
                }

                Blockchange(this, x, y, z, type, extType);
                return;
            }
            if ( PlayerBlockChange != null )
                PlayerBlockChange(this, x, y, z, type, extType);
            OnBlockChangeEvent.Call(this, x, y, z, type, extType);
            if ( cancelBlock ) {
                cancelBlock = false;
                return;
            }

            if ( group.Permission == LevelPermission.Banned ) return;
            if ( group.Permission == LevelPermission.Guest ) {
                int Diff = Math.Abs((pos[0] / 32) - x) + Math.Abs((pos[1] / 32) - y) 
                    + Math.Abs((pos[2] / 32) - z);

                if ((Diff > ReachDistance + 4) && lastCMD != "click") {
                    Server.s.Log(name + " attempted to build with a " + Diff + " distance offset");
                    SendMessage("You can't build that far away.");
                    RevertBlock(x, y, z); return;
                }
            }

            if (!Block.canPlace(this, b) && !Block.BuildIn(b) && !Block.AllowBreak(b)) {
                SendMessage("Cannot build here!");
                RevertBlock(x, y, z); return;
            }

            if (!Block.canPlace(this, type)) {
                SendMessage("You can't place this block type!");
                RevertBlock(x, y, z); return;
            }

            if (b >= 200 && b < 220) {
                SendMessage("Block is active, you cant disturb it!");
                RevertBlock(x, y, z); return;
            }

            if (action > 1 ) { Kick("Unknown block action!"); return; }
            byte oldType = type;
            if (type < 128) type = bindings[type];
            
            //Ignores updating blocks that are the same and send block only to the player
            byte newBlock = (painting || action == 1) ? type : (byte)0;
            if (b == newBlock && (painting || oldType != type)) {
                if (b != Block.custom_block || extType == level.GetExtTile(x, y, z)) {
                    RevertBlock(x, y, z); return;
                }
            }
            //else
            if ( !painting && action == 0 ) {
                if ( !deleteMode ) {
                    if ( Block.portal(b) ) { HandlePortal(this, x, y, z, b); return; }
                    if ( Block.mb(b) ) { HandleMsgBlock(this, x, y, z, b); return; }
                }

                bP.deleted = true;
                level.blockCache.Add(bP);
                DeleteBlock(b, x, y, z, type, extType);
            } else {
                bP.deleted = false;
                level.blockCache.Add(bP);
                PlaceBlock(b, x, y, z, type, extType);
            }
        }

        void HandlePortal(Player p, ushort x, ushort y, ushort z, byte b) {
            try {
                //safe against SQL injections because no user input is given here
                DataTable Portals = Database.fillData("SELECT * FROM `Portals" + level.name + "` WHERE EntryX=" + (int)x + " AND EntryY=" + (int)y + " AND EntryZ=" + (int)z);

                int LastPortal = Portals.Rows.Count - 1;
                if ( LastPortal > -1 ) {
                    if ( level.name != Portals.Rows[LastPortal]["ExitMap"].ToString() ) {
                        if ( level.permissionvisit > this.group.Permission ) {
                            Player.SendMessage(this, "You do not have the adequate rank to visit this map!");
                            return;
                        }
                        ignorePermission = true;
                        Level thisLevel = level;
                        Command.all.Find("goto").Use(this, Portals.Rows[LastPortal]["ExitMap"].ToString());
                        if ( thisLevel == level ) { Player.SendMessage(p, "The map the portal goes to isn't loaded."); return; }
                        ignorePermission = false;
                    }
                    else SendBlockchange(x, y, z, b);

                    p.BlockUntilLoad(10);
                    Command.all.Find("move").Use(this, this.name + " " + Portals.Rows[LastPortal]["ExitX"].ToString() + " " + Portals.Rows[LastPortal]["ExitY"].ToString() + " " + Portals.Rows[LastPortal]["ExitZ"].ToString());
                }
                else {
                    Blockchange(this, x, y, z, Block.air, 0);
                }
                Portals.Dispose();
            }
            catch { Player.SendMessage(p, "Portal had no exit."); return; }
        }

        static char[] trimChars = { ' '};
        void HandleMsgBlock(Player p, ushort x, ushort y, ushort z, byte b) {
            try {
                //safe against SQL injections because no user input is given here
                DataTable Messages = Database.fillData("SELECT * FROM `Messages" + level.name + "` WHERE X=" + (int)x + " AND Y=" + (int)y + " AND Z=" + (int)z);

                int LastMsg = Messages.Rows.Count - 1;
                if ( LastMsg > -1 ) {
                    string message = Messages.Rows[LastMsg]["Message"].ToString().Trim();
                    message = message.Replace("\\'", "\'");
                    if ( message != prevMsg || Server.repeatMessage ) {
                        if ( message.StartsWith("/") ) {
                    	    string[] parts = message.Remove(0, 1).Split(trimChars, 2);
                            HandleCommand(parts[0], parts.Length > 1 ? parts[1] : "");
                    	} else {
                            Player.SendMessage(p, message);
                        }
                        prevMsg = message;
                    }
                    SendBlockchange(x, y, z, b);
                } else {
                    Blockchange(this, x, y, z, Block.air, 0);
                }
                Messages.Dispose();
            } catch { 
        	    Player.SendMessage(p, "No message was stored.");
        	    RevertBlock(x, y, z); return;
        	}
        }
        
        void DeleteBlock(byte b, ushort x, ushort y, ushort z, byte type, byte extType) {
            if ( deleteMode && b != Block.c4det ) { level.Blockchange(this, x, y, z, Block.air); return; }

            if ( Block.tDoor(b) ) { RevertBlock(x, y, z); return; }
            if ( Block.DoorAirs(b) != 0 ) {
                if ( level.physics != 0 ) 
                    level.Blockchange(x, y, z, Block.DoorAirs(b));
                else 
                    RevertBlock(x, y, z);
                return;
            }
            if ( Block.odoor(b) != Block.Zero ) {
                if ( b == Block.odoor8 || b == Block.odoor8_air ) {
                    level.Blockchange(this, x, y, z, Block.odoor(b));
                } else {
                   RevertBlock(x, y, z);
                }
                return;
            }

            switch ( b ) {
                case Block.door_air: //Door_air
                case Block.door2_air:
                case Block.door3_air:
                case Block.door4_air:
                case Block.door5_air:
                case Block.door6_air:
                case Block.door7_air:
                case Block.door8_air:
                case Block.door9_air:
                case Block.door10_air:
                case Block.door_iron_air:
                case Block.door_gold_air:
                case Block.door_cobblestone_air:
                case Block.door_red_air:

                case Block.door_dirt_air:
                case Block.door_grass_air:
                case Block.door_blue_air:
                case Block.door_book_air:
                    break;
                case Block.rocketstart:
                    if ( level.physics < 2 || level.physics == 5 ) {
                        RevertBlock(x, y, z);
                    } else {
                        int newZ = 0, newX = 0, newY = 0;

                        SendBlockchange(x, y, z, Block.rocketstart);
                        if ( rot[0] < 48 || rot[0] > ( 256 - 48 ) )
                            newZ = -1;
                        else if ( rot[0] > ( 128 - 48 ) && rot[0] < ( 128 + 48 ) )
                            newZ = 1;

                        if ( rot[0] > ( 64 - 48 ) && rot[0] < ( 64 + 48 ) )
                            newX = 1;
                        else if ( rot[0] > ( 192 - 48 ) && rot[0] < ( 192 + 48 ) )
                            newX = -1;

                        if ( rot[1] >= 192 && rot[1] <= ( 192 + 32 ) )
                            newY = 1;
                        else if ( rot[1] <= 64 && rot[1] >= 32 )
                            newY = -1;

                        if ( 192 <= rot[1] && rot[1] <= 196 || 60 <= rot[1] && rot[1] <= 64 ) { newX = 0; newZ = 0; }

                        byte b1 = level.GetTile((ushort)( x + newX * 2 ), (ushort)( y + newY * 2 ), (ushort)( z + newZ * 2 ));
                        byte b2 = level.GetTile((ushort)( x + newX ), (ushort)( y + newY ), (ushort)( z + newZ ));
                        if ( b1 == Block.air && b2 == Block.air && level.CheckClear((ushort)( x + newX * 2 ), (ushort)( y + newY * 2 ), (ushort)( z + newZ * 2 )) && level.CheckClear((ushort)( x + newX ), (ushort)( y + newY ), (ushort)( z + newZ )) ) {
                            level.Blockchange((ushort)( x + newX * 2 ), (ushort)( y + newY * 2 ), (ushort)( z + newZ * 2 ), Block.rockethead);
                            level.Blockchange((ushort)( x + newX ), (ushort)( y + newY ), (ushort)( z + newZ ), Block.fire);
                        }
                    }
                    break;
                case Block.firework:
                    if ( level.physics == 5 ) {
                        RevertBlock(x, y, z); return;
                    }
                    if ( level.physics != 0 ) {
                        Random rand = new Random();
                        int mx = rand.Next(0, 2); int mz = rand.Next(0, 2);
                        byte b1 = level.GetTile((ushort)( x + mx - 1 ), (ushort)( y + 2 ), (ushort)( z + mz - 1 ));
                        byte b2 = level.GetTile((ushort)( x + mx - 1 ), (ushort)( y + 1 ), (ushort)( z + mz - 1 ));
                        if ( b1 == Block.air && b2 == Block.air && level.CheckClear((ushort)( x + mx - 1 ), (ushort)( y + 2 ), (ushort)( z + mz - 1 )) && level.CheckClear((ushort)( x + mx - 1 ), (ushort)( y + 1 ), (ushort)( z + mz - 1 )) ) {
                            level.Blockchange((ushort)( x + mx - 1 ), (ushort)( y + 2 ), (ushort)( z + mz - 1 ), Block.firework);
                            level.Blockchange((ushort)( x + mx - 1 ), (ushort)( y + 1 ), (ushort)( z + mz - 1 ), Block.lavastill, false, "wait 1 dissipate 100");
                        }
                    }
                    RevertBlock(x, y, z);
                    break;

                case Block.c4det:
                    Level.C4.BlowUp(new ushort[] { x, y, z }, level);
                    level.Blockchange(x, y, z, Block.air);
                    break;

                default:
                    level.Blockchange(this, x, y, z, (byte)( Block.air ));
                    break;
            }
            if ( (level.physics == 0 || level.physics == 5) && level.GetTile(x, (ushort)( y - 1 ), z) == Block.dirt ) 
                level.Blockchange(this, x, (ushort)( y - 1 ), z, Block.grass);
        }

        void PlaceBlock(byte b, ushort x, ushort y, ushort z, byte type, byte extType) {
            if ( Block.odoor(b) != Block.Zero ) { SendMessage("oDoor here!"); return; }
            switch ( BlockAction ) {
                case 0: //normal
                    if ( level.physics == 0 || level.physics == 5 ) {
                        switch ( type ) {
                            case Block.dirt: //instant dirt to grass
            				    byte above = level.GetTile(x, (ushort)(y + 1), z), extAbove = 0;
                                if (type == Block.custom_block)
                    	            extAbove = level.GetExtTile(x, (ushort)(y + 1), z);
                                
                                if (Block.LightPass(above, extAbove, level.CustomBlockDefs)) 
                                	level.Blockchange(this, x, y, z, (byte)Block.grass);
                                else
                                	level.Blockchange(this, x, y, z, (byte)Block.dirt);
                                break;
                            case Block.staircasestep: //stair handler
                                if ( level.GetTile(x, (ushort)( y - 1 ), z) == Block.staircasestep ) {
                                    SendBlockchange(x, y, z, Block.air); //send the air block back only to the user.
                                    //level.Blockchange(this, x, y, z, (byte)(Block.air));
                                    level.Blockchange(this, x, (ushort)( y - 1 ), z, (byte)( Block.staircasefull ));
                                    break;
                                }
                                //else
                                level.Blockchange(this, x, y, z, type, extType);
                                break;
                            default:
                                level.Blockchange(this, x, y, z, type, extType);
                                break;
                        }
                    } else {
                        level.Blockchange(this, x, y, z, type, extType);
                    }
                    break;
                case 6:
                    if ( b == modeType ) { SendBlockchange(x, y, z, b); return; }
                    level.Blockchange(this, x, y, z, modeType);
                    break;
                case 13: //Small TNT
                    level.Blockchange(this, x, y, z, Block.smalltnt);
                    break;
                case 14: //Big TNT
                    level.Blockchange(this, x, y, z, Block.bigtnt);
                    break;
                case 15: //Nuke TNT
                    level.Blockchange(this, x, y, z, Block.nuketnt);
                    break;
                default:
                    Server.s.Log(name + " is breaking something");
                    BlockAction = 0;
                    break;
            }
        }

        void HandleMovement(byte[] message) {
            if ( !loggedIn || trainGrab || following != "" || frozen )
                return;
            /*if (CheckIfInsideBlock())
{
this.SendPos(0xFF, (ushort)(clippos[0] - 18), (ushort)(clippos[1] - 18), (ushort)(clippos[2] - 18), cliprot[0], cliprot[1]);
return;
}*/
            byte thisid = message[0];

            if ( this.incountdown && Server.Countdown.gamestatus == CountdownGameStatus.InProgress && Server.Countdown.freezemode ) {
                if ( this.countdownsettemps ) {
                    countdowntempx = NetUtils.ReadU16(message, 1);
                    Thread.Sleep(100);
                    countdowntempz = NetUtils.ReadU16(message, 5);
                    Thread.Sleep(100);
                    countdownsettemps = false;
                }
                ushort x = countdowntempx;
                ushort y = NetUtils.ReadU16(message, 3);
                ushort z = countdowntempz;
                byte rotx = message[7];
                byte roty = message[8];
                pos = new ushort[3] { x, y, z };
                rot = new byte[2] { rotx, roty };
                if ( countdowntempx != NetUtils.ReadU16(message, 1) || countdowntempz != NetUtils.ReadU16(message, 5) ) {
                    this.SendPos(0xFF, pos[0], pos[1], pos[2], rot[0], rot[1]);
                }
            } else {
                ushort x = NetUtils.ReadU16(message, 1);
                ushort y = NetUtils.ReadU16(message, 3);
                ushort z = NetUtils.ReadU16(message, 5);
                byte rotx = message[7];
                byte roty = message[8];

                if (Server.ZombieModeOn && Server.zombie.HandlesMovement(this, x, y, z, rotx, roty))
                    return;
                if ( OnMove != null )
                    OnMove(this, x, y, z);
                if ( PlayerMove != null )
                    PlayerMove(this, x, y, z);
                PlayerMoveEvent.Call(this, x, y, z);

                if (OnRotate != null)
                    OnRotate(this, rot);
                if (PlayerRotate != null)
                    PlayerRotate(this, rot);
                PlayerRotateEvent.Call(this, rot);
                if ( cancelmove ) {
                    SendPos(0xFF, pos[0], pos[1], pos[2], rot[0], rot[1]);
                    return;
                }
               
                pos = new ushort[3] { x, y, z };
                rot = new byte[2] { rotx, roty };
                /*if (!CheckIfInsideBlock())
{
clippos = pos;
cliprot = rot;
}*/
            }
        }

        internal void CheckSurvival(ushort x, ushort y, ushort z) {
            byte bFeet = GetSurvivalBlock(x, (ushort)(y - 2), z);
            byte bHead = GetSurvivalBlock(x, y, z);
            if (level.PosToInt(x, y, z) != oldIndex || y != oldFallY) {
                byte conv = Block.Convert(bFeet);
                if (conv == Block.air) {
                    if (y < oldFallY)
                        fallCount++;
                    else if (y > oldFallY) // flying up, for example
                        fallCount = 0;
                    oldFallY = y;
                    drownCount = 0;
                    return;
                } else if (!(conv == Block.water || conv == Block.waterstill ||
                             conv == Block.lava || conv == Block.lavastill)) {
                    if (fallCount > level.fall)
                        HandleDeath(Block.air, null, false, true);
                    fallCount = 0;
                    drownCount = 0;
                    return;
                }
            }

            switch (Block.Convert(bHead)) {
                case Block.water:
                case Block.waterstill:
                case Block.lava:
                case Block.lavastill:
                    fallCount = 0;
                    drownCount++;
                    if (drownCount > level.drown * (100/3)) {
                        HandleDeath(Block.water);
                        drownCount = 0;
                    }
                    break;
                case Block.air:
                    drownCount = 0;
                    break;
                default:
                    fallCount = 0;
                    drownCount = 0;
                    break;
            }
        }
        
        byte GetSurvivalBlock(ushort x, ushort y, ushort z) {
            if (y >= ushort.MaxValue - 512) return Block.blackrock;
            if (y >= level.Height) return Block.air;
            return level.GetTile(x, y, z);
        }

        internal void CheckBlock(ushort x, ushort y, ushort z) {
            byte b = level.GetTile(x, y, z);
            byte b1 = level.GetTile(x, (ushort)(y - 1), z);

            if ( Block.Mover(b) || Block.Mover(b1) ) {
                if ( Block.DoorAirs(b) != 0 )
                    level.Blockchange(x, y, z, Block.DoorAirs(b));
                if ( Block.DoorAirs(b1) != 0 )
                    level.Blockchange(x, (ushort)(y - 1), z, Block.DoorAirs(b1));

                if ( level.PosToInt( x, y, z ) != oldIndex ) {
                    if ( b == Block.air_portal || b == Block.water_portal || b == Block.lava_portal ) {
                        HandlePortal(this, x, y, z, b);
                    } else if ( b1 == Block.air_portal || b1 == Block.water_portal || b1 == Block.lava_portal ) {
                        HandlePortal(this, x, (ushort)(y - 1), z, b1);
                    }

                    if ( b == Block.MsgAir || b == Block.MsgWater || b == Block.MsgLava ) {
                        HandleMsgBlock(this, x, y, z, b);
                    } else if ( b1 == Block.MsgAir || b1 == Block.MsgWater || b1 == Block.MsgLava ) {
                        HandleMsgBlock(this, x, (ushort)(y - 1), z, b1);
                	} else if ( b == Block.checkpoint ) {
                		useCheckpointSpawn = true;
                        checkpointX = x; checkpointY = y; checkpointZ = z;
                        SendSpawn(0xFF, color + name, pos[0], (ushort)(pos[1] - 22), pos[2], rot[0], rot[1]);
                	} else if ( b1 == Block.checkpoint ) {
                		useCheckpointSpawn = true;
                		checkpointX = x; checkpointY = (ushort)(y + 1); checkpointZ = z;
                		SendSpawn(0xFF, color + name, pos[0], (ushort)(pos[1] - 22), pos[2], rot[0], rot[1]);
                	}
                }
            }
            if ( ( b == Block.tntexplosion || b1 == Block.tntexplosion ) && PlayingTntWars ) { }
            else if ( Block.Death(b) ) HandleDeath(b); 
            else if ( Block.Death(b1) ) HandleDeath(b1);
        }

        public void HandleDeath(byte b, string customMessage = "", bool explode = false, bool immediate = false) {
            ushort x = (ushort)(pos[0] / 32), y = (ushort)(pos[1] / 32), z = (ushort)(pos[2] / 32);
            if ( OnDeath != null )
                OnDeath(this, b);
            if ( PlayerDeath != null )
                PlayerDeath(this, b);
            OnPlayerDeathEvent.Call(this, b);
            if ( Server.lava.active && Server.lava.HasPlayer(this) && Server.lava.IsPlayerDead(this) )
                return;
            
            if ( immediate || lastDeath.AddSeconds(2) < DateTime.Now ) {

                if ( level.Killer && !invincible && !hidden ) {

                    switch ( b ) {
                        case Block.tntexplosion: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " &cblew into pieces.", false); break;
                        case Block.deathair: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " walked into &cnerve gas and suffocated.", false); break;
                        case Block.deathwater:
                        case Block.activedeathwater: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " stepped in &dcold water and froze.", false); break;
                        case Block.deathlava:
                        case Block.activedeathlava:
                        case Block.fastdeathlava: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " stood in &cmagma and melted.", false); break;
                        case Block.magma: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was hit by &cflowing magma and melted.", false); break;
                        case Block.geyser: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was hit by &cboiling water and melted.", false); break;
                        case Block.birdkill: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was hit by a &cphoenix and burnt.", false); break;
                        case Block.train: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was hit by a &ctrain.", false); break;
                        case Block.fishshark: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was eaten by a &cshark.", false); break;
                        case Block.fire: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " burnt to a &ccrisp.", false); break;
                        case Block.rockethead: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was &cin a fiery explosion.", false); level.MakeExplosion(x, y, z, 0); break;
                        case Block.zombiebody: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " died due to lack of &5brain.", false); break;
                        case Block.creeper: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was killed &cb-SSSSSSSSSSSSSS", false); level.MakeExplosion(x, y, z, 1); break;
                        case Block.air: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " hit the floor &chard.", false); break;
                        case Block.water: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " &cdrowned.", false); break;
                        case Block.Zero: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was &cterminated", false); break;
                        case Block.fishlavashark: Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + " was eaten by a ... LAVA SHARK?!", false); break;
                        case Block.rock:
                            if ( explode ) level.MakeExplosion(x, y, z, 1);
                            GlobalChat(this, this.FullName + Server.DefaultColor + customMessage, false);
                            break;
                        case Block.stone:
                            if ( explode ) level.MakeExplosion(x, y, z, 1);
                            Chat.GlobalChatLevel(this, this.FullName + Server.DefaultColor + customMessage, false);
                            break;
                    }
                    if ( team != null && this.level.ctfmode ) {
                        //if (carryingFlag)
                        //{
                        // level.ctfgame.DropFlag(this, hasflag);
                        //}
                        team.SpawnPlayer(this);
                        //this.health = 100;
                    }
                    else if ( Server.Countdown.playersleftlist.Contains(this) ) {
                        Server.Countdown.Death(this);
                        Command.all.Find("spawn").Use(this, "");
                    }
                    else if ( PlayingTntWars ) {
                        TntWarsKillStreak = 0;
                        TntWarsScoreMultiplier = 1f;
                    }
                    else if ( Server.lava.active && Server.lava.HasPlayer(this) ) {
                        if ( !Server.lava.IsPlayerDead(this) ) {
                            Server.lava.KillPlayer(this);
                            Command.all.Find("spawn").Use(this, "");
                        }
                    }
                    else {
                        Command.all.Find("spawn").Use(this, "");
                        overallDeath++;
                    }

                    if ( Server.deathcount )
                        if ( overallDeath > 0 && overallDeath % 10 == 0 ) GlobalChat(this, this.FullName + Server.DefaultColor + " has died &3" + overallDeath + " times", false);
                }
                lastDeath = DateTime.Now;

            }
        }

        /* void HandleFly(Player p, ushort x, ushort y, ushort z) {
FlyPos pos;

ushort xx; ushort yy; ushort zz;

TempFly.Clear();

if (!flyGlass) y = (ushort)(y + 1);

for (yy = y; yy >= (ushort)(y - 1); --yy)
for (xx = (ushort)(x - 2); xx <= (ushort)(x + 2); ++xx)
for (zz = (ushort)(z - 2); zz <= (ushort)(z + 2); ++zz)
if (p.level.GetTile(xx, yy, zz) == Block.air) {
pos.x = xx; pos.y = yy; pos.z = zz;
TempFly.Add(pos);
}

FlyBuffer.ForEach(delegate(FlyPos pos2) {
try { if (!TempFly.Contains(pos2)) SendBlockchange(pos2.x, pos2.y, pos2.z, Block.air); } catch { }
});

FlyBuffer.Clear();

TempFly.ForEach(delegate(FlyPos pos3){
FlyBuffer.Add(pos3);
});

if (flyGlass) {
FlyBuffer.ForEach(delegate(FlyPos pos1) {
try { SendBlockchange(pos1.x, pos1.y, pos1.z, Block.glass); } catch { }
});
} else {
FlyBuffer.ForEach(delegate(FlyPos pos1) {
try { SendBlockchange(pos1.x, pos1.y, pos1.z, Block.waterstill); } catch { }
});
}
} */
        
        void HandleChat(byte[] message) {
            try {
                if ( !loggedIn ) return;
                byte continued = message[0];
                string text = GetString(message, 1);

                // handles the /womid client message, which displays the WoM vrersion
                if ( text.Truncate(6) == "/womid" ) {
                    string version = (text.Length <= 21 ? text.Substring(text.IndexOf(' ') + 1) : text.Substring(7, 15));
                    Player.GlobalMessage(Colors.red + "[INFO] " + color + DisplayName + "%f is using wom client");
                    Player.GlobalMessage(Colors.red + "[INFO] %fVersion: " + version);
                    Server.s.Log(Colors.red + "[INFO] " + color + DisplayName + "%f is using wom client");
                    Server.s.Log(Colors.red + "[INFO] %fVersion: " + version);
                    UsingWom = true;
                    WoMVersion = version.Split('-')[1];
                    return;
                }
                
                if( HasCpeExt(CpeExt.LongerMessages) && continued != 0 ) {
                    storedMessage += text;
                    return;
                }

                if ( storedMessage != "" ) {
                    if ( !text.EndsWith(">") && !text.EndsWith("<") ) {
                        text = storedMessage.Replace("|>|", " ").Replace("|<|", "") + text;
                        storedMessage = "";
                    }
                }
                //if (text.StartsWith(">") || text.StartsWith("<")) return;
                if (text.EndsWith(">"))
                {
                    storedMessage += text.Replace(">", "|>|");
                    SendMessage(Colors.teal + "Partial message: " + Colors.white + storedMessage.Replace("|>|", " ").Replace("|<|", ""));
                    return;
                }
                if (text.EndsWith("<"))
                {
                    storedMessage += text.Replace("<", "|<|");
                    SendMessage(Colors.teal + "Partial message: " + Colors.white + storedMessage.Replace("|<|", "").Replace("|>|", " "));
                    return;
                }

                text = Regex.Replace(text, @"\s\s+", " ");
                if ( text.Any(ch => ch == '&') ) {
                    Kick("Illegal character in chat message!");
                    return;
                }
                if ( text.Length == 0 )
                    return;
                afkCount = 0;

                if ( text != "/afk" ) {
                    if ( Server.afkset.Contains(this.name) ) {
                        Server.afkset.Remove(this.name);
                        Player.GlobalMessage("-" + this.color + this.DisplayName + Server.DefaultColor + "- is no longer AFK");
                        Server.IRC.Say(this.DisplayName + " is no longer AFK");
                    }
                }
                // This will allow people to type
                // //Command
                // and in chat it will appear as
                // /Command
                // Suggested by McMrCat
                if ( text.StartsWith("//") ) {
                    text = text.Remove(0, 1);
                    goto hello;
                }
                //This will make / = /repeat
                //For lazy people :P
                if ( text == "/" ) {
                    HandleCommand("repeat", "");
                    return;
                }
                if ( text[0] == '/' || text[0] == '!' ) {
                    text = text.Remove(0, 1);

                    int pos = text.IndexOf(' ');
                    if ( pos == -1 ) {
                        HandleCommand(text.ToLower(), "");
                        return;
                    }
                    string cmd = text.Substring(0, pos).ToLower();
                    string msg = text.Substring(pos + 1);
                    HandleCommand(cmd, msg);
                    return;
                }
            hello:
                // People who are muted can't speak or vote
                if ( muted ) { this.SendMessage("You are muted."); return; } //Muted: Only allow commands

                // Lava Survival map vote recorder
                if ( Server.lava.HasPlayer(this) && Server.lava.HasVote(text.ToLower()) ) {
                    if ( Server.lava.AddVote(this, text.ToLower()) ) {
                        SendMessage("Your vote for &5" + text.ToLower().Capitalize() + Server.DefaultColor + " has been placed. Thanks!");
                        Server.lava.map.ChatLevelOps(name + " voted for &5" + text.ToLower().Capitalize() + Server.DefaultColor + ".");
                        return;
                    }
                    else {
                        SendMessage("&cYou already voted!");
                        return;
                    }
                }

                //CmdVoteKick core vote recorder
                if ( Server.voteKickInProgress && text.Length == 1 ) {
                    if ( text.ToLower() == "y" ) {
                        this.voteKickChoice = VoteKickChoice.Yes;
                        SendMessage("Thanks for voting!");
                        return;
                    }
                    if ( text.ToLower() == "n" ) {
                        this.voteKickChoice = VoteKickChoice.No;
                        SendMessage("Thanks for voting!");
                        return;
                    }
                }
                
                // Put this after vote collection so that people can vote even when chat is moderated
                if ( Server.chatmod && !this.voice ) { this.SendMessage("Chat moderation is on, you cannot speak."); return; }

                // Filter out bad words
                if ( Server.profanityFilter ) {
                    text = ProfanityFilter.Parse(text);
                }

                if ( Server.checkspam ) {
                    //if (consecutivemessages == 0)
                    //{
                    // consecutivemessages++;
                    //}
                    if ( Player.lastMSG == this.name ) {
                        consecutivemessages++;
                    }
                    else {
                        consecutivemessages--;
                    }

                    if ( this.consecutivemessages >= Server.spamcounter ) {
                        int total = Server.mutespamtime;
                        Command.all.Find("mute").Use(null, this.name);
                        Player.GlobalMessage(this.color + this.DisplayName + Server.DefaultColor + " has been &0muted &efor spamming!");
                        muteTimer.Elapsed += delegate {
                            total--;
                            if ( total <= 0 ) {
                                muteTimer.Stop();
                                if ( this.muted ) {
                                    Command.all.Find("mute").Use(null, this.name);
                                }
                                this.consecutivemessages = 0;
                                Player.SendMessage(this, "Remember, no &cspamming &e" + "next time!");
                            }
                        };
                        muteTimer.Start();
                        return;
                    }
                }
                Player.lastMSG = this.name;

                if( Chat.HandleModes(this, text) )
                	return;

                if ( InGlobalChat ) {
                    Command.all.Find("global").Use(this, text); //Didn't want to rewrite the whole command... you lazy bastard :3
                    return;
                }

                if ( text[0] == ':' ) {
                    if ( PlayingTntWars ) {
                        string newtext = text;
                        if ( text[0] == ':' ) newtext = text.Remove(0, 1).Trim();
                        TntWarsGame it = TntWarsGame.GetTntWarsGame(this);
                        if ( it.GameMode == TntWarsGame.TntWarsGameMode.TDM ) {
                            TntWarsGame.player pl = it.FindPlayer(this);
                            foreach ( TntWarsGame.player p in it.Players ) {
                                if ( pl.Red && p.Red ) SendMessage(p.p, "To Team " + Colors.red + "-" + color + name + Colors.red + "- " + Server.DefaultColor + newtext);
                                if ( pl.Blue && p.Blue ) SendMessage(p.p, "To Team " + Colors.blue + "-" + color + name + Colors.blue + "- " + Server.DefaultColor + newtext);
                            }
                            Server.s.Log("[TNT Wars] [TeamChat (" + ( pl.Red ? "Red" : "Blue" ) + ") " + name + " " + newtext);
                            return;
                        }
                    }
                }

                /*if (this.teamchat)
{
if (team == null)
{
Player.SendMessage(this, "You are not on a team.");
return;
}
foreach (Player p in team.players)
{
Player.SendMessage(p, "(" + team.teamstring + ") " + this.color + this.name + ":&f " + text);
}
return;
}*/
                if ( this.joker ) {
                    if ( File.Exists("text/joker.txt") ) {
                        Server.s.Log("<JOKER>: " + this.name + ": " + text);
                        Chat.GlobalMessageOps(Server.DefaultColor + "<&aJ&bO&cK&5E&9R" + Server.DefaultColor + ">: " + this.color + this.DisplayName + ":&f " + text);
                        FileInfo jokertxt = new FileInfo("text/joker.txt");
                        StreamReader stRead = jokertxt.OpenText();
                        List<string> lines = new List<string>();
                        Random rnd = new Random();
                        int i = 0;

                        while ( !( stRead.Peek() == -1 ) )
                            lines.Add(stRead.ReadLine());

                        stRead.Close();
                        stRead.Dispose();

                        if ( lines.Count > 0 ) {
                            i = rnd.Next(lines.Count);
                            text = lines[i];
                        }

                    }
                    else { File.Create("text/joker.txt").Dispose(); }

                }

                //chatroom stuff
                if ( this.Chatroom != null ) {
                    Chat.ChatRoom(this, text, true, this.Chatroom);
                    return;
                }

                if ( !level.worldChat ) {
                    Server.s.Log("<" + name + ">[level] " + text);
                    Chat.GlobalChatLevel(this, text, true);
                    return;
                }

                if ( text[0] == '%' ) {
                    string newtext = text;
                    if ( !Server.worldChat ) {
                        newtext = text.Remove(0, 1).Trim();
                        Chat.GlobalChatWorld(this, newtext, true);
                    } else {
                        GlobalChat(this, newtext);
                    }
                    Server.s.Log("<" + name + "> " + newtext);
                    //IRCBot.Say("<" + name + "> " + newtext);
                    if ( OnChat != null )
                        OnChat(this, text);
                    if ( PlayerChat != null )
                        PlayerChat(this, text);
                    OnPlayerChatEvent.Call(this, text);
                    return;
                }
                Server.s.Log("<" + name + "> " + text);
                if ( OnChat != null )
                    OnChat(this, text);
                if ( PlayerChat != null )
                    PlayerChat(this, text);
                OnPlayerChatEvent.Call(this, text);
                if ( cancelchat ) {
                    cancelchat = false;
                    return;
                }
                if ( Server.worldChat ) {
                    GlobalChat(this, text);
                } else {
                    Chat.GlobalChatLevel(this, text, true);
                }

                //IRCBot.Say(name + ": " + text);
            }
            catch ( Exception e ) { Server.ErrorLog(e); Player.GlobalMessage("An error occurred: " + e.Message); }
        }
        public void HandleCommand(string cmd, string message) {
            try {
                if ( Server.verifyadmins ) {
                    if ( cmd.ToLower() == "setpass" ) {
                        Command.all.Find(cmd).Use(this, message);
                        Server.s.CommandUsed(this.name + " used /setpass");
                        return;
                    }
                    if ( cmd.ToLower() == "pass" ) {
                        Command.all.Find(cmd).Use(this, message);
                        Server.s.CommandUsed(this.name + " used /pass");
                        return;
                    }
                }
                if ( Server.agreetorulesonentry ) {
                    if ( cmd.ToLower() == "agree" ) {
                        Command.all.Find(cmd).Use(this, String.Empty);
                        Server.s.CommandUsed(this.name + " used /agree");
                        return;
                    }
                    if ( cmd.ToLower() == "rules" ) {
                        Command.all.Find(cmd).Use(this, String.Empty);
                        Server.s.CommandUsed(this.name + " used /rules");
                        return;
                    }
                    if ( cmd.ToLower() == "disagree" ) {
                        Command.all.Find(cmd).Use(this, String.Empty);
                        Server.s.CommandUsed(this.name + " used /disagree");
                        return;
                    }
                }

                if ( cmd == String.Empty ) { SendMessage("No command entered."); return; }

                if ( Server.agreetorulesonentry && !agreed ) {
                        SendMessage("You must read /rules then agree to them with /agree!");
                        return;
                }
                if ( jailed ) {
                    SendMessage("You cannot use any commands while jailed.");
                    return;
                }
                if ( Server.verifyadmins ) {
                    if ( this.adminpen ) {
                        this.SendMessage("&cYou must use &a/pass [Password]&c to verify!");
                        return;
                    }
                }

                //DO NOT REMOVE THE TWO COMMANDS BELOW, /PONY AND /RAINBOWDASHLIKESCOOLTHINGS. -EricKilla
                if ( cmd.ToLower() == "pony" ) {
                    if ( ponycount < 2 ) {
                        GlobalMessage(this.color + this.DisplayName + Server.DefaultColor + " just so happens to be a proud brony! Everyone give " + this.color + this.name + Server.DefaultColor + " a brohoof!");
                        ponycount += 1;
                    }
                    else {
                        SendMessage("You have used this command 2 times. You cannot use it anymore! Sorry, Brony!");
                    }
                    return;
                }
                if ( cmd.ToLower() == "rainbowdashlikescoolthings" ) {
                    if ( rdcount < 2 ) {
                        GlobalMessage("&1T&2H&3I&4S &5S&6E&7R&8V&9E&aR &bJ&cU&dS&eT &fG&0O&1T &22&30 &4P&CE&7R&DC&EE&9N&1T &5C&6O&7O&8L&9E&aR&b!");
                        rdcount += 1;
                    }
                    else {
                        SendMessage("You have used this command 2 times. You cannot use it anymore! Sorry, Brony!");
                    }
                    return;
                }

                string foundShortcut = Command.all.FindShort(cmd);
                if ( foundShortcut != "" ) cmd = foundShortcut;
                if ( OnCommand != null )
                    OnCommand(cmd, this, message);
                if ( PlayerCommand != null )
                    PlayerCommand(cmd, this, message);
                OnPlayerCommandEvent.Call(cmd, this, message);
                if ( cancelcommand ) {
                    cancelcommand = false;
                    return;
                }
                try {
                    int foundCb = int.Parse(cmd);
                    if ( messageBind[foundCb] == null ) { SendMessage("No CMD is stored on /" + cmd); return; }
                    message = messageBind[foundCb] + " " + message;
                    message = message.TrimEnd(' ');
                    cmd = cmdBind[foundCb];
                }
                catch { }
                Alias alias =  Alias.Find(cmd);
                if (alias != null)
                {
                    string[] pars = alias.Command.Split(new string[] { " " }, (int)2, StringSplitOptions.None);
                    try
                    {
                        Command.all.Find(pars[0]).Use(this, pars[1] + " " + message);
                    }
                    catch
                    { //pars[1] is empty/null
                        Command.all.Find(pars[0]).Use(this, message);
                    }
                    return;
                }
                Command command = Command.all.Find(cmd);
                //Group old = null;
                if ( command != null ) {
                    //this part checks if MCGalaxy staff are able to USE protection commands
                    /*if (isProtected && Server.ProtectOver.Contains(cmd.ToLower())) {
                        old = Group.findPerm(this.group.Permission);
                        this.group = Group.findPerm(LevelPermission.Nobody);
                    }*/

                    if ( group.CanExecute(command)) {
                        if ( cmd != "repeat" ) lastCMD = cmd + " " + message;
                        if ( level.name.Contains("Museum " + Server.DefaultColor) ) {
                            if ( !command.museumUsable ) {
                                SendMessage("Cannot use this command while in a museum!");
                                return;
                            }
                        }
                        if ( this.joker || this.muted ) {
                            if ( cmd.ToLower() == "me" ) {
                                SendMessage("Cannot use /me while muted or jokered.");
                                return;
                            }
                        }
                        if ( cmd.ToLower() != "setpass" || cmd.ToLower() != "pass" ) {
                            Server.s.CommandUsed(name + " used /" + cmd + " " + message);
                        }

                        try { //opstats patch (since 5.5.11)
                            if (Server.opstats.Contains(cmd.ToLower()) || (cmd.ToLower() == "review" && message.ToLower() == "next" && Server.reviewlist.Count > 0)) {
                                Database.AddParams("@Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                Database.AddParams("@Name", name);
                                Database.AddParams("@Cmd", cmd);
                                Database.AddParams("@Cmdmsg", message);
                                Database.executeQuery("INSERT INTO Opstats (Time, Name, Cmd, Cmdmsg) VALUES (@Time, @Name, @Cmd, @Cmdmsg)");
                            }
                        } catch { }

                        Thread commThread = new Thread(new ThreadStart(delegate {
                            try {
                                command.Use(this, message);
                            } catch (Exception e) {
                                Server.ErrorLog(e);
                                Player.SendMessage(this, "An error occured when using the command!");
                                Player.SendMessage(this, e.GetType().ToString() + ": " + e.Message);
                            }
                            //finally { if (old != null) this.group = old; }
                        }));
                        commThread.Name = "MCG_Command";
                        commThread.Start();
                    }
                    else { SendMessage("You are not allowed to use \"" + cmd + "\"!"); }
                }
                else if ( Block.Byte(cmd.ToLower()) != Block.Zero ) {
                    HandleCommand("mode", cmd.ToLower());
                }
                else {
                    bool retry = true;

                    switch ( cmd.ToLower() ) { //Check for command switching
                        case "guest": message = message + " " + cmd.ToLower(); cmd = "setrank"; break;
                        case "builder": message = message + " " + cmd.ToLower(); cmd = "setrank"; break;
                        case "advbuilder":
                        case "adv": message = message + " " + cmd.ToLower(); cmd = "setrank"; break;
                        case "operator":
                        case "op": message = message + " " + cmd.ToLower(); cmd = "setrank"; break;
                        case "super":
                        case "superop": message = message + " " + cmd.ToLower(); cmd = "setrank"; break;
                        case "cut": cmd = "copy"; message = "cut"; break;
                        case "admins": message = "superop"; cmd = "viewranks"; break;
                        case "ops": message = "op"; cmd = "viewranks"; break;
                        case "banned": message = cmd; cmd = "viewranks"; break;

                        case "ps": message = "ps " + message; cmd = "map"; break;

                        //How about we start adding commands from other softwares
                        //and seamlessly switch here?
                        case "bhb":
                        case "hbox": cmd = "cuboid"; message = "hollow"; break;
                        case "blb":
                        case "box": cmd = "cuboid"; break;
                        case "sphere": cmd = "spheroid"; break;
                        case "cmdlist":
                        case "commands": cmd = "help"; message = "old"; break;
                        case "cmdhelp": cmd = "help"; break;
                        case "worlds":
                        case "mapsave": cmd = "save"; break;
                        case "mapload": cmd = "load"; break;
                        case "colour": cmd = "color"; break;
                        case "materials": cmd = "blocks"; break;
                        case "zz": cmd = "static"; message = "cuboid " + message; break;
                        case "fetch": cmd = "summon"; break;
                        case "ranks": cmd = "help"; message = "ranks"; break;

                        default: retry = false; break; //Unknown command, then
                    }

                    if ( retry ) HandleCommand(cmd, message);
                    else SendMessage("Unknown command \"" + cmd + "\"!");
                }
            }
            catch ( Exception e ) { Server.ErrorLog(e); SendMessage("Command failed."); }
        }
        
        #endregion
        #region == GLOBAL MESSAGES ==
        
        public static void GlobalBlockchange(Level level, int b, byte type, byte extType) {
            ushort x, y, z;
            level.IntToPos(b, out x, out y, out z);
            GlobalBlockchange(level, x, y, z, type, extType);
        }
        
        public static void GlobalBlockchange(Level level, ushort x, ushort y, ushort z, byte type, byte extType) {
            PlayerInfo.players.ForEach(delegate(Player p) { if ( p.level == level ) { p.SendBlockchange(x, y, z, type, extType); } });
        }

        // THIS IS NOT FOR SENDING GLOBAL MESSAGES!!! IT IS TO SEND A MESSAGE FROM A SPECIFIED PLAYER!!!!!!!!!!!!!!
        public static void GlobalChat(Player from, string message) { GlobalChat(from, message, true); }
        public static void GlobalChat(Player from, string message, bool showname) {
            if ( from == null ) return; // So we don't fucking derp the hell out!

            if ( Server.lava.HasPlayer(from) && Server.lava.HasVote(message.ToLower()) ) {
                if ( Server.lava.AddVote(from, message.ToLower()) ) {
                    SendMessage(from, "Your vote for &5" + message.ToLower().Capitalize() + Server.DefaultColor + " has been placed. Thanks!");
                    Server.lava.map.ChatLevelOps(from.name + " voted for &5" + message.ToLower().Capitalize() + Server.DefaultColor + ".");
                    return;
                } else {
                    SendMessage(from, "&cYou already voted!");
                    return;
                }
            }

            if (Server.voting) {
            	string test = message.ToLower();
            	if (CheckVote(test, from, "y", "yes", ref Server.YesVotes) ||
            	    CheckVote(test, from, "n", "no", ref Server.NoVotes)) return;
            	
            	if (!from.voice && (test == "y" || test == "n" || test == "yes" || test == "no")) {
            		from.SendMessage("Chat moderation is on while voting is on!"); return;
                }
            }

            if (Server.votingforlevel && Server.zombie.HandlesChatMessage(from, message))
            	return;
            
            if (Last50Chat.Count() == 50)
                Last50Chat.RemoveAt(0);
            var chatmessage = new ChatMessage();
            chatmessage.text = message;
            chatmessage.username = from.color + from.name;
            chatmessage.time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");


            Last50Chat.Add(chatmessage);
            if ( showname ) {
                String referee = "";
                if ( from.referee ) {
                    referee = Colors.green + "[Referee] ";
                }
                message = referee + from.color + from.voicestring + from.color + from.prefix + from.DisplayName + ": %r&f" + message;
            }
            PlayerInfo.players.ForEach(delegate(Player p) {
                if ( p.level.worldChat && p.Chatroom == null ) {
                    if ( p.ignoreglobal == false ) {
                        if ( from != null ) {
                            if ( !p.listignored.Contains(from.name) ) {
                                Player.SendMessage(p, message);
                                return;
                            }
                            return;
                        }
                        Player.SendMessage(p, message);
                        return;
                    }
                    if ( Server.globalignoreops == false ) {
                        if ( from.group.Permission >= Server.opchatperm ) {
                            if ( p.group.Permission < from.group.Permission ) {
                                Player.SendMessage(p, message);
                            }
                        }
                    }
                    if ( from != null ) {
                        if ( from == p ) {
                            Player.SendMessage(from, message);
                            return;
                        }
                    }
                }
            });
        }

        public static bool CommandHasBadColourCodes(Player who, string message) {
            string[] checkmessagesplit = message.Split(' ');
            bool lastendwithcolour = false;
            foreach ( string s in checkmessagesplit ) {
                s.Trim();
                if ( s.StartsWith("%") ) {
                    if ( lastendwithcolour ) {
                        if ( who != null ) {
                            who.SendMessage("Sorry, Your colour codes in this command were invalid (You cannot use 2 colour codes next to each other");
                            who.SendMessage("Command failed.");
                            Server.s.Log(who.name + " attempted to send a command with invalid colours codes (2 colour codes were next to each other)!");
                            Chat.GlobalMessageOps(who.color + who.DisplayName + " " + Server.DefaultColor + " attempted to send a command with invalid colours codes (2 colour codes were next to each other)!");
                        }
                        return true;
                    }
                    else if ( s.Length == 2 ) {
                        lastendwithcolour = true;
                    }
                }
                if ( s.TrimEnd(Server.ColourCodesNoPercent).EndsWith("%") ) {
                    lastendwithcolour = true;
                }
                else {
                    lastendwithcolour = false;
                }

            }
            return false;
        }

        public static List<ChatMessage> Last50Chat = new List<ChatMessage>();
        public static void GlobalMessage(string message) {
            GlobalMessage(message, false);
        }
        public static void GlobalMessage(string message, bool global) {
            if ( !global )
                //message = message.Replace("%", "&");
                message = Colors.EscapeColors(message);
            else
                message = message.Replace("%G", Server.GlobalChatColor);
            PlayerInfo.players.ForEach(delegate(Player p) {
                if ( p.level.worldChat && p.Chatroom == null && ( !global || !p.muteGlobal ) ) {
                    Player.SendMessage(p, message, !global);
                }
            });
        }
        
        public static void GlobalSpawn(Player from, ushort x, ushort y, ushort z, byte rotx, byte roty, bool self, string possession = "")
        {
            PlayerInfo.players.ForEach(delegate(Player p)
            {
                if (p.Loading && p != from) { return; }
                if (p.level != from.level || (from.hidden && !self)) { return; }
                
                if (p != from)
                {
                    if (Server.ZombieModeOn && !p.aka) {
                        if (from.infected) {
                            if (Server.ZombieName != "")
                                p.SendSpawn(from.id, Colors.red + Server.ZombieName + possession, x, y, z, rotx, roty);
                            else
                                p.SendSpawn(from.id, Colors.red + from.name + possession, x, y, z, rotx, roty);
                        } else if (!from.referee) {
                            p.SendSpawn(from.id, from.color + from.name + possession, x, y, z, rotx, roty);
                        }
                    } else {
                        p.SendSpawn(from.id, from.color + from.name + possession, x, y, z, rotx, roty);
                    }
                }
                else if (self)
                {
                    p.pos = new ushort[3] { x, y, z }; p.rot = new byte[2] { rotx, roty };
                    p.oldpos = p.pos; p.basepos = p.pos; p.oldrot = p.rot;
                    p.SendSpawn(0xFF, from.color + from.name + possession, x, y, z, rotx, roty);
                }
            });
        }
        public static void GlobalDespawn(Player from, bool self) {
            PlayerInfo.players.ForEach(delegate(Player p) {
                if ( p.level != from.level || ( from.hidden && !self ) ) { return; }
                if ( p != from ) { p.SendDespawn(from.id); }
                else if ( self ) { p.SendDespawn(255); }
            });
        }

        public bool MarkPossessed(string marker = "") {
            if ( marker != "" ) {
                Player controller = PlayerInfo.Find(marker);
                if ( controller == null ) {
                    return false;
                }
                marker = " (" + controller.color + controller.name + color + ")";
            }
            GlobalDespawn(this, true);
            GlobalSpawn(this, pos[0], pos[1], pos[2], rot[0], rot[1], true, marker);
            return true;
        }

        public static void GlobalUpdate() { 
            PlayerInfo.players.ForEach(
                delegate(Player p) {
                    if ( !p.hidden ) 
                        p.UpdatePosition();
                });
        }
        #endregion
        #region == DISCONNECTING ==
        public void Disconnect() { leftGame(); }
        public void Kick(string kickString, bool sync = false) { leftGame(kickString, sync); }

        public void leftGame(string kickString = "", bool sync = false) {

            OnPlayerDisconnectEvent.Call(this, kickString);

            //Umm...fixed?
            if ( name == "" ) {
                if ( socket != null )
                    CloseSocket();
                if ( connections.Contains(this) )
                    connections.Remove(this);
                SaveUndo();
                disconnected = true;
                return;
            }
            Server.reviewlist.Remove(name);
            
            try {
 
                if ( disconnected ) {
                    this.CloseSocket();
                    if ( connections.Contains(this) )
                        connections.Remove(this);
                    return;
                }
                // FlyBuffer.Clear();
                disconnected = true;
                pingTimer.Stop();
                pingTimer.Dispose();
                if ( File.Exists("ranks/ignore/" + this.name + ".txt") ) {
                    try {
                        File.WriteAllLines("ranks/ignore/" + this.name + ".txt", this.listignored.ToArray());
                    }
                    catch {
                        Server.s.Log("Failed to save ignored list for player: " + this.name);
                    }
                }
                if ( File.Exists("ranks/ignore/GlobalIgnore.xml") ) {
                    try {
                        File.WriteAllLines("ranks/ignore/GlobalIgnore.xml", globalignores.ToArray());
                    }
                    catch {
                        Server.s.Log("failed to save global ignore list!");
                    }
                }
                afkTimer.Stop();
                afkTimer.Dispose();
                muteTimer.Stop();
                muteTimer.Dispose();
                timespent.Stop();
                timespent.Dispose();
                afkCount = 0;
                afkStart = DateTime.Now;

                if ( Server.afkset.Contains(name) ) Server.afkset.Remove(name);

                if ( kickString == "" ) kickString = "Disconnected.";

                SendKick(kickString, sync);


                if ( loggedIn ) {
                    isFlying = false;
                    aiming = false;

                    if ( team != null ) {
                        team.RemoveMember(this);
                    }

                    if ( Server.Countdown.players.Contains(this) ) {
                        if ( Server.Countdown.playersleftlist.Contains(this) ) {
                            Server.Countdown.PlayerLeft(this);
                        }
                        Server.Countdown.players.Remove(this);
                    }

                    TntWarsGame tntwarsgame = TntWarsGame.GetTntWarsGame(this);
                    if ( tntwarsgame != null ) {
                        tntwarsgame.Players.Remove(tntwarsgame.FindPlayer(this));
                        tntwarsgame.SendAllPlayersMessage("TNT Wars: " + color + name + Server.DefaultColor + " has left TNT Wars!");
                    }

                    GlobalDespawn(this, false);
                    if ( kickString == "Disconnected." || kickString.IndexOf("Server shutdown") != -1 || kickString == Server.customShutdownMessage ) {
                        if ( !Directory.Exists("text/logout") ) {
                            Directory.CreateDirectory("text/logout");
                        }
                        if ( !File.Exists("text/logout/" + name + ".txt") ) {
                            CP437Writer.WriteAllText("text/logout/" + name + ".txt", "Disconnected.");
                        }
                        if ( !hidden ) {
                            string leavem = "&c- " + color + prefix + DisplayName + Server.DefaultColor + " " + 
                                CP437Reader.ReadAllText("text/logout/" + name + ".txt");
                            if ((Server.guestLeaveNotify && group.Permission <= LevelPermission.Guest) || group.Permission > LevelPermission.Guest)
                            {
                                PlayerInfo.players.ForEach(p1 => Player.SendMessage(p1, leavem));
                            }
                        }
                        //IRCBot.Say(name + " left the game.");
                        Server.s.Log(name + " disconnected.");
                    }
                    else {
                        totalKicked++;
                        GlobalChat(this, "&c- " + color + prefix + DisplayName + Server.DefaultColor + " kicked (" + kickString + Server.DefaultColor + ").", false);
                        //IRCBot.Say(name + " kicked (" + kickString + ").");
                        Server.s.Log(name + " kicked (" + kickString + ").");
                    }

                    try { save(); }
                    catch ( Exception e ) { Server.ErrorLog(e); }

                    PlayerInfo.players.Remove(this);
                    Server.s.PlayerListUpdate();
                    try {
                        left.Add(this.name.ToLower(), this.ip);
                    }
                    catch ( Exception ) {
                        //Server.ErrorLog(e);
                    }

                    /*if (Server.AutoLoad && level.unload)
{

foreach (Player pl in PlayerInfo.players)
if (pl.level == level) hasplayers = true;
if (!level.name.Contains("Museum " + Server.DefaultColor) && hasplayers == false)
{
level.Unload();
}
}*/

                    if ( Server.AutoLoad && level.unload && !level.name.Contains("Museum " + Server.DefaultColor) && IsAloneOnCurrentLevel() )
                        level.Unload(true);

                    if ( PlayerDisconnect != null )
                        PlayerDisconnect(this, kickString);

                    this.Dispose();
                }
                else {
                    connections.Remove(this);

                    Server.s.Log(ip + " disconnected.");
                }

                Server.zombie.PlayerLeftServer(this);

            }
            catch ( Exception e ) { Server.ErrorLog(e); }
            finally {
                CloseSocket();
            }
        }

        public void SaveUndo() { SaveUndo(this); }
        
        public static void SaveUndo(Player p) {
            try {
                UndoFile.SaveUndo(p);
            } catch (Exception e) { 
                Server.s.Log("Error saving undo data for " + p.name + "!"); Server.ErrorLog(e); 
            }
        }

        public void Dispose() {
            //throw new NotImplementedException();
            if ( connections.Contains(this) ) connections.Remove(this);
            Extras.Clear();
            if (CopyBuffer != null)
                CopyBuffer.Clear();
            RedoBuffer.Clear();
            UndoBuffer.Clear();
            spamBlockLog.Clear();
            //spamChatLog.Clear();
            spyChatRooms.Clear();
            /*try
{
//this.commThread.Abort();
}
catch { }*/
        }
        //fixed undo code
        public bool IsAloneOnCurrentLevel() {
            return PlayerInfo.players.All(pl => pl.level != level || pl == this);
        }

        #endregion
        #region == OTHER ==
        
        [Obsolete]
        public static List<Player> players { get { return PlayerInfo.players; } }
        
        [Obsolete]
        public static Player Find(string name) { return PlayerInfo.Find(name); }
        
        [Obsolete]
        public static Player FindExact(string name) { return PlayerInfo.FindExact(name); }
        
        [Obsolete]
        public static Player FindNick(string name) { return PlayerInfo.FindNick(name); }
        
        static byte FreeId() {
            /*
for (byte i = 0; i < 255; i++)
{
foreach (Player p in players)
{
if (p.id == i) { goto Next; }
} return i;
Next: continue;
} unchecked { return 0xFF; }*/

            for ( byte i = 0; i < 255; i++ ) {
                bool used = PlayerInfo.players.Any(p => p.id == i);

                if ( !used )
                    return i;
            }
            return (byte)1;
        }

        // TODO: Optimize this using a StringBuilder
        static List<string> Wordwrap(string message) {
            List<string> lines = new List<string>();
            message = Regex.Replace(message, @"(&[0-9a-f])+(&[0-9a-f])", "$2");
            message = Regex.Replace(message, @"(&[0-9a-f])+$", "");

            int limit = 64; string color = "";
            while ( message.Length > 0 ) {
                //if (Regex.IsMatch(message, "&a")) break;

                if ( lines.Count > 0 ) {
                    if ( message[0].ToString() == "&" )
                        message = "> " + message.Trim();
                    else
                        message = "> " + color + message.Trim();
                }

                if ( message.IndexOf("&") == message.IndexOf("&", message.IndexOf("&") + 1) - 2 )
                    message = message.Remove(message.IndexOf("&"), 2);

                if ( message.Length <= limit ) { lines.Add(message); break; }
                for ( int i = limit - 1; i > limit - 20; --i )
                    if ( message[i] == ' ' ) {
                        lines.Add(message.Substring(0, i));
                        goto Next;
                    }

            retry:
                if ( message.Length == 0 || limit == 0 ) { return lines; }

                try {
                    if ( message.Substring(limit - 2, 1) == "&" || message.Substring(limit - 1, 1) == "&" ) {
                        message = message.Remove(limit - 2, 1);
                        limit -= 2;
                        goto retry;
                    }
                    else if ( message[limit - 1] < 32 || message[limit - 1] > 127 ) {
                        message = message.Remove(limit - 1, 1);
                        limit -= 1;
                        //goto retry;
                    }
                }
                catch { return lines; }
                lines.Add(message.Substring(0, limit));

            Next: message = message.Substring(lines[lines.Count - 1].Length);
                if ( lines.Count == 1 ) limit = 60;

                int index = lines[lines.Count - 1].LastIndexOf('&');
                if ( index != -1 ) {
                    if ( index < lines[lines.Count - 1].Length - 1 ) {
                        char next = lines[lines.Count - 1][index + 1];
                        if ( Colors.MapColor(ref next) ) color = "&" + next;
                        if ( index == lines[lines.Count - 1].Length - 1 ) {
                            lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 2);
                        }
                    }
                    else if ( message.Length != 0 ) {
                        char next = message[0];
                        if ( Colors.MapColor(ref next) ) color = "&" + next;
                        lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 1);
                        message = message.Substring(1);
                    }
                }
            }
            char[] temp;
            for ( int i = 0; i < lines.Count; i++ ) // Gotta do it the old fashioned way...
            {
                temp = lines[i].ToCharArray();
                if ( temp[temp.Length - 2] == '%' || temp[temp.Length - 2] == '&' ) {
                    temp[temp.Length - 1] = ' ';
                    temp[temp.Length - 2] = ' ';
                }
                StringBuilder message1 = new StringBuilder();
                message1.Append(temp);
                lines[i] = message1.ToString();
            }
            return lines;
        }
        public static bool ValidName(string name) {
            string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890._+";
            return name.All(ch => allowedchars.IndexOf(ch) != -1);
        }

        public static int GetBannedCount() {
            try {
                return File.ReadAllLines("ranks/banned.txt").Length;
            }
            catch/* (Exception ex)*/
            {
                return 0;
            }
        }
        #endregion

        public void BlockUntilLoad(int sleep) {
            while (Loading) 
                Thread.Sleep(sleep);
        }
        public void RevertBlock(ushort x, ushort y, ushort z) {
            byte b = level.GetTile(x, y, z);
            SendBlockchange(x, y, z, b);
        }
        
        bool CheckBlockSpam() {
            if ( spamBlockLog.Count >= spamBlockCount ) {
                DateTime oldestTime = spamBlockLog.Dequeue();
                double spamTimer = DateTime.Now.Subtract(oldestTime).TotalSeconds;
                if ( spamTimer < spamBlockTimer && !ignoreGrief ) {
                    this.Kick("You were kicked by antigrief system. Slow down.");
                    SendMessage(Colors.red + DisplayName + " was kicked for suspected griefing.");
                    Server.s.Log(name + " was kicked for block spam (" + spamBlockCount + " blocks in " + spamTimer + " seconds)");
                    return true;
                }
            }
            spamBlockLog.Enqueue(DateTime.Now);
            return false;
        }

        public static bool IPInPrivateRange(string ip) {
            //range of 172.16.0.0 - 172.31.255.255
            if (ip.StartsWith("172.") && (int.Parse(ip.Split('.')[1]) >= 16 && int.Parse(ip.Split('.')[1]) <= 31))
                return true;
            return IPAddress.IsLoopback(IPAddress.Parse(ip)) || ip.StartsWith("192.168.") || ip.StartsWith("10.");
            //return IsLocalIpAddress(ip);
        }

        /*public string ResolveExternalIP(string ip) {
            HTTPGet req = new HTTPGet();
            req.Request("http://checkip.dyndns.org");
            string[] a1 = req.ResponseBody.Split(':');
            string a2 = a1[1].Substring(1);
            string[] a3 = a2.Split('<');
            return a3[0];
        }*/

        public static bool IsLocalIpAddress(string host) {
            try { // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach ( IPAddress hostIP in hostIPs ) {
                    // is localhost
                    if ( IPAddress.IsLoopback(hostIP) ) return true;
                    // is local address
                    foreach ( IPAddress localIP in localIPs ) {
                        if ( hostIP.Equals(localIP) ) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public bool EnoughMoney(int amount) {
            if (this.money >= amount)
                return true;
            return false;
        }
        public void ReviewTimer() {
            this.canusereview = false;
            System.Timers.Timer Clock = new System.Timers.Timer(1000 * Server.reviewcooldown);
            Clock.Elapsed += delegate { this.canusereview = true; Clock.Dispose(); };
            Clock.Start();
        }

        public void TntAtATime() {
            new Thread(() => {
                CurrentAmountOfTnt += 1;
                switch ( TntWarsGame.GetTntWarsGame(this).GameDifficulty ) {
                    case TntWarsGame.TntWarsDifficulty.Easy:
                        Thread.Sleep(3250);
                        break;

                    case TntWarsGame.TntWarsDifficulty.Normal:
                        Thread.Sleep(2250);
                        break;

                    case TntWarsGame.TntWarsDifficulty.Hard:
                    case TntWarsGame.TntWarsDifficulty.Extreme:
                        Thread.Sleep(1250);
                        break;
                }
                CurrentAmountOfTnt -= 1;
            }).Start();
        }
        
        public static bool BlacklistCheck(string name, string foundLevel)
        {
            string path = "levels/blacklists/" + foundLevel + ".txt";
            if (!File.Exists(path)) { return false; }
            if (File.ReadAllText(path).Contains(name)) { return true; }
            return false;
        }
        
        public static string GetIPLocation(string IP)
        {
            string direction;
            string direction2;
            string city = "http://ipinfo.io/" + IP + "/city";
            string country = "http://ipinfo.io/" + IP + "/country";
            string replacement;
            string replacement2;
            WebRequest requestcity = WebRequest.Create(city);
            WebRequest requestcountry = WebRequest.Create(country);
            using (WebResponse response1 = requestcity.GetResponse())
            using (StreamReader stream = new StreamReader(response1.GetResponseStream()))
            {
                direction = stream.ReadToEnd();
                replacement = Regex.Replace(direction, @"\n", "");
                if (replacement == "")
                {
                    replacement = "Unknown";
                }
            }
            using (WebResponse response2 = requestcountry.GetResponse())
            using (StreamReader stream2 = new StreamReader(response2.GetResponseStream()))
            {
                direction2 = stream2.ReadToEnd();
                replacement2 = Regex.Replace(direction2, @"\n", "");
            }
            return replacement + "/" + replacement2;
        }
        
        internal static bool CheckVote(string message, Player p, string a, string b, ref int totalVotes) {
            if (!p.voted && (message == a || message == b)) {
                totalVotes++;
                p.SendMessage(Colors.red + "Thanks for voting!");
                p.voted = true;
                return true;
            }
            return false;
        }
    }
}
