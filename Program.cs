using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using Color = System.Drawing.Color;
using Version = System.Version;
using System.Net;
using System.Text.RegularExpressions;

namespace PerfectNami
{
    static class Program
    {
        public static AIHeroClient _Player { get { return ObjectManager.Player; } }

        public static Item Bilgewater, Randuin, QSS, Glory, FOTMountain, Mikael, IronSolari;
        public static Menu menu,ComboMenu,HarassMenu,AutoMenu,DrawMenu;
        public static AIHeroClient Target = null;
        public static List<string> DodgeSpells = new List<string>() { "LuxMaliceCannon", "LuxMaliceCannonMis", "EzrealtrueShotBarrage", "KatarinaR", "YasuoDashWrapper", "ViR", "NamiR", "ThreshQ", "xerathrmissilewrapper", "yasuoq3w", "UFSlash" };
        public static readonly Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 875, EloBuddy.SDK.Enumerations.SkillShotType.Circular, 250, 1000, 130);
        public static readonly Spell.Targeted W = new Spell.Targeted(SpellSlot.W, 725);
        public static readonly Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 800);
        public static readonly Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 2750, EloBuddy.SDK.Enumerations.SkillShotType.Linear, 250, 500, 160);

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Nami")
            {
            return;
            }

            Bilgewater = new Item(3144, 550);
            Randuin = new Item(3143, 500);
            Glory = new Item(3800);
            QSS = new Item(3140);
            FOTMountain = new Item(3401);
            Mikael = new Item(3222, 750);
            IronSolari = new Item(3190, 600);

            menu = MainMenu.AddMenu("Perfect Nami", "PerfectNami");
            ComboMenu = menu.AddSubMenu("Combo Settings", "ComboMenu");
            ComboMenu.Add("ComboUseQ", new CheckBox("Use Q"));
            ComboMenu.Add("ComboUseW", new CheckBox("Use W"));
            ComboMenu.Add("ComboUseE", new CheckBox("Use E"));
            ComboMenu.Add("ComboUseR", new CheckBox("Use R"));

            HarassMenu = menu.AddSubMenu("Harass Settings", "HarassMenu");
            HarassMenu.Add("HarassUseQ", new CheckBox("Use Q"));
            HarassMenu.Add("HarassUseW", new CheckBox("Use W"));
            HarassMenu.Add("HarassUseE", new CheckBox("Use E"));

            AutoMenu = menu.AddSubMenu("Auto Settings", "AutoMenu");
            AutoMenu.Add("AutoW", new CheckBox("Use W(Heal)"));
            AutoMenu.Add("AutoWV", new Slider("Ally Health < % ", 50, 1, 100));
            AutoMenu.Add("ManaToW", new Slider("Mana <  %", 30, 1, 100));
            AutoMenu.Add("AutoR", new CheckBox("Use Auto R"));
            AutoMenu.Add("AutoRCount", new Slider("Auto R Count >= ", 3, 1, 5));
            AutoMenu.Add("useItems", new CheckBox("Use Items"));
            AutoMenu.AddLabel("Mikael, FOT Mountain, Glory, Randuin, IronSolari");
            AutoMenu.Add("AutoQInterrupt", new CheckBox("Auto Q to Interrupt"));
            AutoMenu.AddLabel("e.g Katarina R");

            DrawMenu = menu.AddSubMenu("Draw Settings", "DrawMenu");
            DrawMenu.Add("DrawAA", new CheckBox("Draw AA"));
            DrawMenu.Add("DrawQ", new CheckBox("Draw Q"));
            DrawMenu.Add("DrawW", new CheckBox("Draw W"));
            DrawMenu.Add("DrawE", new CheckBox("Draw E"));
            DrawMenu.Add("DrawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
        }

        

        static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (DodgeSpells.Any(el => el == args.SData.Name))
            {
                if (Q.IsReady() && Q.IsInRange(sender))
                {
                    Q.Cast();
                }
                   
            }
        }
        
    
        //----------------------------------------------Drawing_OnDraw----------------------------------------

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!_Player.IsDead)
            {
                if (DrawMenu["DrawQ"].Cast<CheckBox>().CurrentValue)
                {
                    Drawing.DrawCircle(_Player.Position, Q.Range, Q.IsReady() ? Color.Gold : Color.Red);
                }
                if (DrawMenu["DrawW"].Cast<CheckBox>().CurrentValue)
                { 
                        Drawing.DrawCircle(_Player.Position, W.Range, W.IsReady() ? Color.Gold : Color.Red);
                }
                if (DrawMenu["DrawE"].Cast<CheckBox>().CurrentValue)
                { 
                        Drawing.DrawCircle(_Player.Position, E.Range, E.IsReady() ? Color.Gold : Color.Red);
                }
                if (DrawMenu["DrawR"].Cast<CheckBox>().CurrentValue)
                { 
                        Drawing.DrawCircle(_Player.Position, R.Range, R.IsReady() ? Color.Gold : Color.Red);
                }
            }
            return;
        }

        //-------------------------------------------Game_OnTick----------------------------------------------

        static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead) { return; }
            var useItems = AutoMenu["useItems"].Cast<CheckBox>().CurrentValue;
            if (_Player.CountEnemiesInRange(1000) > 0)
            {
                foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies)
                {
                    foreach (AIHeroClient ally in EntityManager.Heroes.Allies)
                    {
                        if (ally.IsFacing(enemy) && ally.HealthPercent <= 30 && _Player.Distance(ally) <= 750)
                        {
                            if (useItems && FOTMountain.IsReady())
                            {
                                FOTMountain.Cast(ally);
                            }


                            if ((useItems && ally.HasBuffOfType(BuffType.Charm) || ally.HasBuffOfType(BuffType.Fear) || ally.HasBuffOfType(BuffType.Poison) || ally.HasBuffOfType(BuffType.Polymorph) || ally.HasBuffOfType(BuffType.Silence) || ally.HasBuffOfType(BuffType.Sleep) || ally.HasBuffOfType(BuffType.Slow) || ally.HasBuffOfType(BuffType.Snare) || ally.HasBuffOfType(BuffType.Stun) || ally.HasBuffOfType(BuffType.Taunt)) && Mikael.IsReady())
                            {
                                Mikael.Cast(ally);
                            }
                            
                        }

                        if (ally.IsFacing(enemy) && ally.HealthPercent <= 30 && _Player.Distance(ally) <= 600)
                        {
                            if (useItems && IronSolari.IsReady())
                            {
                                IronSolari.Cast();
                            }
                        }
                    }
                }
            }

            if (!_Player.HasBuff("recall"))
            {
                foreach (AIHeroClient allys in EntityManager.Heroes.Allies)
                {
                    if (W.IsReady() && allys != _Player && EntityManager.Heroes.Allies.Where(ally => ally.HealthPercent <= AutoMenu["AutoWV"].Cast<Slider>().CurrentValue && W.IsInRange(ally)).Any() && _Player.ManaPercent >= AutoMenu["ManaToW"].Cast<Slider>().CurrentValue)
                    {                       
                            W.Cast(allys);
                    }
                }
            }


            

                    
                    
                   
                    //-------------------------------------------------Harass-------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    {
                        Target = TargetSelector.GetTarget(875, DamageType.Magical);
                var HarassUseQ = HarassMenu["HarassUseQ"].Cast<CheckBox>().CurrentValue;
                var HarassUseW = HarassMenu["HarassUseW"].Cast<CheckBox>().CurrentValue;
                var HarassUseE = HarassMenu["HarassUseE"].Cast<CheckBox>().CurrentValue;
                if (HarassUseQ && Q.IsReady() && Target.IsValidTarget(Q.Range - 20))
                        {
                            Q.Cast(Target);
                        }
                            

                        if (HarassUseW && W.IsReady() && Target.IsValidTarget(W.Range))
                        {
                            W.Cast(Target);
                        }
                        foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies)
                        {
                            foreach (AIHeroClient ally in EntityManager.Heroes.Allies)
                            {
                                if (HarassUseE && E.IsReady() && _Player.Distance(enemy) <= 1000 && _Player.Distance(ally) <= 725)
                                {
                                    E.Cast(ally);
                                }
                            }
                        }
                              
                    }

                    //---------------------------------------------------Combo--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                var ComboUseQ = ComboMenu["ComboUseQ"].Cast<CheckBox>().CurrentValue;
                var ComboUseW = ComboMenu["ComboUseW"].Cast<CheckBox>().CurrentValue;
                var ComboUseE = ComboMenu["ComboUseE"].Cast<CheckBox>().CurrentValue;
                var ComboUseR = ComboMenu["ComboUseR"].Cast<CheckBox>().CurrentValue;
                Target = TargetSelector.GetTarget(875, DamageType.Magical);
                        foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies)
                        {
                            foreach (AIHeroClient ally in EntityManager.Heroes.Allies)
                            {
                                if (ComboUseE && E.IsReady() && ally != _Player && _Player.Distance(enemy) <= 1000 && _Player.Distance(ally) <= 725)
                                {
                                    E.Cast(ally);
                                }
                        var AutoRCount = AutoMenu["AutoRCount"].Cast<Slider>().CurrentValue;
                        if (ComboUseR && enemy.IsFacing(ally) && _Player.CountEnemiesInRange(2000) > AutoRCount)
                                {
                                    R.Cast(enemy);
                                }
                            }

                        }
                        if (useItems && QSS.IsReady() && (_Player.HasBuffOfType(BuffType.Charm) || _Player.HasBuffOfType(BuffType.Blind) || _Player.HasBuffOfType(BuffType.Fear) || _Player.HasBuffOfType(BuffType.Polymorph) || _Player.HasBuffOfType(BuffType.Silence) || _Player.HasBuffOfType(BuffType.Sleep) || _Player.HasBuffOfType(BuffType.Snare) || _Player.HasBuffOfType(BuffType.Stun) || _Player.HasBuffOfType(BuffType.Suppression) || _Player.HasBuffOfType(BuffType.Taunt)))
                        {
                            QSS.Cast();
                        }                      
                        if (ComboUseQ && Q.IsReady() && Target.IsValidTarget(Q.Range))
                            Q.Cast(Target);

                        if (ComboUseW && W.IsReady() && Target.IsValidTarget(W.Range))
                            W.Cast(Target);

                        if (useItems && Target.IsValidTarget(Bilgewater.Range) && Bilgewater.IsReady())
                            Bilgewater.Cast(Target);

                        if (useItems && Target.IsValidTarget(Randuin.Range) && Randuin.IsReady())
                            Randuin.Cast();

                    }


                }
            

        
    }
}
