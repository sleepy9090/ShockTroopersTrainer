/**
 *  File           Form1.cs
 *  Brief          Shock Troopers (GOG Version) PC Trainer for use with version: setup_shock_troopers_gog-3_(12274).exe
 *  Copyright      2025 Shawn M. Crawford [sleepy]
 *  Date           11/28/2025
 *
 *  Author         Shawn M. Crawford [sleepy]
 *
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShockTroopersTrainer
{
    public partial class Form1 : Form
    {

        #region Imports

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

        #endregion

        #region Enums

        [Flags]
        private enum ProcessAccessRights
        {
            PROCESS_CREATE_PROCESS = 0x0080,
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_DUP_HANDLE = 0x0040,
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            PROCESS_SET_INFORMATION = 0x0200,
            PROCESS_SET_QUOTA = 0x0100,
            PROCESS_SUSPEND_RESUME = 0x0800,
            PROCESS_TERMINATE = 0x0001,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ = 0x0010,
            PROCESS_VM_WRITE = 0x0020,
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            SYNCHRONIZE = 0x00100000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            STANDARD_RIGHTS_REQUIRED = 0x000f0000,
            PROCESS_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF)
        }

        #endregion


        #region Constants

        private const string MODULE_NAME = "shocktro.exe";
        private const string PROCESS_NAME = "shocktro";

        // Level timer, Player 1, and Player 2 can use either of these base addresses
        private const int BASE_ADDRESS_1 = 0x000E3A08;
        private const int BASE_ADDRESS_2 = 0x000E3A0C;

        // Credits use a different base address
        private const int BASE_ADDRESS_3 = 0x000E3A10;

        private const int OFFSET_LEVEL_TIMER = 0x8D00;

        // Player 1
        private const int OFFSET_P1_CREDITS_COUNT = 0x0034;
        private const int OFFSET_P1_HEALTH_COUNT = 0x1B4;
        private const int OFFSET_P1_INVINCIBILITY_TIMER = 0x0126;
        private const int OFFSET_P1_SCORE = 0x8DB2;
        private const int OFFSET_P1_VEHICLE_HEALTH_COUNT = 0;
        private const int OFFSET_P1_VEHICLE_AMMO_CANON_COUNT = 0;
        private const int OFFSET_P1_WEAPON_TYPE = 0x8DB8;
        private const int OFFSET_P1_BOMB_TBC1_COUNT = 0x8DC2;
        private const int OFFSET_P1_BOMB_TBC2_COUNT = 0x8DC6;
        private const int OFFSET_P1_BOMB_TBC3_COUNT = 0x8DCA;
        private const int OFFSET_P1_AMMO_COUNT = 0x8DBA;

        // Player 2
        private const int OFFSET_P2_CREDITS_COUNT = 0x0035;
        private const int OFFSET_P2_HEALTH_COUNT = 0x21B4;
        private const int OFFSET_P2_INVINCIBILITY_TIMER = 0x2126;
        private const int OFFSET_P2_SCORE = 0x8FB2;
        private const int OFFSET_P2_VEHICLE_HEALTH_COUNT = 0;
        private const int OFFSET_P2_VEHICLE_AMMO_CANON_COUNT = 0;
        private const int OFFSET_P2_WEAPON_TYPE = 0x8FB8;
        private const int OFFSET_P2_BOMB_TBC1_COUNT = 0x8FC2;
        private const int OFFSET_P2_BOMB_TBC2_COUNT = 0x8FC6;
        private const int OFFSET_P2_BOMB_TBC3_COUNT = 0x8FCA;
        private const int OFFSET_P2_AMMO_COUNT = 0x8FBA;

        #endregion

        #region Fields

        private Process game;

        private IntPtr hProc = IntPtr.Zero;
        private IntPtr levelTimerAddressGlobal = IntPtr.Zero;
        private IntPtr healthCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr healthCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr invincibilityTimerP1AddressGlobal = IntPtr.Zero;
        private IntPtr invincibilityTimerP2AddressGlobal = IntPtr.Zero;
        private IntPtr weaponTypeP1AddressGlobal = IntPtr.Zero;
        private IntPtr weaponTypeP2AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP1TBC1AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP1TBC2AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP1TBC3AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP2TBC1AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP2TBC2AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP2TBC3AddressGlobal = IntPtr.Zero;
        private IntPtr ammoCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr ammoCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleAmmoCanonCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleAmmoCanonCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleHealthCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleHealthCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr creditsCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr creditsCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr scoreP1AddressGlobal = IntPtr.Zero;
        private IntPtr scoreP2AddressGlobal = IntPtr.Zero;

        private Timer timer = new Timer();

        private string previousLogEntry = "";

        #endregion

        public Form1()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;

            Text = "Shock Troopers (GOG Version) Trainer by sLeEpY9090";
            textBoxLog.Text = "Shock Troopers (GOG Version) Trainer by sLeEpY9090" + Environment.NewLine + "For use with version: setup_shock_troopers_gog-3_(12274).exe" + Environment.NewLine;

            PopulateWeaponTypes();
            PopulateScore();
            SetTextBoxMaxLength();
            SetDefaultTextBoxValues();

            // Keep trainer updated about the game process and game memory.
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        #region Form Setup Methods

        private void PopulateWeaponTypes()
        {
            Dictionary<int, string> weaponTypesDictionary = new Dictionary<int, string>
            {
                {  0,  " 0 - Normal"  },
                {  4,  " 4 - Heavy"   },
                {  5,  " 8 - Vulcan"  },
                {  12, "12 - 3-Way"   },
                {  16, "16 - Buster"  },
                {  20, "20 - Flame"   },
                {  24, "24 - Rocket"  },
                {  28, "28 - Missile" },
                {  32, "32 - Hyper"   }
            };

            comboBoxP1WeaponTypeRead.Items.Clear();
            comboBoxP1WeaponTypeRead.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP1WeaponTypeRead.DisplayMember = "Value";
            comboBoxP1WeaponTypeRead.ValueMember = "Key";
            comboBoxP1WeaponTypeRead.SelectedIndex = 0;

            comboBoxP2WeaponTypeRead.Items.Clear();
            comboBoxP2WeaponTypeRead.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP2WeaponTypeRead.DisplayMember = "Value";
            comboBoxP2WeaponTypeRead.ValueMember = "Key";
            comboBoxP2WeaponTypeRead.SelectedIndex = 0;

            comboBoxP1WeaponTypeWrite.Items.Clear();
            comboBoxP1WeaponTypeWrite.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP1WeaponTypeWrite.DisplayMember = "Value";
            comboBoxP1WeaponTypeWrite.ValueMember = "Key";
            comboBoxP1WeaponTypeWrite.SelectedIndex = 0;

            comboBoxP2WeaponTypeWrite.Items.Clear();
            comboBoxP2WeaponTypeWrite.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP2WeaponTypeWrite.DisplayMember = "Value";
            comboBoxP2WeaponTypeWrite.ValueMember = "Key";
            comboBoxP2WeaponTypeWrite.SelectedIndex = 0;
        }

        private void PopulateScore()
        {
            // TODO: This needs to include hex? or its parsing wrong.
            // 0-9 = 0-9, 16-25=10-19
            // counts 0 to 9, then 16 to 19, etc
            // counts in decimal but displays as hex, this way A-F never shows in score also, but why is it done this way?
            // code (hex):    0,1,2,3,4,5,6,7,8,9,16,17,18,...
            // display (dec): 0,1,2,3,4,5,6,7,8,9,10,11,12,...
            for (int i = 0; i < 100; i++)
            {
                comboBoxP1Score1.Items.Add(i);
                comboBoxP1Score2.Items.Add(i);
                comboBoxP1Score3.Items.Add(i);
                comboBoxP1Score4.Items.Add(i);

                comboBoxP2Score1.Items.Add(i);
                comboBoxP2Score2.Items.Add(i);
                comboBoxP2Score3.Items.Add(i);
                comboBoxP2Score4.Items.Add(i);
            }

            // Convert the above to hex before writing it back
            comboBoxP1Score1.SelectedIndex = 63; // 63h = 99d
            comboBoxP1Score2.SelectedIndex = 63; // 63h = 99d
            comboBoxP1Score3.SelectedIndex = 63; // 63h = 99d
            comboBoxP1Score4.SelectedIndex = 63; // 63h = 99d

            comboBoxP2Score1.SelectedIndex = 63; // 63h = 99d
            comboBoxP2Score2.SelectedIndex = 63; // 63h = 99d
            comboBoxP2Score3.SelectedIndex = 63; // 63h = 99d
            comboBoxP2Score4.SelectedIndex = 63; // 63h = 99d
        }

        public void SetDefaultTextBoxValues()
        {
            // TODO: Vehicle Ammo and Health
            checkBoxP1VehicleAmmoCount.Enabled = false;
            checkBoxP2VehicleAmmoCount.Enabled = false;
            textBoxP1VehicleAmmoCountWrite.Enabled = false;
            textBoxP2VehicleAmmoCountWrite.Enabled = false;
            checkBoxP1VehicleHealthCount.Enabled = false;
            checkBoxP2VehicleHealthCount.Enabled = false;
            textBoxP1VehicleHealthCountWrite.Enabled = false;
            textBoxP2VehicleHealthCountWrite.Enabled = false;

            textBoxLevelTimerWrite.Text = "153"; // 153h = 99d, higher can kill the player, overflow?

            textBoxP1HealthCountWrite.Text = "128"; // 129+ is garbled
            textBoxP1InvincibilityTimerWrite.Text = "255"; // Invincible
            textBoxP1BombCountTBC1Write.Text = "255";
            textBoxP1BombCountTBC2Write.Text = "255";
            textBoxP1BombCountTBC3Write.Text = "255";
            textBoxP1AmmoCountWrite.Text = "209"; // 0 - 209 displayable, rest is garbled, max 65535
            textBoxP1VehicleAmmoCountWrite.Text = "255";
            textBoxP1VehicleHealthCountWrite.Text = "255";
            textBoxP1CreditsCountWrite.Text = "153"; // Infinite

            textBoxP2HealthCountWrite.Text = "128"; // 129+ is garbled
            textBoxP2InvincibilityTimerWrite.Text = "255"; // Invincible
            textBoxP2BombCountTBC1Write.Text = "255";
            textBoxP2BombCountTBC2Write.Text = "255";
            textBoxP2BombCountTBC3Write.Text = "255";
            textBoxP2AmmoCountWrite.Text = "209"; // 0 - 209 displayable, rest is garbled, max 65535
            textBoxP2VehicleAmmoCountWrite.Text = "255";
            textBoxP2VehicleHealthCountWrite.Text = "255";
            textBoxP2CreditsCountWrite.Text = "153"; // Infinite

            textBoxLevelTimerRead.Enabled = false;

            textBoxP1HealthCountRead.Enabled = false;
            textBoxP1InvincibilityTimerRead.Enabled = false;
            textBoxP1BombCountTBC1Read.Enabled = false;
            textBoxP1BombCountTBC2Read.Enabled = false;
            textBoxP1BombCountTBC3Read.Enabled = false;
            textBoxP1AmmoCountRead.Enabled = false;
            textBoxP1VehicleAmmoCountRead.Enabled = false;
            textBoxP1VehicleHealthCountRead.Enabled = false;
            textBoxP1CreditsCountRead.Enabled = false;
            textBoxP1ScoreRead.Enabled = false;
            comboBoxP1WeaponTypeRead.Enabled = false;

            textBoxP2HealthCountRead.Enabled = false;
            textBoxP2InvincibilityTimerRead.Enabled = false;
            textBoxP2BombCountTBC1Read.Enabled = false;
            textBoxP2BombCountTBC2Read.Enabled = false;
            textBoxP2BombCountTBC3Read.Enabled = false;
            textBoxP2AmmoCountRead.Enabled = false;
            textBoxP2VehicleAmmoCountRead.Enabled = false;
            textBoxP2VehicleHealthCountRead.Enabled = false;
            textBoxP2CreditsCountRead.Enabled = false;
            textBoxP2ScoreRead.Enabled = false;
            comboBoxP2WeaponTypeRead.Enabled = false;

            textBoxModuleName.Text = MODULE_NAME;
            textBoxProcessName.Text = PROCESS_NAME;
        }

        private void SetTextBoxMaxLength()
        {
            textBoxLevelTimerWrite.MaxLength = 3;

            textBoxP1HealthCountWrite.MaxLength = 3;
            textBoxP1InvincibilityTimerWrite.MaxLength = 3;
            textBoxP1BombCountTBC1Write.MaxLength = 3;
            textBoxP1BombCountTBC2Write.MaxLength = 3;
            textBoxP1BombCountTBC3Write.MaxLength = 3;
            textBoxP1AmmoCountWrite.MaxLength = 5;
            textBoxP1VehicleAmmoCountWrite.MaxLength = 3;
            textBoxP1VehicleHealthCountWrite.MaxLength = 3;
            textBoxP1CreditsCountWrite.MaxLength = 3;

            textBoxP2HealthCountWrite.MaxLength = 3;
            textBoxP2InvincibilityTimerWrite.MaxLength = 3;
            textBoxP2BombCountTBC1Write.MaxLength = 3;
            textBoxP2BombCountTBC2Write.MaxLength = 3;
            textBoxP2BombCountTBC3Write.MaxLength = 3;
            textBoxP2AmmoCountWrite.MaxLength = 5;
            textBoxP2VehicleAmmoCountWrite.MaxLength = 3;
            textBoxP2VehicleHealthCountWrite.MaxLength = 3;
            textBoxP2CreditsCountWrite.MaxLength = 3;
        }

        #endregion

        // Gets game connection or returns null if not found
        private Process GameConnect()
        {
            string processName = PROCESS_NAME;
            if (checkBoxProcessName.Checked)
            {
                processName = textBoxProcessName.Text;
            }

            // Game process: shocktro
            game = Process.GetProcessesByName(processName).FirstOrDefault();
            if (game == null)
            {
                hProc = IntPtr.Zero;
            }
            else
            {
                // Open handle with full permission to game process
                hProc = OpenProcess((int)ProcessAccessRights.PROCESS_ALL_ACCESS, false, game.Id);
            }

            return game;
        }

        private IntPtr GetBaseAddress()
        {
            string moduleName = MODULE_NAME;
            if (checkBoxModuleName.Checked)
            {
                moduleName = textBoxModuleName.Text;
            }

            IntPtr baseAddress = IntPtr.Zero;
            foreach (ProcessModule module in game.Modules)
            {
                if (module.ModuleName.ToLower() == moduleName)
                {
                    baseAddress = module.BaseAddress;
                    break;
                }
            }

            return baseAddress;
        }

        private IntPtr GetOffsetAddress(IntPtr hProc, int address, int offset)
        {
            IntPtr baseAddress = GetBaseAddress();
            IntPtr offsetAddress = IntPtr.Zero;
            if (baseAddress != IntPtr.Zero)
            {
                offsetAddress = IntPtr.Add(baseAddress, address);
                offsetAddress = ReadPointerUInt32(hProc, offsetAddress, offset);
            }
            return offsetAddress;
        }

        #region Display Value Methods

        private string DisplayByteValue(IntPtr hProc, IntPtr addressGlobal, int baseAddress, int valueOffset)
        {
            if (addressGlobal == IntPtr.Zero || ((int)addressGlobal) == valueOffset)
            {
                addressGlobal = GetOffsetAddress(hProc, baseAddress, valueOffset);
            }
            int tempValue = ReadByte(hProc, addressGlobal);
            return tempValue.ToString();
        }

        private string DisplayUInt16Value(IntPtr hProc, IntPtr addressGlobal, int baseAddress, int valueOffset)
        {
            if (addressGlobal == IntPtr.Zero || ((int)addressGlobal) == valueOffset)
            {
                addressGlobal = GetOffsetAddress(hProc, baseAddress, valueOffset);
            }
            uint tempValue = ReadUInt16(hProc, addressGlobal);
            return tempValue.ToString();
        }

        private string DisplayUInt32Value(IntPtr hProc, IntPtr addressGlobal, int baseAddress, int valueOffset)
        {
            if (addressGlobal == IntPtr.Zero || ((int)addressGlobal) == valueOffset)
            {
                addressGlobal = GetOffsetAddress(hProc, baseAddress, valueOffset);
            }
            uint tempValue = ReadUInt32(hProc, addressGlobal);
            return tempValue.ToString();
        }

        #endregion

        #region Get Pointer Methods

        private IntPtr ReadPointerInt64(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[8];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToInt64(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerUInt64(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[8];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToUInt64(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerInt32(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToInt32(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerUInt32(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToUInt32(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerInt16(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToInt16(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerUInt16(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToUInt16(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        #endregion

        #region Get/Set Memory Value Methods

        private float ReadFloat(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4]; // FLOAT = 4 byte
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToSingle(buffer, 0);
        }

        private void WriteFloat(IntPtr hProcess, IntPtr address, float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        private int ReadInt(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4]; // INT = 4 byte
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        private void WriteInt(IntPtr hProcess, IntPtr address, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        private int ReadByte(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[1];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return buffer[0];
        }

        private void WriteByte(IntPtr hProcess, IntPtr address, byte value)
        {
            // Since bitconverter getbytes with a byte value handles it as a short (no overload for byte), we get 2 bytes back as a short, but we only want the first
            // https://learn.microsoft.com/en-us/dotnet/api/system.bitconverter.getbytes?view=net-9.0#system-bitconverter-getbytes(system-int16)
            byte[] buffer = BitConverter.GetBytes(value);
            //WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length - 1, out int bytesWritten);
        }

        private uint ReadUInt16(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToUInt16(buffer, 0);
        }

        private void WriteUInt16(IntPtr hProcess, IntPtr address, ushort value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        private uint ReadUInt32(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToUInt32(buffer, 0);
        }

        private void WriteUInt32(IntPtr hProcess, IntPtr address, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        #endregion

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((GameConnect() == null) || (GetBaseAddress() == IntPtr.Zero))
            {
                ForeColor = Color.Red;
                string logEntry = "Shock Troopers Process Not Found.";
                logEntry += Environment.NewLine;
                if (logEntry != previousLogEntry)
                {
                    textBoxLog.AppendText(logEntry + Environment.NewLine);
                    previousLogEntry = logEntry;
                }
            }
            else
            {
                ForeColor = Color.Green;
                string logEntry = $"[Shock Troopers Process {game.ProcessName} found in {game} with PID: {game.Id}]";
                logEntry += Environment.NewLine;
                logEntry += $"Start Time: {game.StartTime}";

                if (logEntry != previousLogEntry)
                {
                    previousLogEntry = logEntry;

                    textBoxLog.AppendText(logEntry + Environment.NewLine);
                    textBoxLog.AppendText($"Total Processor Time: {game.TotalProcessorTime}"
                        + Environment.NewLine);
                    textBoxLog.AppendText($"Physical Memory Usage (MB): {game.WorkingSet64 / (1024 * 1024)}"
                        + Environment.NewLine
                        + "---------------------------------------------------"
                        + Environment.NewLine);
                }

                levelTimerAddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_LEVEL_TIMER);

                healthCountP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_HEALTH_COUNT);
                invincibilityTimerP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_INVINCIBILITY_TIMER);
                weaponTypeP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_WEAPON_TYPE);
                bombCountP1TBC1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_BOMB_TBC1_COUNT);
                bombCountP1TBC2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_BOMB_TBC2_COUNT);
                bombCountP1TBC3AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_BOMB_TBC3_COUNT);
                ammoCountP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_AMMO_COUNT);
                vehicleAmmoCanonCountP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_AMMO_CANON_COUNT);
                vehicleHealthCountP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_HEALTH_COUNT);
                scoreP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_SCORE);
                creditsCountP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_3, OFFSET_P1_CREDITS_COUNT);

                healthCountP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_HEALTH_COUNT);
                invincibilityTimerP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_INVINCIBILITY_TIMER);
                weaponTypeP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_WEAPON_TYPE);
                bombCountP2TBC1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_BOMB_TBC1_COUNT);
                bombCountP2TBC2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_BOMB_TBC2_COUNT);
                bombCountP2TBC3AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_BOMB_TBC3_COUNT);
                ammoCountP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_AMMO_COUNT);
                vehicleAmmoCanonCountP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_AMMO_CANON_COUNT);
                vehicleHealthCountP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_HEALTH_COUNT);
                scoreP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_SCORE);
                creditsCountP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_3, OFFSET_P2_CREDITS_COUNT);

                #region Level Timer

                if (checkBoxLevelTimer.Checked)
                {
                    try
                    {
                        // 153h = 99d
                        if (int.TryParse(textBoxLevelTimerWrite.Text, out int levelTimer)
                            && (levelTimer >= 0 && levelTimer <= 255))
                        {
                            WriteByte(hProc, levelTimerAddressGlobal, Convert.ToByte(levelTimer));
                        }
                        else
                        {
                            textBoxLog.AppendText("Level Timer value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Level Timer value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxLevelTimerRead.Text = DisplayByteValue(hProc, levelTimerAddressGlobal, BASE_ADDRESS_1, OFFSET_LEVEL_TIMER);

                #endregion

                #region Health Count

                if (checkBoxP1HealthCount.Checked)
                {
                    try
                    {
                        // byte, 0-128 (129-255 garbled)
                        if (int.TryParse(textBoxP1HealthCountWrite.Text, out int healthCount)
                            && (healthCount >= 0 && healthCount <= 255))
                        {
                            WriteByte(hProc, healthCountP1AddressGlobal, Convert.ToByte(healthCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Health Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Health Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1HealthCountRead.Text = DisplayByteValue(hProc, healthCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_HEALTH_COUNT);

                if (checkBoxP2HealthCount.Checked)
                {
                    try
                    {
                        // byte, 0-128 (129-255 garbled)
                        if (int.TryParse(textBoxP2HealthCountWrite.Text, out int healthCount)
                            && (healthCount >= 0 && healthCount <= 255))
                        {
                            WriteByte(hProc, healthCountP2AddressGlobal, Convert.ToByte(healthCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Health Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Health Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2HealthCountRead.Text = DisplayByteValue(hProc, healthCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_HEALTH_COUNT);

                #endregion

                #region Invincibility Timer

                if (checkBoxP1InvincibilityTimer.Checked)
                {
                    try
                    {
                        // Freeze at 255 - Invincible
                        if (int.TryParse(textBoxP1InvincibilityTimerWrite.Text, out int invincibilityTimer)
                            && (invincibilityTimer >= 0 && invincibilityTimer <= 255))
                        {
                            WriteByte(hProc, invincibilityTimerP1AddressGlobal, Convert.ToByte(invincibilityTimer));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Invincibility Timer value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Invincibility Timer value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1InvincibilityTimerRead.Text = DisplayByteValue(hProc, invincibilityTimerP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_INVINCIBILITY_TIMER);

                if (checkBoxP2InvincibilityTimer.Checked)
                {
                    try
                    {
                        // Freeze at 255 - Invincible
                        if (int.TryParse(textBoxP2InvincibilityTimerWrite.Text, out int invincibilityTimer)
                            && (invincibilityTimer >= 0 && invincibilityTimer <= 255))
                        {
                            WriteByte(hProc, invincibilityTimerP2AddressGlobal, Convert.ToByte(invincibilityTimer));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Invincibility Timer value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Invincibility Timer value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2InvincibilityTimerRead.Text = DisplayByteValue(hProc, invincibilityTimerP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_INVINCIBILITY_TIMER);

                #endregion

                #region Bomb Count

                if (checkBoxP1BombCountTBC1.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1BombCountTBC1Write.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP1TBC1AddressGlobal, Convert.ToByte(bombCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Bomb Count 1 value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Bomb Count 1 value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1BombCountTBC1Read.Text = DisplayByteValue(hProc, bombCountP1TBC1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_BOMB_TBC1_COUNT);

                if (checkBoxP1BombCountTBC2.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1BombCountTBC2Write.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP1TBC2AddressGlobal, Convert.ToByte(bombCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Bomb Count 2 value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Bomb Count 2 value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1BombCountTBC2Read.Text = DisplayByteValue(hProc, bombCountP1TBC2AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_BOMB_TBC2_COUNT);

                if (checkBoxP1BombCountTBC3.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1BombCountTBC3Write.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP1TBC3AddressGlobal, Convert.ToByte(bombCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Bomb Count 3 value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Bomb Count 3 value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1BombCountTBC3Read.Text = DisplayByteValue(hProc, bombCountP1TBC3AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_BOMB_TBC3_COUNT);

                if (checkBoxP2BombCountTBC1.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2BombCountTBC1Write.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP2TBC1AddressGlobal, Convert.ToByte(bombCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Bomb Count 1 value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Bomb Count 1 value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2BombCountTBC1Read.Text = DisplayByteValue(hProc, bombCountP2TBC1AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_BOMB_TBC1_COUNT);

                if (checkBoxP2BombCountTBC2.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2BombCountTBC2Write.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP2TBC2AddressGlobal, Convert.ToByte(bombCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Bomb Count 2 value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Bomb Count 2 value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2BombCountTBC2Read.Text = DisplayByteValue(hProc, bombCountP2TBC2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_BOMB_TBC2_COUNT);

                if (checkBoxP2BombCountTBC3.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2BombCountTBC3Write.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP2TBC3AddressGlobal, Convert.ToByte(bombCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Bomb Count value 3 must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Bomb Count value 3 must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2BombCountTBC3Read.Text = DisplayByteValue(hProc, bombCountP2TBC3AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_BOMB_TBC3_COUNT);

                #endregion

                #region Vehicle Ammo Count

                if (checkBoxP1VehicleAmmoCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1VehicleAmmoCountWrite.Text, out int vehicleAmmoCount)
                            && (vehicleAmmoCount >= 0 && vehicleAmmoCount <= 255))
                        {
                            WriteByte(hProc, vehicleAmmoCanonCountP1AddressGlobal, Convert.ToByte(vehicleAmmoCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Vehicle Ammo Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Vehicle Ammo Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1VehicleAmmoCountRead.Text = DisplayByteValue(hProc, vehicleAmmoCanonCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_AMMO_CANON_COUNT);

                if (checkBoxP2VehicleAmmoCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2VehicleAmmoCountWrite.Text, out int vehicleAmmoCount)
                            && (vehicleAmmoCount >= 0 && vehicleAmmoCount <= 255))
                        {
                            WriteByte(hProc, vehicleAmmoCanonCountP2AddressGlobal, Convert.ToByte(vehicleAmmoCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Vehicle Ammo Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Vehicle Ammo Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2VehicleAmmoCountRead.Text = DisplayByteValue(hProc, vehicleAmmoCanonCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_AMMO_CANON_COUNT);

                #endregion

                #region Vehicle Health Count

                if (checkBoxP1VehicleHealthCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1VehicleHealthCountWrite.Text, out int vehicleHealthCount)
                            && (vehicleHealthCount >= 0 && vehicleHealthCount <= 255))
                        {
                            WriteByte(hProc, vehicleHealthCountP1AddressGlobal, Convert.ToByte(vehicleHealthCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Vehicle Health Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Vehicle Health Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1VehicleHealthCountRead.Text = DisplayByteValue(hProc, vehicleHealthCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_HEALTH_COUNT);

                if (checkBoxP2VehicleHealthCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2VehicleHealthCountWrite.Text, out int vehicleHealthCount)
                            && (vehicleHealthCount >= 0 && vehicleHealthCount <= 255))
                        {
                            WriteByte(hProc, vehicleHealthCountP2AddressGlobal, Convert.ToByte(vehicleHealthCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Vehicle Health Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Vehicle Health Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2VehicleHealthCountRead.Text = DisplayByteValue(hProc, vehicleHealthCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_HEALTH_COUNT);

                #endregion

                #region Credits Count

                if (checkBoxP1CreditsCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1CreditsCountWrite.Text, out int creditsCount)
                            && (creditsCount >= 0 && creditsCount <= 255))
                        {
                            WriteByte(hProc, creditsCountP1AddressGlobal, Convert.ToByte(creditsCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Credits Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Credits Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1CreditsCountRead.Text = DisplayByteValue(hProc, creditsCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_CREDITS_COUNT);

                if (checkBoxP2CreditsCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2CreditsCountWrite.Text, out int creditsCount)
                            && (creditsCount >= 0 && creditsCount <= 255))
                        {
                            WriteByte(hProc, creditsCountP2AddressGlobal, Convert.ToByte(creditsCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Credits Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Credits Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2CreditsCountRead.Text = DisplayByteValue(hProc, creditsCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_CREDITS_COUNT);

                #endregion

                #region Ammo Count

                // Ammo is 2 bytes
                if (checkBoxP1AmmoCount.Checked)
                {
                    try
                    {
                        if (ushort.TryParse(textBoxP1AmmoCountWrite.Text, out ushort ammoCount)
                            && (ammoCount >= 0 && ammoCount <= 65535))
                        {
                            WriteUInt16(hProc, ammoCountP1AddressGlobal, ammoCount);
                        }
                        else
                        {
                            textBoxLog.AppendText("P1 Ammo Count value must be between 0 and 65535." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P1 Ammo value must be between 0 and 65535."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1AmmoCountRead.Text = DisplayUInt16Value(hProc, ammoCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_AMMO_COUNT);

                if (checkBoxP2AmmoCount.Checked)
                {
                    try
                    {

                        if (ushort.TryParse(textBoxP2AmmoCountWrite.Text, out ushort ammoCount)
                            && (ammoCount >= 0 && ammoCount <= 65535))
                        {
                            WriteUInt16(hProc, ammoCountP2AddressGlobal, ammoCount);
                        }
                        else
                        {
                            textBoxLog.AppendText("P2 Ammo Count value must be between 0 and 65535." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("P2 Ammo value must be between 0 and 65535."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2AmmoCountRead.Text = DisplayUInt16Value(hProc, ammoCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_AMMO_COUNT);

                #endregion

                #region Score

                /* TODO: Convert the selected value in the combobox to hex before writing it back
                 * P1 Score
                    byte, hex
                    example score: 34127856
                    0D53064A = 12
                    0D53064B = 34
                    0D53064C = 56
                    0D53064D = 78
                    "shocktro.exe"+000E3A08 8DB2
                    "shocktro.exe"+000E3A08 8DB3
                    "shocktro.exe"+000E3A08 8DB4
                    "shocktro.exe"+000E3A08 8DB5

                    "shocktro.exe"+000E3A0C 8DB2
                    "shocktro.exe"+000E3A0C 8DB3
                    "shocktro.exe"+000E3A0C 8DB4
                    "shocktro.exe"+000E3A0C 8DB5

                    P2 Score
                    byte, hex
                    example score: 34127856
                    0D53064A = 12
                    0D53064B = 34
                    0D53064C = 56
                    0D53064D = 78
                    "shocktro.exe"+000E3A08 8FB2
                    "shocktro.exe"+000E3A08 8FB3
                    "shocktro.exe"+000E3A08 8FB4
                    "shocktro.exe"+000E3A08 8FB5

                    "shocktro.exe"+000E3A0C 8FB2
                    "shocktro.exe"+000E3A0C 8FB3
                    "shocktro.exe"+000E3A0C 8FB4
                    "shocktro.exe"+000E3A0C 8FB5
                 */
                // Score is 4 bytes
                if (checkBoxP1Score.Checked)
                {
                    // 12345678
                    // | ED19 | ED18 | ED1B | ED1A |
                    // | 12   | 34   | 56   | 78   |
                    int eD19 = comboBoxP1Score1.SelectedIndex; //.ToString("X");
                    int eD18 = comboBoxP1Score2.SelectedIndex;
                    int eD1B = comboBoxP1Score3.SelectedIndex;
                    int eD1A = comboBoxP1Score4.SelectedIndex;

                    WriteByte(hProc, scoreP1AddressGlobal, (byte)eD18);
                    WriteByte(hProc, scoreP1AddressGlobal + 1, (byte)eD19);
                    WriteByte(hProc, scoreP1AddressGlobal + 2, (byte)eD1A);
                    WriteByte(hProc, scoreP1AddressGlobal + 3, (byte)eD1B);
                }

                int tempValue2 = ReadByte(hProc, scoreP1AddressGlobal + 1);
                textBoxP1ScoreRead.Text = tempValue2.ToString().PadLeft(2, '0');
                int tempValue = ReadByte(hProc, scoreP1AddressGlobal);
                textBoxP1ScoreRead.Text += tempValue.ToString().PadLeft(2, '0');
                int tempValue4 = ReadByte(hProc, scoreP1AddressGlobal + 3);
                textBoxP1ScoreRead.Text += tempValue4.ToString().PadLeft(2, '0');
                int tempValue3 = ReadByte(hProc, scoreP1AddressGlobal + 2);
                textBoxP1ScoreRead.Text += tempValue3.ToString().PadLeft(2, '0');


                if (checkBoxP2Score.Checked)
                {

                    int eD21 = comboBoxP2Score1.SelectedIndex;
                    int eD20 = comboBoxP2Score2.SelectedIndex;
                    int eD23 = comboBoxP2Score3.SelectedIndex;
                    int eD22 = comboBoxP2Score4.SelectedIndex;

                    WriteByte(hProc, scoreP2AddressGlobal, (byte)eD20);
                    WriteByte(hProc, scoreP2AddressGlobal + 1, (byte)eD21);
                    WriteByte(hProc, scoreP2AddressGlobal + 2, (byte)eD22);
                    WriteByte(hProc, scoreP2AddressGlobal + 3, (byte)eD23);
                }

                tempValue2 = ReadByte(hProc, scoreP2AddressGlobal + 1);
                textBoxP2ScoreRead.Text = tempValue2.ToString().PadLeft(2, '0');
                tempValue = ReadByte(hProc, scoreP2AddressGlobal);
                textBoxP2ScoreRead.Text += tempValue.ToString().PadLeft(2, '0');
                tempValue4 = ReadByte(hProc, scoreP2AddressGlobal + 3);
                textBoxP2ScoreRead.Text += tempValue4.ToString().PadLeft(2, '0');
                tempValue3 = ReadByte(hProc, scoreP2AddressGlobal + 2);
                textBoxP2ScoreRead.Text += tempValue3.ToString().PadLeft(2, '0');

                #endregion

                #region Weapon Type

                if (checkBoxP1WeaponType.Checked)
                {

                    try
                    {
                        WriteByte(hProc, weaponTypeP1AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP1WeaponTypeWrite.SelectedItem).Key));
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 1 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, weaponTypeP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_WEAPON_TYPE), out int weaponTypeP1))
                {
                    try
                    {
                        switch (weaponTypeP1)
                        {
                            case 0:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 0;
                                break;
                            case 4:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 1;
                                break;
                            case 8:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 2;
                                break;
                            case 12:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 3;
                                break;
                            case 16:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 4;
                                break;
                            case 20:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 5;
                                break;
                            case 24:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 6;
                                break;
                            case 28:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 7;
                                break;
                            case 32:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 8;
                                break;
                            default:
                                comboBoxP1WeaponTypeRead.SelectedIndex = 0;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 1 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }

                if (checkBoxP2WeaponType.Checked)
                {
                    try
                    {
                        WriteByte(hProc, weaponTypeP2AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP2WeaponTypeWrite.SelectedItem).Key));
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 2 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, weaponTypeP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_WEAPON_TYPE), out int weaponTypeP2))
                {
                    try
                    {
                        switch (weaponTypeP2)
                        {
                            case 0:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 0;
                                break;
                            case 4:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 1;
                                break;
                            case 8:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 2;
                                break;
                            case 12:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 3;
                                break;
                            case 16:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 4;
                                break;
                            case 20:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 5;
                                break;
                            case 24:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 6;
                                break;
                            case 28:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 7;
                                break;
                            case 32:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 8;
                                break;
                            default:
                                comboBoxP2WeaponTypeRead.SelectedIndex = 0;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 2 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                #endregion

            }
        }
    }
}
