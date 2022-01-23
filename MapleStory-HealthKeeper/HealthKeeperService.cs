using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Iced.Intel;
using MapleStory_HealthKeeper.Model;
using MapleStory_HealthKeeper.Plugin;
using Reloaded.Memory.Buffers;
using Reloaded.Memory.Sources;
using static Iced.Intel.AssemblerRegisters;

namespace MapleStory_HealthKeeper
{
    public class HealthKeeperService
    {
        private MainWindowViewModel ViewModel { get; set; }
        private Dictionary<IntPtr, HealthPointer> HealthPointerCache = new();

        public HealthKeeperService(MainWindowViewModel mainWindowViewModel)
        {
            backgroundWorker = new();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            ViewModel = mainWindowViewModel;
        }

        public void Start()
        {
            if (backgroundWorker.IsBusy == false)
                backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BackgroundSimulate backgroundSimulate = new();
            while (true)
            {
                try
                {
                    var ps = Process.GetProcessesByName(ViewModel.MapleStoryProcessName);
                    if (ps.Length == 0)
                    {
                        ViewModel.Status = MainWindowViewModel.MapleStoryNotFound;
                        PreciseDelay.Delay(3000);
                        continue;
                    }
                    ViewModel.Status = $"Found {ps.Length} process";
                    foreach (var process in ps)
                    {
                        HealthPointer? health = TryGetHealthPointer(process);
                        if (health == null)
                            continue;

                        IMemory memory = new ExternalMemory(process);
                        memory.Read(health.Value.Hp, out int HP);
                        memory.Read(health.Value.MaxHp, out int MaxHP);

                        memory.Read(health.Value.Mp, out int MP);
                        memory.Read(health.Value.MaxMp, out int MaxMP);

                        //memory.Read(health.Value.Exp, out int Exp);
                        //memory.Read(health.Value.MaxExp, out int MaxExp);
                        if (HP == 0)
                            continue;
                        backgroundSimulate.Hwnd = process.MainWindowHandle;
                        if (HP * 100d / MaxHP < ViewModel.KeepHpOverThen)
                        {
                            backgroundSimulate.KeyPress(ViewModel.HpKey);
                        }

                        if (MP * 100d / MaxMP < ViewModel.KeepMpOverThen)
                        {
                            backgroundSimulate.KeyPress(ViewModel.MpKey);
                        }
                    }
                }
                catch
                {
                }
                PreciseDelay.Delay(ViewModel.Delay);
            }
        }

        private PreciseDelay PreciseDelay { get; } = new();
        private BackgroundWorker backgroundWorker { get; }

        internal HealthPointer? TryGetHealthPointer(Process process)
        {
            if (HealthPointerCache.ContainsKey(process.MainWindowHandle))
            {
                return HealthPointerCache[process.MainWindowHandle];
            }
            else
            {
                if (GetHealthPointer(process, out HealthPointer? health))
                {
                    HealthPointerCache[process.MainWindowHandle] = (HealthPointer)health;
                }
                else
                {
                    PreciseDelay.Delay(5000);
                    if (HealthPointerCache.Count > 10)
                        HealthPointerCache.Clear();
                    if (WriteHook(process))
                    {
                        if (GetHealthPointer(process, out health))
                            HealthPointerCache[process.MainWindowHandle] = (HealthPointer)health;
                    }
                }
                return health;
            }
        }

        internal bool WriteHook(Process process)
        {
            if (process.HasExited || process.MainModule == null)
                return false;
            IMemory memory = new ExternalMemory(process);
            Reloaded.Memory.Sigscan.Scanner scanner = new Reloaded.Memory.Sigscan.Scanner(process, process.MainModule);

            var HookPointerAOB = scanner.SimpleFindPattern("8D 45 D0 50 E8 ?? ?? ?? ?? 8B F8 39 1F");
            if (HookPointerAOB.Found == false)
            {
                // Can not get HP
                return false;
            }
            IntPtr HookPointer = IntPtr.Add(IntPtr.Add(process.MainModule.BaseAddress, HookPointerAOB.Offset), 4);
            memory.ReadRaw(HookPointer, out byte[] HookPointerBackup, 5);

            var newMemoryPointer = memory.Allocate(128);
            var HP = memory.Allocate(4);
            var MaxHP = memory.Allocate(4);
            var MP = memory.Allocate(4);
            var MaxMP = memory.Allocate(4);
            var Exp = memory.Allocate(4);
            var MaxExp = memory.Allocate(4);

            var asm = new Assembler(32);
            asm.push(ebx);
            asm.mov(ebx, __dword_ptr[ebp + 0x8]);
            asm.mov(__dword_ptr[(long)HP], ebx);
            asm.mov(ebx, __dword_ptr[ebp + 0xC]);
            asm.mov(__dword_ptr[(long)MaxHP], ebx);
            asm.mov(ebx, __dword_ptr[ebp + 0x10]);
            asm.mov(__dword_ptr[(long)MP], ebx);
            asm.mov(ebx, __dword_ptr[ebp + 0x14]);
            asm.mov(__dword_ptr[(long)MaxMP], ebx);
            asm.mov(ebx, __dword_ptr[ebp + 0x18]);
            asm.mov(__dword_ptr[(long)Exp], ebx);
            asm.mov(ebx, __dword_ptr[ebp + 0x1C]);
            asm.mov(__dword_ptr[(long)MaxExp], ebx);
            asm.pop(ebx);

            //把被換掉的call放回去
            var codeReader = new ByteArrayCodeReader(HookPointerBackup);
            var decoder = Iced.Intel.Decoder.Create(32, codeReader);
            decoder.IP = (ulong)HookPointer;
            ulong endRip = decoder.IP + (uint)HookPointerBackup.Length;

            var instructions = new List<Instruction>();
            while (decoder.IP < endRip)
                instructions.Add(decoder.Decode());
            asm.AddInstruction(instructions[0]);

            //跳回去原本那行的下一行
            asm.jmp((ulong)(HookPointer + 5));

            //寫入newMemory
            var stream = new MemoryStream();
            asm.Assemble(new StreamCodeWriter(stream), (ulong)newMemoryPointer);
            var OpCode = stream.ToArray();
            memory.WriteRaw(newMemoryPointer, OpCode);

            //寫入各個指針位置到Buffer中
            var memoryBufferHelper = new MemoryBufferHelper(process);
            var buffer = memoryBufferHelper.CreateMemoryBuffer(128);
            var healthPointer = new HealthPointer()
            {
                Hook = HookPointer,
                Hp = HP,
                MaxHp = MaxHP,
                Mp = MP,
                MaxMp = MaxMP,
                Exp = Exp,
                MaxExp = MaxExp
            };
            if (buffer.Add(ref healthPointer) == IntPtr.Zero)
                throw new Exception("無法寫入指針記憶體區段");

            //把原本的call替換成jmp
            asm = new Assembler(32);
            asm.jmp((ulong)newMemoryPointer);
            stream = new MemoryStream();
            asm.Assemble(new StreamCodeWriter(stream), (ulong)HookPointer);
            OpCode = stream.ToArray();

            memory.WriteRaw(HookPointer, OpCode);
            return true;
        }

        internal bool GetHealthPointer(Process process, [NotNullWhen(true)] out HealthPointer? healthPointer)
        {
            healthPointer = null;
            if (process.HasExited || process.MainModule == null)
                return false;
            IMemory memory = new ExternalMemory(process);

            var memoryBufferHelper = new MemoryBufferHelper(process);
            var buffers = memoryBufferHelper.FindBuffers(128);
            foreach (var buffer in buffers)
            {
                try
                {
                    memory.Read(buffer.Properties.DataPointer, out HealthPointer pointer);
                    healthPointer = pointer;
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }
    }
}