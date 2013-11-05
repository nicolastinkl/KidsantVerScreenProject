using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SoundUtil
{
    static class SoundUtil
    {
       /* private static Int32 BUILD_SOUND_LENGTH = 44100;

        public static Int16[] CreateSoundStream(Int32 id)
        {
            Int16[] soundStreams = new Int16[BUILD_SOUND_LENGTH];
            GCHandle gh = GCHandle.Alloc(soundStreams, GCHandleType.Pinned);

            BuildSoundStream(id, gh.AddrOfPinnedObject());

            return soundStreams;
        }

        public static Int32 AnalyseSoundStream(Int16[] inBuffer, Int32 length)
        {
            GCHandle gh = GCHandle.Alloc(inBuffer, GCHandleType.Pinned);
            return ParseSoundStream(gh.AddrOfPinnedObject(), length);
        }

        public static void Send(Int32 length)
        { 
            sendSoundWaveStart(length);
             
        }

        public static void StopSend()
        {
            sendSoundWaveStop();
        }

        public static void StartReceive()
        { 
               startingGetWaweData();
        }

        [DllImport("SoundUtilNative.dll", EntryPoint = "process_play", CallingConvention = CallingConvention.Cdecl)]
        private extern static void BuildSoundStream(Int32 id, IntPtr outBuffer);

        [DllImport("SoundUtilNative.dll", EntryPoint = "process_record", CallingConvention = CallingConvention.Cdecl)]
        private extern static Int32 ParseSoundStream(IntPtr inBuffer, Int32 length);

        [DllImport("SoundUtilNative.dll", EntryPoint = "sum", CallingConvention = CallingConvention.Cdecl)]
        public extern static Int32 Sum(ref Int16 a, ref Int16 b);

        [DllImport("SoundUtilNative.dll", EntryPoint = "valuation", CallingConvention = CallingConvention.Cdecl)]
        public extern static Int32 Valuation(IntPtr buf, Int32 b);




        //chagne by ljh
        //TODO:
        [DllImport("SoundUtilNative.dll", EntryPoint = "sendSoundWaveStart", CallingConvention = CallingConvention.Cdecl)]
        public extern static void sendSoundWaveStart(Int32 waveCode);


        [DllImport("SoundUtilNative.dll", EntryPoint = "sendSoundWaveStop", CallingConvention = CallingConvention.Cdecl)]
        public extern static void sendSoundWaveStop();

        [DllImport("SoundUtilNative.dll", EntryPoint = "startingGetWaweData", CallingConvention = CallingConvention.Cdecl)]
        public extern static void startingGetWaweData();

        [DllImport("SoundUtilNative.dll", EntryPoint = "startgetWaweDataSuccess", CallingConvention = CallingConvention.Cdecl)]

        public extern static Int32 startgetWaweDataSuccess();*/

        /* by yuanhe */

        public static void Sounding(Int32 id)
        {
            // if sending  ... preformselector @<StopReceive()>
            // StopReceive();
            StartBuild(id);
        }

        public static void StopSounding()
        {
            StopBuild();
        }

       
        public static void Recording(SoundReceivedDelegate callback)
        {
            // if recording ... preformselector @<StopBuild()>
             
            //  StopBuild();
            StartReceive(callback);
        }

        public static void StopRecording()
        {
            StopReceive();
        }

        [DllImport("SoundUtilNative.dll", EntryPoint = "StartBuild", CallingConvention = CallingConvention.Cdecl)]
        private extern static void StartBuild(Int32 id);

        [DllImport("SoundUtilNative.dll",  EntryPoint = "StartReceive", CallingConvention = CallingConvention.Cdecl)]
        private extern static Int32 StartReceive([MarshalAs(UnmanagedType.FunctionPtr)]SoundReceivedDelegate soundDelegate);

        [DllImport("SoundUtilNative.dll", EntryPoint = "StopBuild", CallingConvention = CallingConvention.Cdecl)]
        private extern static void StopBuild();

        [DllImport("SoundUtilNative.dll",  EntryPoint = "StopReceive", CallingConvention = CallingConvention.Cdecl)]
        private extern static void StopReceive();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SoundReceivedDelegate(Int32 id); 

    }
}
