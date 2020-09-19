using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace ParentChildProcessRelation
{
    public class ChildProcess
    {
        private readonly string Guid;
        private readonly CommandProcessorDelegate CommandProcessor;
        public ChildProcess(string Guid, CommandProcessorDelegate commandProcessor)
        {
            this.Guid = Guid;
            CommandProcessor = commandProcessor;
        }
        public void Start()
        {
            new Thread(new ThreadStart(commandListenerThread)).Start();
        }
        private void commandListenerThread()
        {
            while (true)
            {
                NamedPipeClientStream namedPipeClientStream = null;
                try
                {
                    var namedPipeServerStream = new NamedPipeServerStream(Guid, PipeDirection.In, -1);
                    namedPipeServerStream.WaitForConnection();
                    var streamReader = new StreamReader(namedPipeServerStream);
                    var inputParameters = streamReader.ReadToEnd();
                    var value = CommandProcessor(inputParameters).Result;
                    streamReader.Close();
                    namedPipeServerStream.Close();
                    namedPipeServerStream.Dispose();
                     namedPipeClientStream = new NamedPipeClientStream(".", CommonConstants.replyPipeGuidPrefix + Guid, PipeDirection.Out, PipeOptions.Asynchronous);
                    namedPipeClientStream.Connect(3000);
                    var streamWriter = new StreamWriter(namedPipeClientStream);
                    streamWriter.WriteLine(value);
                    streamWriter.Flush();
                    namedPipeClientStream.Flush();
                    namedPipeClientStream.Close();
                    namedPipeClientStream.Dispose();
                }
                catch (Exception ex)
                {
                    namedPipeClientStream.Close();
                    namedPipeClientStream.Dispose();
                    File.WriteAllText("logs.txt",ex.Message+ex.StackTrace+ex.Source);
                }
            }
        }
    }
}
