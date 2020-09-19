using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace ParentChildProcessRelation
{
    public class ParentProcess
    {
        private Dictionary<string, Process> Processes;

        private Dictionary<string, bool> waitClientReplyFlagDict;

        private string childProcessPath;

        private readonly int parentClientWaitConnectionTimeoutMs;

        private readonly int parentClientWaitClientReplyTimeoutMs;
        public Dictionary<string, Process> Childrens => Processes;

        public ParentProcess(string _childProcessPath, int _parentClientWaitConnectionTimeout, int _parentClientWaitClientReplyTimeout)
        {
            Processes = new Dictionary<string, Process>();
            waitClientReplyFlagDict = new Dictionary<string, bool>();
            childProcessPath = _childProcessPath;
            parentClientWaitConnectionTimeoutMs = _parentClientWaitConnectionTimeout * 1000;
            parentClientWaitClientReplyTimeoutMs = _parentClientWaitClientReplyTimeout * 1000;
        }

        public void StartNewChild(string guid)
        {
            Process value = null;
            if (waitClientReplyFlagDict.ContainsKey(guid))
            {
                waitClientReplyFlagDict.Remove(guid);
            }
            waitClientReplyFlagDict.Add(guid, false);
            var startInfo = new ProcessStartInfo
            {
                FileName = childProcessPath,
                Arguments = guid
            };
            value = Process.Start(startInfo);
            Processes.Add(guid, value);
        }

        public string SendCommandAndGetResult(string guid, string inputXml)
        {
            var result = "";
            try
            {
                var namedPipeClientStream = new NamedPipeClientStream(".", guid, PipeDirection.Out, PipeOptions.Asynchronous);
                if (parentClientWaitConnectionTimeoutMs > 0)
                {
                    namedPipeClientStream.Connect(parentClientWaitConnectionTimeoutMs);
                }
                else
                {
                    namedPipeClientStream.Connect();
                }
                var streamWriter = new StreamWriter(namedPipeClientStream);
                streamWriter.WriteLine(inputXml);
                streamWriter.Flush();
                namedPipeClientStream.Flush();
                namedPipeClientStream.Close();
                namedPipeClientStream.Dispose();
                NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(CommonConstants.replyPipeGuidPrefix + guid, PipeDirection.In, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                var asyncResult = namedPipeServerStream.BeginWaitForConnection(null, null);
                if (!asyncResult.AsyncWaitHandle.WaitOne(parentClientWaitClientReplyTimeoutMs))
                {
                    throw new Exception("Pipe reply wait timed out");
                }
                namedPipeServerStream.EndWaitForConnection(asyncResult);
                var streamReader = new StreamReader(namedPipeServerStream);
                result = streamReader.ReadToEnd();
                streamReader.Close();
                namedPipeServerStream.Close();
                namedPipeServerStream.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Send command: "+ex.Message);
            }
            return result;
        }

        public void KillProcess(string guid)
        {
            Processes[guid].Kill();
        }
    }
}
