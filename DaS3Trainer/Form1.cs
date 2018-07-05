using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace DaS3App
{
    public partial class DaS3App : Form
    {


        public DaS3App()
        {
            InitializeComponent();
        }

        public static bool Is64Bit => IntPtr.Size == 8;

        public IntPtr ProcessHandle { get; private set; }

        public Process Process { get; private set; }

        public IntPtr BaseAddress { get; private set; }

        //attach Proc
        public IntPtr AttachProc(string procName)
        {
            var ZeroRt = new IntPtr(0);
            var processes = System.Diagnostics.Process.GetProcessesByName(procName);
            if (processes.Length > 0)
            {
                var Process = processes[0];
                BaseAddress = Process.MainModule.BaseAddress;
                ProcessHandle = Kernel32.OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, Process.Id);
                return ProcessHandle;
            }
            else
            {
                MessageBox.Show("Cant find process. Is it running?", "Process");
                return ZeroRt;
            }
        }
        //load app
        private void DaS3App_Load(object sender, EventArgs e)
        {
            var delay = 10;
            mainTimer.Interval = delay;
            mainTimer.Start();

            ProcessHandle = AttachProc("darksoulsiii");
        }

        // read an address
        public byte ReadInt8(IntPtr address)
        {
            var readBuffer = new byte[sizeof(byte)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
            var value = readBuffer[0];
            return value;
        }

        public short ReadInt16(IntPtr address)
        {
            var readBuffer = new byte[sizeof(short)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)2, UIntPtr.Zero);
            var value = BitConverter.ToInt16(readBuffer, 0);
            return value;
        }

        public int ReadInt32(IntPtr address)
        {
            var readBuffer = new byte[sizeof(int)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero); 
            var value = BitConverter.ToInt32(readBuffer, 0);
            return value;
        }

        public long ReadInt64(IntPtr address)
        {
            var readBuffer = new byte[sizeof(long)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToInt64(readBuffer, 0);
            return value;
        }

        public float ReadFloat(IntPtr address)
        {
            var readBuffer = new byte[sizeof(float)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToSingle(readBuffer, 0);
            return value;
        }

        public double ReadDouble(IntPtr address)
        {
            var readBuffer = new byte[sizeof(double)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToDouble(readBuffer, 0);
            return value;
        }

        public string ReadString(IntPtr address, int length, string encodingName)
        {
            var readBuffer = new byte[length];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var encodingType = System.Text.Encoding.GetEncoding(encodingName);
            string value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

            return value;
        }

        //write to address
        public bool WriteInt8(IntPtr address, byte value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)1, UIntPtr.Zero);
        }

        public bool WriteInt16(IntPtr address, short value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)2, UIntPtr.Zero);
        }

        public bool WriteInt32(IntPtr address, int value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
        }

        public bool WriteInt64(IntPtr address, long value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
        }

        public bool WriteFloat(IntPtr address, float value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
        }

        public bool WriteDouble(IntPtr address, double value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
        }

        public bool WriteBytes(IntPtr addr, Byte[] val)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, addr, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
        }

        private void UncheckAll(Control ctrl)
        {
            CheckBox chkBox = ctrl as CheckBox;
            if (chkBox == null)
            {
                foreach (Control child in ctrl.Controls)
                {
                    UncheckAll(child);
                }
            }
            else
            {
                chkBox.Checked = false;
            }
        }

        //Tab with pointers

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            if ((long)BaseAddress > 0)
            {
                isChkEnbl.Text = "True";
                isChkEnbl.ForeColor = System.Drawing.Color.FromArgb(0,0,100,0);

                if (ReadInt32(IntPtr.Add(BaseAddress, 0x494C768)) == (int)1)
                {
                    tabs.Enabled = true;
                }
                else
                {
                    tabs.Enabled = false;

                    UncheckAll(this);
                }
            }

            var WorldChrMan = IntPtr.Add(BaseAddress, 0x4768E78);
            var WorldChrDbg = IntPtr.Add(BaseAddress, 0x4768F98);
            var EventDbg = IntPtr.Add(BaseAddress, 0x473AD78);
            var ChrClassBase = IntPtr.Add(BaseAddress, 0x4740178);
            var ChrAddClassBase = IntPtr.Add(BaseAddress, 0x4743AB0);
            switch (tabs.SelectedIndex)
            {
                case 0:
                    {
                        var startPtr_1 = new IntPtr (ReadInt64(WorldChrMan)); //jump
                        startPtr_1 = IntPtr.Add(startPtr_1, 0x80); //add offset
                        var BasicStats = new IntPtr(ReadInt64(startPtr_1)); //jump
                        BasicStats = IntPtr.Add(BasicStats, 0x1F90);
                        BasicStats = new IntPtr(ReadInt64(BasicStats));
                        BasicStats = IntPtr.Add(BasicStats, 0x18);
                        BasicStats = new IntPtr(ReadInt64(BasicStats));
                        var HP_STAT = IntPtr.Add(BasicStats, 0xD8);
                        var STAMINA_STAT = IntPtr.Add(BasicStats, 0xF0);
                        var MANA_STAT = IntPtr.Add(BasicStats, 0xE4);
                        var MHP_STAT = IntPtr.Add(BasicStats, 0xE0);
                        var MSTAMINA_STAT = IntPtr.Add(BasicStats, 0xF8);
                        var MMANA_STAT = IntPtr.Add(BasicStats, 0xEC);

                        hpread.Text = (ReadInt32(HP_STAT)).ToString();
                        staminaRead.Text = (ReadInt32(STAMINA_STAT)).ToString();
                        manaRead.Text = (ReadInt32(MANA_STAT)).ToString();
                        maxhpRead.Text = (ReadInt32(MHP_STAT)).ToString();
                        maxStaminaRead.Text = (ReadInt32(MSTAMINA_STAT)).ToString();
                        maxManaRead.Text = (ReadInt32(MMANA_STAT)).ToString();

                        var LocationPtr = IntPtr.Add(BaseAddress, 0x463F1F0);
                        LocationPtr = new IntPtr(ReadInt32(LocationPtr));
                        int LocationID = (int)(LocationPtr);
                        string skip = LocationID.ToString("X");
                        LocationID = Convert.ToInt32(skip, 16);
                        switch (LocationID)
                        {
                            case (0x1E000000):
                                {
                                    locIDREAD.Text = "High Wall of Lothric";
                                    break;
                                }
                            case (0x1E010000):
                                {
                                    locIDREAD.Text = "Lothric Castle";
                                    break;
                                }
                            case (0x1F000000):
                                {
                                    locIDREAD.Text = "Undead Settlement";
                                    break;
                                }
                            case (0x20000000):
                                {
                                    locIDREAD.Text = "Archdragon Peak";
                                    break;
                                }
                            case (0x21000000):
                                {
                                    locIDREAD.Text = "Road of Sacrifices";
                                    break;
                                }
                            case (0x23000000):
                                {
                                    locIDREAD.Text = "Cathedral of the Deep";
                                    break;
                                }
                            case (0x25000000):
                                {
                                    locIDREAD.Text = "Irithyll of the Boreal Valley";
                                    break;
                                }
                            case (0x22010000):
                                {
                                    locIDREAD.Text = "Grand Archives";
                                    break;
                                }
                            case (0x27000000):
                                {
                                    locIDREAD.Text = "Irithyll Dungeon";
                                    break;
                                }
                            case (0x28000000):
                                {
                                    locIDREAD.Text = "Untended Graves";
                                    break;
                                }
                            case (0x29000000):
                                {
                                    locIDREAD.Text = "Kiln of the First Flame";
                                    break;
                                }
                            case (0x2D000000):
                                {
                                    locIDREAD.Text = "Painted World of Ariandel";
                                    break;
                                }
                            case (0x2E000000):
                                {
                                    locIDREAD.Text = "Grand Rooftop(PvP)";
                                    break;
                                }
                            case (0x2F000000):
                                {
                                    locIDREAD.Text = "Kiln of the First Flame(PvP)";
                                    break;
                                }
                            case (0x32000000):
                                {
                                    locIDREAD.Text = "The Dreg Heap";
                                    break;
                                }
                            case (0x33000000):
                                {
                                    locIDREAD.Text = "The Ringed City";
                                    break;
                                }
                            case (0x33010000):
                                {
                                    locIDREAD.Text = "Filianore's Rest";
                                    break;
                                }
                            case (0x35000000):
                                {
                                    locIDREAD.Text = "Dragon Ruins(PvP)";
                                    break;
                                }
                            case (0x36000000):
                                {
                                    locIDREAD.Text = "Round Plaza(PvP)";
                                    break;
                                }
                        }

                        var ChrData_PTR = new IntPtr(ReadInt64(ChrClassBase));
                        ChrData_PTR = IntPtr.Add(ChrData_PTR, 0x10);
                        ChrData_PTR = new IntPtr(ReadInt64(ChrData_PTR));

                        var genderPtr = IntPtr.Add(ChrData_PTR, 0xAA);
                        genderPtr = new IntPtr(ReadInt8(genderPtr));
                        var genderval = (int)genderPtr;
                        switch (genderval)
                        {
                            case 0:
                                {
                                    genderRead.Text = "Female";
                                    break;
                                }
                            case 1:
                                {
                                    genderRead.Text = "Male";
                                    break;
                                }
                        }                      

                        var PlayTimePtr = new IntPtr(ReadInt64(ChrClassBase));
                        PlayTimePtr = IntPtr.Add(PlayTimePtr, 0xA4);
                        PlayTimePtr = new IntPtr(ReadInt32(PlayTimePtr));
                        var playtimeval = (int)PlayTimePtr;
                        TimeSpan PlayTime = TimeSpan.FromMilliseconds(playtimeval);
                        playTimeRead.Text = PlayTime.ToString("g");

                        var deathcountptr = new IntPtr(ReadInt64(ChrClassBase));
                        deathcountptr = IntPtr.Add(deathcountptr, 0x98);
                        deathcountptr = new IntPtr(ReadInt32(deathcountptr));
                        var deathcountval = (int)deathcountptr;
                        DeathCountRead.Text = deathcountval.ToString();

                        var areaidptr = new IntPtr(ReadInt64(WorldChrMan));
                        areaidptr = IntPtr.Add(areaidptr, 0x80);
                        areaidptr = new IntPtr(ReadInt64(areaidptr));
                        areaidptr = IntPtr.Add(areaidptr, 0x1ABC);
                        areaidptr = new IntPtr(ReadInt32(areaidptr));
                        var areaidval = (int)areaidptr;
                        areaREAD.Text = areaidval.ToString();

                        var stableposPtr = new IntPtr(ReadInt64(ChrAddClassBase));
                        var stableXPtr = IntPtr.Add(stableposPtr, 0xB40);
                        var stableZPtr = IntPtr.Add(stableposPtr, 0xB44);
                        var stableYPtr = IntPtr.Add(stableposPtr, 0xB48);
                        var stableAngPtr = IntPtr.Add(stableposPtr, 0xB54);
                        var xStable = ReadFloat(stableXPtr);
                        var zStable = ReadFloat(stableZPtr);
                        var yStable = ReadFloat(stableYPtr);
                        var AngStable = ReadFloat(stableAngPtr);
                        stableXR.Text = xStable.ToString();
                        stableZR.Text = zStable.ToString();
                        stableYR.Text = yStable.ToString();
                        stableAngleR.Text = AngStable.ToString();

                        var statsPtr = new IntPtr(ReadInt64(ChrClassBase));
                        statsPtr = IntPtr.Add(statsPtr, 0x10);
                        statsPtr = new IntPtr(ReadInt64(statsPtr));

                        var SLPTR = IntPtr.Add(statsPtr, 0x70);
                        var SLval = ReadInt32(SLPTR);
                        var VIGPTR = IntPtr.Add(statsPtr, 0x44);
                        var Vigval = ReadInt32(VIGPTR);
                        var ATNPTR = IntPtr.Add(statsPtr, 0x48);
                        var ATNval = ReadInt32(ATNPTR);
                        var ENDPTR = IntPtr.Add(statsPtr, 0x4C);
                        var Endval = ReadInt32(ENDPTR);
                        var VITPTR = IntPtr.Add(statsPtr, 0x6C);
                        var Vitval = ReadInt32(VITPTR);
                        var StrPTR = IntPtr.Add(statsPtr, 0x50);
                        var Strval = ReadInt32(StrPTR);
                        var DexPTR = IntPtr.Add(statsPtr, 0x54);
                        var Dexval = ReadInt32(DexPTR);
                        var INTPTR = IntPtr.Add(statsPtr, 0x58);
                        var INTval = ReadInt32(INTPTR);
                        var FthPTR = IntPtr.Add(statsPtr, 0x5C);
                        var Fthval = ReadInt32(FthPTR);
                        var LUCKPTR = IntPtr.Add(statsPtr, 0x60);
                        var luckval = ReadInt32(LUCKPTR);

                        SoulLvl.Text = SLval.ToString();
                        VigorRead.Text = Vigval.ToString();
                        AttunementRead.Text = ATNval.ToString();
                        EnduranceRead.Text = Endval.ToString();
                        VitalityRead.Text = Vitval.ToString();
                        StrengthRead.Text = Strval.ToString();
                        DexterityRead.Text = Dexval.ToString();
                        IntelligenceRead.Text = INTval.ToString();
                        FaithRead.Text = Fthval.ToString();
                        LuckRead.Text = luckval.ToString();

                        var animPtr = new IntPtr (ReadInt64(WorldChrMan));
                        animPtr = IntPtr.Add(animPtr, 0x80);
                        animPtr = new IntPtr(ReadInt64(animPtr));
                        animPtr = IntPtr.Add(animPtr, 0x1F90);
                        animPtr = new IntPtr(ReadInt64(animPtr));
                        animPtr = IntPtr.Add(animPtr, 0x80);
                        animPtr = new IntPtr(ReadInt64(animPtr));
                        animPtr = IntPtr.Add(animPtr, 0xC8);
                        var animRes = ReadInt32(animPtr);
                        AnimIdPtr.Text = animRes.ToString();

                        var BasePosPtr = new IntPtr(ReadInt64(WorldChrMan));
                        BasePosPtr = IntPtr.Add(BasePosPtr, 0x40);
                        BasePosPtr = new IntPtr(ReadInt64(BasePosPtr));
                        BasePosPtr = IntPtr.Add(BasePosPtr, 0x28);
                        BasePosPtr = new IntPtr(ReadInt64(BasePosPtr));
                        var xBase = ReadFloat(IntPtr.Add(BasePosPtr, 0x80));
                        var zBase = ReadFloat(IntPtr.Add(BasePosPtr, 0x84));
                        var yBase = ReadFloat(IntPtr.Add(BasePosPtr, 0x88));

                        xRead.Text = xBase.ToString();
                        zRead.Text = zBase.ToString();
                        yRead.Text = yBase.ToString();

                        var animNamePtr = new IntPtr(ReadInt64(WorldChrMan));
                        animNamePtr = IntPtr.Add(animNamePtr, 0x80);
                        animNamePtr = new IntPtr(ReadInt64(animNamePtr));
                        animNamePtr = IntPtr.Add(animNamePtr, 0x1F90);
                        animNamePtr = new IntPtr(ReadInt64(animNamePtr));
                        animNamePtr = IntPtr.Add(animNamePtr, 0x28);
                        animNamePtr = new IntPtr(ReadInt64(animNamePtr));
                        animNamePtr = IntPtr.Add(animNamePtr, 0x898);
                        var AnimName_P = ReadString(animNamePtr, 60, "Unicode");

                        animNamePtrL.Text = AnimName_P;
                        break;
                    }
                case 1:
                    {
                        break;
                    }
                case 2:
                    {
                        break;
                    }
                case 3:
                    {
                        break;
                    }
                case 4:
                    {
                        break;
                    }
            }
        }

        //Buttons
        private void TeleportButton_Click_1(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0x83, 0xEC, 0x48, 0x49, 0xBE, 0x80, 0x19, 0x47, 0x40, 0x01, 0x00, 0x00, 0x00, 0xBA, 0x01, 0x00, 0x00, 0x00, 0x48, 0xA1, 0xC8, 0xA9, 0x73, 0x44, 0x01, 0x00, 0x00, 0x00, 0x48, 0x8B, 0xC8, 0x48, 0x8B, 0x49, 0x08, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x48, 0xC3 };

            var buffer = 512;
            var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            if (address != IntPtr.Zero)
            {
                if (Kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        Kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                Kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }
        

        private void addItem__Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x41, 0x5F, 0x48, 0x83, 0xEC, 0x58, 0x49, 0xBD, 0xD0, 0xFD, 0x57, 0x40, 0x01, 0x00, 0x00, 0x00, 0x41, 0xB6, 0x00, 0x48, 0xA1, 0x78, 0x01, 0x74, 0x44, 0x01, 0x00, 0x00, 0x00, 0x44, 0x88, 0x74, 0x24, 0x30, 0xBA, 0, 0, 0, 0, 0x41, 0xBE, 0, 0, 0, 0, 0xBF, 0, 0, 0, 0, 0x45, 0x8B, 0xCE, 0x48, 0x8B, 0x58, 0x10, 0x44, 0x8B, 0xC7, 0xBE, 0, 0, 0, 0, 0xC6, 0x44, 0x24, 0x28, 0x01, 0x48, 0x8D, 0x8B, 0x28, 0x02, 0x00, 0x00, 0xC6, 0x44, 0x24, 0x20, 0x01, 0x41, 0xFF, 0xD5, 0x48, 0x83, 0xC4, 0x58, 0x41, 0x57, 0xC3 };
            var bytesPopUp = new byte[] { 0x49, 0xBE, 0x40, 0x1E, 0x70, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0xA1, 0x68, 0x8E, 0x74, 0x44, 0x01, 0x00, 0x00, 0x00, 0x48, 0x8B, 0xC8, 0xBA, 0, 0, 0, 0, 0x41, 0xB9, 0, 0, 0, 0, 0x41, 0xB8, 0, 0, 0, 0, 0xC6, 0x44, 0x24, 0x20, 0x00, 0x48, 0x83, 0xEC, 0x40, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x40, 0xC3 };
            var bytes2 = new byte[15];

            var bytjmp1 = 0x23;
            var bytjmp2 = 0x29;
            var bytjmp3 = 0x2E;
            var bytjmp4 = 0x3D;

            var buffer = 512;
            var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            byte[] ItemBytes1 = System.Text.Encoding.UTF8.GetBytes(ItemID_.Text);
            string StringBytes1 = System.Text.Encoding.UTF8.GetString(ItemBytes1, 0, 8);

            byte[] ItemBytes2 = System.Text.Encoding.UTF8.GetBytes(DropdownItemType.Text);
            string StringBytes2 = System.Text.Encoding.UTF8.GetString(ItemBytes2, 0, 8);

            var ItemID = Convert.ToInt32(StringBytes1);
            var itemType = Convert.ToInt32(StringBytes2, 16);
            var ItemAmmount = Convert.ToInt32(itemNum_.Value);
            var ItemDurability = 100;

            bytes2 = BitConverter.GetBytes(itemType);
            Array.Copy(bytes2, 0, bytes, bytjmp1, bytes2.Length);

            bytes2 = BitConverter.GetBytes(ItemAmmount);
            Array.Copy(bytes2, 0, bytes, bytjmp2, bytes2.Length);

            bytes2 = BitConverter.GetBytes(ItemID);
            Array.Copy(bytes2, 0, bytes, bytjmp3, bytes2.Length);

            bytes2 = BitConverter.GetBytes(ItemDurability);
            Array.Copy(bytes2, 0, bytes, bytjmp4, bytes2.Length);
            //Second Buffer

            var buffer2 = 512;
            var address2 = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer2, 0x1000 | 0x2000, 0X40);

            var bytjmp1_1 = 0x18;
            var bytjmp1_2 = 0x1E;
            var bytjmp1_3 = 0x24;

            bytes2 = BitConverter.GetBytes(itemType);
            Array.Copy(bytes2, 0, bytesPopUp, bytjmp1_1, bytes2.Length);

            bytes2 = BitConverter.GetBytes(ItemAmmount);
            Array.Copy(bytes2, 0, bytesPopUp, bytjmp1_2, bytes2.Length);

            bytes2 = BitConverter.GetBytes(ItemID);
            Array.Copy(bytes2, 0, bytesPopUp, bytjmp1_3, bytes2.Length);

            Kernel32.WriteProcessMemory(ProcessHandle, address2, bytesPopUp, new UIntPtr((uint)bytesPopUp.Length), UIntPtr.Zero);

            if (address != IntPtr.Zero)
            {
                if (Kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {   
                    var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    var threadHandle2 = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address2, IntPtr.Zero, 0, out var threadId2);
                    if (threadHandle != IntPtr.Zero)
                    {
                        Kernel32.WaitForSingleObject(threadHandle, 30000);
                        Kernel32.WaitForSingleObject(threadHandle2, 30000);
                    }
                }
                Kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
                Kernel32.VirtualFreeEx(ProcessHandle, address2, buffer2, 2);
            }

        }

        private void hideMap__CheckedChanged(object sender, EventArgs e)
        {
            if (hideMap_.Checked)
            {
                WriteInt8(new IntPtr(0x144555CF0), 0);
            }
            else
            {
                WriteInt8(new IntPtr(0x144555CF0), 1);
            }
        }

        private void objHide_CheckedChanged(object sender, EventArgs e)
        {
            if (objHide.Checked)
            {
                WriteInt8(new IntPtr(0x144555CF1), 0);
            }
            else
            {
                WriteInt8(new IntPtr(0x144555CF1), 1);
            }
        }

        private void ChrHide_CheckedChanged(object sender, EventArgs e)
        {
            if (ChrHide.Checked)
            {
                WriteInt8(new IntPtr(0x144555CF2), 0);
            }
            else
            {
                WriteInt8(new IntPtr(0x144555CF2), 1);
            }
        }

        private void sfxHide_CheckedChanged(object sender, EventArgs e)
        {
            if (sfxHide.Checked)
            {
                WriteInt8(new IntPtr(0x144555CF3), 0);
            }
            else
            {
                WriteInt8(new IntPtr(0x144555CF3), 1);
            }
        }

        private void hideRemo_CheckedChanged(object sender, EventArgs e)
        {
            if (hideRemo.Checked)
            {
                WriteInt8(new IntPtr(0x144555CF4), 0);
            }
            else
            {
                WriteInt8(new IntPtr(0x144555CF4), 1);
            }
        }

        private void evntdraw_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x473AD78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0xA8);
            if (evntdraw.Checked)
            {
                WriteInt8(FlgPtr, 1);
            }
            else
            {
                WriteInt8(FlgPtr, 0);
            }
        }

        private void feetdraw_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768F98);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x6B);
            if (feetdraw.Checked)
            {
                WriteInt8(FlgPtr, 1);
            }
            else
            {
                WriteInt8(FlgPtr, 0);
            }
        }

        private void stpEvent_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x473AD78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0xD4);
            if (stpEvent.Checked)
            {
                WriteInt8(FlgPtr, 1);
            }
            else
            {
                WriteInt8(FlgPtr, 0);
            }
        }

        private void pryMode_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768F78);
            if (pryMode.Checked)
            {
                WriteInt8(FlgPtr, 1);
            }
            else
            {
                WriteInt8(FlgPtr, 0);
            }
        }

        private void plrExterm_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768F69);
            if (plrExterm.Checked)
            {
                WriteInt8(FlgPtr, 1);
            }
            else
            {
                WriteInt8(FlgPtr, 0);
            }
        }

        private void dsbGrav_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1A08);
            byte bit = 0x40;
            byte OverflowChk = ReadInt8(FlgPtr);
            if (dsbGrav.Checked)
            {
                byte SumByte = (byte)(OverflowChk + bit);
                WriteInt8(FlgPtr, SumByte);
            }
            else
            {
                byte SumByte = (byte)(OverflowChk - bit);
                WriteInt8(FlgPtr, SumByte);
            }
        }

        private void noDead_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1C0);
            byte bit = 0x4;
            byte OverflowChk = ReadInt8(FlgPtr);
            if (noDead.Checked)
            {
                byte SumByte = (byte)(OverflowChk + bit);
                WriteInt8(FlgPtr, SumByte);
            }
            else
            {
                byte SumByte = (byte)(OverflowChk - bit);
                WriteInt8(FlgPtr, SumByte);
            }
        }

        private void backReadF_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1EE9);
            byte bit = 0x80;
            byte OverflowChk = ReadInt8(FlgPtr);
            if (backReadF.Checked)
            {
                byte SumByte = (byte)(OverflowChk + bit);
                WriteInt8(FlgPtr, SumByte);
            }
            else
            {
                byte SumByte = (byte)(OverflowChk - bit);
                WriteInt8(FlgPtr, SumByte);
            }
        }

        private void infStam_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1C0);
            byte bit = 0x10;
            byte OverflowChk = ReadInt8(FlgPtr);
            if (infStam.Checked)
            {
                byte SumByte = (byte)(OverflowChk + bit);
                WriteInt8(FlgPtr, SumByte);
            }
            else
            {
                byte SumByte = (byte)(OverflowChk - bit);
                WriteInt8(FlgPtr, SumByte);
            }
        }

        private void infMana_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1C0);
            byte bit = 0x20;
            byte OverflowChk = ReadInt8(FlgPtr);
            if (infMana.Checked)
            {
                byte SumByte = (byte)(OverflowChk + bit);
                WriteInt8(FlgPtr, SumByte);
            }
            else
            {
                byte SumByte = (byte)(OverflowChk - bit);
                WriteInt8(FlgPtr, SumByte);
            }
        }

        private void goodsConsum_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1EEA);
            byte bit = 0x8;
            byte OverflowChk = ReadInt8(FlgPtr);
            if (goodsConsum.Checked)
            {
                byte SumByte = (byte)(OverflowChk + bit);
                WriteInt8(FlgPtr, SumByte);
            }
            else
            {
                byte SumByte = (byte)(OverflowChk - bit);
                WriteInt8(FlgPtr, SumByte);
            }
        }

        private void btnScd_Click(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0xD8);
            WriteInt32(FlgPtr, 0);
        }

        private void StayAnimID_TextChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x58);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x20);
            try
            {
                var value = Convert.ToInt32(StayAnimID.Text);
                WriteInt32(FlgPtr, value);
            }
            catch (System.FormatException)
            {
                var value = 0xFF;
                WriteInt32(FlgPtr, value);
            }
        }

        private void DeathCam_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x90);
            if (DeathCam.Checked)
            {
                WriteInt8(FlgPtr, 1);
            }
            else
            {
                WriteInt8(FlgPtr, 0);
            }
        }

        private void UnlockGestures_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4740178);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x10);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x7B8);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x10);
            if (UnlockGestures.Checked)
            {
                byte[] Aob = { 0x3, 0x0, 0x0, 0x0, 0x5, 0x0, 0x1, 0x0, 0x7, 0x0, 0x2, 0x0, 0x9, 0x0, 0x3, 0x0, 0xB, 0x0, 0x4, 0x0, 0xD, 0x0, 0x5, 0x0, 0xF, 0x0, 0x6, 0x0, 0x11, 0x0, 0x7, 0x0, 0x13, 0x0, 0x8, 0x0, 0x15, 0x0, 0x9, 0x0, 0x17, 0x0, 0xA, 0x0, 0x19, 0x0, 0xB, 0x0, 0x1B, 0x0, 0xC, 0x0, 0x1D, 0x0, 0xD, 0x0, 0x1F, 0x0, 0xE, 0x0, 0x21, 0x0, 0xF, 0x0, 0x23, 0x0, 0x10, 0x0, 0x5, 0x0, 0x11, 0x0, 0x27, 0x0, 0x12, 0x0, 0x29, 0x0, 0x13, 0x0, 0x2B, 0x0, 0x14, 0x0, 0x2D, 0x0, 0x15, 0x0, 0x2F, 0x0, 0x16, 0x0, 0x31, 0x0, 0x17, 0x0, 0x33, 0x0, 0x18, 0x0, 0x35, 0x0, 0x19, 0x0, 0x37, 0x0, 0x1A, 0x0, 0x39, 0x0, 0x1B, 0x0, 0x3B, 0x0, 0x1C, 0x0, 0x3D, 0x0, 0x1D, 0x0, 0x3F, 0x0, 0x1E, 0x0, 0x41, 0x0, 0x1F, 0x0, 0x43, 0x0, 0x20, 0x0, 0x45, 0x0, 0x21, 0x0, 0x47 };
                WriteBytes(FlgPtr, Aob);
            }
            else
            {

            }
        }

        private void equipSpellF_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x41, 0x5F, 0x48, 0x83, 0xEC, 0x48, 0x49, 0xBE, 0xE0, 0xBE, 0xAF, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0xB8, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xD0, 0xB9, 0, 0, 0, 0, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x48, 0x41, 0x57, 0xC3 };
            var bytes2 = new byte[7];
            var BufferAob = new byte[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 0, 0, 0, 0, 00, 00, 00, 00, 00, 00, 00, 00 };
            var bytjmp1 = 0x12;
            var bytjmp2 = 0x1E;
            var spelljmp = 0x3C;

            byte[] CheckSpellIdB = System.Text.Encoding.UTF8.GetBytes(spellBoxE.Text);
            string CheckSpellIdS = System.Text.Encoding.UTF8.GetString(CheckSpellIdB, 0, 7);
            int SpellID = Convert.ToInt32(CheckSpellIdS);
            int SlotID = Convert.ToInt32(slotUpDownN.Value);

            var buffer = 512;
            var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            var buffer2 = 0x60;
            var address2 = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer2, 0x1000 | 0x2000, 0X40);
            var addr2L = (long)address2;

            bytes2 = BitConverter.GetBytes(addr2L);
            Array.Copy(bytes2, 0, bytes, bytjmp1, bytes2.Length);

            bytes2 = BitConverter.GetBytes(SlotID);
            Array.Copy(bytes2, 0, bytes, bytjmp2, bytes2.Length);

            bytes2 = BitConverter.GetBytes(SpellID);
            Array.Copy(bytes2, 0, BufferAob, spelljmp, bytes2.Length);


            Kernel32.WriteProcessMemory(ProcessHandle, address2, BufferAob, new UIntPtr((uint)BufferAob.Length), UIntPtr.Zero);

            if (address != IntPtr.Zero)
            {
                if (Kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        Kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                Kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
                Kernel32.VirtualFreeEx(ProcessHandle, address2, buffer2, 2);
            }
        }
       

        private void applyPos_Click(object sender, EventArgs e)
        {
            var posPtr_1 = IntPtr.Add(BaseAddress, 0x4768E78);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x80);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x50);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            WriteInt8(posPtr_1 + 0x18A, 1);
        }

        private void changePosX_ValueChanged(object sender, EventArgs e)
        {
            var posPtr_1 = IntPtr.Add(BaseAddress, 0x4768E78);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x80);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x50);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            WriteFloat(posPtr_1 + 0x190, (float)changePosX.Value);
        }

        private void changePosZ_ValueChanged(object sender, EventArgs e)
        {
            var posPtr_1 = IntPtr.Add(BaseAddress, 0x4768E78);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x80);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x50);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            WriteFloat(posPtr_1 + 0x194, (float)changePosZ.Value);
        }

        private void changePosY_ValueChanged(object sender, EventArgs e)
        {
            var posPtr_1 = IntPtr.Add(BaseAddress, 0x4768E78);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x80);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x50);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            WriteFloat(posPtr_1 + 0x198, (float)changePosY.Value);
        }

        private void angle_Tp_ValueChanged(object sender, EventArgs e)
        {
            var posPtr_1 = IntPtr.Add(BaseAddress, 0x4768E78);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x80);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            posPtr_1 = IntPtr.Add(posPtr_1, 0x50);
            posPtr_1 = new IntPtr(ReadInt64(posPtr_1));
            WriteFloat(posPtr_1 + 0x1A4, (float)angle_Tp.Value);
        }

        private void ezStateLadd_SelectedIndexChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            var MovPtr = IntPtr.Add(FlgPtr, 0x1F90);
            MovPtr = new IntPtr(ReadInt64(MovPtr));
            MovPtr = IntPtr.Add(MovPtr, 0x58);
            MovPtr = new IntPtr(ReadInt64(MovPtr));
            MovPtr = IntPtr.Add(MovPtr, 0x28);
            MovPtr = new IntPtr(ReadInt64(MovPtr));

            FlgPtr = IntPtr.Add(FlgPtr, 0x50);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x48);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x55C);
            try
            {
                byte[] unicodeBytes = System.Text.Encoding.UTF32.GetBytes(ezStateLadd.Text);
                string substring = System.Text.Encoding.UTF32.GetString(unicodeBytes, 0, 4);
                var value = Convert.ToInt32(substring);
                WriteInt32(FlgPtr, value);
                WriteInt8(MovPtr, 1);
            }
            catch (System.FormatException)
            {
                var value = 0xFF;
                WriteInt32(FlgPtr, value);
            }
        }

        private void DamageLevelLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x0);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1C);
            try
            {
                byte[] unicodeBytes = System.Text.Encoding.UTF32.GetBytes(DamageLevelLabel.Text);
                string substring = System.Text.Encoding.UTF32.GetString(unicodeBytes, 0, 2*4);
                var value = Convert.ToInt32(substring);
                WriteInt32(FlgPtr, value);
            }
            catch (System.FormatException)
            {
                var value = 0xFF;
                WriteInt32(FlgPtr, value);
            }
        }

        private void SpeedLabel_ValueChanged_1(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x28);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0xA58);
            try
            {
                float value = (float)SpeedLabel.Value;
                WriteFloat(FlgPtr, value);
            }
            catch (System.FormatException)
            {
                float value = 1;
                WriteFloat(FlgPtr, value);
            }
        }

        private void emberst__Click(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4740178);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x10);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            WriteInt8(FlgPtr + 0x100, 1);
        }

        private void covenantChng_SelectedIndexChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4740178);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x10);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            try
            {
                byte[] unicodeBytes = System.Text.Encoding.UTF32.GetBytes(covenantChng.Text);
                string substring = System.Text.Encoding.UTF32.GetString(unicodeBytes, 0, 5 * 4);
                short value = Convert.ToInt16(substring);
                WriteInt32(FlgPtr + 0x328, value);
            }
            catch (System.FormatException)
            {
                var value = 0;
                WriteInt32(FlgPtr, value);
            }
        }

        private void DropdownItemType_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            byte[] ItemTypeB = System.Text.Encoding.UTF8.GetBytes(DropdownItemType.Text);
            string ItemTypeS = System.Text.Encoding.UTF8.GetString(ItemTypeB, 0, 8);
            var ItemType32 = Convert.ToInt32(ItemTypeS, 16);
            var encodingType = System.Text.Encoding.GetEncoding("UTF-8");

            var path = Environment.CurrentDirectory;

            switch (ItemType32)
            {
                case (0x00000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Weapons.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        //. = Res;
                        break;
                    }
                case (0x10000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Armors.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        break;
                    }
                case (0x20000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Rings.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        break;
                    }
                case (0x40000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Goods.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        break;
                    }
            }
        }

        private void hpread_ValueChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            WriteInt32(FlgPtr + 0xD8, (int)hpread.Value);
        }

        private void staminaRead_ValueChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            WriteInt32(FlgPtr + 0xF0, (int)staminaRead.Value);
        }

        private void manaRead_ValueChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            WriteInt32(FlgPtr + 0xE4, (int)manaRead.Value);
        }

        private void maxhpRead_ValueChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            WriteInt32(FlgPtr + 0xE0, (int)maxhpRead.Value);
        }

        private void maxStaminaRead_ValueChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            WriteInt32(FlgPtr + 0xF8, (int)maxStaminaRead.Value);
        }

        private void maxManaRead_ValueChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x1F90);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x18);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));

            WriteInt32(FlgPtr + 0xEC, (int)maxManaRead.Value);
        }

        private void DropdownItemType_SelectedIndexChanged_2(object sender, EventArgs e)
        {
            byte[] ItemTypeB = System.Text.Encoding.UTF8.GetBytes(DropdownItemType.Text);
            string ItemTypeS = System.Text.Encoding.UTF8.GetString(ItemTypeB, 0, 8);
            var ItemType32 = Convert.ToInt32(ItemTypeS, 16);
            var encodingType = System.Text.Encoding.GetEncoding("UTF-8");

            var path = Environment.CurrentDirectory;

            switch (ItemType32)
            {
                case (0x00000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Weapons.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        //. = Res;
                        break;
                    }
                case (0x10000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Armors.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        break;
                    }
                case (0x20000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Rings.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        break;
                    }
                case (0x40000000):
                    {
                        ItemID_.Items.Clear();
                        path = System.IO.Path.Combine(path, "txtDataTr", "Goods.txt");
                        System.IO.StreamReader readIds = System.IO.File.OpenText(path);
                        int IdNumber = System.IO.File.ReadAllLines(path).Length;

                        for (int i = 0; i < IdNumber; i++)
                        {

                            var stringF = readIds.ReadLine();
                            ItemID_.Items.Add(stringF);
                        }
                        readIds.Close();
                        break;
                    }
            }
        }

        private void warpN_SelectedIndexChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4743AB0);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            try
            {
                byte[] unicodeBytes = System.Text.Encoding.UTF32.GetBytes(warpN.Text);
                string substring = System.Text.Encoding.UTF32.GetString(unicodeBytes, 0, 7 * 4);
                int value = Convert.ToInt32(substring);
                WriteInt32(FlgPtr + 0xACC, value);
            }
            catch (System.FormatException)
            {
               
            }
        }

        private void erasebtnEff_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x49, 0xBE, 0x70, 0x40, 0x9F, 0x40, 0x01, 0x00, 0x00, 0x00, 0xBA, 0, 0, 0, 0, 0x48, 0xA1, 0x78, 0x8E, 0x76, 0x44, 0x01, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x80, 0x80, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x80, 0xC8, 0x11, 0x00, 0x00, 0x48, 0x8B, 0xC8, 0x48, 0x83, 0xEC, 0x48, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x48, 0xC3 };
            var bytes2 = new byte[7];

            var bytjmp1 = 0xB;

            var buffer = 512;
            var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            var value = 0;

            try
            {
                value = Convert.ToInt32(effectBase_1.Text);
            }
            catch (System.FormatException)
            {
                value = 0;
            }

            bytes2 = BitConverter.GetBytes(value);
            Array.Copy(bytes2, 0, bytes, bytjmp1, bytes2.Length);

            if (address != IntPtr.Zero)
            {
                if (Kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        Kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                Kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void addEffecx_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x49, 0xBE, 0x30, 0x6A, 0x88, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0xA1, 0x78, 0x8E, 0x76, 0x44, 0x01, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x80, 0x80, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x80, 0x90, 0x20, 0x00, 0x00, 0x48, 0x8B, 0x40, 0x10, 0x48, 0x8B, 0xC8, 0xBA, 0, 0, 0, 0, 0x48, 0x83, 0xEC, 0x38, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x38, 0xC3 };
            var bytes2 = new byte[7];

            var bytjmp1 = 0x2A;

            var buffer = 512;
            var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            var value = 0;

            try
            {
                value = Convert.ToInt32(effectBase__.Text);
            }
            catch (System.FormatException)
            {
                value = 0;
            }

            bytes2 = BitConverter.GetBytes(value);
            Array.Copy(bytes2, 0, bytes, bytjmp1, bytes2.Length);

            if (address != IntPtr.Zero)
            {
                if (Kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        Kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                Kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void maphitChkbox_CheckedChanged(object sender, EventArgs e)
        {
            var FlgPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x80);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            FlgPtr = IntPtr.Add(FlgPtr, 0x50);
            FlgPtr = new IntPtr(ReadInt64(FlgPtr));
            byte bit = 0x10;
            byte OverflowChk = ReadInt8(FlgPtr + 0x186);
            if (goodsConsum.Checked)
            {
                byte SumByte = (byte)(OverflowChk + bit);
                WriteInt8(FlgPtr + 0x186, SumByte);
            }
            else
            {
                byte SumByte = (byte)(OverflowChk - bit);
                WriteInt8(FlgPtr + 0x186, SumByte);
            }
        }

        private void LoadEffectHuman_Click(object sender, EventArgs e)
        {
            dataGridEffectsBase_.Rows.Clear();
            dataGridEffectsBase_.Columns.Clear();

            dataGridEffectsBase_.Columns.Add("", "EffectID");

            var EffectPtr = IntPtr.Add(BaseAddress, 0x4768E78);
            EffectPtr = new IntPtr(ReadInt64(EffectPtr));
            EffectPtr = IntPtr.Add(EffectPtr, 0x80);
            EffectPtr = new IntPtr(ReadInt64(EffectPtr));
            EffectPtr = IntPtr.Add(EffectPtr, 0x11C8);
            EffectPtr = new IntPtr(ReadInt64(EffectPtr));
            EffectPtr = IntPtr.Add(EffectPtr, 0x8);
            EffectPtr = new IntPtr(ReadInt64(EffectPtr));

            for (int i = 0; i > -1; i++)
            {
                var Null__ = (ReadInt32(EffectPtr + 0x60)).ToString();
                dataGridEffectsBase_.Rows.Add(Null__, "");
                EffectPtr = IntPtr.Add(EffectPtr, 0x78);

                var ChkQ = ReadInt64(EffectPtr);
                if (ChkQ == 0)
                {
                    break;
                }
                EffectPtr = new IntPtr(ReadInt64(EffectPtr));
            }
            
        }



        // Param Edits


    }
}
