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
using System.IO;
using System.Timers;
using MCGalaxy.SQL;

namespace MCGalaxy {
    public sealed partial class Player : IDisposable {

        void InitTimers() {
            timespent.Elapsed += TimeSpentElapsed;
            timespent.Start();
            
            loginTimer.Elapsed += LoginTimerElapsed;
            loginTimer.Start();
            extraTimer.Elapsed += ExtraTimerElapsed;
            
            pingTimer.Elapsed += delegate { SendPing(); };
            pingTimer.Start();
            
            afkTimer.Elapsed += AfkTimerElapsed;
            if (Server.afkminutes > 0)
                afkTimer.Start();
        }

        static readonly TimeSpan delta = TimeSpan.FromSeconds(1);
        void TimeSpentElapsed(object sender, ElapsedEventArgs e) {
            time = time.Add(delta);
        }
        
        void LoginTimerElapsed(object sender, ElapsedEventArgs e) {
            if ( !Loading ) {
                loginTimer.Stop();
                if ( File.Exists("text/welcome.txt") ) {
                    try {
                        List<string> welcome = CP437Reader.ReadAllLines("text/welcome.txt");
                        foreach (string w in welcome)
                            SendMessage(w);
                    } catch {
                    }
                } else {
                    Server.s.Log("Could not find Welcome.txt. Using default.");
                    CP437Writer.WriteAllText("text/welcome.txt", "Welcome to my server!");
                    SendMessage("Welcome to my server!");
                }
                loginTimer.Dispose();
                extraTimer.Start();
            }
        }
        
        void ExtraTimerElapsed(object sender, ElapsedEventArgs e) {
            extraTimer.Stop();

            try {
                if ( !Group.Find("Nobody").commands.Contains("inbox") && !Group.Find("Nobody").commands.Contains("send") ) {
                    //safe against SQL injections because no user input is given here
                    DataTable Inbox = Database.fillData("SELECT * FROM `Inbox" + name + "`", true);

                    SendMessage("&cYou have &f" + Inbox.Rows.Count + " &cmessages in /inbox");
                    Inbox.Dispose();
                }
            } catch {
            }
            
            if ( Server.updateTimer.Interval > 1000 )
                SendMessage("Lowlag mode is currently &aON.");
            if (Economy.Enabled)
                SendMessage("You currently have &a" + money + " %S" + Server.moneys);
            
            try {
                if ( !Group.Find("Nobody").commands.Contains("award") && !Group.Find("Nobody").commands.Contains("awards") && !Group.Find("Nobody").commands.Contains("awardmod") )
                    SendMessage("You have " + Awards.AwardAmount(name) + " awards.");
            } catch {
            }
            
            Player[] players = PlayerInfo.Online.Items;
            SendMessage("You have modified &a" + overallBlocks + " %Sblocks!");
            string suffix = players.Length == 1 ? " player online" : " players online";
            SendMessage("There are currently &a" + players.Length + suffix);
            
            if (Server.lava.active)
                SendMessage("There is a &aLava Survival %Sgame active! Join it by typing /ls go");
            extraTimer.Dispose();
        }
        
        void AfkTimerElapsed(object sender, ElapsedEventArgs e) {
            if ( name == "" ) return;

            if ( Server.afkset.Contains(name) ) {
                afkCount = 0;
                if ( Server.afkkick > 0 && group.Permission < Server.afkkickperm )
                    if ( afkStart.AddMinutes(Server.afkkick) < DateTime.Now )
                        Kick("Auto-kick, AFK for " + Server.afkkick + " minutes");
                if ( ( oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2] ) && ( oldrot[0] != rot[0] || oldrot[1] != rot[1] ) )
                    Command.all.Find("afk").Use(this, "");
            }
            else {
                if ( oldpos[0] == pos[0] && oldpos[1] == pos[1] && oldpos[2] == pos[2] && oldrot[0] == rot[0] && oldrot[1] == rot[1] )
                    afkCount++;
                else
                    afkCount = 0;

                if ( afkCount > Server.afkminutes * 30 ) {
                    if ( name != null && !String.IsNullOrEmpty(name.Trim()) && !hidden ) {
                        Command.all.Find("afk").Use(this, "auto: Not moved for " + Server.afkminutes + " minutes");
                        if ( AFK != null )
                            AFK(this);
                        if ( ONAFK != null )
                            ONAFK(this);
                        OnPlayerAFKEvent.Call(this);
                        afkCount = 0;
                    }
                }
            }
        }
    }
}
