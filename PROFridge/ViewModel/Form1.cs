﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using Memory;
using PROFridge.Annotations;

namespace PROFridge
{
    public partial class Form1 : Form, INotifyPropertyChanged
    {

        // TODO
        


        public Form1()
        {
            InitializeComponent();
        }

        public bool startBot = false;

        private int abilityPP1;
        public int abilityPP2;
        public int abilityPP3;
        public int abilityPP4;
        public bool PPCheck = false;
        public bool needHeal = false;

        private string _status;
        public int pID;
        public bool openProc;

        private int _currentHealth;
        private int _currentMaxHealth;
        private int _enemyCurrentHealth;
        private int _enemyMaxHealth;
        private int _encounterPokeIndex;
        private int _farmSpecifPokeID;
        private float _xPos;
        private float _yPos;
        private int _PokeDollars;
        private int _isFight;

        public float startCoordX;
        public float startCoordY;

        public Mem m = new Mem();

        InputSimulator sim = new InputSimulator();

        public RamGecTools.KeyboardHook KeyboardHook = new RamGecTools.KeyboardHook();

        private void Form1_Load(object sender, EventArgs e)
        {
            KeyboardHook.KeyDown += new RamGecTools.KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyDown);
            // KeyboardHook.KeyUp += new RamGecTools.KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyUp);
            KeyboardHook.Install();

            // Get Process ID
            int pID = m.getProcIDFromName("PROClient");
            
            // check
            openProc = false;

            if (pID > 0) 
            {
                m.OpenProcess(pID);
                openProc = true;
            }

            
            OnTickMemoryRead();


            try
            {
                AbilityPP1 = Int32.Parse(txtbx_abilityPP1.Text);


            }
            catch (Exception exception)
            {

            }

            if (chkbx_farmSpecifPoke.Checked)
            {
                FarmSpecifPokeID = Int32.Parse(txtbx_farmSpcifPoke.Text);
            }

            if (!chkbx_UseFishRod.Checked)
            {
                Move();
            }

            if (chkbx_UseFishRod.Checked)
            {
                Fishing();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
                        
        }

        // Continuesly reading memory and updating the UI
        private async void OnTickMemoryRead()
        {
            if (openProc)
            {
                CurrentHealth = m.readInt("GameAssembly.dll+00A427F8,0xB8,0x0,0x158,0x1A8,0x38");
                txtbx_currentHealth.Text = CurrentHealth.ToString();

                EnemyCurrentHealth = m.readInt("GameAssembly.dll+00A42B10,0x50,0x40,0x48,0xB8,0x0,0xA04");
                txtbx_enemyCurrentHealth.Text = EnemyCurrentHealth.ToString();

                IsFight= m.readInt("GameAssembly.dll+00A68178,0x210,0x528,0x548,0x88,0x768");
                txtbx_fightState.Text = IsFight.ToString();

                EncounterPokeIndex = m.readInt("UnityPlayer.dll+014B8980,0x48,0x118,0x138,0x60,0x138,0x9FC");
                txtbx_pokemonID.Text = EncounterPokeIndex.ToString();

                PokeDollars = m.readInt("GameAssembly.dll+00A508B0,0xB8,0x48,0xB8,0x298,0x290");
                txtbx_pokeDollar.Text = PokeDollars.ToString();
                
                XPos = m.readFloat("GameAssembly.dll+00A427F8,0x48,0xB8,0x0,0x21C");
                txtbx_xPos.Text = XPos.ToString();

                YPos= m.readFloat("GameAssembly.dll+00A427F8,0x48,0xB8,0x0,0x220");
                txtbx_yPos.Text = YPos.ToString();

                txtbx_currentPP1.Text = AbilityPP1.ToString();

                await Task.Delay(10);
                
                OnTickMemoryRead();

            }
        }


        public void SnapshotCoords()
        {
            startCoordX = m.readFloat("GameAssembly.dll+00A427F8,0x48,0xB8,0x0,0x21C");
            
            startCoordY = m.readFloat("GameAssembly.dll+00A427F8,0x48,0xB8,0x0,0x220");
            
        }


        public async void Move()
        {
            if (IsFight == 0 && startBot)
            {
                txtbx_status.Text = Status = "World";

                if (startCoordX < XPos+1)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                    XPos--;
                    sim.Keyboard.Sleep(500);
                }

                if (startCoordX > XPos -1)
                {
                   
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_D);
                    XPos++;
                    sim.Keyboard.Sleep(500);
                }

            }

            if (IsFight != 0 && EncounterPokeIndex != 0)
            {
                txtbx_status.Text = Status = "Pokemon Encountered";

                FightLogic();
            }



            await Task.Delay(500);
            Move();
        }

        public async void Fishing()
        {
            if (IsFight == 0)
            {
                txtbx_status.Text = Status = "World Fishing";
                

                sim.Keyboard.Sleep(500);

                sim.Keyboard.KeyPress(VirtualKeyCode.VK_6);

            }

            if (IsFight != 0 && EncounterPokeIndex != 0)
            {
                txtbx_status.Text = Status = "Pokemon Encountered";

                FightLogic();

            }



            await Task.Delay(500);
            Fishing();
        }


        // Fight logic
        public void FightLogic()
        {
            if (IsFight == 7 && AbilityPP1 > 0)
            {
                txtbx_status.Text = Status = "Lets Fight";



                
                // FARMING BOT LOGIC

                if (EnemyCurrentHealth > 1 && chkbx_farmSpecifPoke.Checked)
                {
                    if (EncounterPokeIndex == FarmSpecifPokeID)
                    {
                        txtbx_status.Text = "Found: " + EncounterPokeIndex;

                        // Specific Poke to LOW
                        txtbx_status.Text = Status = "Fighting";

                        sim.Keyboard.Sleep(1000);

                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);

                        sim.Keyboard.Sleep(1000);

                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);

                        AbilityPP1--;

                    }
                    else if (EncounterPokeIndex != FarmSpecifPokeID)
                    {
                        txtbx_status.Text = "Found wrong Pokemon: " + EncounterPokeIndex + ", LEAVING!";

                        // Leave Logic
                        sim.Keyboard.Sleep(1000);

                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_4);

                        sim.Keyboard.Sleep(1000);
                    }
                    
                }
                else if (EnemyCurrentHealth == 1 && chkbx_farmSpecifPoke.Checked)
                {
                    txtbx_status.Text = "Catching Specific Poke: " + EncounterPokeIndex;
                    // Catch Logic
                }









                // LEVELING BOT LOGIC

                if (EnemyCurrentHealth > 0 && !chkbx_farmSpecifPoke.Checked)
                {
                    txtbx_status.Text = Status = "Fighting";

                    sim.Keyboard.Sleep(1000);

                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);

                    sim.Keyboard.Sleep(1000);

                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);

                    AbilityPP1--;
                }
                
            }

        }

        async void KeyboardHook_KeyDown(RamGecTools.KeyboardHook.VKeys key)
        {
            //Console.WriteLine(key.ToString());
            if (key.ToString() == "F5")
            {
                SnapshotCoords();
                startBot = true;
                txtbx_status.Text = "Go";

            }
            else if (key.ToString() == "F6")
            {
                startBot = false;
                txtbx_status.Text = "Stopped";
            }
            
        }





        


        public string EnemyPokemon(int id)
        {
            switch (id)
            {
                case 1:
                    return "";
                case 2:
                    return "";
                case 3:
                    return "";
                case 4:
                    return "";
                case 5:
                    return "";
                case 6:
                    return "";
                case 7:
                    return "";
                case 8:
                    return "";
                case 9:
                    return "";

                // In the end ALL pokemon names
            }

            return id.ToString();
        }

        public int CurrentHealth
        {
            get { return _currentHealth; }
            set { _currentHealth = value; }
        }

        public int CurrentMaxHealth
        {
            get { return _currentMaxHealth; }
            set { _currentMaxHealth = value; }
        }

        public int EnemyCurrentHealth
        {
            get { return _enemyCurrentHealth; }
            set { _enemyCurrentHealth = value; }
        }

        public int EnemyMaxHealth
        {
            get { return _enemyMaxHealth; }
            set { _enemyMaxHealth = value; }
        }

        public int EncounterPokeIndex
        {
            get { return _encounterPokeIndex; }
            set { _encounterPokeIndex = value; }
        }

        public float XPos
        {
            get { return _xPos; }
            set { _xPos = value; }
        }

        public float YPos
        {
            get { return _yPos; }
            set { _yPos = value; }
        }

        public int PokeDollars
        {
            get { return _PokeDollars; }
            set { _PokeDollars = value; }
        }

        public int IsFight
        {
            get { return _isFight; }
            set { _isFight = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int AbilityPP1
        {
            get { return abilityPP1; }
            set { abilityPP1 = value; }
        }

        public int FarmSpecifPokeID
        {
            get { return _farmSpecifPokeID; }
            set { _farmSpecifPokeID = value; }
        }


        #region OnPropChange

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        private void txtbx_farmSpcifPoke_TextChanged(object sender, EventArgs e)
        {

        }
    }
}